import hashlib
import os
import secrets
from pathlib import Path

import psycopg
import pytest
from flask import Flask, session
from psycopg.conninfo import make_conninfo, conninfo_to_dict
from seguridad import configure_session
from session_store import PostgresSessionCache, SessionStoreUnavailable


@pytest.fixture
def session_dsn():
    value=os.getenv('MIRA_SESSION_TEST_DSN')
    if not value and os.getenv('MIRA_TEST_DB'):
        fields=dict(part.split('=',1) for part in os.environ['MIRA_TEST_DB'].split(';') if '=' in part)
        fields={k.lower().strip():v.strip().strip('"') for k,v in fields.items()}
        value=make_conninfo(host=fields['host'],port=fields.get('port','5432'),dbname=fields['database'],user=fields['username'],password=fields['password'])
    if not value and os.getenv('MIRA_CI')=='1':
        value=make_conninfo(host=os.environ['PGHOST'],port=os.environ['PGPORT'],dbname=os.environ['PGDATABASE'],user=os.environ['PGUSER'],password=os.environ['PGPASSWORD'])
    if not value:
        pytest.skip('Requiere la base PostgreSQL aislada del verificador integrado.')
    settings=conninfo_to_dict(value)
    assert settings['host'] in ('127.0.0.1','localhost') and settings['dbname'].startswith(('mira_audit_','mira_ci_'))
    return value


@pytest.fixture
def applications(monkeypatch,session_dsn):
    monkeypatch.setenv('MIRA_ENV','development');monkeypatch.setenv('MIRA_SESSION_BACKEND','postgres')
    monkeypatch.setenv('MIRA_SESSION_DATABASE_URL',session_dsn);monkeypatch.delenv('VERCEL',raising=False)
    apps=[]
    for i in range(2):
        app=Flask('shared-session-'+str(i),template_folder=str(Path(__file__).resolve().parents[1]/'templates'))
        app.secret_key='isolated-shared-session-fixture-32-bytes-never-deploy';app.config['SESSION_COOKIE_SECURE']=False
        configure_session(app)
        app.add_url_rule('/',endpoint='index',view_func=lambda:'home')
        @app.get('/write')
        def write():session['token']='server-token-fixture';session['marker']='shared';return 'saved'
        @app.get('/read')
        def read():return {'marker':session.get('marker'),'tokenPresent':bool(session.get('token'))}
        @app.post('/rotate')
        def rotate(current=app):session['marker']='rotated';current.session_interface.regenerate(session);return 'rotated'
        apps.append(app)
    return apps


def test_sessions_survive_another_application_instance_and_keep_tokens_off_cookie(applications):
    first,second=(app.test_client() for app in applications)
    response=first.get('/write');sid=first.get_cookie('session').value
    assert response.status_code==200 and 'server-token-fixture' not in str(response.headers)
    second.set_cookie('session',sid)
    assert second.get('/read').json=={'marker':'shared','tokenPresent':True}
    cache=applications[0].config['SESSION_CACHELIB']
    with cache._connect() as db:
        key=db.execute('SELECT clave_hash FROM mira_web.sesion WHERE clave_hash=%s',(hashlib.sha256(('session:'+sid).encode()).hexdigest(),)).fetchone()[0]
    assert key!=sid


def test_rotation_removes_old_identifier_from_shared_store(applications):
    first,second=(app.test_client() for app in applications)
    first.get('/write');old=first.get_cookie('session').value
    assert first.post('/rotate').status_code==200
    assert first.get_cookie('session').value!=old
    second.set_cookie('session',old)
    assert second.get('/read').json=={'marker':None,'tokenPresent':False}


def test_expired_or_tampered_database_session_fails_closed(applications):
    client=applications[0].test_client();cache=applications[0].config['SESSION_CACHELIB']
    client.get('/write');sid=client.get_cookie('session').value;key=hashlib.sha256(('session:'+sid).encode()).hexdigest()
    with cache._connect() as db:
        db.execute('UPDATE mira_web.sesion SET datos=%s WHERE clave_hash=%s',(b'{"marker":"forged"}',key))
    assert client.get('/read').json=={'marker':None,'tokenPresent':False}
    client.get('/write');sid=client.get_cookie('session').value;key=hashlib.sha256(('session:'+sid).encode()).hexdigest()
    with cache._connect() as db:db.execute("UPDATE mira_web.sesion SET expira_en=CURRENT_TIMESTAMP-interval '1 second' WHERE clave_hash=%s",(key,))
    assert client.get('/read').json=={'marker':None,'tokenPresent':False}


def test_read_outage_returns_503_without_replacing_the_existing_cookie(applications,monkeypatch):
    app=applications[0];client=app.test_client();client.get('/write');sid=client.get_cookie('session').value
    def fail():raise psycopg.OperationalError('private-dsn-not-for-response')
    monkeypatch.setattr(app.config['SESSION_CACHELIB'],'_connect',fail)
    response=client.get('/read')
    assert response.status_code==503 and client.get_cookie('session').value==sid
    assert 'private-dsn-not-for-response' not in response.text and not response.headers.get('Set-Cookie')


def test_write_outage_does_not_report_a_successful_login_or_emit_cookie(applications,monkeypatch):
    app=applications[0];client=app.test_client()
    def fail(*args,**kwargs):raise SessionStoreUnavailable('private-store-error')
    monkeypatch.setattr(app.config['SESSION_CACHELIB'],'set',fail)
    response=client.get('/write')
    assert response.status_code==503 and not response.headers.get('Set-Cookie')
    assert 'saved' not in response.text and 'private-store-error' not in response.text


def test_session_configuration_rejects_missing_tls_without_exposing_password():
    with pytest.raises(RuntimeError) as error:PostgresSessionCache('host=example.invalid password=never-show-this','x'*32,production=True)
    assert 'never-show-this' not in str(error.value)


def test_vercel_cannot_use_ephemeral_session_files(monkeypatch):
    monkeypatch.setenv('MIRA_ENV','development');monkeypatch.setenv('VERCEL','1');monkeypatch.setenv('MIRA_SESSION_BACKEND','filesystem')
    app=Flask('no-ephemeral');app.secret_key='x'*32;app.config['SESSION_COOKIE_SECURE']=False
    with pytest.raises(RuntimeError,match='compartidas'):configure_session(app)
