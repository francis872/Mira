"""Request diagnostics without bodies, query strings, cookies or credentials."""
import json
import time
import uuid
from flask import g, request


def configure_observability(app):
    original = app.wsgi_app

    def observe(environ, start_response):
        started = time.monotonic()
        correlation = uuid.uuid4().hex
        environ['mira.correlation_id'] = correlation

        def completed(status, headers, exc_info=None):
            app.logger.info(json.dumps(dict(event='http_request', correlation_id=correlation,
                method=environ.get('REQUEST_METHOD'), endpoint=environ.get('mira.endpoint'),
                status=int(status.split()[0]), elapsed_ms=round((time.monotonic()-started)*1000,2)), separators=(',', ':')))
            return start_response(status, headers, exc_info)
        return original(environ, completed)

    app.wsgi_app = observe

    @app.before_request
    def begin_request():
        g.correlation_id = request.environ.get('mira.correlation_id', uuid.uuid4().hex)
        request.environ['mira.endpoint'] = request.endpoint

    @app.after_request
    def record_request(response):
        correlation = getattr(g, 'correlation_id', uuid.uuid4().hex)
        response.headers['X-Correlation-ID'] = correlation
        return response
