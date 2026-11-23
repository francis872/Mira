param([switch]$ResetPassword)

# Operación administrativa explícita; nunca recibe contraseñas por argumentos.
$ErrorActionPreference = 'Stop'
$repoRoot = Split-Path $PSScriptRoot -Parent
if (-not $env:ConnectionStrings__PostgreSql) { throw 'Configure ConnectionStrings__PostgreSql en el entorno privado del operador.' }
if (-not $env:Jwt__Key) { throw 'Configure Jwt__Key en el entorno privado del operador.' }
$keys = @('SAPIENS_INITIAL_EMAIL','SAPIENS_INITIAL_PASSWORD','SAPIENS_INITIAL_ROLE')
$previous = @{}
foreach ($key in $keys) { $previous[$key] = [Environment]::GetEnvironmentVariable($key,'Process') }
try {
    if (-not $env:SAPIENS_INITIAL_EMAIL) { $env:SAPIENS_INITIAL_EMAIL = 'sapiens@gmail.com' }
    if (-not $env:SAPIENS_INITIAL_ROLE) { $env:SAPIENS_INITIAL_ROLE = 'Investigador' }
    if (-not $env:SAPIENS_INITIAL_PASSWORD) {
        $secure = Read-Host 'Contraseña institucional privada' -AsSecureString
        $env:SAPIENS_INITIAL_PASSWORD = [Net.NetworkCredential]::new('', $secure).Password
    }
    $operation = if ($ResetPassword) { '--reset-institutional-password' } else { '--provision-institutional' }
    & dotnet run --project (Join-Path $repoRoot 'MIRA.Api') --no-launch-profile -- $operation
    if ($LASTEXITCODE -ne 0) { throw 'La operación institucional no se completó. Revise el mensaje de validación; no se realizó un restablecimiento implícito.' }
} finally {
    foreach ($key in $keys) { [Environment]::SetEnvironmentVariable($key,$previous[$key],'Process') }
}
