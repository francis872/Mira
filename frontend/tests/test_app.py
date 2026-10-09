import os

os.environ["SECRET_KEY"] = "test-secret-key-for-frontend-tests-only"
os.environ["API_URL"] = "http://api.test/api"

import pytest
from flask.testing import FlaskClient

import app as frontend

CATALOG_ROUTES = [
    "/area_conocimiento",
    "/objetivo_desarrollo_sostenible",
    "/area_aplicacion",
    "/termino_clave",
    "/universidad",
    "/linea_investigacion",
]


class FakeResponse:
    def __init__(self, status=200, body=None):
        self.status_code = status
        self._body = body

    def json(self):
        if self._body is None:
            raise ValueError("no body")
        return self._body


class Calls(list):
    def __init__(self):
        super().__init__()
        self.responses = {}


@pytest.fixture
def calls(monkeypatch):
    recorded = Calls()
    responses = recorded.responses

    def fake_request(method, url, json=None, headers=None, timeout=None):
        recorded.append({"method": method, "url": url, "json": json, "headers": headers or {}})
        return responses.get((method, url.split("/api/", 1)[1]), FakeResponse(200, []))

    monkeypatch.setattr(frontend.requests, "request", fake_request)
    return recorded


class CsrfClient(FlaskClient):
    """Envía el token CSRF de la cookie firmada salvo que se pida lo contrario."""

    def open(self, *args, **kwargs):
        skip = kwargs.pop("no_csrf", False)
        if kwargs.get("method") == "POST" and not skip:
            with self.session_transaction() as session:
                token = session.setdefault("csrf_token", "csrf-test-token")
            data = dict(kwargs.get("data") or {})
            data.setdefault("csrf_token", token)
            kwargs["data"] = data
        return super().open(*args, **kwargs)


@pytest.fixture
def client():
    frontend.app.config["TESTING"] = True
    frontend.app.test_client_class = CsrfClient
    with frontend.app.test_client() as test_client:
        yield test_client


def test_dashboard_opens_directly_without_login(client, calls):
    response = client.get("/")
    assert response.status_code == 200
    assert "Bienvenido a MIRA" in response.get_data(as_text=True)


@pytest.mark.parametrize("path", CATALOG_ROUTES)
def test_catalog_pages_open_without_login(client, calls, path):
    assert client.get(path).status_code == 200
    assert client.get(path + "/nuevo").status_code == 200


@pytest.mark.parametrize("path", ["/login", "/logout", "/usuarios", "/usuarios/nuevo"])
def test_authentication_and_user_routes_do_not_exist(client, calls, path):
    assert client.get(path).status_code == 404
    assert client.post(path, no_csrf=True).status_code in (400, 404)


def test_no_authentication_artifacts_are_sent_to_the_api(client, calls):
    for path in CATALOG_ROUTES:
        client.get(path)
    client.get("/")
    assert calls
    assert all("Authorization" not in c["headers"] for c in calls)
    assert not [c for c in calls if "auth" in c["url"]]


def test_navigation_has_no_login_or_logout_controls(client, calls):
    html = client.get("/").get_data(as_text=True)
    assert "/login" not in html and "/logout" not in html and "Salir" not in html


def test_create_goes_to_the_api_without_token(client, calls):
    calls.responses[("POST", "universidad")] = FakeResponse(201, {"id": 1})
    response = client.post("/universidad/crear", data={"nombre": "U", "tipo": "Privada", "ciudad": "Medellín"})
    assert response.status_code == 302
    sent = [c for c in calls if c["method"] == "POST"]
    assert sent[-1]["json"] == {"nombre": "U", "tipo": "Privada", "ciudad": "Medellín"}
    assert "Authorization" not in sent[-1]["headers"]


def test_update_and_logical_delete_reach_the_api(client, calls):
    calls.responses[("PUT", "universidad/3")] = FakeResponse(200, {"mensaje": "ok"})
    calls.responses[("DELETE", "universidad/3")] = FakeResponse(200, {"mensaje": "ok"})
    assert client.post("/universidad/editar/3", data={"nombre": "U2", "tipo": "Pública", "ciudad": "Bogotá"}).status_code == 302
    assert client.post("/universidad/eliminar/3").status_code == 302
    assert [c["method"] for c in calls if c["method"] != "GET"] == ["PUT", "DELETE"]


def test_api_validation_error_is_shown_to_the_user(client, calls):
    calls.responses[("POST", "universidad")] = FakeResponse(400, {"mensaje": "El nombre es obligatorio."})
    response = client.post("/universidad/crear", data={"nombre": "", "tipo": "", "ciudad": ""})
    assert response.status_code == 200
    assert "El nombre es obligatorio." in response.get_data(as_text=True)


def test_post_without_csrf_token_is_rejected_and_nothing_is_sent(client, calls):
    response = client.post("/universidad/crear", data={"nombre": "U"}, no_csrf=True)
    assert response.status_code == 400
    assert calls == []


def test_post_with_wrong_csrf_token_is_rejected(client, calls):
    client.get("/universidad/nuevo")
    response = client.post("/universidad/crear", data={"nombre": "U", "csrf_token": "forged"}, no_csrf=True)
    assert response.status_code == 400
    assert not [c for c in calls if c["method"] == "POST"]


def test_every_rendered_form_embeds_the_csrf_token(client, calls):
    for path in ("/universidad/nuevo", "/area_conocimiento/nuevo", "/termino_clave/nuevo", "/linea_investigacion/nuevo"):
        assert 'name="csrf_token"' in client.get(path).get_data(as_text=True), path
    calls.responses[("GET", "universidad")] = FakeResponse(200, [{"id": 1, "nombre": "U", "tipo": "Privada", "ciudad": "X", "activo": True}])
    assert 'name="csrf_token"' in client.get("/universidad").get_data(as_text=True)


def test_session_cookie_is_httponly_and_samesite_strict(client, calls):
    cookie = client.get("/universidad/nuevo").headers.get("Set-Cookie", "")
    assert "HttpOnly" in cookie
    assert "SameSite=Strict" in cookie


def test_dashboard_shows_real_counts_from_the_api(client, calls):
    calls.responses[("GET", "universidad")] = FakeResponse(200, [{"id": 1}, {"id": 2}])
    calls.responses[("GET", "area_aplicacion")] = FakeResponse(200, [])
    html = client.get("/").get_data(as_text=True)
    assert "Registros activos: <strong>2</strong>" in html
    assert "Registros activos: <strong>0</strong>" in html


def test_dashboard_reports_unavailable_data_instead_of_inventing_numbers(client, monkeypatch):
    monkeypatch.setattr(frontend.requests, "request", lambda *a, **k: (_ for _ in ()).throw(frontend.requests.exceptions.ConnectionError()))
    html = client.get("/").get_data(as_text=True)
    assert "no disponible" in html
    assert "<strong>0</strong>" not in html


def test_dashboard_states_honestly_what_is_pending(client, calls):
    html = client.get("/").get_data(as_text=True)
    assert "Pendiente: requiere el modelo relacional oficial" in html
    assert "Fuera de alcance (versión futura)" in html
