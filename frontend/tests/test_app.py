import os

os.environ["SECRET_KEY"] = "test-secret-key-for-frontend-tests-only"
os.environ["API_URL"] = "http://api.test/api"

import pytest

import app as frontend


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


@pytest.fixture
def client():
    frontend.app.config["TESTING"] = True
    with frontend.app.test_client() as test_client:
        yield test_client


def sign_in(client, roles, token="jwt-token"):
    with client.session_transaction() as session:
        session["token"] = token
        session["correo"] = "user@example.invalid"
        session["roles"] = roles


def test_anonymous_user_is_redirected_to_login(client, calls):
    response = client.get("/universidad")
    assert response.status_code == 302
    assert response.headers["Location"].endswith("/login")
    assert calls == []


def test_login_success_stores_session_and_roles(client, calls):
    calls.responses[("POST", "auth/login")] = FakeResponse(
        200, {"accessToken": "abc", "tokenType": "Bearer", "roles": ["Coordinador"]}
    )
    response = client.post("/login", data={"correo": "Coord@Example.invalid", "password": "x" * 12})
    assert response.status_code == 302
    with client.session_transaction() as session:
        assert session["token"] == "abc"
        assert session["roles"] == ["Coordinador"]
        assert session["correo"] == "coord@example.invalid"


def test_login_invalid_credentials_returns_401_without_session(client, calls):
    calls.responses[("POST", "auth/login")] = FakeResponse(401, {"mensaje": "Credenciales inválidas."})
    response = client.post("/login", data={"correo": "a@b.co", "password": "bad"})
    assert response.status_code == 401
    with client.session_transaction() as session:
        assert "token" not in session


def test_logout_clears_session(client, calls):
    sign_in(client, ["Administrador"])
    response = client.post("/logout")
    assert response.status_code == 302
    with client.session_transaction() as session:
        assert "token" not in session


def test_read_only_role_can_list_but_not_write(client, calls):
    sign_in(client, ["Investigador"])
    assert client.get("/universidad").status_code == 200
    assert client.get("/universidad/nuevo").status_code == 403
    assert client.post("/universidad/crear", data={"nombre": "U"}).status_code == 403
    assert client.post("/universidad/eliminar/1").status_code == 403
    assert not [c for c in calls if c["method"] in ("POST", "PUT", "DELETE")]


def test_non_admin_cannot_open_user_administration(client, calls):
    sign_in(client, ["Coordinador"])
    assert client.get("/usuarios").status_code == 403
    assert client.post("/usuarios/crear", data={"correo": "a@b.co"}).status_code == 403
    assert calls == []


def test_coordinator_write_forwards_bearer_token(client, calls):
    sign_in(client, ["Coordinador"], token="tok-123")
    calls.responses[("POST", "universidad")] = FakeResponse(201, {"id": 1})
    response = client.post("/universidad/crear", data={"nombre": "U", "tipo": "Privada", "ciudad": "Medellín"})
    assert response.status_code == 302
    assert calls[-1]["headers"]["Authorization"] == "Bearer tok-123"


def test_api_401_expires_session_and_redirects_to_login(client, calls):
    sign_in(client, ["Administrador"])
    calls.responses[("GET", "universidad")] = FakeResponse(401)
    response = client.get("/universidad")
    assert response.status_code == 302
    assert response.headers["Location"].endswith("/login")
    with client.session_transaction() as session:
        assert "token" not in session


def test_api_403_renders_forbidden_page(client, calls):
    sign_in(client, ["Administrador"])
    calls.responses[("GET", "universidad")] = FakeResponse(403)
    assert client.get("/universidad").status_code == 403


def test_user_form_uses_select_populated_from_api_not_typed_ids(client, calls):
    sign_in(client, ["Administrador"])
    calls.responses[("GET", "auth/roles")] = FakeResponse(200, [{"id": 1, "nombre": "Administrador"}, {"id": 2, "nombre": "Investigador"}])
    html = client.get("/usuarios/nuevo").get_data(as_text=True)
    assert '<select class="form-select" name="roles"' in html
    assert "Investigador" in html
    assert 'name="roles" type="number"' not in html and 'name="roles" type="text"' not in html


def test_user_creation_is_single_atomic_api_call_with_role_collection(client, calls):
    sign_in(client, ["Administrador"])
    calls.responses[("POST", "auth/usuarios")] = FakeResponse(201, {"id": 9})
    response = client.post(
        "/usuarios/crear",
        data={"correo": "new@example.invalid", "password": "p" * 12, "roles": ["1", "2", "2", "abc"]},
    )
    assert response.status_code == 302
    writes = [c for c in calls if c["method"] in ("POST", "PUT", "DELETE")]
    assert len(writes) == 1
    assert writes[0]["json"]["roles"] == [1, 2]


def test_failed_user_creation_shows_api_message(client, calls):
    sign_in(client, ["Administrador"])
    calls.responses[("POST", "auth/usuarios")] = FakeResponse(409, {"mensaje": "El correo ya está registrado."})
    response = client.post("/usuarios/crear", data={"correo": "dup@example.invalid", "password": "p" * 12, "roles": ["1"]})
    assert response.status_code == 400
    assert "El correo ya está registrado." in response.get_data(as_text=True)
