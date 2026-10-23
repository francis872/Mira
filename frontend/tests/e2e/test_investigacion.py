"""Actual browser HTTP -> Flask -> API -> database research workflows."""
import html
import re
import uuid
import requests
import pytest
from test_local_platform import BASE, API, Browser

pytestmark=pytest.mark.skipif(not BASE or not API,reason='Configure las URL E2E de la plataforma.')


def api(method,path,payload=None):
    return requests.request(method,f'{API}/api/investigacion/{path}',json=payload,timeout=30)


@pytest.fixture
def records():
    created=[]
    def create(resource,payload):
        response=api('POST',resource,payload)
        assert response.status_code==201,response.text
        row=response.json()
        created.append((resource,row['id']))
        return row['id']
    yield create
    for resource,key in reversed(created):
        api('DELETE',f'{resource}/{key}')


def test_group_sheet_http_creates_edits_and_retires_multiple_details(records):
    marker='e2e-sheet-'+uuid.uuid4().hex[:10]
    sede=records('sedes',{'nombre':marker,'ciudad':'Medellín'})
    browser=Browser()
    path='/investigacion/grupos_investigacion/nuevo'
    form=browser.get(path)
    assert f'<option value="{sede}"' in form.text
    assert marker in form.text
    data={'sede_id':str(sede),'gruptac_url':'https://example.org/'+marker,'categoria':'',
          'detalle_id':['',''],'detalle_nombre':[marker+'-uno',marker+'-dos'],'detalle_descripcion':['','Descripción']}
    response=browser.post(path,data,path)
    assert response.status_code==302,response.text
    edit_path=response.headers['Location']
    key=int(edit_path.rsplit('/',1)[1])
    try:
        sheet=api('GET',f'grupos/ficha/{key}').json()
        assert len(sheet['semilleros'])==2
        assert sheet['grupo']['categoria'] is None
        assert all(row['grupo_id']==key for row in sheet['semilleros'])
        assert sheet['semilleros'][0]['descripcion'] is None
        data.update(detalle_id=[str(sheet['semilleros'][0]['id'])],detalle_nombre=[marker+'-editado'],detalle_descripcion=[''])
        response=browser.post(edit_path,data,edit_path)
        assert response.status_code==302,response.text
        after=api('GET',f'grupos/ficha/{key}').json()
        assert len(after['semilleros'])==1
        assert after['semilleros'][0]['nombre']==marker+'-editado'
        assert marker+'-editado' in Browser().get(edit_path).text
    finally:
        for row in api('GET',f'grupos/ficha/{key}').json().get('semilleros',[]):
            api('DELETE',f'semilleros/{row["id"]}')
        api('DELETE',f'grupos_investigacion/{key}')


@pytest.mark.parametrize('resource',['sedes','lineas_investigacion','areas_conocimiento','ods','areas_aplicacion','palabras_clave'])
def test_research_resource_full_http_form_lifecycle(resource):
    marker='e2e-research-'+uuid.uuid4().hex[:10]
    data={'nombre':marker}
    if resource=='sedes': data['ciudad']='Medellín'
    if resource=='lineas_investigacion': data.update(codigo=marker,descripcion='')
    browser=Browser()
    path=f'/investigacion/{resource}'
    assert browser.post(path+'/nuevo',data,path+'/nuevo').status_code==302
    row=next(row for row in api('GET',resource).json() if row['nombre']==marker)
    key=row['id']
    try:
        edit=f'{path}/editar/{key}'
        assert marker in Browser().get(edit).text
        data['nombre']=marker+'-editado'
        assert browser.post(edit,data,edit).status_code==302
        assert api('GET',f'{resource}/{key}').json()['nombre']==data['nombre']
    finally:
        assert browser.post(f'{path}/retirar/{key}',{},path).status_code==302
    assert api('GET',f'{resource}/{key}').status_code==404


@pytest.mark.parametrize('bridge,left,right',[
    ('grupo_lineas','grupo_id','linea_id'),('semillero_lineas','semillero_id','linea_id'),
    ('linea_disciplinas','linea_id','disciplina_id'),('linea_ods','linea_id','ods_id'),('linea_aplicacion','linea_id','aplicacion_id')])
