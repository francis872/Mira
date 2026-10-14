param(
    [string]$ConfigFile = '.env.local',
    [string]$PostgresBin = 'C:\Program Files\PostgreSQL\17\bin'
)

# Alternativa local a Docker. Los datos y logs permanecen en .local (ignorado).
$ErrorActionPreference = 'Stop'
$repoRoot = Split-Path $PSScriptRoot -Parent
Set-Location $repoRoot
$config = @{}
Get-Content -LiteralPath $ConfigFile | ForEach-Object {
    if ($_ -match '^([^#=]+)=(.*)$') { $config[$matches[1].Trim()] = $matches[2].Trim() }
}
foreach ($key in @('POSTGRES_DB', 'POSTGRES_USER', 'POSTGRES_PASSWORD', 'POSTGRES_PORT')) {
    if (-not $config[$key]) { throw "Falta $key en $ConfigFile" }
}
# Identificadores simples para las operaciones de creación de base de datos.
foreach ($key in @('POSTGRES_DB', 'POSTGRES_USER')) {
    if ($config[$key] -notmatch '^[a-zA-Z_][a-zA-Z0-9_]*$') { throw "$key no es un identificador válido" }
}
$pgPort = [int]$config['POSTGRES_PORT']
$apiPort = if ($config['API_PORT']) { [int]$config['API_PORT'] } else { 8081 }
$webPort = if ($config['FRONTEND_PORT']) { [int]$config['FRONTEND_PORT'] } else { 5000 }
$runtime = Join-Path $repoRoot '.local'
$data = Join-Path $runtime 'postgres'
New-Item -ItemType Directory -Force -Path $runtime | Out-Null
$psql = Join-Path $PostgresBin 'psql.exe'
$pgCtl = Join-Path $PostgresBin 'pg_ctl.exe'
if (-not (Test-Path $psql)) { throw "PostgreSQL no encontrado en $PostgresBin" }
$env:PGPASSWORD = $config['POSTGRES_PASSWORD']
$env:PGCLIENTENCODING = 'UTF8'
$env:PGCONNECT_TIMEOUT = '5'
$dbArgs = @('-h', '127.0.0.1', '-p', "$pgPort", '-U', $config['POSTGRES_USER'], '-w', '-v', 'ON_ERROR_STOP=1')

function Invoke-Sql([string]$Database, [string]$Sql) {
    $result = & $psql @dbArgs -d $Database -tA -c $Sql
    if ($LASTEXITCODE -ne 0) { throw 'Falló una operación PostgreSQL; revise la salida anterior' }
    return $result
}

if (-not (Test-Path (Join-Path $data 'PG_VERSION'))) {
    $pwFile = Join-Path $runtime 'pg-password'
    [IO.File]::WriteAllText($pwFile, $config['POSTGRES_PASSWORD'], [Text.UTF8Encoding]::new($false))
    try {
        & (Join-Path $PostgresBin 'initdb.exe') -D $data -U $config['POSTGRES_USER'] --encoding=UTF8 --locale=C --auth=scram-sha-256 "--pwfile=$pwFile"
        if ($LASTEXITCODE -ne 0) { throw 'No se pudo inicializar PostgreSQL' }
    } finally { Remove-Item -LiteralPath $pwFile -ErrorAction SilentlyContinue }
}
& $pgCtl status -D $data *> $null
if ($LASTEXITCODE -ne 0) {
    & $pgCtl start -D $data -l (Join-Path $runtime 'postgres.log') -o "-p $pgPort -h 127.0.0.1" -w
    if ($LASTEXITCODE -ne 0) { throw 'No se pudo iniciar PostgreSQL; revise .local/postgres.log' }
}
$exists = Invoke-Sql 'postgres' "SELECT 1 FROM pg_database WHERE datname = '$($config['POSTGRES_DB'])'"
if ($exists -ne '1') {
    Invoke-Sql 'postgres' "CREATE DATABASE $($config['POSTGRES_DB'])" | Out-Null
}
Invoke-Sql $config['POSTGRES_DB'] 'CREATE TABLE IF NOT EXISTS schema_migrations (nombre TEXT PRIMARY KEY, aplicada_en TIMESTAMPTZ NOT NULL DEFAULT now())' | Out-Null
foreach ($migration in (Get-ChildItem (Join-Path $repoRoot 'database/migrations') -Filter '*.sql' | Where-Object Name -Match '^\d' | Sort-Object Name)) {
    $name = $migration.Name
    $applied = Invoke-Sql $config['POSTGRES_DB'] "SELECT 1 FROM schema_migrations WHERE nombre = '$name'"
    if ($applied -eq '1') { continue }
    & $psql @dbArgs -d $config['POSTGRES_DB'] --single-transaction -f $migration.FullName -c "INSERT INTO schema_migrations(nombre) VALUES ('$name')"
    if ($LASTEXITCODE -ne 0) { throw "Falló la migración $name" }
}

