from test_app import client, calls, FakeResponse


def test_multi_line_form_sends_one_atomic_request(client,calls):
    path='investigacion/asociaciones/grupo_lineas/asignar'
    calls.responses['POST',path]=FakeResponse(201,{'asociaciones':[]})
    result=client.post('/investigacion/asociaciones/grupo_lineas',data={'grupo_id':'4','linea_id':['8','9']})
    assert result.status_code==302
    writes=[c for c in calls if c['method']=='POST']
    assert len(writes)==1 and writes[0]['json']=={'padreId':4,'lineas':[8,9]}


def test_line_form_sends_required_ods_and_disciplines_in_one_request(client,calls):
    calls.responses['POST','investigacion/lineas_investigacion']=FakeResponse(201,{'id':4})
    result=client.post('/investigacion/lineas_investigacion/nuevo',data={'codigo':'PRUEBA','nombre':'Línea','descripcion':'Descripción','disciplina_ids':['1','2'],'ods_ids':['3']})
    assert result.status_code==302
    writes=[c for c in calls if c['method']=='POST']
    assert len(writes)==1 and writes[0]['json']=={'codigo':'PRUEBA','nombre':'Línea','descripcion':'Descripción','disciplina_ids':[1,2],'ods_ids':[3]}


def test_line_removal_requires_named_confirmation_dialog(client,calls):
    calls.responses['GET','investigacion/grupo_lineas']=FakeResponse(200,[{'grupo_id':4,'linea_id':8}])
    text=client.get('/investigacion/asociaciones/grupo_lineas').get_data(as_text=True)
    assert 'multiple size="4"' in text
    assert 'data-bs-target="#retirar-asociacion-4-8"' in text
    assert 'aria-labelledby="titulo-retirar-asociacion-4-8"' in text
    assert 'Confirmar retiro' in text


def test_semillero_filters_are_sent_to_api_and_kept_in_form(client,calls):
    response=client.get('/investigacion/semilleros?grupo_id=4&docente_id=7')
    assert response.status_code==200
    assert any(c['url'].endswith('semilleros/buscar?grupoId=4&docenteId=7') for c in calls)
    assert 'name="grupo_id"' in response.get_data(as_text=True)
    assert 'name="docente_id"' in response.get_data(as_text=True)


def test_group_summary_keeps_inactive_line_names_escaped(client,calls):
    calls.responses['GET','investigacion/grupos/ficha/4']=FakeResponse(200,{'grupo':{'id':4,'sede_id':1,'sede_nombre':'Sede histórica','gruptac_url':'https://example.invalid/g'},'semilleros':[]})
    calls.responses['GET','investigacion/asociaciones/grupo_lineas/resumen/4']=FakeResponse(200,[{'id':8,'codigo':'L8','nombre':'<script>línea</script>','activo':False}])
    text=client.get('/investigacion/grupos_investigacion/editar/4').get_data(as_text=True)
    assert '&lt;script&gt;línea&lt;/script&gt;' in text and '<script>línea</script>' not in text
    assert 'Inactiva' in text and 'Líneas de investigación vinculadas' in text
    assert 'Sede histórica (inactiva; vínculo existente)' in text


def test_sede_options_are_authenticated_and_do_not_expose_credentials(client,calls):
    calls.responses['GET','investigacion/sedes']=FakeResponse(200,[{'id':4,'nombre':'Sede nueva','ciudad':'Cali','codigo_institucional':'PRUEBA'}])
    r=client.get('/investigacion/opciones/sedes')
    assert r.status_code==200 and r.json=={'options':[{'id':4,'label':'Sede nueva'}]}
    with client.session_transaction() as session:session.clear()
    assert client.get('/investigacion/opciones/sedes').status_code==302


def test_reactivation_is_admin_only_and_uses_csrf_protected_route(client,calls):
    calls.responses['PUT','investigacion/lineas_investigacion/8/estado']=FakeResponse(200,{'id':8,'activo':True})
    assert client.post('/investigacion/lineas_investigacion/reactivar/8').status_code==302
    assert [c for c in calls if c['method']=='PUT'][-1]['json']=={'activo':True}
    with client.session_transaction() as session:session['user']=dict(session['user'],roles=['Coordinador'])
    calls.clear()
    assert client.post('/investigacion/lineas_investigacion/reactivar/8').status_code==403
    assert not [c for c in calls if c['method']=='PUT']
