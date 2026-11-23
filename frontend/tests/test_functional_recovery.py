from io import BytesIO
from openpyxl import load_workbook
import pytest
from test_app import client, calls, FakeResponse
from investigacion import RESOURCES, BRIDGES
from consultas import CATALOG_DETAILS
from presentacion import data_observation


def test_observations_match_documented_fixtures_without_guessing_from_generic_words():
    assert data_observation('sedes',dict(nombre='BROWSER-1333e771',ciudad='Prueba',codigo_institucional='BROWSER-1333e771'))
    assert data_observation('grupos_investigacion',dict(gruptac_url='https://example.invalid/BROWSER-1333e771'))
    assert data_observation('objetivo_desarrollo_sostenible',dict(nombre='e2e-f5-ods-a86c31e0f4'))
    assert not data_observation('sedes',dict(nombre='Laboratorio de prueba de hipótesis',ciudad='Medellín',codigo_institucional='INST-1'))


def reader(client):
    with client.session_transaction() as state:
        state['user']={**state['user'],'roles':['Investigador']}


@pytest.mark.parametrize('catalog', CATALOG_DETAILS)
def test_catalog_details_are_readable_without_write_access(client,calls,catalog):
    reader(client)
    title,fields,_=CATALOG_DETAILS[catalog]
    row={key:'Texto académico' for key,_ in fields}
    row.update(id=1,activo=True,termino='editar')
    key='editar' if catalog=='termino_clave' else '1'
    calls.responses['GET',catalog+'/'+key]=FakeResponse(200,row)
    response=client.get('/'+catalog+'/detalles/'+key)
    assert response.status_code==200
    html=response.get_data(as_text=True)
    assert 'Texto académico' in html and 'Volver al listado' in html
    assert '>Editar</a>' not in html and 'Confirmar retiro' not in html
    assert client.head('/'+catalog+'/detalles/'+key).status_code==200


@pytest.mark.parametrize('resource',RESOURCES)
def test_research_details_use_descriptive_references_for_reader(client,calls,resource):
    reader(client)
    spec=RESOURCES[resource]
    row={'id':8,'activo':True}
    refs={}
    for f in spec['fields']:
        row[f['name']]=[7] if f['multiple'] else 7 if f['source'] else 'Dato académico'
        if f['source']:refs[f['source']]=[dict(id=7,nombre='Nombre asociado verificable',activo=False)]
    calls.responses['GET',f'investigacion/{resource}/8/detalle']=FakeResponse(200,dict(registro=row,relaciones={},referencias=refs))
    response=client.get(f'/investigacion/{resource}/detalles/8')
    assert response.status_code==200
    html=response.get_data(as_text=True)
    if refs:assert 'Nombre asociado verificable (inactiva)' in html
    assert 'Confirmar retiro' not in html and 'Editar grupo' not in html


@pytest.mark.parametrize('resource',BRIDGES)
def test_association_details_exist_for_readers(client,calls,resource):
    reader(client)
    row={f['name']:7 for f in BRIDGES[resource]['fields']}
    refs={f['source']:[dict(id=7,nombre='Referencia académica')] for f in BRIDGES[resource]['fields']}
    calls.responses['GET',f'investigacion/{resource}/7/7']=FakeResponse(200,row)
    calls.responses['GET','investigacion/referencias/catalogos']=FakeResponse(200,refs)
    response=client.get(f'/investigacion/asociaciones/{resource}/detalles/7/7')
    assert response.status_code==200
    assert 'Referencia académica' in response.get_data(as_text=True)


@pytest.mark.parametrize('target',['https://evil.invalid','//evil.invalid','/usuarios','/investigacion/sedes/nuevo','/\\evil.invalid'])
def test_post_return_cannot_redirect_to_external_or_unsafe_operation(client,calls,target):
    calls.responses['POST','universidad']=FakeResponse(201,{'id':1})
    response=client.post('/universidad/crear',data={'nombre':'U','tipo':'Privada','ciudad':'Ciudad','return_to':target})
    assert response.status_code==302 and response.headers['Location']=='/universidad'


def test_post_keeps_search_and_parent_context(client,calls):
    calls.responses['POST','universidad']=FakeResponse(201,{'id':1})
    response=client.post('/universidad/crear',data={'nombre':'U','tipo':'Privada','ciudad':'Ciudad','return_to':'/universidad?q=academia&page=2'})
    assert response.headers['Location']=='/universidad?q=academia&page=2'


def test_group_details_expose_actual_children_and_contextual_create(client,calls):
    body=dict(registro=dict(id=8,sede_id=3,gruptac_url='https://example.invalid/grupo',activo=True),relaciones=dict(semilleros=[dict(id=9,grupo_id=8,nombre='Semillero asociado',activo=True)]),referencias=dict(sedes=[dict(id=3,nombre='Sede académica')],grupos_investigacion=[dict(id=8,nombre='Grupo académico')]))
    calls.responses['GET','investigacion/grupos_investigacion/8/detalle']=FakeResponse(200,body)
    response=client.get('/investigacion/grupos_investigacion/detalles/8')
    html=response.get_data(as_text=True)
    assert response.status_code==200 and 'Semillero asociado' in html and 'Sede académica' in html
    assert '/investigacion/semilleros/nuevo?grupo_id=8' in html
    assert '/investigacion/semilleros/detalles/9' in html


def test_missing_detail_and_unavailable_backend_are_not_empty_records(client,calls):
    calls.responses['GET','investigacion/sedes/8/detalle']=FakeResponse(404,{'mensaje':'No encontrado'})
    assert client.get('/investigacion/sedes/detalles/8').status_code==404
    calls.responses['GET','investigacion/sedes/8/detalle']=FakeResponse(503,{'mensaje':'No disponible'})
    assert client.get('/investigacion/sedes/detalles/8').status_code==503


def test_export_search_matches_displayed_academic_columns(client,calls):
    rows=[dict(sede='Sede Ágora',grupo='Grupo académico',grupo_activo=True),dict(sede='Otra sede',grupo='Otro grupo',grupo_activo=False)]
    calls.responses['GET','investigacion/reportes/articulacion']=FakeResponse(200,rows)
    response=client.get('/investigacion/reportes/articulacion.xlsx?q=agora&page=9')
    assert response.status_code==200
    values=list(load_workbook(BytesIO(response.data)).active.values)
    assert len(values)==2 and values[1][0]=='Sede Ágora'
    # The page retains all rows, so clearing client search restores the complete report.
    calls.responses['GET','investigacion/reportes/opciones']=FakeResponse(200,{'sedes':[],'lineas':[]})
    html=client.get('/investigacion/reportes?q=agora').get_data(as_text=True)
    assert 'Sede Ágora' in html and 'Otra sede' in html
