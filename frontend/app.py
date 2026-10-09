import hmac
import os
import secrets
import requests
from markupsafe import Markup
from urllib.parse import quote
from flask import Flask, render_template, request, redirect, url_for, flash, session, abort

app = Flask(__name__)
_secret = os.getenv("SECRET_KEY", "")
if not _secret or _secret.startswith("REPLACE"):
    # La cookie firmada solo transporta el token CSRF y los mensajes flash; no hay autenticación.
    _secret = secrets.token_hex(32)
app.secret_key = _secret
app.config.update(
    SESSION_COOKIE_HTTPONLY=True,
    SESSION_COOKIE_SAMESITE="Strict",
    SESSION_COOKIE_SECURE=os.getenv("SESSION_COOKIE_SECURE", "false").lower() == "true",
)
SWAGGER_URL = os.getenv("SWAGGER_URL", "http://localhost:8081/swagger")


def _csrf_token():
    token = session.get("csrf_token")
    if not token:
        token = secrets.token_urlsafe(32)
        session["csrf_token"] = token
    return token


app.jinja_env.globals["csrf_input"] = lambda: Markup(
    f'<input type="hidden" name="csrf_token" value="{_csrf_token()}">'
)


def _headers():
    return {"Content-Type": "application/json"}


def error_message(resp, default):
    if resp is None:
        return "Error de conexión."
    try:
        body = resp.json()
    except ValueError:
        return default
    if isinstance(body, dict):
        return body.get("mensaje") or body.get("title") or default
    return default


@app.context_processor
def inject_globals():
    return {"swagger_url": SWAGGER_URL}


@app.errorhandler(400)
def handle_bad_request(_error):
    return render_template(
        "error.html", status=400,
        message="La solicitud fue rechazada: el token de seguridad es inválido o venció. Recargue la página e intente de nuevo.",
    ), 400


@app.before_request
def verify_csrf():
    if request.method == "POST":
        sent = request.form.get("csrf_token", "")
        expected = session.get("csrf_token", "")
        if not expected or not hmac.compare_digest(sent, expected):
            abort(400)
    return None

# URL base de la API REST de MIRA
API_URL = os.getenv("API_URL", "http://localhost:8081/api").rstrip("/")

def api_call(method, path, json_data=None):
    """Función auxiliar para realizar llamadas seguras a la API REST."""
    url = f"{API_URL}/{path.lstrip('/')}"
    try:
        response = requests.request(
            method=method,
            url=url,
            json=json_data,
            headers=_headers(),
            timeout=8
        )
    except requests.exceptions.RequestException:
        return None
    return response

# =============================================================================
# Dashboard Principal
# =============================================================================
CATALOGS = [
    {"path": "area_conocimiento", "title": "Área de Conocimiento", "icon": "bi-book", "color": "danger",
     "list": "area_conocimiento_list", "new": "area_conocimiento_new",
     "text": "Clasificación académica por gran área, área específica y disciplina."},
    {"path": "objetivo_desarrollo_sostenible", "title": "ODS", "icon": "bi-globe-americas", "color": "success",
     "list": "ods_list", "new": "ods_new",
     "text": "Objetivos de Desarrollo Sostenible con su categoría temática."},
    {"path": "area_aplicacion", "title": "Área de Aplicación", "icon": "bi-cpu", "color": "primary",
     "list": "area_aplicacion_list", "new": "area_aplicacion_new",
     "text": "Sectores y ámbitos donde se aplican los resultados de investigación."},
    {"path": "termino_clave", "title": "Término Clave", "icon": "bi-tags", "color": "warning",
     "list": "termino_clave_list", "new": "termino_clave_new",
     "text": "Palabras clave normalizadas en español e inglés."},
    {"path": "universidad", "title": "Universidad", "icon": "bi-buildings", "color": "info",
     "list": "universidad_list", "new": "universidad_new",
     "text": "Instituciones de educación superior, tipo y ciudad sede."},
    {"path": "linea_investigacion", "title": "Línea de Investigación", "icon": "bi-diagram-3", "color": "secondary",
     "list": "linea_investigacion_list", "new": "linea_investigacion_new",
     "text": "Líneas temáticas institucionales con su descripción."},
]


def _count(path):
    resp = api_call("GET", path)
    if resp is None or resp.status_code != 200:
        return None
    return len(resp.json())


