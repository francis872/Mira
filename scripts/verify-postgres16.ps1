param(
    [string]$PostgresBin = '.local/postgresql16/pgsql/bin',
    [int]$DatabasePort = 5547,
    [int]$ApiPort = 8082,
    [int]$FrontendPort = 5001
)

# Instancia exclusiva: nunca abre el directorio PostgreSQL 17 ni detiene la plataforma habitual.
$ErrorActionPreference = 'Stop'
$repoRoot = Split-Path $PSScriptRoot -Parent
Set-Location $repoRoot
$PostgresBin = (Resolve-Path -LiteralPath $PostgresBin).Path
$pgCtl = Join-Path $PostgresBin 'pg_ctl.exe'
$psql = Join-Path $PostgresBin 'psql.exe'
$version = & (Join-Path $PostgresBin 'postgres.exe') --version
if ($LASTEXITCODE -ne 0 -or $version -notmatch 'PostgreSQL\) 16\.') { throw 'Se requieren binarios PostgreSQL 16' }
foreach ($port in @($DatabasePort, $ApiPort, $FrontendPort)) {
    $listener = [Net.Sockets.TcpListener]::new([Net.IPAddress]::Loopback, $port)
    try { $listener.Start() } catch { throw "El puerto de pruebas $port esta ocupado; no se detuvo ningun servicio" }
    finally { $listener.Stop() }
}
if (@($DatabasePort, $ApiPort, $FrontendPort | Select-Object -Unique).Count -ne 3) { throw 'Los tres puertos deben ser diferentes' }

