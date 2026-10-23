"""Forms for the confirmed research model. All foreign keys use real API options."""
from flask import Blueprint, abort, flash, redirect, render_template, request, url_for


def field(name, label, kind='text', maximum=None, optional=False, source=None, options=None):
    return dict(name=name, label=label, kind=kind, maximum=maximum, optional=optional, source=source, options=options)


RESOURCES = {
    'sedes': dict(title='Sedes', fields=[field('nombre','Nombre',maximum=120), field('ciudad','Ciudad',maximum=120)]),
    'docentes': dict(title='Docentes', fields=[field('usuario_id','Persona',source='referencias/usuarios'), field('cvlac_url','Enlace CvLAC','url',500,True), field('escalafon','Escalafón',maximum=100,optional=True), field('minciencias_categoria','Categoría Minciencias',optional=True,options=['Senior','Asociado','Junior','Sin Categoría'])]),
    'grupos_investigacion': dict(title='Grupos de investigación', fields=[field('sede_id','Sede',source='sedes'), field('gruptac_url','Enlace GrupLAC','url',500), field('categoria','Categoría',maximum=100,optional=True)]),
    'semilleros': dict(title='Semilleros', fields=[field('grupo_id','Grupo de investigación',source='grupos_investigacion'), field('nombre','Nombre',maximum=200), field('descripcion','Descripción','textarea',optional=True)]),
    'lineas_investigacion': dict(title='Líneas académicas', fields=[field('codigo','Código',maximum=50), field('nombre','Nombre',maximum=200), field('descripcion','Descripción','textarea',optional=True)]),
    'grupo_docentes': dict(title='Docentes por grupo', fields=[field('grupo_id','Grupo',source='grupos_investigacion'), field('docente_id','Docente',source='docentes'), field('rol','Rol',options=['Líder de Grupo','Coinvestigador','Colaborador']), field('fecha_inicio','Fecha de inicio','date'), field('fecha_fin','Fecha de finalización','date',optional=True)]),
    'semillero_tutores': dict(title='Tutores de semilleros', fields=[field('semillero_id','Semillero',source='semilleros'), field('docente_id','Docente',source='docentes'), field('rol','Rol',options=['Director','Tutor','Coinvestigador']), field('fecha_inicio','Fecha de inicio','date'), field('fecha_fin','Fecha de finalización','date',optional=True)]),
    **{name: dict(title=title, fields=[field('nombre','Nombre',maximum=160)]) for name,title in (
        ('areas_conocimiento','Disciplinas de líneas'), ('ods','ODS de líneas'), ('areas_aplicacion','Aplicaciones de líneas'), ('palabras_clave','Palabras del modelo académico'))},
}
BRIDGES = {
    'grupo_lineas': dict(title='Líneas de grupos', fields=[field('grupo_id','Grupo',source='grupos_investigacion'),field('linea_id','Línea',source='lineas_investigacion')]),
    'semillero_lineas': dict(title='Líneas de semilleros',fields=[field('semillero_id','Semillero',source='semilleros'),field('linea_id','Línea',source='lineas_investigacion')]),
    'linea_disciplinas': dict(title='Disciplinas por línea',fields=[field('linea_id','Línea',source='lineas_investigacion'),field('disciplina_id','Disciplina',source='areas_conocimiento')]),
    'linea_ods': dict(title='ODS por línea',fields=[field('linea_id','Línea',source='lineas_investigacion'),field('ods_id','ODS',source='ods')]),
    'linea_aplicacion': dict(title='Aplicaciones por línea',fields=[field('linea_id','Línea',source='lineas_investigacion'),field('aplicacion_id','Aplicación',source='areas_aplicacion')]),
}


def label(item):
    return item.get('usuario_nombre') or item.get('etiqueta') or item.get('nombre') or f"Registro #{item['id']}"


