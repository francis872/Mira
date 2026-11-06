import time
from datetime import datetime, timezone, timedelta

import app as frontend
from test_app import client, calls, FakeResponse


def test_anonymous_and_expired_sessions_redirect_to_login(client, calls):
    with client.session_transaction() as session:
        session.clear()
    assert client.get('/').location.endswith('/login')
    assert calls == []
    with client.session_transaction() as session:
        session.update(token='expired',expires=time.time()-1)
    assert client.get('/universidad').location.endswith('/login')


def test_login_rotates_identifier_and_never_exposes_jwt(client, calls):
    client.get('/login')
    before = client.get_cookie('session').value
    calls.responses[('POST','auth/login')] = FakeResponse(200, {
        'token':'private-jwt-not-for-browser', 'expiraEn':(datetime.now(timezone.utc)+timedelta(minutes=20)).isoformat(),
        'usuario':{'id':2,'correo':'person@invalid.test','roles':['Investigador']},
    })
    response = client.post('/login',data={'correo':'person@invalid.test','password':'Password-test-2026!'})
    assert response.status_code == 302
    after = client.get_cookie('session').value
    assert before != after
    assert 'private-jwt-not-for-browser' not in str(response.headers)
    assert 'private-jwt-not-for-browser' not in client.get('/').get_data(as_text=True)
    with client.session_transaction() as session:
        assert session['token'] == 'private-jwt-not-for-browser'
        assert 'csrf_token' in session


def test_login_error_and_csrf_are_preserved(client, calls):
    calls.responses[('POST','auth/login')] = FakeResponse(401,{'mensaje':'Credenciales inválidas.'})
    response=client.post('/login',data={'correo':'person@invalid.test','password':'wrong'})
    assert response.status_code == 401
    assert 'Credenciales inválidas.' in response.get_data(as_text=True)
    assert client.post('/login',data={},no_csrf=True).status_code == 400


def test_backend_401_clears_local_session(client,calls):
    calls.responses[('GET','auth/me')] = FakeResponse(401,{})
    assert client.get('/').location.endswith('/login')
    with client.session_transaction() as session:
        assert 'token' not in session


def test_read_only_identity_hides_mutations_and_denies_direct_routes(client,calls):
    with client.session_transaction() as session:
        session['user']={'id':1,'correo':'test@invalid.test','roles':['Investigador']}
    html=client.get('/').get_data(as_text=True)
    assert 'class="catalog-add"' not in html and '/usuarios' not in html
    assert client.get('/universidad/nuevo').status_code == 403
    assert client.post('/universidad/crear',data={}).status_code == 403
    assert client.get('/usuarios').status_code == 403
    assert not [c for c in calls if c['method']=='POST']


def test_coordinator_cannot_delete_or_manage_users(client,calls):
    with client.session_transaction() as session:
        session['user']={'id':1,'correo':'test@invalid.test','roles':['Coordinador']}
    assert client.get('/universidad/nuevo').status_code == 200
    assert client.post('/universidad/eliminar/1').status_code == 403
    assert client.post('/investigacion/sedes/retirar/1').status_code == 403
    assert client.get('/usuarios').status_code == 403


def test_logout_revokes_before_clearing_session(client,calls):
    calls.responses[('POST','auth/logout')]=FakeResponse(200,{})
    assert client.post('/logout').location.endswith('/login')
    assert calls[-1]['headers']['Authorization']=='Bearer isolated-test-token'
    with client.session_transaction() as session:
        assert 'token' not in session


def test_users_page_never_renders_password_hash(client,calls):
    calls.responses[('GET','auth/usuarios')]=FakeResponse(200,[{'id':2,'correo':'user@invalid.test','activo':True,'roles':[{'id':1,'nombre':'Administrador'}]}])
    calls.responses[('GET','auth/roles')]=FakeResponse(200,[{'id':1,'nombre':'Administrador'}])
    html=client.get('/usuarios').get_data(as_text=True)
    assert 'user@invalid.test' in html and 'password_hash' not in html


def test_server_outage_during_logout_reports_revocation_uncertainty(client,calls):
    calls.responses[('POST','auth/logout')]=FakeResponse(503,{})
    response=client.post('/logout',follow_redirects=True)
    assert 'No se pudo confirmar la revocación' in response.get_data(as_text=True)


def test_backend_forbidden_is_not_rendered_as_success(client,calls):
    calls.responses[('POST','universidad')]=FakeResponse(403,{})
    assert client.post('/universidad/crear',data={}).status_code==403
