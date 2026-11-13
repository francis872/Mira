FROM python:3.11-slim

WORKDIR /app

ENV PYTHONUNBUFFERED=1 \
    PYTHONDONTWRITEBYTECODE=1 \
    MIRA_ENV=production \
    SESSION_COOKIE_SECURE=true \
    SESSION_DIRECTORY=/var/lib/mira/sessions \
    PORT=5000

COPY requirements.txt .
COPY requirements.lock .
RUN pip install --no-cache-dir -r requirements.lock \
    && python -m pip check \
    && python -m pip uninstall --yes setuptools wheel pip

RUN useradd --uid 10001 --create-home --shell /usr/sbin/nologin mira \
    && mkdir -p /var/lib/mira/sessions \
    && chown mira:mira /var/lib/mira/sessions \
    && chmod 700 /var/lib/mira/sessions

COPY app.py investigacion.py reportes.py seguridad.py session_store.py login_protection.py observabilidad.py gunicorn.conf.py ./
COPY templates/ templates/
COPY static/ static/

EXPOSE 5000

USER mira
CMD ["gunicorn", "--config", "gunicorn.conf.py", "app:app"]

