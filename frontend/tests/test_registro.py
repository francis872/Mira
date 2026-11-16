from datetime import datetime, timezone, timedelta

import pytest
from test_app import client, calls, FakeResponse


def test_registration_get_and_head_are_public_and_readonly(client, calls):
    with client.session_transaction() as session:
        session.clear()
    assert client.get('/registro').status_code == 200
    assert client.head('/registro').status_code == 200
    assert calls == []
    html = client.get('/registro').get_data(as_text=True)
    assert 'Crear mi cuenta' in html and 'confirm_password' in html
    assert 'sapiens-dark.css' in html


def test_registration_rotates_session_and_sends_no_roles(client, calls):
    client.get('/registro')
    before = client.get_cookie('session').value
    calls.responses[('POST', 'auth/registro')] = FakeResponse(201, {
        'token': 'isolated-registration-jwt',
        'expiraEn': (datetime.now(timezone.utc)+timedelta(minutes=20)).isoformat(),
        'usuario': {'id': 71, 'correo': 'person@invalid.test', 'roles': ['Investigador']},
    })
    response = client.post('/registro', data={'correo': 'person@invalid.test', 'password': 'Test-password-2026!', 'confirm_password': 'Test-password-2026!', 'roles': 'Administrador'})
    assert response.status_code == 302
    assert before != client.get_cookie('session').value
    assert set(calls[-1]['json']) == {'correo', 'password'}
    assert 'isolated-registration-jwt' not in str(response.headers)
    assert client.get('/usuarios').status_code == 403
    assert client.get('/universidad/nuevo').status_code == 403


@pytest.mark.parametrize('password,confirmation', [('short','short'), ('Test-password-2026!','different'), ('ñ'*37,'ñ'*37)])
def test_bad_registration_password_does_not_reach_api(client, calls, password, confirmation):
    assert client.post('/registro', data={'correo':'person@invalid.test', 'password':password, 'confirm_password':confirmation}).status_code == 400
    assert calls == []


def test_registration_preserves_csrf_and_duplicate_error(client, calls):
    assert client.post('/registro', data={}, no_csrf=True).status_code == 400
    calls.responses[('POST','auth/registro')] = FakeResponse(409, {})
    response = client.post('/registro', data={'correo':'person@invalid.test','password':'Test-password-2026!','confirm_password':'Test-password-2026!'})
    assert response.status_code == 409
    html = response.get_data(as_text=True)
    assert 'Ese correo ya tiene una cuenta' in html
    assert 'Test-password-2026!' not in html
