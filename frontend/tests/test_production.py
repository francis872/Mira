"""Guardias de producción y sesión del servidor entre instancias de aplicación."""
import runpy
from pathlib import Path
import pytest
from flask import Flask, session
from seguridad import configure_session
from test_app import client, calls


def make_app(directory, secure=True):
    app = Flask("isolated-session-test")
    app.secret_key = "isolated-test-key-at-least-32-characters"
    app.config.update(TESTING=True, SESSION_COOKIE_SECURE=secure, SESSION_COOKIE_HTTPONLY=True, SESSION_COOKIE_SAMESITE="Strict")
    configure_session(app)

    @app.get("/probe")
    def probe():
        return {"authenticated": session.get("token") == "isolated-private-token"}

    return app


def production_env(monkeypatch, tmp_path):
    monkeypatch.setenv("MIRA_ENV", "production")
    monkeypatch.setenv("SECRET_KEY", "isolated-production-test-key-32-characters")
    monkeypatch.setenv("SESSION_DIRECTORY", str(tmp_path))
    monkeypatch.setenv("API_URL", "https://api.example.invalid/api")


def test_production_requires_https_and_explicit_storage(monkeypatch, tmp_path):
    production_env(monkeypatch, tmp_path)
    with pytest.raises(RuntimeError, match="Secure|SECURE"):
        make_app(tmp_path, secure=False)
    monkeypatch.delenv("SESSION_DIRECTORY")
    with pytest.raises(RuntimeError, match="SESSION_DIRECTORY"):
        make_app(tmp_path)
    monkeypatch.setenv("SESSION_DIRECTORY", "relative-sessions")
    with pytest.raises(RuntimeError, match="SESSION_DIRECTORY"):
        make_app(tmp_path)


def test_production_rejects_placeholder_secret(monkeypatch, tmp_path):
    production_env(monkeypatch, tmp_path)
    monkeypatch.setenv("SECRET_KEY", "REPLACE" + "x" * 40)
    with pytest.raises(RuntimeError, match="SECRET_KEY"):
        make_app(tmp_path)


def test_production_rejects_plaintext_api(monkeypatch, tmp_path):
    production_env(monkeypatch, tmp_path)
    monkeypatch.setenv("API_URL", "http://api.example.invalid/api")
    with pytest.raises(RuntimeError, match="API_URL HTTPS"):
        make_app(tmp_path)


def test_session_survives_app_restart_and_disappears_without_storage(monkeypatch, tmp_path):
    production_env(monkeypatch, tmp_path)
    first = make_app(tmp_path).test_client()
    with first.session_transaction(base_url="https://localhost") as state:
        state.update(token="isolated-private-token", expires=9999999999)
    cookie = first.get_cookie("session").value
    assert "isolated-private-token" not in cookie
    restarted = make_app(tmp_path).test_client()
    restarted.set_cookie("session", cookie)
    assert restarted.get("/probe", base_url="https://localhost").json["authenticated"]
    # Otro disco vacío representa una sesión perdida: el resultado es cerrado, no acceso.
    monkeypatch.setenv("SESSION_DIRECTORY", str(tmp_path / "empty-volume"))
    empty = make_app(tmp_path / "empty-volume").test_client()
    empty.set_cookie("session", cookie)
    assert not empty.get("/probe", base_url="https://localhost").json["authenticated"]


def test_secure_cookie_is_httponly_strict_and_not_jwt(monkeypatch, tmp_path):
    production_env(monkeypatch, tmp_path)
    app = make_app(tmp_path)

    @app.get("/initialize")
    def initialize():
        session["token"] = "isolated-private-token"
        return {"ok": True}

    response = app.test_client().get("/initialize", base_url="https://localhost")
    cookie = response.headers["Set-Cookie"]
    assert all(flag in cookie for flag in ("Secure", "HttpOnly", "SameSite=Strict"))
    assert "isolated-private-token" not in cookie


def test_health_is_anonymous_and_does_not_create_session(client, calls):
    with client.session_transaction() as state:
        state.clear()
    response = client.get("/health")
    assert response.status_code == 200 and response.json == {"status": "ok"}
    assert "Set-Cookie" not in response.headers and not calls


def test_login_ignores_external_redirect(client, calls):
    from test_app import FakeResponse
    from datetime import datetime, timezone, timedelta
    calls.responses[("POST", "auth/login")] = FakeResponse(200, {"token": "test-token", "usuario": {"roles": ["Investigador"]},
        "expiraEn": (datetime.now(timezone.utc) + timedelta(minutes=20)).isoformat()})
    response = client.post("/login?next=https://example.invalid/steal", data={"correo": "test@invalid.test", "password": "test-only-password"})
    assert response.location == "/"


def test_gunicorn_uses_assigned_port_and_one_worker(monkeypatch):
    monkeypatch.setenv("PORT", "12345")
    monkeypatch.setenv("WEB_CONCURRENCY", "1")
    config = runpy.run_path(str(Path(__file__).resolve().parents[1] / "gunicorn.conf.py"))
    assert config["bind"] == "0.0.0.0:12345" and config["workers"] == 1
    assert "%(q)" not in config["access_log_format"] and "%(r)" not in config["access_log_format"]
    monkeypatch.setenv("WEB_CONCURRENCY", "2")
    with pytest.raises(ValueError):
        runpy.run_path(str(Path(__file__).resolve().parents[1] / "gunicorn.conf.py"))
