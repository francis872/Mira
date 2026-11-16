"""Una instancia con sesiones compartidas por sus threads en un directorio privado."""
import os

port = int(os.getenv("PORT", "5000"))
if not 1 <= port <= 65535:
    raise ValueError("PORT debe estar entre 1 y 65535.")
if os.getenv("WEB_CONCURRENCY", "1") != "1":
    raise ValueError("El perfil de sesiones en archivos admite WEB_CONCURRENCY=1. No usar réplicas con directorios independientes.")

bind = f"0.0.0.0:{port}"
workers = 1
worker_class = "gthread"
threads = 4
timeout = 60
graceful_timeout = 30
accesslog = "-"
errorlog = "-"
# No registrar query strings, headers, cuerpos, credenciales ni tokens.
access_log_format = '%(m)s %(U)s %(s)s %(L)s'
forwarded_allow_ips = ""
preload_app = False