def test_all_bridge_forms_assign_reject_duplicate_and_remove(records,bridge,left,right):
    marker='e2e-bridge-'+uuid.uuid4().hex[:10]
    sede=records('sedes',{'nombre':marker,'ciudad':'Medellín'})
    group=records('grupos_investigacion',{'sede_id':sede,'gruptac_url':'https://example.org/'+marker})
    semillero=records('semilleros',{'grupo_id':group,'nombre':marker+' semillero'})
    linea=records('lineas_investigacion',{'codigo':marker,'nombre':marker+' línea'})
    target={'grupo_id':group,'semillero_id':semillero,'linea_id':linea,
        'disciplina_id':records('areas_conocimiento',{'nombre':marker+' disciplina'}),
        'ods_id':records('ods',{'nombre':marker+' ods'}),
        'aplicacion_id':records('areas_aplicacion',{'nombre':marker+' aplicación'})}
    data={left:str(target[left]),right:str(target[right])}
    path='/investigacion/asociaciones/'+bridge
    browser=Browser()
    assert browser.post(path,data,path).status_code==302
    try:
        duplicate=browser.post(path,data,path)
        assert duplicate.status_code==409
        assert 'Ya existe' in duplicate.text
        assert marker in Browser().get(path).text
    finally:
        assert browser.post(f'{path}/retirar/{target[left]}/{target[right]}',{},path).status_code==302
    assert api('GET',f'{bridge}/{target[left]}/{target[right]}').status_code==404


def test_docente_and_participation_forms_use_existing_person_and_real_reference_names(records):
    people=api('GET','referencias/usuarios').json()
    if not people: pytest.skip('No hay personas en esta base; no se generan identidades en la plataforma habitual.')
    used={row['usuario_id'] for row in api('GET','docentes').json()}
    person=next((row for row in people if row['id'] not in used),None)
    if not person: pytest.skip('Todas las personas existentes tienen perfil docente.')
    marker='e2e-teacher-'+uuid.uuid4().hex[:10]
    browser=Browser()
    path='/investigacion/docentes/nuevo'
    assert html.escape(person['nombre']) in browser.get(path).text
    assert browser.post(path,{'usuario_id':str(person['id']),'cvlac_url':'','escalafon':'','minciencias_categoria':''},path).status_code==302
    docente=next(row['id'] for row in api('GET','docentes').json() if row['usuario_id']==person['id'])
    try:
        edit_docente=f'/investigacion/docentes/editar/{docente}'
        assert browser.post(edit_docente,{'usuario_id':str(person['id']),'cvlac_url':'https://example.org/cvlac','escalafon':'Asociado','minciencias_categoria':'Junior'},edit_docente).status_code==302
        assert api('GET',f'docentes/{docente}').json()['escalafon']=='Asociado'
        sede=records('sedes',{'nombre':marker,'ciudad':'Medellín'})
        grupo=records('grupos_investigacion',{'sede_id':sede,'gruptac_url':'https://example.org/'+marker})
        semillero=records('semilleros',{'grupo_id':grupo,'nombre':marker})
        for resource,parent,key,rol in [('grupo_docentes','grupo_id',grupo,'Coinvestigador'),('semillero_tutores','semillero_id',semillero,'Tutor')]:
            form=f'/investigacion/{resource}/nuevo'
            data={parent:str(key),'docente_id':str(docente),'rol':rol,'fecha_inicio':'2026-01-01','fecha_fin':''}
            assert browser.post(form,data,form).status_code==302
            row=next(row for row in api('GET',resource).json() if row[parent]==key)
            assert row['fecha_fin'] is None
            edit=f'/investigacion/{resource}/editar/{row["id"]}'
            assert browser.post(edit,data,edit).status_code==302
            assert browser.post(f'/investigacion/{resource}/retirar/{row["id"]}',{},f'/investigacion/{resource}').status_code==302
    finally:
        assert browser.post(f'/investigacion/docentes/retirar/{docente}',{},'/investigacion/docentes').status_code==302


def test_semillero_independent_form_lifecycle_uses_group_selector(records):
    marker='e2e-semillero-'+uuid.uuid4().hex[:10]
    sede=records('sedes',{'nombre':marker,'ciudad':'Medellín'})
    group=records('grupos_investigacion',{'sede_id':sede,'gruptac_url':'https://example.org/'+marker})
    browser=Browser()
    path='/investigacion/semilleros'
    data={'grupo_id':str(group),'nombre':marker,'descripcion':''}
    assert marker in browser.get(path+'/nuevo').text
    assert browser.post(path+'/nuevo',data,path+'/nuevo').status_code==302
    child=next(row['id'] for row in api('GET','semilleros').json() if row['nombre']==marker)
    try:
        edit=f'{path}/editar/{child}'
        data['nombre']=marker+'-editado'
        assert browser.post(edit,data,edit).status_code==302
        assert api('GET',f'semilleros/{child}').json()['nombre']==data['nombre']
    finally:
        assert browser.post(f'{path}/retirar/{child}',{},path).status_code==302


def test_research_csrf_blocks_writes_and_fk_conflict_is_spanish():
    assert Browser().post('/investigacion/sedes/nuevo',{'nombre':'No debe crearse','ciudad':'Ciudad'},'/investigacion/sedes/nuevo',with_token=False).status_code==400
    response=api('POST','grupos_investigacion',{'sede_id':2147483647,'gruptac_url':'https://example.org/invalido'})
    assert response.status_code==409
    assert 'referencias' in response.json()['mensaje']
    assert '23503' not in response.text
