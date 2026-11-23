"""Shared server-side sessions; no cookie JWT, pickle, filesystem or runtime DDL."""
import hashlib
import hmac
import json
import re

import certifi
import psycopg
from cachelib import BaseCache
from flask import render_template, request
from flask_session.cachelib import CacheLibSessionInterface
from psycopg.conninfo import conninfo_to_dict


class SessionStoreUnavailable(RuntimeError):
    pass


class PostgresSessionCache(BaseCache):
    def __init__(self, conninfo, secret, production=False):
        super().__init__(default_timeout=3600)
        if not conninfo or len(secret.encode()) < 32:
            raise RuntimeError('Configure conexión y clave externa para las sesiones compartidas.')
        try:
            settings = conninfo_to_dict(conninfo)
        except psycopg.Error:
            raise RuntimeError('La configuración de sesiones PostgreSQL no es válida.') from None
        if production and (settings.get('sslmode') != 'verify-full' or settings.get('channel_binding') != 'require'):
            raise RuntimeError('Sesiones de producción requieren sslmode=verify-full y channel_binding=require.')
        self._conninfo = conninfo
        self._production = production
        self._signing_key = hmac.digest(secret.encode(), b'mira-web-session-storage-v1', 'sha256')

    def _connect(self):
        options = dict(connect_timeout=5, application_name='mira-flask-session',
                       options='-c statement_timeout=5000 -c lock_timeout=3000')
        if self._production:
            options['sslrootcert'] = certifi.where()
        return psycopg.connect(self._conninfo, **options)

    @staticmethod
    def _key(key):
        if not isinstance(key, str) or not re.fullmatch(r'session:[A-Za-z0-9_-]{43}', key):
            return None
        return hashlib.sha256(key.encode()).hexdigest()

    def get(self, key):
        digest = self._key(key)
        if digest is None:
            return None
        try:
            with self._connect() as db:
                row = db.execute('SELECT datos,firma FROM mira_web.sesion WHERE clave_hash=%s AND expira_en>CURRENT_TIMESTAMP', (digest,)).fetchone()
        except psycopg.Error:
            raise SessionStoreUnavailable('No fue posible consultar la sesión.') from None
        if row is None:
            return None
        data, signature = bytes(row[0]), bytes(row[1])
        if len(data) > 65536 or not hmac.compare_digest(signature, hmac.digest(self._signing_key, digest.encode()+data, 'sha256')):
            return None
        try:
            value = json.loads(data)
        except (ValueError, UnicodeError):
            return None
        return value if isinstance(value, dict) else None

    def set(self, key, value, timeout=None):
        digest = self._key(key)
        if digest is None or not isinstance(value, dict):
            raise ValueError('Identificador o contenido de sesión inválido.')
        data = json.dumps(value, ensure_ascii=False, allow_nan=False, separators=(',', ':')).encode()
        if len(data) > 65536:
            raise SessionStoreUnavailable('La sesión supera el tamaño permitido.')
        ttl = min(int(timeout if timeout is not None else self.default_timeout), 3600)
        if ttl <= 0:
            return self.delete(key)
        signature = hmac.digest(self._signing_key, digest.encode()+data, 'sha256')
        try:
            with self._connect() as db:
                db.execute('''INSERT INTO mira_web.sesion(clave_hash,datos,firma,expira_en)
                    VALUES(%s,%s,%s,CURRENT_TIMESTAMP+make_interval(secs=>%s))
                    ON CONFLICT(clave_hash) DO UPDATE SET datos=EXCLUDED.datos,firma=EXCLUDED.firma,expira_en=EXCLUDED.expira_en''',
                           (digest, data, signature, ttl))
                db.execute('DELETE FROM mira_web.sesion WHERE clave_hash IN (SELECT clave_hash FROM mira_web.sesion WHERE expira_en<=CURRENT_TIMESTAMP LIMIT 100)')
        except psycopg.Error:
            raise SessionStoreUnavailable('No fue posible guardar la sesión.') from None
        return True

    def delete(self, key):
        digest = self._key(key)
        if digest is None:
            return True
        try:
            with self._connect() as db:
                db.execute('DELETE FROM mira_web.sesion WHERE clave_hash=%s', (digest,))
        except psycopg.Error:
            raise SessionStoreUnavailable('No fue posible retirar la sesión.') from None
        return True

    def check(self):
        try:
            with self._connect() as db:
                db.execute('SELECT 1 FROM mira_web.sesion LIMIT 0')
        except psycopg.Error:
            raise SessionStoreUnavailable('Almacenamiento de sesiones no disponible.') from None
        return True


class SharedSessionInterface(CacheLibSessionInterface):
    def open_session(self, app, request):
        try:
            return super().open_session(app, request)
        except SessionStoreUnavailable:
            app.logger.warning('session_store_unavailable operation=read')
            result = self.session_class(sid=self._generate_sid(self.sid_length), permanent=False)
            result.store_unavailable = True
            return result

    def save_session(self, app, session, response):
        if request.endpoint == 'health' and getattr(session, 'store_unavailable', False):
            return
        try:
            if getattr(session, 'store_unavailable', False):
                raise SessionStoreUnavailable()
            super().save_session(app, session, response)
        except SessionStoreUnavailable:
            app.logger.warning('session_store_unavailable operation=write')
            # Preserve the browser's previous identifier for recovery; never
            # claim login/logout succeeded or fall back to a cookie/local store.
            response.status_code = 503
            response.set_data(render_template('error.html', status=503, message='El servicio de sesiones no está disponible. Intente de nuevo.'))
            response.content_type = 'text/html; charset=utf-8'
            response.headers.pop('Set-Cookie', None)
            response.headers['Cache-Control'] = 'no-store'
