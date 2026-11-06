"""Arranque local en modo Production con destinos ficticios; NO prueba TLS cloud."""
import json
import secrets
import subprocess
import time
from pathlib import Path
from urllib.error import HTTPError, URLError
from urllib.request import urlopen


def main():
    suffix = secrets.token_hex(6)
    runtime = Path('.local') / ('production-smoke-' + suffix)
    runtime.mkdir(parents=True)
    api_env = runtime / 'api.env'
    api_env.write_text('\n'.join([
        'ASPNETCORE_ENVIRONMENT=Production', 'PORT=10000',
        'Jwt__Key=' + secrets.token_urlsafe(48),
        'ConnectionStrings__PostgreSql=Host=example.invalid;Database=unused;Username=unused;Password=unused;SSL Mode=VerifyFull;Channel Binding=Require;GSS Encryption Mode=Disable',
    ]) + '\n', encoding='utf-8')
    web_env = runtime / 'frontend.env'
    web_env.write_text('\n'.join([
        'PORT=10000', 'MIRA_ENV=production', 'SESSION_COOKIE_SECURE=true',
        'SESSION_DIRECTORY=/var/lib/mira/sessions', 'WEB_CONCURRENCY=1',
        'API_URL=https://api.example.invalid/api', 'SECRET_KEY=' + secrets.token_urlsafe(48),
    ]) + '\n', encoding='utf-8')
    containers = []
    checks = []

    def status(url):
        try:
            with urlopen(url, timeout=2) as response:
                return response.status, response.headers
        except HTTPError as error:
            return error.code, error.headers

    try:
        for label, image, env, port in [
            ('api', 'mira-v3-api:stabilization', api_env, 18087),
            ('frontend', 'mira-v3-frontend:stabilization', web_env, 15007),
        ]:
            name = 'mira-v3-smoke-' + label + '-' + suffix
            subprocess.run(['docker', 'run', '--detach', '--name', name, '--env-file', str(env),
                '-p', f'127.0.0.1:{port}:10000', image], check=True, capture_output=True)
            containers.append(name)
            base = f'http://127.0.0.1:{port}'
            for attempt in range(60):
                try:
                    if status(base + '/health')[0] == 200:
                        break
                except (URLError, OSError):
                    pass
                time.sleep(.5)
            else:
                raise AssertionError(label + ' no respondió en PORT=10000')
            if label == 'api':
                assert status(base + '/api/universidad')[0] == 401
                assert status(base + '/swagger/index.html')[0] == 404
                checks.append({'component': label, 'production': True, 'port': 10000,
                    'health': 200, 'anonymousCatalog': 401, 'swagger': 404,
                    'databaseContacted': False})
            else:
                code, headers = status(base + '/login')
                assert code == 200
                cookie = headers.get('Set-Cookie', '')
                assert all(flag in cookie for flag in ('Secure', 'HttpOnly', 'SameSite=Strict'))
                checks.append({'component': label, 'production': True, 'port': 10000,
                    'health': 200, 'login': 200, 'secureHttpOnlyStrictCookie': True,
                    'apiContacted': False})
        report = {'date': '2026-11-05', 'status': 'passed', 'checks': checks,
            'scope': 'Docker local Production-mode startup over loopback HTTP; no Render, Neon, real TLS, login or persistence validation'}
        Path('docs/evidencias/v3/production-smoke.json').write_text(json.dumps(report, indent=2) + '\n', encoding='utf-8')
        print(json.dumps(report))
    finally:
        for name in containers:
            subprocess.run(['docker', 'rm', '--force', name], capture_output=True)


if __name__ == '__main__':
    main()