@app.route("/")
def index():
    catalogs = [{**c, "count": _count(c["path"])} for c in CATALOGS]
    api_ok = all(c["count"] is not None for c in catalogs)
    return render_template("index.html", catalogs=catalogs, api_ok=api_ok)

# =============================================================================
# Módulo 1: Área de Conocimiento
# =============================================================================
@app.route("/area_conocimiento")
def area_conocimiento_list():
    resp = api_call("GET", "area_conocimiento")
    items = resp.json() if resp and resp.status_code == 200 else []
    if resp is None:
        flash("No fue posible conectar con la API REST.", "danger")
    return render_template("modulos/area_conocimiento/list.html", items=items)

@app.route("/area_conocimiento/nuevo")
def area_conocimiento_new():
    return render_template("modulos/area_conocimiento/form.html", item=None)

@app.route("/area_conocimiento/crear", methods=["POST"])
def area_conocimiento_create():
    payload = {
        "granArea": request.form.get("gran_area", "").strip(),
        "area": request.form.get("area", "").strip(),
        "disciplina": request.form.get("disciplina", "").strip()
    }
    resp = api_call("POST", "area_conocimiento", payload)
    if resp and resp.status_code == 201:
        flash("Área de conocimiento creada exitosamente.", "success")
        return redirect(url_for("area_conocimiento_list"))
    error_msg = resp.json().get("mensaje", "Error al crear.") if resp else "Error de conexión."
    flash(f"Error: {error_msg}", "danger")
    return render_template("modulos/area_conocimiento/form.html", item=payload)

@app.route("/area_conocimiento/editar/<int:id>")
def area_conocimiento_edit(id):
    resp = api_call("GET", f"area_conocimiento/{id}")
    if resp and resp.status_code == 200:
        return render_template("modulos/area_conocimiento/form.html", item=resp.json())
    flash("Registro no encontrado o inactivo.", "warning")
    return redirect(url_for("area_conocimiento_list"))

@app.route("/area_conocimiento/editar/<int:id>", methods=["POST"])
def area_conocimiento_update(id):
    payload = {
        "id": id,
        "granArea": request.form.get("gran_area", "").strip(),
        "area": request.form.get("area", "").strip(),
        "disciplina": request.form.get("disciplina", "").strip()
    }
    resp = api_call("PUT", f"area_conocimiento/{id}", payload)
    if resp and resp.status_code == 200:
        flash("Área de conocimiento actualizada exitosamente.", "success")
        return redirect(url_for("area_conocimiento_list"))
    error_msg = resp.json().get("mensaje", "Error al actualizar.") if resp else "Error de conexión."
    flash(f"Error: {error_msg}", "danger")
    return render_template("modulos/area_conocimiento/form.html", item=payload)

@app.route("/area_conocimiento/eliminar/<int:id>", methods=["POST"])
def area_conocimiento_delete(id):
    resp = api_call("DELETE", f"area_conocimiento/{id}")
    if resp and resp.status_code == 200:
        flash("Área de conocimiento eliminada lógicamente.", "success")
    else:
        flash("No se pudo eliminar el registro.", "danger")
    return redirect(url_for("area_conocimiento_list"))

# =============================================================================
# Módulo 2: Objetivo de Desarrollo Sostenible (ODS)
# =============================================================================
@app.route("/objetivo_desarrollo_sostenible")
def ods_list():
    resp = api_call("GET", "objetivo_desarrollo_sostenible")
    items = resp.json() if resp and resp.status_code == 200 else []
    if resp is None:
        flash("No fue posible conectar con la API REST.", "danger")
    return render_template("modulos/objetivo_desarrollo_sostenible/list.html", items=items)

@app.route("/objetivo_desarrollo_sostenible/nuevo")
def ods_new():
    return render_template("modulos/objetivo_desarrollo_sostenible/form.html", item=None)

@app.route("/objetivo_desarrollo_sostenible/crear", methods=["POST"])
def ods_create():
    payload = {
        "nombre": request.form.get("nombre", "").strip(),
        "categoria": request.form.get("categoria", "").strip()
    }
    resp = api_call("POST", "objetivo_desarrollo_sostenible", payload)
    if resp and resp.status_code == 201:
        flash("ODS creado exitosamente.", "success")
        return redirect(url_for("ods_list"))
    error_msg = resp.json().get("mensaje", "Error al crear.") if resp else "Error de conexión."
    flash(f"Error: {error_msg}", "danger")
    return render_template("modulos/objetivo_desarrollo_sostenible/form.html", item=payload)

