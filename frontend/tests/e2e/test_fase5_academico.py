from io import BytesIO
import re
import uuid
import requests
from openpyxl import load_workbook
from pypdf import PdfReader
from test_local_platform import Browser,API
from test_investigacion import records,api


def test_members_excel_pdf_and_filtered_report_use_persisted_relations(records):
    marker='e2e-f5-reporte-'+uuid.uuid4().hex[:8]
    sede=records('sedes',{'nombre':marker,'ciudad':'Prueba'})
    group=records('grupos_investigacion',{'sede_id':sede,'gruptac_url':'https://example.invalid/'+marker})
    people=api('GET','referencias/usuarios').json()
    used={row['usuario_id'] for row in api('GET','docentes').json()}
    person=next((row for row in people if row['id'] not in used),None)
    assert person is not None,'La base aislada requiere una persona QA sin perfil docente.'
    teacher=records('docentes',{'usuario_id':person['id']})
    records('grupo_docentes',{'grupo_id':group,'docente_id':teacher,'rol':'Coinvestigador','fecha_inicio':'2026-01-01'})
    line=records('lineas_investigacion',{'codigo':marker,'nombre':marker})
    assert api('POST','grupo_lineas',{'grupo_id':group,'linea_id':line}).status_code==201
    browser=Browser();base=f'/investigacion/reportes/grupos/{group}/integrantes'
    page=browser.get(base);assert page.status_code==200 and person['nombre'] in page.text
    excel=browser.get(base+'.xlsx');assert excel.status_code==200
    row=list(load_workbook(BytesIO(excel.content)).active.values)[1]
    assert row[:3]==(person['nombre'],'Coinvestigador','2026-01-01')
    pdf=browser.get(base+'.pdf');assert pdf.status_code==200
    pdf_text=' '.join(' '.join(p.extract_text().split()) for p in PdfReader(BytesIO(pdf.content)).pages)
    assert ' '.join(person['nombre'].split()) in pdf_text
    filtered=browser.get(f'/investigacion/reportes?sede_id={sede}&linea_id={line}')
    workbook=load_workbook(BytesIO(browser.get(f'/investigacion/reportes/articulacion.xlsx?sede_id={sede}&linea_id={line}').content))
    rows=list(workbook.active.values)
    assert filtered.status_code==200 and len(rows)==2 and rows[1][0]==marker and rows[1][3]==person['nombre']
    assert 'password' not in api('GET',f'reportes/articulacion?sedeId={sede}').text.lower()


def test_line_save_rejects_missing_links_and_rolls_back_bad_reference(records):
    marker='e2e-f5-min-'+uuid.uuid4().hex[:8];browser=Browser();path='/investigacion/lineas_investigacion/nuevo'
    bad={'codigo':marker,'nombre':marker,'descripcion':'Descripción'}
    assert browser.post(path,bad,path).status_code==400
    assert not [r for r in api('GET','lineas_investigacion').json() if r['codigo']==marker]
    line=records('lineas_investigacion',bad)
    before=api('GET',f'lineas_investigacion/{line}').json()
    payload=dict(codigo=marker,nombre='No guardar',descripcion='Descripción',disciplina_ids=[str(i) for i in before['disciplina_ids']],ods_ids=['2147483647'])
    edit=f'/investigacion/lineas_investigacion/editar/{line}'
    assert browser.post(edit,payload,edit).status_code==409
    assert api('GET',f'lineas_investigacion/{line}').json()==before


def test_multiple_association_summary_and_reactivation_preserve_links(records):
    marker='e2e-f5-multi-'+uuid.uuid4().hex[:8]
    sede=records('sedes',{'nombre':marker,'ciudad':'Prueba'})
    group=records('grupos_investigacion',{'sede_id':sede,'gruptac_url':'https://example.invalid/'+marker})
    lines=[records('lineas_investigacion',{'codigo':marker+str(i),'nombre':marker+str(i)}) for i in range(2)]
    browser=Browser();path='/investigacion/asociaciones/grupo_lineas'
    assert browser.post(path,{'grupo_id':str(group),'linea_id':[str(i) for i in lines]},path).status_code==302
    summary=browser.get(f'/investigacion/grupos_investigacion/editar/{group}')
    assert all(marker+str(i) in summary.text for i in range(2))
    assert api('DELETE',f'lineas_investigacion/{lines[0]}').status_code==200
    inactive='/investigacion/lineas_investigacion/inactivas'
    assert marker+'0' in browser.get(inactive).text
    assert browser.post(f'/investigacion/lineas_investigacion/reactivar/{lines[0]}',{},inactive).status_code==302
    assert api('GET',f'lineas_investigacion/{lines[0]}').status_code==200
    assert api('GET',f'grupo_lineas/{group}/{lines[0]}').status_code==200


def test_report_api_and_download_do_not_restore_public_access():
    assert requests.get(API+'/api/investigacion/reportes/articulacion',timeout=30).status_code==401
    import os
    assert requests.get(os.environ['MIRA_E2E_URL']+'/investigacion/reportes/articulacion.xlsx',allow_redirects=False,timeout=30).status_code==302
