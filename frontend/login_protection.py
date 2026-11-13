"""Authenticated Flask-to-API login origin; trust proxy headers only from explicit peers."""
import hashlib
import hmac
import ipaddress
import os
import time
from urllib.parse import urlsplit

from requests.auth import AuthBase
from flask import current_app, request


def canonical_ip(value):
    if not value or '%' in value:
        raise ValueError('Dirección de origen inválida')
    address = ipaddress.ip_address(value)
    return str(address.ipv4_mapped if isinstance(address, ipaddress.IPv6Address) and address.ipv4_mapped else address)


def configure_login_protection(app):
    platform = os.getenv('MIRA_PLATFORM', '')
    if platform not in ('', 'vercel') or (platform == 'vercel' and os.getenv('VERCEL') != '1'):
        raise RuntimeError('El perfil de proxy Vercel requiere el entorno oficial VERCEL=1.')
    app.config['LOGIN_PLATFORM'] = platform
    key = os.getenv('MIRA_LOGIN_FORWARDING_KEY', '')
    if (os.getenv('MIRA_ENV') == 'production' or key) and (len(key.encode('utf-8')) < 32 or key.startswith('REPLACE')):
        raise RuntimeError('Configure MIRA_LOGIN_FORWARDING_KEY externa, independiente de JWT/SECRET_KEY y de al menos 32 bytes.')
    if key and key == app.secret_key:
        raise RuntimeError('MIRA_LOGIN_FORWARDING_KEY debe ser diferente de SECRET_KEY.')
    networks = []
    for value in os.getenv('MIRA_TRUSTED_PROXY_NETWORKS', '').split(','):
        if value.strip():
            network = ipaddress.ip_network(value.strip(), strict=True)
            if network.prefixlen == 0:
                raise RuntimeError('No se permite confiar en todos los proxies.')
            networks.append(network)
    app.config['LOGIN_FORWARDING_KEY'] = key.encode('utf-8') if key else None
    app.config['LOGIN_TRUSTED_PROXY_NETWORKS'] = tuple(networks)


def client_origin(networks):
    if current_app.config.get('LOGIN_PLATFORM') == 'vercel':
        # Vercel overwrites this header at its managed ingress. This profile is
        # enabled only by server environment, never by an incoming header.
        try:
            return canonical_ip(request.headers.get('X-Vercel-Forwarded-For'))
        except ValueError:
            return None
    # Neither X-Real-IP nor arbitrary X-Forwarded-For override an untrusted socket peer.
    try:
        peer = canonical_ip(request.remote_addr)
    except ValueError:
        return None

    def trusted(value):
        address = ipaddress.ip_address(value)
        return any(address in network for network in networks)

    if not trusted(peer):
        return peer
    header = request.headers.get('X-Forwarded-For', '')
    if not header or len(header) > 1024:
        return peer
    parts = header.split(',')
    if len(parts) > 16:
        return peer
    try:
        chain = [canonical_ip(part.strip()) for part in parts]
    except ValueError:
        return peer
    for address in reversed(chain):
        if not trusted(peer):
            break
        peer = address
    return peer


class LoginOriginAuth(AuthBase):
    def __init__(self, key, origin):
        self._key, self._origin = key, origin

    def __call__(self, prepared):
        timestamp = str(int(time.time()))
        body = prepared.body or b''
        if isinstance(body, str):
            body = body.encode('utf-8')
        path = urlsplit(prepared.url).path.rstrip('/').lower()
        payload = f'mira-login-v1\n{timestamp}\n{self._origin}\n{prepared.method.upper()}\n{path}\n{hashlib.sha256(body).hexdigest()}'
        signature = hmac.new(self._key, payload.encode('utf-8'), hashlib.sha256).hexdigest()
        # Construct from server state, never copy a browser-supplied assertion.
        prepared.headers['X-Mira-Login-Origin'] = f'v1;{timestamp};{self._origin};{signature}'
        return prepared