$runtime = Join-Path $repoRoot '.local/pg16-validation'
$data = Join-Path $runtime 'postgres'
New-Item -ItemType Directory -Path $runtime -Force | Out-Null
$configFile = Join-Path $runtime 'config.env'
$passwordFile = Join-Path $runtime 'init-password'
$encoding = [Text.UTF8Encoding]::new($false)
$role = 'mira_validation'
$password = $null
if (Test-Path -LiteralPath $configFile) {
    foreach ($line in (Get-Content -LiteralPath $configFile)) {
        if ($line -match '^POSTGRES_PASSWORD=(.*)$') { $password = $matches[1] }
    }
}
if (-not $password) {
    if (Test-Path -LiteralPath (Join-Path $data 'PG_VERSION')) { throw 'Falta la configuracion de una instancia existente; no se reemplazo' }
    $bytes = New-Object byte[] 32
    $rng = [Security.Cryptography.RandomNumberGenerator]::Create()
    try { $rng.GetBytes($bytes) } finally { $rng.Dispose() }
    $password = [BitConverter]::ToString($bytes).Replace('-', '')
}
$database = 'mira_pg16_' + (Get-Date -Format 'yyyyMMddHHmmss') + '_' + $PID
[IO.File]::WriteAllLines($configFile, @(
    "POSTGRES_USER=$role", "POSTGRES_PASSWORD=$password", "POSTGRES_DB=$database",
    "POSTGRES_PORT=$DatabasePort", "API_PORT=$ApiPort", "FRONTEND_PORT=$FrontendPort"
), $encoding)
$previous = @{}
foreach ($key in @('PGPASSWORD','PGCLIENTENCODING','PGCONNECT_TIMEOUT','ConnectionStrings__PostgreSql','ASPNETCORE_URLS','ASPNETCORE_ENVIRONMENT','API_URL','SWAGGER_URL','PORT','SECRET_KEY','SESSION_COOKIE_SECURE')) {
    $previous[$key] = [Environment]::GetEnvironmentVariable($key, 'Process')
}
$startedDatabase = $false
$apiProcess = $null
$webProcess = $null
try {
    $env:PGPASSWORD = $password
    $env:PGCLIENTENCODING = 'UTF8'
    $env:PGCONNECT_TIMEOUT = '5'
    if (-not (Test-Path -LiteralPath (Join-Path $data 'PG_VERSION'))) {
        [IO.File]::WriteAllText($passwordFile, $password, $encoding)
        try {
            & (Join-Path $PostgresBin 'initdb.exe') -D $data -U $role --encoding=UTF8 --locale=C --auth=scram-sha-256 "--pwfile=$passwordFile"
            if ($LASTEXITCODE -ne 0) { throw 'No se pudo inicializar la instancia exclusiva' }
        } finally { Remove-Item -LiteralPath $passwordFile -ErrorAction SilentlyContinue }
    }
    & $pgCtl start -D $data -l (Join-Path $runtime 'postgres.log') -o "-p $DatabasePort -h 127.0.0.1" -w
    if ($LASTEXITCODE -ne 0) { throw 'No se pudo arrancar PostgreSQL 16 de pruebas' }
    $startedDatabase = $true
    $dbArgs = @('-h','127.0.0.1','-p',"$DatabasePort",'-U',$role,'-w','-v','ON_ERROR_STOP=1')
    $serverVersion = & $psql @dbArgs -d postgres -tA -c 'SHOW server_version_num'
    if ($LASTEXITCODE -ne 0 -or [int]$serverVersion -lt 160000 -or [int]$serverVersion -ge 170000) { throw 'La instancia no es PostgreSQL 16' }
    & $psql @dbArgs -d postgres -c "CREATE DATABASE $database"
    if ($LASTEXITCODE -ne 0) { throw 'No se pudo crear la base de la plataforma de pruebas' }
    & $psql @dbArgs -d $database -q -c 'CREATE TABLE schema_migrations (nombre TEXT PRIMARY KEY, aplicada_en TIMESTAMPTZ NOT NULL DEFAULT now())'
    if ($LASTEXITCODE -ne 0) { throw 'No se pudo crear el registro de migraciones' }
    foreach ($migration in (Get-ChildItem (Join-Path $repoRoot 'database/migrations') -Filter '*.sql' | Where-Object Name -Match '^\d' | Sort-Object Name)) {
        & $psql @dbArgs -d $database --single-transaction -q -f $migration.FullName -c "INSERT INTO schema_migrations(nombre) VALUES ('$($migration.Name)')"
        if ($LASTEXITCODE -ne 0) { throw "Fallo la migracion $($migration.Name)" }
    }
    $apiOutput = Join-Path $runtime 'api'
    # Compilar el codigo actual en Debug no bloquea los binarios Release de la plataforma habitual.
    & dotnet publish (Join-Path $repoRoot 'MIRA.Api/MIRA.Api.csproj') -c Debug --no-restore -o $apiOutput --verbosity minimal
    if ($LASTEXITCODE -ne 0) { throw 'Fallo la compilacion de la API de pruebas; restaure las dependencias antes de verificar' }
    $apiExecutable = Join-Path $apiOutput 'MIRA.Api.exe'
    $connection = [System.Data.Common.DbConnectionStringBuilder]::new()
    $connection['Host'] = '127.0.0.1'; $connection['Port'] = $DatabasePort
    $connection['Database'] = $database; $connection['Username'] = $role; $connection['Password'] = $password
    $env:ConnectionStrings__PostgreSql = $connection.ConnectionString
    $env:ASPNETCORE_URLS = "http://127.0.0.1:$ApiPort"
    $env:ASPNETCORE_ENVIRONMENT = 'Development'
    $apiProcess = Start-Process -FilePath $apiExecutable -WorkingDirectory $repoRoot -WindowStyle Hidden -PassThru -RedirectStandardOutput (Join-Path $runtime 'api.log') -RedirectStandardError (Join-Path $runtime 'api-error.log')
    $python = (Get-Command python).Source
    $env:API_URL = "http://127.0.0.1:$ApiPort/api"
    $env:SWAGGER_URL = "http://127.0.0.1:$ApiPort/swagger"
    $env:PORT = "$FrontendPort"
    $env:SECRET_KEY = $password
    $env:SESSION_COOKIE_SECURE = 'false'
    $webProcess = Start-Process -FilePath $python -ArgumentList '-c', "`"from app import app; app.run(host='127.0.0.1', port=$FrontendPort, debug=False)`"" -WorkingDirectory (Join-Path $repoRoot 'frontend') -WindowStyle Hidden -PassThru -RedirectStandardOutput (Join-Path $runtime 'frontend.log') -RedirectStandardError (Join-Path $runtime 'frontend-error.log')
    foreach ($url in @("http://127.0.0.1:$ApiPort/api/universidad", "http://127.0.0.1:$FrontendPort/")) {
        $ready = $false
        for ($attempt = 0; $attempt -lt 30; $attempt++) {
            try { Invoke-WebRequest -Uri $url -UseBasicParsing -TimeoutSec 2 | Out-Null; $ready = $true; break } catch { Start-Sleep -Seconds 1 }
        }
        if (-not $ready) { throw "Servicio de pruebas no disponible: $url" }
    }
    Write-Output "Plataforma de pruebas: $version; base $database; puertos $DatabasePort/$ApiPort/$FrontendPort"
    & (Join-Path $PSScriptRoot 'verify-local.ps1') -ConfigFile $configFile -PostgresBin $PostgresBin
    Write-Output "PostgreSQL 16: verificacion finalizada. Datos conservados en $runtime"
} finally {
    foreach ($process in @($webProcess, $apiProcess)) {
        if ($process -and -not $process.HasExited) { $process.Kill(); $process.WaitForExit() }
    }
    if ($startedDatabase) {
        & $pgCtl stop -D $data -m fast -w
        if ($LASTEXITCODE -ne 0) { Write-Warning 'No se pudo detener la instancia PostgreSQL 16; revisar su log' }
    }
    foreach ($key in $previous.Keys) { [Environment]::SetEnvironmentVariable($key, $previous[$key], 'Process') }
}
