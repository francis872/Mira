param(
    [string]$ConfigFile = '.env.local',
    [string]$PostgresBin = 'C:\Program Files\PostgreSQL\17\bin'
)

# Ejecutar con la plataforma local iniciada. Nunca prueba contra la base de la aplicacion.
$ErrorActionPreference = 'Stop'
$repoRoot = Split-Path $PSScriptRoot -Parent
Set-Location $repoRoot
$config = @{}
Get-Content -LiteralPath $ConfigFile | ForEach-Object {
    if ($_ -match '^([^#=]+)=(.*)$') { $config[$matches[1].Trim()] = $matches[2].Trim() }
}
foreach ($key in @('POSTGRES_USER', 'POSTGRES_PASSWORD', 'POSTGRES_PORT')) {
    if (-not $config[$key]) { throw "Falta $key" }
}
$psql = Join-Path $PostgresBin 'psql.exe'
$testDatabase = 'mira_audit_' + (Get-Date -Format 'yyyyMMddHHmmss') + '_' + $PID
$previous = @{}
foreach ($key in @('PGPASSWORD','PGCLIENTENCODING','PGCONNECT_TIMEOUT','MIRA_TEST_DB','MIRA_E2E_URL','MIRA_E2E_API_URL')) {
    $previous[$key] = [Environment]::GetEnvironmentVariable($key, 'Process')
}
try {
    $env:PGPASSWORD = $config['POSTGRES_PASSWORD']
    $env:PGCLIENTENCODING = 'UTF8'
    $env:PGCONNECT_TIMEOUT = '5'
    $dbArgs = @('-h','127.0.0.1','-p',$config['POSTGRES_PORT'],'-U',$config['POSTGRES_USER'],'-w','-v','ON_ERROR_STOP=1')
    & $psql @dbArgs -d postgres -c "CREATE DATABASE $testDatabase"
    if ($LASTEXITCODE -ne 0) { throw 'No se pudo crear la base exclusiva de pruebas' }
    foreach ($migration in (Get-ChildItem (Join-Path $repoRoot 'database/migrations') -Filter '*.sql' | Where-Object Name -Match '^\d' | Sort-Object Name)) {
        & $psql @dbArgs -d $testDatabase --single-transaction -q -f $migration.FullName
        if ($LASTEXITCODE -ne 0) { throw "Fallo la migracion $($migration.Name)" }
    }
    & $psql @dbArgs -d $testDatabase -f (Join-Path $repoRoot 'database/tests/v2_usuarios_roles.sql')
    if ($LASTEXITCODE -ne 0) { throw 'Fallaron las pruebas SQL' }
    $connection = [System.Data.Common.DbConnectionStringBuilder]::new()
    $connection['Host'] = '127.0.0.1'
    $connection['Port'] = [int]$config['POSTGRES_PORT']
    $connection['Database'] = $testDatabase
    $connection['Username'] = $config['POSTGRES_USER']
    $connection['Password'] = $config['POSTGRES_PASSWORD']
    $env:MIRA_TEST_DB = $connection.ConnectionString
    & dotnet test MIRA.sln --no-restore --verbosity minimal
    if ($LASTEXITCODE -ne 0) { throw 'Fallaron las pruebas .NET' }
    Push-Location (Join-Path $repoRoot 'frontend')
    try {
        & python -m pytest tests/test_app.py -q
        if ($LASTEXITCODE -ne 0) { throw 'Fallaron las pruebas Flask' }
    } finally { Pop-Location }
    $webPort = if ($config['FRONTEND_PORT']) { $config['FRONTEND_PORT'] } else { '5000' }
    $apiPort = if ($config['API_PORT']) { $config['API_PORT'] } else { '8081' }
    $env:MIRA_E2E_URL = "http://127.0.0.1:$webPort"
    $env:MIRA_E2E_API_URL = "http://127.0.0.1:$apiPort"
    & python -m pytest frontend/tests/e2e -q
    if ($LASTEXITCODE -ne 0) { throw 'Fallaron las pruebas E2E' }
    Write-Output "Verificacion completa. Base de pruebas conservada: $testDatabase"
} finally {
    foreach ($key in $previous.Keys) {
        [Environment]::SetEnvironmentVariable($key, $previous[$key], 'Process')
    }
}
