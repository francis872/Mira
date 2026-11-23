"""Presentación académica y retorno contextual; sin acceso directo a base de datos."""
from urllib.parse import urlsplit
import re
from flask import request
from markupsafe import Markup, escape

CATALOG_PATHS = ('area_conocimiento','objetivo_desarrollo_sostenible','area_aplicacion','termino_clave','universidad','linea_investigacion')


def safe_return(default=None):
    value = request.form.get('return_to') or request.args.get('return_to')
    if not value or '\\' in value or any(ord(c)<32 for c in value):
        return default
    parsed = urlsplit(value)
    if parsed.scheme or parsed.netloc or not parsed.path.startswith('/') or parsed.path.startswith('//'):
        return default
    parts = parsed.path.strip('/').split('/')
    valid = parts[0] in CATALOG_PATHS and (len(parts)==1 or len(parts)>=3 and parts[1]=='detalles')
    if parts[0]=='investigacion':
        valid = not any(p in ('nuevo','editar','retirar','reactivar') for p in parts)
    return value if valid else default


def return_input():
    target = safe_return()
    return Markup('<input type="hidden" name="return_to" value="{}">').format(escape(target)) if target else Markup('')


def reference_maps(spec, references):
    return {f['name']:{row['id']:str(row.get('nombre') or 'Referencia sin descripción')+(' (inactiva)' if row.get('activo') is False else '') for row in references.get(f['source'],[]) if isinstance(row,dict) and 'id' in row} for f in spec['fields'] if f['source']}


def field_value(row, field, maps):
    value = row.get(field['name'])
    if value is None or value=='': return '—'
    if field.get('source'):
        names = maps.get(field['name'],{})
        def resolve(key):
            joined = {'usuario_id':'usuario_nombre','sede_id':'sede_nombre','grupo_id':'grupo_etiqueta','docente_id':'docente_nombre','semillero_id':'semillero_nombre','linea_id':'linea_nombre','disciplina_id':'disciplina_nombre','ods_id':'ods_nombre','aplicacion_id':'aplicacion_nombre'}
            return names.get(key) or names.get(str(key)) or (row.get(joined.get(field['name'],'')) if not field.get('multiple') else None) or f'Referencia no disponible ({key})'
        return '; '.join(resolve(key) for key in value) if isinstance(value,(list,tuple)) else resolve(value)
    if isinstance(value,bool):return 'Sí' if value else 'No'
    return str(value)


def data_observation(resource,row):
    # Coincidencia literal con fixtures documentados, no inferencia del actor.
    # No oculta, modifica ni considera inválido el dato institucional original.
    observed=False
    if resource=='sedes' and row.get('ciudad')=='Prueba':
        code=str(row.get('codigo_institucional') or '')
        if code.startswith('BROWSER-') and (row.get('nombre')==code or row.get('nombre')=='Sede actualización de navegador'):
            observed=True
    elif resource=='grupos_investigacion':
        observed=bool(re.fullmatch(r'https://example\.invalid/BROWSER-[0-9a-f]{8}',row.get('gruptac_url') or ''))
    elif resource=='lineas_investigacion':
        code=row.get('codigo') or ''
        observed=bool(re.fullmatch(r'BROWSER-[0-9a-f]{8}',code)) and row.get('nombre')==code and row.get('descripcion')=='Descripción de prueba'
    elif resource=='objetivo_desarrollo_sostenible':
        observed=bool(re.fullmatch(r'e2e-f5-ods-[0-9a-f]{10}',row.get('nombre') or ''))
    if observed:
        return 'Coincide con datos de una prueba documentada. Pendiente de validación institucional.'
    return None
