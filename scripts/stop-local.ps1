param([string]$PostgresBin = 'C:\Program Files\PostgreSQL\17\bin')
$ErrorActionPreference = 'Stop'
$repoRoot = Split-Path $PSScriptRoot -Parent
$runtime = Join-Path $repoRoot '.local'
$apiExe = Join-Path $repoRoot 'MIRA.Api/bin/Release/net10.0/MIRA.Api.exe'
$pythonExe = (Get-Command python).Source
foreach ($service in @(
    @{ Name = 'frontend'; Executable = $pythonExe },
    @{ Name = 'api'; Executable = $apiExe }
)) {
    $pidFile = Join-Path $runtime "$($service.Name).pid"
    if (-not (Test-Path $pidFile)) { continue }
    $serviceProcess = Get-Process -Id ([int](Get-Content $pidFile)) -ErrorAction SilentlyContinue
    if ($serviceProcess) {
        if ($serviceProcess.Path -ne $service.Executable) { throw "El PID de $($service.Name) pertenece a otro ejecutable; no se detuvo" }
        Stop-Process -Id $serviceProcess.Id
    }
    Remove-Item -LiteralPath $pidFile
}
$data = Join-Path $runtime 'postgres'
if (Test-Path (Join-Path $data 'PG_VERSION')) {
    & (Join-Path $PostgresBin 'pg_ctl.exe') status -D $data *> $null
    if ($LASTEXITCODE -eq 0) {
        & (Join-Path $PostgresBin 'pg_ctl.exe') stop -D $data -m fast -w
        if ($LASTEXITCODE -ne 0) { throw 'No se pudo detener PostgreSQL' }
    }
}
Write-Output 'MIRA detenida; datos conservados en .local/postgres.'