@app.route("/objetivo_desarrollo_sostenible/editar/<int:id>")
def ods_edit(id):
    resp = api_call("GET", f"objetivo_desarrollo_sostenible/{id}")
    if resp and resp.status_code == 200:
        return render_template("modulos/objetivo_desarrollo_sostenible/form.html", item=resp.json())
    flash("Registro no encontrado o inactivo.", "warning")
    return redirect(url_for("ods_list"))

@app.route("/objetivo_desarrollo_sostenible/editar/<int:id>", methods=["POST"])
def ods_update(id):
    payload = {
        "id": id,
        "nombre": request.form.get("nombre", "").strip(),
        "categoria": request.form.get("categoria", "").strip()
    }
    resp = api_call("PUT", f"objetivo_desarrollo_sostenible/{id}", payload)
    if resp and resp.status_code == 200:
        flash("ODS actualizado exitosamente.", "success")
        return redirect(url_for("ods_list"))
    error_msg = resp.json().get("mensaje", "Error al actualizar.") if resp else "Error de conexión."
    flash(f"Error: {error_msg}", "danger")
    return render_template("modulos/objetivo_desarrollo_sostenible/form.html", item=payload)

@app.route("/objetivo_desarrollo_sostenible/eliminar/<int:id>", methods=["POST"])
def ods_delete(id):
    resp = api_call("DELETE", f"objetivo_desarrollo_sostenible/{id}")
    if resp and resp.status_code == 200:
        flash("ODS eliminado lógicamente.", "success")
    else:
        flash("No se pudo eliminar el registro.", "danger")
    return redirect(url_for("ods_list"))

# =============================================================================
# Módulo 3: Área de Aplicación
# =============================================================================
@app.route("/area_aplicacion")
def area_aplicacion_list():
    resp = api_call("GET", "area_aplicacion")
    items = resp.json() if resp and resp.status_code == 200 else []
    if resp is None:
        flash("No fue posible conectar con la API REST.", "danger")
    return render_template("modulos/area_aplicacion/list.html", items=items)

@app.route("/area_aplicacion/nuevo")
def area_aplicacion_new():
    return render_template("modulos/area_aplicacion/form.html", item=None)

@app.route("/area_aplicacion/crear", methods=["POST"])
def area_aplicacion_create():
    payload = {
        "nombre": request.form.get("nombre", "").strip()
    }
    resp = api_call("POST", "area_aplicacion", payload)
    if resp and resp.status_code == 201:
        flash("Área de aplicación creada exitosamente.", "success")
        return redirect(url_for("area_aplicacion_list"))
    error_msg = resp.json().get("mensaje", "Error al crear.") if resp else "Error de conexión."
    flash(f"Error: {error_msg}", "danger")
    return render_template("modulos/area_aplicacion/form.html", item=payload)

@app.route("/area_aplicacion/editar/<int:id>")
def area_aplicacion_edit(id):
    resp = api_call("GET", f"area_aplicacion/{id}")
    if resp and resp.status_code == 200:
        return render_template("modulos/area_aplicacion/form.html", item=resp.json())
    flash("Registro no encontrado o inactivo.", "warning")
    return redirect(url_for("area_aplicacion_list"))

@app.route("/area_aplicacion/editar/<int:id>", methods=["POST"])
def area_aplicacion_update(id):
    payload = {
        "id": id,
        "nombre": request.form.get("nombre", "").strip()
    }
    resp = api_call("PUT", f"area_aplicacion/{id}", payload)
    if resp and resp.status_code == 200:
        flash("Área de aplicación actualizada exitosamente.", "success")
        return redirect(url_for("area_aplicacion_list"))
    error_msg = resp.json().get("mensaje", "Error al actualizar.") if resp else "Error de conexión."
    flash(f"Error: {error_msg}", "danger")
    return render_template("modulos/area_aplicacion/form.html", item=payload)