# No reemplaza procesos ajenos. Los servicios propios se reutilizan por su PID y ruta.
function Test-OwnProcess([string]$PidFile, [string]$Executable) {
    if (-not (Test-Path $PidFile)) { return $false }
    $serviceProcess = Get-Process -Id ([int](Get-Content $PidFile)) -ErrorAction SilentlyContinue
    return ($serviceProcess -and $serviceProcess.Path -eq $Executable)
}
function Assert-FreePort([int]$Port) {
    $listener = [Net.Sockets.TcpListener]::new([Net.IPAddress]::Loopback, $Port)
    try { $listener.Start() } catch { throw "Puerto $Port ocupado; detenga el servicio anterior de MIRA antes de iniciar" }
    finally { $listener.Stop() }
}
function Wait-Http([string]$Url) {
    for ($attempt = 0; $attempt -lt 30; $attempt++) {
        try { Invoke-WebRequest -Uri $Url -UseBasicParsing -TimeoutSec 2 | Out-Null; return } catch { Start-Sleep -Seconds 1 }
    }
    throw "El servicio no respondió: $Url. Revise .local/*.log"
}
$apiExe = Join-Path $repoRoot 'MIRA.Api/bin/Release/net10.0/MIRA.Api.exe'
$apiPidFile = Join-Path $runtime 'api.pid'
if (-not (Test-OwnProcess $apiPidFile $apiExe)) {
    Assert-FreePort $apiPort
    & dotnet build (Join-Path $repoRoot 'MIRA.Api/MIRA.Api.csproj') -c Release --no-restore --verbosity minimal
    if ($LASTEXITCODE -ne 0) { throw 'Falló el build de la API; ejecute dotnet restore MIRA.sln' }
    # DbConnectionStringBuilder escapa contraseñas con caracteres reservados.
    $connection = [System.Data.Common.DbConnectionStringBuilder]::new()
    $connection['Host'] = '127.0.0.1'; $connection['Port'] = $pgPort
    $connection['Database'] = $config['POSTGRES_DB']; $connection['Username'] = $config['POSTGRES_USER']
    $connection['Password'] = $config['POSTGRES_PASSWORD']
    $env:ConnectionStrings__PostgreSql = $connection.ConnectionString
    $env:ASPNETCORE_URLS = "http://127.0.0.1:$apiPort"
    $env:ASPNETCORE_ENVIRONMENT = 'Development'
    $apiProcess = Start-Process -FilePath $apiExe -WorkingDirectory $repoRoot -WindowStyle Hidden -PassThru -RedirectStandardOutput (Join-Path $runtime 'api.log') -RedirectStandardError (Join-Path $runtime 'api-error.log')
    $apiProcess.Id | Set-Content $apiPidFile
    Remove-Item Env:ConnectionStrings__PostgreSql
}
Wait-Http "http://127.0.0.1:$apiPort/api/universidad"
$python = (Get-Command python).Source
$webPidFile = Join-Path $runtime 'frontend.pid'
if (-not (Test-OwnProcess $webPidFile $python)) {
    Assert-FreePort $webPort
    $env:API_URL = "http://127.0.0.1:$apiPort/api"
    $env:SWAGGER_URL = "http://localhost:$apiPort/swagger"
    $env:PORT = "$webPort"
    $env:SECRET_KEY = $config['SECRET_KEY']
    $env:SESSION_COOKIE_SECURE = 'false'
    $webProcess = Start-Process -FilePath $python -ArgumentList '-c', "`"from app import app; app.run(host='127.0.0.1', port=$webPort, debug=False)`"" -WorkingDirectory (Join-Path $repoRoot 'frontend') -WindowStyle Hidden -PassThru -RedirectStandardOutput (Join-Path $runtime 'frontend.log') -RedirectStandardError (Join-Path $runtime 'frontend-error.log')
    $webProcess.Id | Set-Content $webPidFile
}
Wait-Http "http://127.0.0.1:$webPort/"
Remove-Item Env:PGPASSWORD -ErrorAction SilentlyContinue
Write-Output "MIRA lista: http://localhost:$webPort | API: http://localhost:$apiPort/swagger"
