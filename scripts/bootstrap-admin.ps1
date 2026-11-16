param([string]$ConfigFile='.env.local', [switch]$ResetPassword)

# Aprovisionamiento local explícito. Nunca recibe contraseñas por argumentos de proceso.
$ErrorActionPreference='Stop'
$repoRoot=Split-Path $PSScriptRoot -Parent
Set-Location $repoRoot
$config=@{}
Get-Content -LiteralPath $ConfigFile | ForEach-Object {
    if ($_ -match '^([^#=]+)=(.*)$') { $config[$matches[1].Trim()]=$matches[2].Trim() }
}
$connection=[System.Data.Common.DbConnectionStringBuilder]::new()
$connection['Host']='127.0.0.1'; $connection['Port']=[int]$config['POSTGRES_PORT']
$connection['Database']=$config['POSTGRES_DB']; $connection['Username']=$config['POSTGRES_USER']
$connection['Password']=$config['POSTGRES_PASSWORD']
$keys=@('ConnectionStrings__PostgreSql','Jwt__Key','MIRA_ADMIN_EMAIL','MIRA_ADMIN_PASSWORD')
$previous=@{}
foreach ($key in $keys) { $previous[$key]=[Environment]::GetEnvironmentVariable($key,'Process') }
try {
    $env:ConnectionStrings__PostgreSql=$connection.ConnectionString
    $env:Jwt__Key=if($config['JWT_KEY']) { $config['JWT_KEY'] } else { [IO.File]::ReadAllText((Join-Path $repoRoot '.local/jwt-key')) }
    if (-not $env:MIRA_ADMIN_EMAIL) { $env:MIRA_ADMIN_EMAIL=Read-Host 'Correo del administrador' }
    if (-not $env:MIRA_ADMIN_PASSWORD) {
        $secure=Read-Host 'Contraseña inicial (12 caracteres mínimo, hasta 72 bytes UTF-8)' -AsSecureString
        $env:MIRA_ADMIN_PASSWORD=[Net.NetworkCredential]::new('', $secure).Password
    }
    $operation=if ($ResetPassword) { '--reset-admin-password' } else { '--bootstrap-admin' }
    & dotnet run --project MIRA.Api --no-launch-profile -- $operation
    if ($LASTEXITCODE -ne 0) { throw 'No fue posible aprovisionar. Puede existir ya un administrador o faltar la migración 008.' }
} finally {
    foreach ($key in $keys) { [Environment]::SetEnvironmentVariable($key,$previous[$key],'Process') }
}
