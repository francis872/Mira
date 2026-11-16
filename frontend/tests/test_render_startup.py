"""Cold API startup must not replay credentials or weaken the managed origin."""
from types import SimpleNamespace
import requests
import pytest
from flask import session
import app as frontend
from test_app import FakeResponse, client, calls


@pytest.fixture
def render_profile(monkeypatch):
    monkeypatch.setitem(frontend.app.config, 'LOGIN_PLATFORM', 'render')
    monkeypatch.setitem(frontend.app.config, 'LOGIN_FORWARDING_KEY', b'isolated-forwarding-key-32-bytes-never-deploy')
    monkeypatch.setitem(frontend.app.config, 'LOGIN_TRUSTED_PROXY_NETWORKS', ())
    elapsed = [0.0]
    clock = SimpleNamespace(monotonic=lambda: elapsed[0], sleep=lambda seconds: elapsed.__setitem__(0, elapsed[0] + seconds))
    monkeypatch.setattr(frontend, 'time', clock)
    return elapsed


def request_context():
    return frontend.app.test_request_context('/login', headers={'CF-Connecting-IP': '198.51.100.41'})


def test_gateway_startup_get_is_retried_without_credentials_and_login_is_signed_once(monkeypatch, render_profile):
    observed = []
    def upstream(method, url, **kwargs):
        observed.append((method, url, kwargs))
        if method == 'GET':
            return FakeResponse(503) if len(observed) == 1 else FakeResponse(200, {'status': 'ok'})
        return FakeResponse(401, {'mensaje': 'Credenciales invalidas.'})
    monkeypatch.setattr(frontend.requests, 'request', upstream)
    with request_context():
        session['token'] = 'isolated-old-server-token'
        result = frontend.api_call('POST', 'auth/login', {'correo': 'qa@invalid.test', 'password': 'isolated-test-password'})
    assert result.status_code == 401
    assert [x[0] for x in observed] == ['GET', 'GET', 'POST']
    for method, url, kwargs in observed[:2]:
        assert url.endswith('/health') and '/api/../' not in url
        assert kwargs['headers'] == {'Accept': '*/*'}
        assert kwargs.get('json') is None and kwargs.get('auth') is None
        assert kwargs['allow_redirects'] is False and kwargs.get('verify', True) is True
    method, url, kwargs = observed[-1]
    prepared = requests.Request(method, url, json=kwargs['json'], headers=kwargs['headers'], auth=kwargs['auth']).prepare()
    assert prepared.headers['X-Mira-Login-Origin'].split(';')[2] == '198.51.100.41'
    assert kwargs['allow_redirects'] is False


@pytest.mark.parametrize('status', [301, 307, 401, 403, 404])
def test_non_startup_errors_never_forward_credentials_or_follow_redirects(monkeypatch, render_profile, status):
    observed = []
    def upstream(method, url, **kwargs):
        observed.append((method, kwargs))
        return FakeResponse(status)
    monkeypatch.setattr(frontend.requests, 'request', upstream)
    with request_context():
        assert frontend.api_call('POST', 'auth/login', {'password': 'must-not-be-sent'}) is None
    assert len(observed) == 1 and observed[0][0] == 'GET'
    assert observed[0][1].get('json') is None and observed[0][1]['allow_redirects'] is False


def test_unready_platform_is_bounded_and_does_not_attempt_login(monkeypatch, render_profile):
    methods = []
    def upstream(method, url, **kwargs):
        methods.append(method)
        return FakeResponse(503)
    monkeypatch.setattr(frontend.requests, 'request', upstream)
    with request_context():
        assert frontend.api_call('POST', 'auth/login', {'password': 'must-not-be-sent'}) is None
    assert 1 < len(methods) <= 20 and set(methods) == {'GET'}
    assert render_profile[0] <= 37


def test_loading_html_does_not_count_as_a_ready_api(monkeypatch, render_profile):
    methods = []
    def upstream(method, url, **kwargs):
        methods.append(method)
        return FakeResponse(200)  # json() raises for a provider loading page.
    monkeypatch.setattr(frontend.requests, 'request', upstream)
    with request_context():
        assert frontend.api_call('POST', 'auth/login', {'password': 'must-not-be-sent'}) is None
    assert 1 < len(methods) <= 20 and set(methods) == {'GET'}


@pytest.mark.parametrize('error_type', [requests.ReadTimeout, requests.ConnectionError])
def test_credential_post_is_never_replayed_and_diagnostics_do_not_log_its_values(monkeypatch, render_profile, caplog, error_type):
    methods = []
    password = 'isolated-sensitive-value-never-log'
    def upstream(method, url, **kwargs):
        methods.append(method)
        if method == 'GET':
            return FakeResponse(200, {'status': 'ok'})
        raise error_type(password)
    monkeypatch.setattr(frontend.requests, 'request', upstream)
    with request_context():
        assert frontend.api_call('POST', 'auth/login', {'correo': 'private@invalid.test', 'password': password}) is None
    assert methods == ['GET', 'POST']
    assert password not in caplog.text and 'private@invalid.test' not in caplog.text
    assert error_type.__name__ in caplog.text


def test_slow_api_startup_reserves_a_longer_login_wait_without_changing_other_calls(monkeypatch, render_profile):
    waits = []
    def upstream(method, url, **kwargs):
        waits.append(kwargs['timeout'])
        if method == 'GET' and url.endswith('/health'):
            render_profile[0] += 16
            return FakeResponse(200, {'status': 'ok'})
        if method == 'GET':
            return FakeResponse(200, [])
        return FakeResponse(401, {'mensaje': 'Credenciales invalidas.'})
    monkeypatch.setattr(frontend.requests, 'request', upstream)
    with request_context():
        assert frontend.api_call('POST', 'auth/login', {'password': 'isolated-test-password'}).status_code == 401
    assert waits[-1][1] > 8
    assert sum(waits[-1]) <= 45 - render_profile[0]
    with frontend.app.test_request_context('/'):
        response = frontend.api_call('GET', 'universidad')
    assert waits[-1] == 8


def test_deadline_exhaustion_does_not_send_credentials(monkeypatch, render_profile):
    methods = []
    def upstream(method, url, **kwargs):
        methods.append(method)
        render_profile[0] += 44
        return FakeResponse(200, {'status': 'ok'})
    monkeypatch.setattr(frontend.requests, 'request', upstream)
    with request_context():
        assert frontend.api_call('POST', 'auth/login', {'password': 'must-not-be-sent'}) is None
    assert methods == ['GET']


def test_csrf_rejection_happens_before_any_startup_probe(client, calls, render_profile):
    assert client.post('/login', data={'password': 'must-not-be-sent'}, no_csrf=True).status_code == 400
    assert not calls


def test_readiness_can_wake_the_api_but_liveness_does_not(client, monkeypatch, render_profile):
    observed = []
    def upstream(method, url, **kwargs):
        observed.append((method, url))
        return FakeResponse(200, {'status': 'ok' if url.endswith('/health') else 'ready'})
    monkeypatch.setattr(frontend.requests, 'request', upstream)
    assert client.get('/health').status_code == 200
    assert observed == []
    assert client.get('/ready').status_code == 200
    assert len(observed) == 2 and all(method == 'GET' for method, url in observed)
    assert observed[0][1].endswith('/health') and observed[1][1].endswith('/ready')


def test_readiness_stays_unavailable_when_api_cannot_wake(client, monkeypatch, render_profile):
    monkeypatch.setattr(frontend.requests, 'request', lambda *args, **kwargs: FakeResponse(503))
    response = client.get('/ready')
    assert response.status_code == 503 and response.json == {'status': 'unavailable'}