@app.route("/area_aplicacion/eliminar/<int:id>", methods=["POST"])
def area_aplicacion_delete(id):
    resp = api_call("DELETE", f"area_aplicacion/{id}")
    if resp and resp.status_code == 200:
        flash("Área de aplicación eliminada lógicamente.", "success")
    else:
        flash("No se pudo eliminar el registro.", "danger")
    return redirect(url_for("area_aplicacion_list"))

# =============================================================================
# Módulo 4: Término Clave (PK string)
# =============================================================================
@app.route("/termino_clave")
def termino_clave_list():
    resp = api_call("GET", "termino_clave")
    items = resp.json() if resp and resp.status_code == 200 else []
    if resp is None:
        flash("No fue posible conectar con la API REST.", "danger")
    return render_template("modulos/termino_clave/list.html", items=items)

@app.route("/termino_clave/nuevo")
def termino_clave_new():
    return render_template("modulos/termino_clave/form.html", item=None, is_new=True)

@app.route("/termino_clave/crear", methods=["POST"])
def termino_clave_create():
    payload = {
        "termino": request.form.get("termino", "").strip(),
        "terminoIngles": request.form.get("termino_ingles", "").strip() or None
    }
    resp = api_call("POST", "termino_clave", payload)
    if resp and resp.status_code == 201:
        flash("Término clave creado exitosamente.", "success")
        return redirect(url_for("termino_clave_list"))
    error_msg = resp.json().get("mensaje", "Error al crear.") if resp else "Error de conexión."
    flash(f"Error: {error_msg}", "danger")
    return render_template("modulos/termino_clave/form.html", item=payload, is_new=True)

@app.route("/termino_clave/editar/<path:termino>")
def termino_clave_edit(termino):
    encoded = quote(termino, safe="")
    resp = api_call("GET", f"termino_clave/{encoded}")
    if resp and resp.status_code == 200:
        return render_template("modulos/termino_clave/form.html", item=resp.json(), is_new=False)
    flash("Registro no encontrado o inactivo.", "warning")
    return redirect(url_for("termino_clave_list"))

@app.route("/termino_clave/editar/<path:termino>", methods=["POST"])
def termino_clave_update(termino):
    encoded = quote(termino, safe="")
    payload = {
        "termino": termino,
        "terminoIngles": request.form.get("termino_ingles", "").strip() or None
    }
    resp = api_call("PUT", f"termino_clave/{encoded}", payload)
    if resp and resp.status_code == 200:
        flash("Término clave actualizado exitosamente.", "success")
        return redirect(url_for("termino_clave_list"))
    error_msg = resp.json().get("mensaje", "Error al actualizar.") if resp else "Error de conexión."
    flash(f"Error: {error_msg}", "danger")
    return render_template("modulos/termino_clave/form.html", item=payload, is_new=False)

@app.route("/termino_clave/eliminar/<path:termino>", methods=["POST"])
def termino_clave_delete(termino):
    encoded = quote(termino, safe="")
    resp = api_call("DELETE", f"termino_clave/{encoded}")
    if resp and resp.status_code == 200:
        flash("Término clave eliminado lógicamente.", "success")
    else:
        flash("No se pudo eliminar el registro.", "danger")
    return redirect(url_for("termino_clave_list"))

# =============================================================================
# Módulo 5: Universidad
# =============================================================================
@app.route("/universidad")
def universidad_list():
    resp = api_call("GET", "universidad")
    items = resp.json() if resp and resp.status_code == 200 else []
    if resp is None:
        flash("No fue posible conectar con la API REST.", "danger")
    return render_template("modulos/universidad/list.html", items=items)

@app.route("/universidad/nuevo")
def universidad_new():
    return render_template("modulos/universidad/form.html", item=None)

@app.route("/universidad/crear", methods=["POST"])
def universidad_create():
    payload = {
        "nombre": request.form.get("nombre", "").strip(),
        "tipo": request.form.get("tipo", "").strip(),
        "ciudad": request.form.get("ciudad", "").strip()
    }
    resp = api_call("POST", "universidad", payload)
    if resp and resp.status_code == 201:
        flash("Universidad creada exitosamente.", "success")
        return redirect(url_for("universidad_list"))
    error_msg = resp.json().get("mensaje", "Error al crear.") if resp else "Error de conexión."
    flash(f"Error: {error_msg}", "danger")
    return render_template("modulos/universidad/form.html", item=payload)

