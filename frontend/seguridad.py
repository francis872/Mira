"""Sesión del servidor: el navegador recibe solo un identificador aleatorio."""
import os
import time
from datetime import datetime, timedelta
from pathlib import Path
from urllib.parse import urlsplit

from cachelib import FileSystemCache
from flask import abort, flash, redirect, render_template, request, session, url_for
from flask_session import Session


class SesionVencida(Exception):
    pass


def configure_session(app):
    production = os.getenv("MIRA_ENV") == "production"
    if production:
        secret = os.getenv("SECRET_KEY", "")
        if len(secret) < 32 or secret.startswith("REPLACE") or not app.config["SESSION_COOKIE_SECURE"]:
            raise RuntimeError("Producción requiere SECRET_KEY externa y SESSION_COOKIE_SECURE=true con HTTPS.")
        api = urlsplit(os.getenv("API_URL", ""))
        if api.scheme != "https" or not api.hostname or api.username or api.password or api.query or api.fragment:
            raise RuntimeError("Producción requiere API_URL HTTPS sin credenciales ni parámetros en la URL.")
    backend = os.getenv('MIRA_SESSION_BACKEND', 'filesystem')
    if backend not in ('filesystem', 'postgres'):
        raise RuntimeError('Seleccione un almacenamiento de sesiones compatible.')
    if os.getenv('VERCEL') == '1' and backend != 'postgres':
        raise RuntimeError('Vercel requiere sesiones PostgreSQL compartidas; no se permite disco efímero.')
    if backend == 'postgres':
        from session_store import PostgresSessionCache, SharedSessionInterface
        cache = PostgresSessionCache(os.getenv('MIRA_SESSION_DATABASE_URL', ''), app.secret_key, production)
    else:
        directory = os.getenv("SESSION_DIRECTORY", str(Path(__file__).resolve().parents[1] / ".local" / "sessions"))
        if production and (not os.getenv("SESSION_DIRECTORY") or not Path(directory).is_absolute()):
            raise RuntimeError("Producción requiere SESSION_DIRECTORY absoluta en almacenamiento privado y persistente para una instancia.")
        Path(directory).mkdir(mode=0o700, parents=True, exist_ok=True)
        cache = FileSystemCache(directory, threshold=2000, default_timeout=3600, mode=0o600)
    app.config.update(
        SESSION_TYPE="cachelib",
        SESSION_CACHELIB=cache,
        SESSION_PERMANENT=False,
        SESSION_REFRESH_EACH_REQUEST=False,
        PERMANENT_SESSION_LIFETIME=timedelta(hours=1),
        MAX_CONTENT_LENGTH=1024 * 1024,
    )
    Session(app)
    if backend == 'postgres':
        app.session_interface = SharedSessionInterface(app, client=cache, permanent=False)
        from session_store import SessionStoreUnavailable

        @app.errorhandler(SessionStoreUnavailable)
        def unavailable_session(_error):
            return render_template('error.html', status=503, message='El servicio de sesiones no está disponible. Intente de nuevo.'), 503

        @app.before_request
        def require_session_store():
            if getattr(session, 'store_unavailable', False) and request.endpoint != 'health':
                return render_template('error.html', status=503, message='El servicio de sesiones no está disponible. Intente de nuevo.'), 503


