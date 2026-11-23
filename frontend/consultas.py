"""Consulta explícita de registros y relaciones con los permisos de lectura existentes."""
from urllib.parse import quote
from flask import abort, render_template, request, url_for
from investigacion import RESOURCES, BRIDGES, label
from presentacion import safe_return, reference_maps, field_value, data_observation

CATALOG_DETAILS = {
    'area_conocimiento':('Área de conocimiento',[('granArea','Gran área'),('area','Área'),('disciplina','Disciplina')],'area_conocimiento'),
    'objetivo_desarrollo_sostenible':('Objetivo de Desarrollo Sostenible',[('nombre','Nombre'),('categoria','Categoría')],'ods'),
    'area_aplicacion':('Área de aplicación',[('nombre','Nombre')],'area_aplicacion'),
    'termino_clave':('Término clave',[('termino','Término en español'),('terminoIngles','Equivalente en inglés')],'termino_clave'),
    'universidad':('Universidad',[('nombre','Nombre'),('tipo','Tipo de institución'),('ciudad','Ciudad sede')],'universidad'),
    'linea_investigacion':('Línea del catálogo general',[('nombre','Nombre'),('descripcion','Descripción')],'linea_investigacion'),
}


def record_title(resource,row):
    return row.get('usuario_nombre') or row.get('nombre') or row.get('etiqueta') or ' · '.join(str(row[k]) for k in ('docente_nombre','grupo_etiqueta','semillero_nombre','rol') if row.get(k)) or RESOURCES.get(resource,BRIDGES.get(resource,{})).get('title','Detalle del registro')


def register_consultas(app,api_call,error_message):
    app.jinja_env.globals.update(valor_campo=field_value,observacion_dato=data_observation,titulo_registro=record_title)

    def get(path):
        response=api_call('GET',path,expect_list=False)
        if response is None:
            return None,(render_template('error.html',status=503,message='No fue posible cargar el registro y sus relaciones. Intente de nuevo.'),503)
        if response.status_code!=200:
            return None,(render_template('error.html',status=response.status_code,message=error_message(response,'El registro no está disponible.')),response.status_code)
        return response.json(),None

    def details_url(resource,row):
        if resource in RESOURCES:return url_for('research_detail',resource=resource,record_id=row['id'])
        spec=BRIDGES[resource]
        return url_for('association_detail',resource=resource,left=row[spec['fields'][0]['name']],right=row[spec['fields'][1]['name']])
    app.jinja_env.globals['detalle_url']=details_url

    @app.get('/<catalog>/detalles/<path:record_key>')
    def catalog_detail(catalog,record_key):
        spec=CATALOG_DETAILS.get(catalog)
        if not spec:abort(404)
        if catalog!='termino_clave' and (not record_key.isdecimal() or int(record_key)<1):abort(404)
        item,error=get(catalog+'/'+quote(record_key,safe=''))
        if error:return error
        if not isinstance(item,dict):abort(502)
        title,fields,prefix=spec
        key={'termino':record_key} if catalog=='termino_clave' else {'id':int(record_key)}
        back=safe_return(url_for(prefix+'_list'))
        return render_template('consultas/catalogo.html',catalog=catalog,item=item,fields=fields,title=title,back_url=back,edit_url=url_for(prefix+'_edit',**key,return_to=request.full_path.rstrip('?')),remove_url=url_for(prefix+'_delete',**key),page_title=title)

    def render_research(resource,item,relations,references,back):
        spec=RESOURCES.get(resource) or BRIDGES.get(resource)
        if not spec:abort(404)
        sections=[]
        for linked,rows in relations.items():
            linked_spec=RESOURCES.get(linked) or BRIDGES.get(linked)
            if not linked_spec:continue
            parent_field=next((f['name'] for f in linked_spec['fields'] if f['source']==resource and not f['multiple']),None)
            args={parent_field:item['id']} if parent_field and 'id' in item else {}
            args['return_to']=request.full_path.rstrip('?')
            add_url=url_for('investigacion.associations',resource=linked,**args) if linked in BRIDGES else url_for('investigacion.new',resource=linked,**args)
            sections.append(dict(resource=linked,spec=linked_spec,items=rows,maps=reference_maps(linked_spec,references),add_url=add_url))
        return render_template('consultas/investigacion.html',resource=resource,spec=spec,item=item,sections=sections,maps=reference_maps(spec,references),references=references,back_url=back,page_title=spec['title'])

    @app.get('/investigacion/<resource>/detalles/<int:record_id>')
    def research_detail(resource,record_id):
        if resource not in RESOURCES:abort(404)
        body,error=get(f'investigacion/{resource}/{record_id}/detalle')
        if error:return error
        return render_research(resource,body['registro'],body.get('relaciones',{}),body.get('referencias',{}),safe_return(url_for('investigacion.listing',resource=resource)))

    @app.get('/investigacion/asociaciones/<resource>/detalles/<int:left>/<int:right>')
    def association_detail(resource,left,right):
        if resource not in BRIDGES:abort(404)
        item,error=get(f'investigacion/{resource}/{left}/{right}')
        if error:return error
        refs,error=get('investigacion/referencias/catalogos')
        if error:return error
        return render_research(resource,item,{},refs,safe_return(url_for('investigacion.associations',resource=resource)))

    @app.after_request
    def restore_return_context(response):
        endpoint=request.endpoint or ''
        mutation=endpoint in ('investigacion.new','investigacion.edit','investigacion.remove','investigacion.associations','investigacion.remove_association') or endpoint.endswith(('_create','_update','_delete'))
        target=safe_return()
        if request.method=='POST' and mutation and response.status_code==302 and target:
            response.headers['Location']=target
        return response
