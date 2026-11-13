"""Arranque local en modo Production con destinos ficticios; NO prueba TLS cloud."""
from datetime import datetime, timedelta, timezone
import json
import argparse
import secrets
import subprocess
import time
from pathlib import Path
from urllib.error import HTTPError, URLError
from urllib.request import urlopen


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument('--output', default='docs/evidencias/v3/production-smoke.json')
    parser.add_argument('--api-image', default='mira-v3-api:stabilization')
    parser.add_argument('--frontend-image', default='mira-v3-frontend:stabilization')
    args = parser.parse_args()
    suffix = secrets.token_hex(6)
    runtime = Path('.local') / ('production-smoke-' + suffix)
    runtime.mkdir(parents=True)
    api_env = runtime / 'api.env'
    forwarding_key = secrets.token_urlsafe(48)
    api_env.write_text('\n'.join([
        'ASPNETCORE_ENVIRONMENT=Production', 'PORT=10000',
        'Jwt__Key=' + secrets.token_urlsafe(48),
        'LoginProtection__ForwardingKey=' + forwarding_key,
        'ConnectionStrings__PostgreSql=Host=example.invalid;Database=unused;Username=unused;Password=unused;SSL Mode=VerifyFull;Channel Binding=Require;GSS Encryption Mode=Disable',
    ]) + '\n', encoding='utf-8')
    web_env = runtime / 'frontend.env'
    web_env.write_text('\n'.join([
        'PORT=10000', 'MIRA_ENV=production', 'SESSION_COOKIE_SECURE=true',
        'SESSION_DIRECTORY=/var/lib/mira/sessions', 'WEB_CONCURRENCY=1',
        'API_URL=https://api.example.invalid/api', 'SECRET_KEY=' + secrets.token_urlsafe(48),
        'MIRA_LOGIN_FORWARDING_KEY=' + forwarding_key,
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
            ('api', args.api_image, api_env, 18087),
            ('frontend', args.frontend_image, web_env, 15007),
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
        report = {'date': datetime.now(timezone(timedelta(hours=-5))).date().isoformat(), 'status': 'passed', 'checks': checks,
            'scope': 'Docker local Production-mode startup over loopback HTTP; no Render, Neon, real TLS, login or persistence validation'}
        Path(args.output).write_text(json.dumps(report, indent=2) + '\n', encoding='utf-8')
        print(json.dumps(report))
    finally:
        for name in containers:
            subprocess.run(['docker', 'rm', '--force', name], capture_output=True)


if __name__ == '__main__':
    main()
