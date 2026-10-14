"""Pruebas de extremo a extremo contra la plataforma en ejecución (sin inicio de sesión):
navegador (cliente HTTP) -> Flask -> API ASP.NET Core -> PostgreSQL.

Se omiten salvo que se defina MIRA_E2E_URL (p. ej. http://localhost:5000).
MIRA_E2E_API_URL (p. ej. http://localhost:8081) habilita las comprobaciones directas contra la API.
Los datos creados llevan el prefijo "e2e-" y se inactivan al terminar cada prueba.
"""
import os
import re
import uuid

import pytest
import requests

BASE = os.environ.get("MIRA_E2E_URL", "").rstrip("/")
API = os.environ.get("MIRA_E2E_API_URL", "").rstrip("/")

pytestmark = pytest.mark.skipif(not BASE, reason="Defina MIRA_E2E_URL para ejecutar las pruebas E2E.")

CSRF = re.compile(r'name="csrf_token" value="([^"]+)"')


class Browser:
    """Simula un navegador: conserva la cookie con el token CSRF y lo reenvía en cada formulario."""

    def __init__(self):
        self.session = requests.Session()

    def get(self, path):
        return self.session.get(BASE + path, allow_redirects=False, timeout=30)

    def post(self, path, data, form_page, with_token=True):
        payload = dict(data)
        if with_token:
            html = self.session.get(BASE + form_page, timeout=30).text
            match = CSRF.search(html)
            assert match, f"{form_page} no contiene token CSRF"
            payload["csrf_token"] = match.group(1)
        return self.session.post(BASE + path, data=payload, allow_redirects=False, timeout=30)


@pytest.fixture
def browser():
    return Browser()


CATALOGS = [
    dict(path="area_conocimiento", field="gran_area", seed="Ingeniería de Software",
         create=lambda m: {"gran_area": m, "area": "Área E2E", "disciplina": "Disciplina E2E"}),
    dict(path="objetivo_desarrollo_sostenible", field="nombre", seed="Educación de Calidad",
         create=lambda m: {"nombre": m, "categoria": "Social"}),
    dict(path="area_aplicacion", field="nombre", seed="Salud y Telemedicina",
         create=lambda m: {"nombre": m}),
    dict(path="termino_clave", field="termino", seed="Inteligencia Artificial", key_is_marker=True,
         create=lambda m: {"termino": m, "termino_ingles": "e2e"}),
    dict(path="universidad", field="nombre", seed="Universidad de San Buenaventura",
         create=lambda m: {"nombre": m, "tipo": "Privada", "ciudad": "Medellín"}),
    dict(path="linea_investigacion", field="nombre", seed="Ciberseguridad y Redes",
         create=lambda m: {"nombre": m, "descripcion": "e2e"}),
]
IDS = [c["path"] for c in CATALOGS]


def row_for(html, marker):
    return next((row for row in html.split("<tr>") if marker in row), None)


# Acceso directo, sin autenticación
def test_root_opens_the_dashboard_directly(browser):
    response = browser.get("/")
    assert response.status_code == 200
    assert "Bienvenido a MIRA" in response.text
    for title in ("Área de Conocimiento", "ODS", "Área de Aplicación", "Término Clave", "Universidad", "Línea de Investigación"):
        assert title in response.text
    assert re.search(r"Registros activos: <strong>\d+</strong>", response.text)
    assert "Pendiente: requiere el modelo relacional oficial" in response.text


@pytest.mark.parametrize("path", ["/login", "/logout", "/usuarios"])
def test_there_is_no_authentication_surface(browser, path):
    assert browser.get(path).status_code == 404


def test_api_answers_without_token():
    if not API:
        pytest.skip("Defina MIRA_E2E_API_URL para comprobar la API directamente.")
    for path in ("universidad", "area_conocimiento", "objetivo_desarrollo_sostenible",
                 "area_aplicacion", "termino_clave", "linea_investigacion"):
        response = requests.get(f"{API}/api/{path}", timeout=30)
        assert response.status_code == 200, path
    assert requests.post(f"{API}/api/auth/login", json={}, timeout=30).status_code == 404