@app.route("/universidad/editar/<int:id>")
def universidad_edit(id):
    resp = api_call("GET", f"universidad/{id}")
    if resp and resp.status_code == 200:
        return render_template("modulos/universidad/form.html", item=resp.json())
    flash("Registro no encontrado o inactivo.", "warning")
    return redirect(url_for("universidad_list"))

@app.route("/universidad/editar/<int:id>", methods=["POST"])
def universidad_update(id):
    payload = {
        "id": id,
        "nombre": request.form.get("nombre", "").strip(),
        "tipo": request.form.get("tipo", "").strip(),
        "ciudad": request.form.get("ciudad", "").strip()
    }
    resp = api_call("PUT", f"universidad/{id}", payload)
    if resp and resp.status_code == 200:
        flash("Universidad actualizada exitosamente.", "success")
        return redirect(url_for("universidad_list"))
    error_msg = resp.json().get("mensaje", "Error al actualizar.") if resp else "Error de conexión."
    flash(f"Error: {error_msg}", "danger")
    return render_template("modulos/universidad/form.html", item=payload)

@app.route("/universidad/eliminar/<int:id>", methods=["POST"])
def universidad_delete(id):
    resp = api_call("DELETE", f"universidad/{id}")
    if resp and resp.status_code == 200:
        flash("Universidad eliminada lógicamente.", "success")
    else:
        flash("No se pudo eliminar el registro.", "danger")
    return redirect(url_for("universidad_list"))

# =============================================================================
# Módulo 6: Línea de Investigación
# =============================================================================
@app.route("/linea_investigacion")
def linea_investigacion_list():
    resp = api_call("GET", "linea_investigacion")
    items = resp.json() if resp and resp.status_code == 200 else []
    if resp is None:
        flash("No fue posible conectar con la API REST.", "danger")
    return render_template("modulos/linea_investigacion/list.html", items=items)

@app.route("/linea_investigacion/nuevo")
def linea_investigacion_new():
    return render_template("modulos/linea_investigacion/form.html", item=None)

@app.route("/linea_investigacion/crear", methods=["POST"])
def linea_investigacion_create():
    payload = {
        "nombre": request.form.get("nombre", "").strip(),
        "descripcion": request.form.get("descripcion", "").strip() or None
    }
    resp = api_call("POST", "linea_investigacion", payload)
    if resp and resp.status_code == 201:
        flash("Línea de investigación creada exitosamente.", "success")
        return redirect(url_for("linea_investigacion_list"))
    error_msg = resp.json().get("mensaje", "Error al crear.") if resp else "Error de conexión."
    flash(f"Error: {error_msg}", "danger")
    return render_template("modulos/linea_investigacion/form.html", item=payload)

@app.route("/linea_investigacion/editar/<int:id>")
def linea_investigacion_edit(id):
    resp = api_call("GET", f"linea_investigacion/{id}")
    if resp and resp.status_code == 200:
        return render_template("modulos/linea_investigacion/form.html", item=resp.json())
    flash("Registro no encontrado o inactivo.", "warning")
    return redirect(url_for("linea_investigacion_list"))

@app.route("/linea_investigacion/editar/<int:id>", methods=["POST"])
def linea_investigacion_update(id):
    payload = {
        "id": id,
        "nombre": request.form.get("nombre", "").strip(),
        "descripcion": request.form.get("descripcion", "").strip() or None
    }
    resp = api_call("PUT", f"linea_investigacion/{id}", payload)
    if resp and resp.status_code == 200:
        flash("Línea de investigación actualizada exitosamente.", "success")
        return redirect(url_for("linea_investigacion_list"))
    error_msg = resp.json().get("mensaje", "Error al actualizar.") if resp else "Error de conexión."
    flash(f"Error: {error_msg}", "danger")
    return render_template("modulos/linea_investigacion/form.html", item=payload)

@app.route("/linea_investigacion/eliminar/<int:id>", methods=["POST"])
def linea_investigacion_delete(id):
    resp = api_call("DELETE", f"linea_investigacion/{id}")
    if resp and resp.status_code == 200:
        flash("Línea de investigación eliminada lógicamente.", "success")
    else:
        flash("No se pudo eliminar el registro.", "danger")
    return redirect(url_for("linea_investigacion_list"))


if __name__ == "__main__":
    port = int(os.getenv("PORT", 5000))
    app.run(host="0.0.0.0", port=port, debug=False)

