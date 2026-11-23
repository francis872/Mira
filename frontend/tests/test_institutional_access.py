import pytest
from test_app import client, calls


@pytest.mark.parametrize('path',['/registro','/register','/signup','/auth/registro','/api/auth/registro'])
def test_public_and_legacy_registration_are_gone(client,calls,path):
    with client.session_transaction() as session:
        session.clear()
    assert client.get(path).status_code == 404
    assert client.post(path,data={'correo':'test@invalid.test','password':'Isolated-test-password!'}).status_code == 404
    assert calls == []


def test_institutional_login_is_public_readonly_and_has_no_signup(client,calls):
    with client.session_transaction() as session:
        session.clear()
    response=client.get('/login')
    assert response.status_code == 200 and client.head('/login').status_code == 200
    assert calls == []
    html=response.get_data(as_text=True)
    assert 'Bienvenido a SAPIENS' in html and 'proporcionadas por la institución' in html
    assert 'Crear cuenta' not in html and 'Regístrate' not in html and '/registro' not in html
    assert 'sapiens-institutional.css' in html and 'sapiens-dark.css' not in html
    assert 'current-password' in html and 'access-processing' in html
    assert 'sapiens@gmail.com' not in html


def test_admin_provisioning_is_not_public(client,calls):
    with client.session_transaction() as session:
        session.clear()
    assert client.get('/usuarios').location.endswith('/login')
    assert client.post('/usuarios',data={'correo':'test@invalid.test'}).location.endswith('/login')
    assert calls == []
