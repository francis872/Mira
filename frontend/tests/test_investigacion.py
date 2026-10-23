from test_app import client, calls, FakeResponse
from investigacion import RESOURCES, BRIDGES
import pytest


@pytest.mark.parametrize('resource',list(RESOURCES))
def test_lists_and_forms_open_with_real_api_option_requests(client,calls,resource):
    assert client.get('/investigacion/'+resource).status_code == 200
    form=client.get('/investigacion/'+resource+'/nuevo')
    assert form.status_code == 200
    text=form.get_data(as_text=True)
    for field in RESOURCES[resource]['fields']:
        if field['source']:
            assert f'name="{field["name"]}"' in text
            assert f'type="number" name="{field["name"]}"' not in text
            assert any(call['url'].endswith('/investigacion/'+field['source']) for call in calls)


@pytest.mark.parametrize('resource',list(BRIDGES))
def test_bridge_lists_and_api_options(client,calls,resource):
    assert client.get('/investigacion/asociaciones/'+resource).status_code == 200
    assert any(call['url'].endswith('/investigacion/'+resource) for call in calls)


def test_group_sends_master_and_all_details_in_one_request_without_parent_fields(client,calls):
    calls.responses['POST','investigacion/grupos/ficha']=FakeResponse(201,{'grupo':{'id':7}})
    response=client.post('/investigacion/grupos_investigacion/nuevo',data={
        'sede_id':'3','gruptac_url':'https://example.org/grupo','categoria':'',
        'detalle_id':['',''],'detalle_nombre':['Primero','Segundo'],'detalle_descripcion':['','Descripción']})
    assert response.status_code == 302
    writes=[call for call in calls if call['method'] != 'GET']
    assert len(writes)==1
    assert writes[0]['url'].endswith('/investigacion/grupos/ficha')
    assert writes[0]['json']=={'grupo':{'sede_id':3,'gruptac_url':'https://example.org/grupo','categoria':None},
        'semilleros':[{'nombre':'Primero','descripcion':None},{'nombre':'Segundo','descripcion':'Descripción'}]}


def test_reference_failure_disables_save_and_preserves_error_data(client,calls):
    calls.responses['GET','investigacion/sedes']=FakeResponse(500,{'mensaje':'No disponible'})
    response=client.get('/investigacion/grupos_investigacion/nuevo')
    text=response.get_data(as_text=True)
    assert 'No fue posible cargar una lista' in text
    assert 'type="submit" disabled' in text


def test_duplicate_association_preserves_selection_and_is_escaped(client,calls):
    calls.responses['GET','investigacion/grupos_investigacion']=FakeResponse(200,[{'id':1,'etiqueta':'<script>bad</script>'}])
    calls.responses['GET','investigacion/lineas_investigacion']=FakeResponse(200,[{'id':2,'nombre':'Línea real'}])
    calls.responses['POST','investigacion/grupo_lineas']=FakeResponse(409,{'mensaje':'Ya existe un registro con esos datos.'})
    response=client.post('/investigacion/asociaciones/grupo_lineas',data={'grupo_id':'1','linea_id':'2'})
    assert response.status_code==409
    text=response.get_data(as_text=True)
    assert 'Ya existe' in text
    assert '<script>bad</script>' not in text
    assert '&lt;script&gt;bad&lt;/script&gt;' in text
    assert '<option value="1" selected>' in text


def test_group_rejects_misaligned_detail_collections_before_calling_api(client,calls):
    response=client.post('/investigacion/grupos_investigacion/nuevo',data={'detalle_nombre':['Uno'],'detalle_id':[]})
    assert response.status_code==400
    assert not calls


def test_research_does_not_add_authentication_or_writable_users(client,calls):
    assert client.get('/investigacion/usuarios').status_code==404
    assert client.get('/investigacion/usuarios/nuevo').status_code==404
    response=client.get('/investigacion/docentes/nuevo')
    assert 'No hay opciones disponibles' in response.get_data(as_text=True)
    assert 'password' not in response.get_data(as_text=True).lower()
