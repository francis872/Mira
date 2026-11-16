param([int]$DatabasePort=15547,[int]$ApiPort=18084,[int]$FrontendPort=15004)
$ErrorActionPreference='Stop'
$repoRoot=Split-Path $PSScriptRoot -Parent
Set-Location $repoRoot
foreach($port in @($DatabasePort,$ApiPort,$FrontendPort)) {
    $listener=[Net.Sockets.TcpListener]::new([Net.IPAddress]::Loopback,$port)
    try {$listener.Start()} catch {throw "Puerto de pruebas $port ocupado"} finally {$listener.Stop()}
}
$project='mira-v3-check-'+(Get-Date -Format 'yyyyMMddHHmmss')
$runtime=Join-Path $repoRoot ('.local/'+$project)
New-Item -ItemType Directory -Force $runtime | Out-Null
$envFile=Join-Path $runtime 'runtime.env'
$override=Join-Path $runtime 'compose.yml'
$password=[Guid]::NewGuid().ToString('N')+[Guid]::NewGuid().ToString('N')
$jwtKey=[Guid]::NewGuid().ToString('N')+[Guid]::NewGuid().ToString('N')
[IO.File]::WriteAllLines($envFile,@(
    'POSTGRES_DB=mira_docker_test','POSTGRES_USER=mira_docker_test',"POSTGRES_PASSWORD=$password", "JWT_KEY=$jwtKey", "SECRET_KEY=$password",
    ('LOGIN_FORWARDING_KEY='+[Guid]::NewGuid().ToString('N')+[Guid]::NewGuid().ToString('N')),
    "POSTGRES_PORT=$DatabasePort","API_PORT=$ApiPort","FRONTEND_PORT=$FrontendPort",'ASPNETCORE_ENVIRONMENT=Development','MIRA_ENV=development','SESSION_COOKIE_SECURE=false'
),[Text.UTF8Encoding]::new($false))
[IO.File]::WriteAllText($override,"services:`n  mira-api:`n    image: mira-v3-api:stabilization`n  mira-frontend:`n    image: mira-v3-frontend:stabilization`n",[Text.UTF8Encoding]::new($false))
$compose=@('compose','--project-name',$project,'--env-file',$envFile,'-f',(Join-Path $repoRoot 'docker-compose.yml'),'-f',$override)
$keys=@('MIRA_ADMIN_EMAIL','MIRA_ADMIN_PASSWORD','MIRA_E2E_EMAIL','MIRA_E2E_PASSWORD','MIRA_E2E_URL','MIRA_E2E_API_URL')
$previous=@{};foreach($key in $keys){$previous[$key]=[Environment]::GetEnvironmentVariable($key,'Process')}
try {
    & docker @compose config --quiet
    if($LASTEXITCODE -ne 0){throw 'Compose invalido'}
    $ErrorActionPreference='Continue'
    & docker @compose up --detach --no-build
    $ErrorActionPreference='Stop'
    if($LASTEXITCODE -ne 0){throw 'Fallo el arranque del proyecto Docker aislado'}
    foreach($url in @("http://127.0.0.1:$ApiPort/ready","http://127.0.0.1:$FrontendPort/ready")) {
        $ready=$false
        for($i=0;$i -lt 60;$i++) {
            try {Invoke-WebRequest $url -UseBasicParsing -TimeoutSec 2|Out-Null;$ready=$true;break} catch {Start-Sleep -Seconds 1}
        }
        if(-not $ready){throw 'Un contenedor de pruebas no respondio; revisar logs locales'}
    }
    & docker @compose exec -T mira-postgres psql -U mira_docker_test -d mira_docker_test -v ON_ERROR_STOP=1 -c "INSERT INTO usuarios(nombre,correo,password_hash) VALUES ('Persona prueba Docker','docker-person@invalid.test','NO_ES_UNA_CREDENCIAL')"
    if($LASTEXITCODE -ne 0){throw 'Fallo la persona academica de prueba'}
    $env:MIRA_ADMIN_EMAIL='docker-validation@invalid.test';$env:MIRA_ADMIN_PASSWORD=$password
    # PowerShell 5 convierte stderr nativo en errores; Npgsql puede emitir avisos
    # recuperables de GSSAPI en Linux. La autoridad es el código de salida.
    $ErrorActionPreference='Continue'
    & docker @compose exec -T -e MIRA_ADMIN_EMAIL -e MIRA_ADMIN_PASSWORD mira-api dotnet MIRA.Api.dll --bootstrap-admin
    $ErrorActionPreference='Stop'
    if($LASTEXITCODE -ne 0){throw 'Fallo el administrador de prueba Docker'}
    $env:MIRA_E2E_EMAIL=$env:MIRA_ADMIN_EMAIL;$env:MIRA_E2E_PASSWORD=$password
    $env:MIRA_E2E_URL="http://127.0.0.1:$FrontendPort";$env:MIRA_E2E_API_URL="http://127.0.0.1:$ApiPort"
    & python -m pytest frontend/tests/e2e -q -p no:cacheprovider
    if($LASTEXITCODE -ne 0){throw 'Fallaron los E2E sobre Gunicorn/Docker'}
    & python (Join-Path $PSScriptRoot 'verify-docker-session.py') --project $project --env-file $envFile --compose-file $override --output docs/evidencias/v3/docker-session-results.json
    if($LASTEXITCODE -ne 0){throw 'Fallo persistencia/revocacion en Docker'}
    Write-Output "Docker validado en proyecto aislado $project; runtime privado: $runtime"
} finally {
    $ErrorActionPreference='Continue'
    & docker @compose logs --no-color *> (Join-Path $runtime 'containers.log')
    & docker @compose down
    foreach($key in $keys){[Environment]::SetEnvironmentVariable($key,$previous[$key],'Process')}
    # Sin -v: conservar solo los datos aislados de pruebas; no tocar volumen habitual mira-v2.
}