def register_security(app, api_call, error_message):
    @app.errorhandler(SesionVencida)
    def expired(_error):
        session.clear()
        flash("Su sesión venció o dejó de ser válida. Inicie sesión nuevamente.", "warning")
        return redirect(url_for("login"))

    @app.errorhandler(403)
    def forbidden(_error):
        return render_template("error.html", status=403, message="Su cuenta no tiene permiso para realizar esta operación."), 403

    @app.before_request
    def protect_routes():
        if request.endpoint in ("static", "login", "health", "ready") or request.endpoint is None:
            return None
        if not session.get("token") or session.get("expires", 0) <= time.time():
            raise SesionVencida()
        if request.endpoint == "logout":
            return None
        # Revalidar cuenta, sesión y roles en el backend, incluso al abrir formularios.
        response = api_call("GET", "auth/me", expect_list=False)
        if response is None or response.status_code != 200:
            return render_template("error.html", status=503, message="No fue posible verificar su sesión. Intente de nuevo."), 503
        session["user"] = response.json()
        roles = set(session["user"].get("roles", []))
        if request.path.startswith("/usuarios") and "Administrador" not in roles:
            abort(403)
        writing = request.method != "GET" or any(part in request.path.split("/") for part in ("nuevo", "editar", "ficha"))
        deleting = bool(set(request.path.split("/")).intersection({"eliminar", "retirar"}))
        if deleting and "Administrador" not in roles:
            abort(403)
        if writing and not roles.intersection({"Administrador", "Coordinador"}):
            abort(403)
        return None

    @app.after_request
    def security_headers(response):
        response.headers["Cache-Control"] = "no-store"
        response.headers["X-Content-Type-Options"] = "nosniff"
        response.headers["X-Frame-Options"] = "DENY"
        response.headers["Referrer-Policy"] = "same-origin"
        return response

    @app.context_processor
    def permissions():
        user = session.get("user", {})
        roles = set(user.get("roles", []))
        return dict(current_user=user, can_write=bool(roles.intersection({"Administrador", "Coordinador"})),
                    can_delete="Administrador" in roles, is_admin="Administrador" in roles)

    @app.route("/login", methods=["GET", "POST"])
    def login():
        if request.method == "GET":
            return render_template("login.html")
        correo = request.form.get("correo", "").strip()
        response = api_call("POST", "auth/login", {"correo": correo, "password": request.form.get("password", "")}, expect_list=False)
        if response is None:
            return render_template("login.html", correo=correo, message="El servicio no está disponible. Intente de nuevo."), 503
        if response.status_code != 200:
            return render_template("login.html", correo=correo, message=error_message(response, "No fue posible iniciar sesión.")), response.status_code
        body = response.json()
        try:
            expires = datetime.fromisoformat(body["expiraEn"].replace("Z", "+00:00")).timestamp()
            if expires <= time.time() or not isinstance(body["token"], str) or not isinstance(body["usuario"], dict):
                raise ValueError("Respuesta inválida")
        except (ValueError, KeyError, TypeError):
            return render_template("login.html", correo=correo, message="El servicio devolvió una respuesta inválida."), 502
        session.clear()
        # regenerate necesita una sesión no vacía para eliminar su identificador anterior.
        session["token"] = body["token"]
        session["user"] = body["usuario"]
        session["expires"] = expires
        app.session_interface.regenerate(session)
        return redirect(url_for("index"))

    @app.post("/logout")
    def logout():
        response = api_call("POST", "auth/logout", {}, expect_list=False)
        session.clear()
        if response is None or response.status_code != 200:
            flash("La sesión local se cerró. No se pudo confirmar la revocación del acceso en el servidor; el token vencerá automáticamente.", "warning")
        return redirect(url_for("login"))

    @app.route("/usuarios", methods=["GET", "POST"])
    def usuarios():
        if request.method == "POST":
            data = {"correo": request.form.get("correo", ""), "password": request.form.get("password", ""),
                    "roles": request.form.getlist("roles", type=int)}
            response = api_call("POST", "auth/usuarios", data, expect_list=False)
            if response is not None and response.status_code == 201:
                flash("Cuenta creada.", "success")
                return redirect(url_for("usuarios"))
            flash(error_message(response, "No fue posible crear la cuenta."), "danger")
            status = response.status_code if response is not None else 503
        else:
            status = 200
        users = api_call("GET", "auth/usuarios", expect_list=True)
        roles = api_call("GET", "auth/roles", expect_list=True)
        if users is None or roles is None or users.status_code != 200 or roles.status_code != 200:
            return render_template("error.html", status=503, message="No fue posible cargar las cuentas."), 503
        return render_template("usuarios.html", users=users.json(), roles=roles.json(), page_title="Cuentas de acceso"), status

    @app.post("/usuarios/<int:id>/editar")
    def editar_usuario(id):
        response = api_call("PUT", f"auth/usuarios/{id}", {"correo": request.form.get("correo", ""), "roles": request.form.getlist("roles", type=int)}, expect_list=False)
        if response is None or response.status_code != 200:
            return render_template("error.html", status=response.status_code if response is not None else 503,
                                   message=error_message(response, "No fue posible actualizar la cuenta.")), response.status_code if response is not None else 503
        flash("Cuenta actualizada. Sus sesiones anteriores deberán autenticarse nuevamente si cambiaron correo o roles.", "success")
        return redirect(url_for("usuarios"))

    @app.post("/usuarios/<int:id>/eliminar")
    def inactivar_usuario(id):
        response = api_call("DELETE", f"auth/usuarios/{id}", expect_list=False)
        if response is None or response.status_code != 200:
            return render_template("error.html", status=response.status_code if response is not None else 503,
                                   message=error_message(response, "No fue posible inactivar la cuenta.")), response.status_code if response is not None else 503
        flash("Cuenta inactivada. Su acceso al backend ha sido bloqueado.", "success")
        return redirect(url_for("usuarios"))

    @app.post("/usuarios/<int:id>/password")
    def password_usuario(id):
        response = api_call("PUT", f"auth/usuarios/{id}/password", {"password": request.form.get("password", "")}, expect_list=False)
        if response is None or response.status_code != 200:
            status = response.status_code if response is not None else 503
            return render_template("error.html", status=status, message=error_message(response, "No fue posible restablecer la contraseña.")), status
        flash("Contraseña restablecida y sesiones anteriores revocadas.", "success")
        return redirect(url_for("usuarios"))