def test_dashboard_counts_match_the_listing(browser):
    dashboard = browser.get("/").text
    listing = browser.get("/universidad").text
    shown = int(re.search(r"Universidad</h3>\s*<small[^>]*>\s*Registros activos: <strong>(\d+)</strong>", dashboard).group(1))
    assert shown == len(re.findall(r'bi-check-circle me-1"></i>Activo', listing))


@pytest.mark.parametrize("catalog", CATALOGS, ids=IDS)
def test_catalog_lists_real_seed_data(browser, catalog):
    response = browser.get("/" + catalog["path"])
    assert response.status_code == 200
    assert catalog["seed"] in response.text


@pytest.mark.parametrize("catalog", CATALOGS, ids=IDS)
def test_catalog_create_update_inactivate_persist(browser, catalog):
    path, field = catalog["path"], catalog["field"]
    marker = f"e2e-{uuid.uuid4().hex[:10]}"
    form = f"/{path}/nuevo"

    created = browser.post(f"/{path}/crear", catalog["create"](marker), form)
    assert created.status_code == 302, created.text[:400]

    fresh = Browser()  # otra "pestaña" sin estado: los datos deben venir de PostgreSQL
    row = row_for(fresh.get(f"/{path}").text, marker)
    assert row, "el registro creado no aparece al volver a consultar"

    key = marker if catalog.get("key_is_marker") else re.search(rf"/{path}/editar/(\d+)", row).group(1)
    edit = f"/{path}/editar/{key}"
    assert marker in browser.get(edit).text

    if catalog.get("key_is_marker"):
        new_value, update = "e2e-modificado", {"termino_ingles": "e2e-modificado"}
    else:
        new_value = marker + "-mod"
        update = {**catalog["create"](marker), field: new_value}
    assert browser.post(edit, update, edit).status_code == 302
    assert new_value in Browser().get(f"/{path}").text

    assert browser.post(f"/{path}/eliminar/{key}", {}, f"/{path}").status_code == 302
    assert marker not in Browser().get(f"/{path}").text
    assert browser.get(edit).status_code == 302  # el registro inactivo ya no se abre


@pytest.mark.parametrize("catalog", CATALOGS, ids=IDS)
def test_empty_form_shows_validation_error_and_creates_nothing(browser, catalog):
    empty = {k: "" for k in catalog["create"]("x")}
    response = browser.post(f"/{catalog['path']}/crear", empty, f"/{catalog['path']}/nuevo")
    assert response.status_code == 200
    assert "Error:" in response.text


def test_duplicate_keyword_shows_conflict(browser):
    term = f"e2e-{uuid.uuid4().hex[:10]}"
    assert browser.post("/termino_clave/crear", {"termino": term, "termino_ingles": "a"}, "/termino_clave/nuevo").status_code == 302
    second = browser.post("/termino_clave/crear", {"termino": term, "termino_ingles": "b"}, "/termino_clave/nuevo")
    assert second.status_code == 200 and "Error:" in second.text
    browser.post(f"/termino_clave/eliminar/{term}", {}, "/termino_clave")


# Medidas generales de seguridad
def test_post_without_csrf_token_is_rejected(browser):
    response = browser.post("/universidad/crear", {"nombre": "e2e-csrf", "tipo": "x", "ciudad": "y"}, "/universidad/nuevo", with_token=False)
    assert response.status_code == 400
    assert "e2e-csrf" not in Browser().get("/universidad").text


def test_injection_text_is_stored_as_data_and_escaped(browser):
    marker = "e2e-<script>alert(1)</script>'; DROP TABLE universidad;--"
    assert browser.post("/area_aplicacion/crear", {"nombre": marker}, "/area_aplicacion/nuevo").status_code == 302
    html = Browser().get("/area_aplicacion").text
    assert "<script>alert(1)</script>" not in html
    assert "&lt;script&gt;alert(1)&lt;/script&gt;" in html
    key = re.search(r"/area_aplicacion/editar/(\d+)", row_for(html, "&lt;script&gt;"))
    browser.post(f"/area_aplicacion/eliminar/{key.group(1)}", {}, "/area_aplicacion")
    assert Browser().get("/universidad").status_code == 200  # la tabla sigue existiendo


def test_session_cookie_flags(browser):
    cookie = browser.session.get(BASE + "/universidad/nuevo", timeout=30).headers.get("Set-Cookie", "")
    assert "HttpOnly" in cookie and "SameSite=Strict" in cookie
