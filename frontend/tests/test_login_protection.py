import hashlib
import hmac
import time

import pytest
import requests
from flask import Flask
from login_protection import configure_login_protection, client_origin, LoginOriginAuth


KEY = 'isolated-forwarding-test-fixture-32-bytes-never-deploy'

@pytest.mark.parametrize('header,expected',[('198.51.100.41','198.51.100.41'),('not-an-ip',None),('198.51.100.41, 203.0.113.2',None)])
def test_vercel_uses_only_its_platform_header(monkeypatch,header,expected):
    monkeypatch.setenv('MIRA_PLATFORM','vercel');monkeypatch.setenv('VERCEL','1')
    app=configured(monkeypatch)
    with app.test_request_context('/',headers={'X-Vercel-Forwarded-For':header,'X-Forwarded-For':'192.0.2.9','X-Real-IP':'192.0.2.10'}):
        assert client_origin(())==expected


def test_vercel_proxy_profile_requires_server_environment(monkeypatch):
    monkeypatch.setenv('MIRA_PLATFORM','vercel');monkeypatch.delenv('VERCEL',raising=False)
    with pytest.raises(RuntimeError,match='VERCEL'):configured(monkeypatch)


def configured(monkeypatch, production=False):
    monkeypatch.setenv('MIRA_ENV', 'production' if production else 'development')
    monkeypatch.setenv('MIRA_LOGIN_FORWARDING_KEY', KEY)
    monkeypatch.delenv('MIRA_TRUSTED_PROXY_NETWORKS', raising=False)
    app = Flask('login-protection-test')
    configure_login_protection(app)
    return app


@pytest.mark.parametrize('production', [False, True])
def test_valid_forwarding_configuration(monkeypatch, production):
    assert configured(monkeypatch, production).config['LOGIN_FORWARDING_KEY'] == KEY.encode()


@pytest.mark.parametrize('key', ['', 'short', 'REPLACE' + 'x' * 40])
def test_production_rejects_missing_weak_or_placeholder_key(monkeypatch, key):
    monkeypatch.setenv('MIRA_ENV', 'production')
    monkeypatch.setenv('MIRA_LOGIN_FORWARDING_KEY', key)
    with pytest.raises(RuntimeError, match='MIRA_LOGIN_FORWARDING_KEY'):
        configure_login_protection(Flask('invalid-login-config'))


def test_development_without_key_does_not_invent_a_trusted_origin(monkeypatch):
    monkeypatch.setenv('MIRA_ENV', 'development')
    monkeypatch.delenv('MIRA_LOGIN_FORWARDING_KEY', raising=False)
    monkeypatch.delenv('MIRA_TRUSTED_PROXY_NETWORKS', raising=False)
    app = Flask('direct-api-development')
    configure_login_protection(app)
    assert app.config['LOGIN_FORWARDING_KEY'] is None


def test_session_key_cannot_be_reused_for_forwarding(monkeypatch):
    app = configured(monkeypatch)
    app.secret_key = KEY
    with pytest.raises(RuntimeError, match='diferente de SECRET_KEY'):
        configure_login_protection(app)


@pytest.mark.parametrize('headers', [{}, {'X-Forwarded-For': '192.0.2.99'}, {'X-Real-IP': '192.0.2.99'}])
def test_untrusted_peer_headers_cannot_change_origin(monkeypatch, headers):
    app = configured(monkeypatch)
    with app.test_request_context('/login', headers=headers, environ_base={'REMOTE_ADDR': '198.51.100.10'}):
        assert client_origin(app.config['LOGIN_TRUSTED_PROXY_NETWORKS']) == '198.51.100.10'


@pytest.mark.parametrize('forwarded,expected', [
    ('192.0.2.99', '192.0.2.99'),
    ('203.0.113.99, 198.51.100.10', '198.51.100.10'),
    ('192.0.2.99, 10.0.0.2', '192.0.2.99'),
    ('invalid-address', '10.0.0.1'),
    ('', '10.0.0.1'),
])
def test_only_explicit_trusted_proxy_chain_is_used(monkeypatch, forwarded, expected):
    app = configured(monkeypatch)
    monkeypatch.setenv('MIRA_TRUSTED_PROXY_NETWORKS', '10.0.0.0/24')
    configure_login_protection(app)
    with app.test_request_context('/login', headers={'X-Forwarded-For': forwarded}, environ_base={'REMOTE_ADDR':'10.0.0.1'}):
        assert client_origin(app.config['LOGIN_TRUSTED_PROXY_NETWORKS']) == expected


def test_trusting_all_proxies_is_rejected(monkeypatch):
    app = configured(monkeypatch)
    monkeypatch.setenv('MIRA_TRUSTED_PROXY_NETWORKS', '0.0.0.0/0')
    with pytest.raises(RuntimeError, match='todos los proxies'):
        configure_login_protection(app)


def test_signer_binds_actual_body_and_ignores_incoming_assertion(monkeypatch):
    configured(monkeypatch)
    auth = LoginOriginAuth(KEY.encode(), '192.0.2.10')
    prepared = requests.Request('POST', 'https://api.example.invalid/api/auth/login',
        json={'correo':'persona@invalid.test','password':'test-only-password'},
        headers={'X-Mira-Login-Origin':'forged'}, auth=auth).prepare()
    version, timestamp, origin, signature = prepared.headers['X-Mira-Login-Origin'].split(';')
    assert version == 'v1' and origin == '192.0.2.10' and abs(int(timestamp)-int(time.time())) <= 1
    payload = f'mira-login-v1\n{timestamp}\n{origin}\nPOST\n/api/auth/login\n{hashlib.sha256(prepared.body).hexdigest()}'
    assert hmac.compare_digest(signature, hmac.new(KEY.encode(),payload.encode(),hashlib.sha256).hexdigest())
    assert KEY not in str(prepared.headers) and 'test-only-password' not in str(prepared.headers)


def test_api_call_attaches_auth_only_to_login_and_signs_socket_origin(client, calls, monkeypatch):
    import app as frontend
    from test_app import FakeResponse
    observed = []
    redirect_options = []

    def capture(method, url, **kwargs):
        redirect_options.append(kwargs.get('allow_redirects'))
        observed.append(requests.Request(method, url, **{k:v for k,v in kwargs.items() if k not in ('timeout','allow_redirects')}).prepare())
        return FakeResponse(401, {'mensaje':'Credenciales inválidas.'})

    monkeypatch.setattr(frontend.requests, 'request', capture)
    monkeypatch.setitem(frontend.app.config, 'LOGIN_FORWARDING_KEY', KEY.encode())
    monkeypatch.setitem(frontend.app.config, 'LOGIN_TRUSTED_PROXY_NETWORKS', ())
    with frontend.app.test_request_context('/login',headers={'X-Forwarded-For':'203.0.113.99','X-Mira-Login-Origin':'forged'},
                                         environ_base={'REMOTE_ADDR':'198.51.100.10'}):
        frontend.api_call('POST','auth/login',{'correo':'test@invalid.test','password':'wrong'})
        assert observed[-1].headers['X-Mira-Login-Origin'].split(';')[2] == '198.51.100.10'
        assert redirect_options[-1] is False
        with pytest.raises(frontend.SesionVencida):
            frontend.api_call('GET','auth/me')
        assert 'X-Mira-Login-Origin' not in observed[-1].headers


# Reuse the established isolated Flask fixture, including its CSRF behavior.
from test_app import client, calls