def register_investigacion(app, api_call, error_message):
    bp = Blueprint('investigacion', __name__, url_prefix='/investigacion')

    def definition(resource, bridge=False):
        result = (BRIDGES if bridge else RESOURCES).get(resource)
        if result is None:
            abort(404)
        return result

    def call(method, path, data=None, listed=False):
        return api_call(method, f'investigacion/{path}', data, expect_list=listed)

    def load(path):
        response = call('GET',path,listed=True)
        if response is None or response.status_code != 200:
            flash('No fue posible cargar una lista de referencias. Recargue la página antes de guardar.','danger')
            return [], False
        return response.json(), True

    def options_for(spec):
        options = {}
        ok = True
        for f in spec['fields']:
            if f['source']:
                rows, loaded = load(f['source'])
                options[f['name']] = [(row['id'], label(row)) for row in rows]
                ok = ok and loaded
        return options, ok

    def payload(spec):
        data = {}
        for f in spec['fields']:
            value = request.form.get(f['name'],'').strip()
            if f['source']:
                try:
                    data[f['name']] = int(value) if value else None
                except ValueError:
                    data[f['name']] = value  # The API returns a field validation error.
            else:
                data[f['name']] = value or None
        return data

    def render_form(resource, spec, item=None, status=200):
        options, loaded = options_for(spec)
        return render_template('investigacion/form.html',resource=resource,spec=spec,item=item or {},options=options,loaded=loaded,page_title=spec['title']),status

    @bp.get('')
    def home():
        return render_template('investigacion/home.html', resources=RESOURCES, bridges=BRIDGES,page_title='Investigación')

    @bp.get('/<resource>')
    def listing(resource):
        spec = definition(resource)
        rows, loaded = load(resource)
        options, references_ok = options_for(spec)
        maps = {key: dict(values) for key,values in options.items()}
        return render_template('investigacion/list.html',resource=resource,spec=spec,items=rows,maps=maps,loaded=loaded and references_ok,page_title=spec['title'])

    @bp.route('/<resource>/nuevo',methods=['GET','POST'])
    def new(resource):
        spec = definition(resource)
        if resource == 'grupos_investigacion':
            return group_form(None)
        if request.method == 'POST':
            data = payload(spec)
            response = call('POST',resource,data)
            if response is not None and response.status_code == 201:
                flash('Registro creado correctamente.','success')
                return redirect(url_for('investigacion.listing',resource=resource))
            flash(error_message(response,'No fue posible crear el registro.'),'danger')
            return render_form(resource,spec,data,400 if response is None else response.status_code)
        return render_form(resource,spec)

    @bp.route('/<resource>/editar/<int:record_id>',methods=['GET','POST'])
    def edit(resource,record_id):
        spec = definition(resource)
        if resource == 'grupos_investigacion':
            return group_form(record_id)
        if request.method == 'POST':
            data = payload(spec)
            response = call('PUT',f'{resource}/{record_id}',data)
            if response is not None and response.status_code == 200:
                flash('Registro actualizado correctamente.','success')
                return redirect(url_for('investigacion.listing',resource=resource))
            flash(error_message(response,'No fue posible actualizar el registro.'),'danger')
            return render_form(resource,spec,dict(data,id=record_id),400 if response is None else response.status_code)
        response = call('GET',f'{resource}/{record_id}')
        if response is None or response.status_code != 200:
            flash('Registro no encontrado o no disponible.','warning')
            return redirect(url_for('investigacion.listing',resource=resource))
        return render_form(resource,spec,response.json())

    @bp.post('/<resource>/retirar/<int:record_id>')
    def remove(resource,record_id):
        definition(resource)
        response = call('DELETE',f'{resource}/{record_id}')
        success = response is not None and response.status_code == 200
        flash('Registro retirado correctamente.' if success else error_message(response,'No fue posible retirar el registro.'),'success' if success else 'danger')
        return redirect(url_for('investigacion.listing',resource=resource))

    def group_form(record_id):
        spec = RESOURCES['grupos_investigacion']
        data = dict(grupo={},semilleros=[])
        status = 200
        if request.method == 'POST':
            names = request.form.getlist('detalle_nombre')
            descriptions = request.form.getlist('detalle_descripcion')
            ids = request.form.getlist('detalle_id')
            if not len(names) == len(descriptions) == len(ids) or len(names)>200:
                flash('La lista de semilleros no es válida.','danger')
                return render_template('error.html',status=400,message='Revise los renglones de semilleros.'),400
            details = []
            for key,name,description in zip(ids,names,descriptions):
                row = dict(nombre=name.strip(),descripcion=description.strip() or None)
                if key:
                    try:
                        row['id'] = int(key)
                    except ValueError:
                        row['id'] = key
                details.append(row)
            data = dict(grupo=payload(spec),semilleros=details)
            path = f'grupos/ficha/{record_id}' if record_id else 'grupos/ficha'
            response = call('PUT' if record_id else 'POST',path,data)
            if response is not None and response.status_code in (200,201):
                flash('Grupo y semilleros guardados correctamente.','success')
                return redirect(url_for('investigacion.edit',resource='grupos_investigacion',record_id=response.json()['grupo']['id']))
            flash(error_message(response,'No fue posible guardar la ficha. No se guardó ningún cambio.'),'danger')
            status = 400 if response is None else response.status_code
        elif record_id:
            response = call('GET',f'grupos/ficha/{record_id}')
            if response is None or response.status_code != 200:
                flash('Grupo no encontrado o no disponible.','warning')
                return redirect(url_for('investigacion.listing',resource='grupos_investigacion'))
            data = response.json()
        options, loaded = options_for(spec)
        return render_template('investigacion/grupo.html',data=data,spec=spec,options=options,loaded=loaded,record_id=record_id,page_title='Ficha de grupo'),status

    @bp.route('/asociaciones/<resource>',methods=['GET','POST'])
    def associations(resource):
        spec = definition(resource,True)
        status = 200
        data = {}
        if request.method == 'POST':
            data = payload(spec)
            response = call('POST',resource,data)
            if response is not None and response.status_code == 201:
                flash('Asociación asignada correctamente.','success')
                return redirect(url_for('investigacion.associations',resource=resource))
            flash(error_message(response,'No fue posible asignar la asociación.'),'danger')
            status = 400 if response is None else response.status_code
        options, loaded = options_for(spec)
        rows, rows_ok = load(resource)
        return render_template('investigacion/asociaciones.html',resource=resource,spec=spec,item=data,items=rows,options=options,maps={key:dict(value) for key,value in options.items()},loaded=loaded and rows_ok,page_title=spec['title']),status

    @bp.post('/asociaciones/<resource>/retirar/<int:left>/<int:right>')
    def remove_association(resource,left,right):
        definition(resource,True)
        response = call('DELETE',f'{resource}/{left}/{right}')
        success = response is not None and response.status_code == 200
        flash('Asociación retirada correctamente.' if success else error_message(response,'No fue posible retirar la asociación.'),'success' if success else 'danger')
        return redirect(url_for('investigacion.associations',resource=resource))

    app.register_blueprint(bp)
