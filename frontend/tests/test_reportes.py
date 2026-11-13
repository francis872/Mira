from io import BytesIO
from html.parser import HTMLParser
import pytest
from openpyxl import load_workbook
from pypdf import PdfReader
from test_app import client,calls,FakeResponse


def members(calls,rows=None):
    rows=rows if rows is not None else [{'docente':'José Pérez','rol':'Coinvestigador','fecha_inicio':'2026-01-01','fecha_fin':None,'activo':True}]
    calls.responses['GET','investigacion/reportes/integrantes/4']=FakeResponse(200,{'grupo':{'id':4,'nombre':'Grupo de prueba'},'integrantes':rows})


def test_member_excel_and_pdf_contain_actual_names_dates_and_states(client,calls):
    members(calls)
    xlsx=client.get('/investigacion/reportes/grupos/4/integrantes.xlsx')
    assert xlsx.status_code==200 and 'spreadsheetml' in xlsx.content_type
    sheet=load_workbook(BytesIO(xlsx.data),data_only=False).active
    assert list(sheet.values)==[('Docente','Rol','Inicio','Fin','Vínculo activo'),('José Pérez','Coinvestigador','2026-01-01','—','Sí')]
    pdf=client.get('/investigacion/reportes/grupos/4/integrantes.pdf')
    assert pdf.status_code==200 and pdf.content_type=='application/pdf'
    text=' '.join(page.extract_text() for page in PdfReader(BytesIO(pdf.data)).pages)
    assert 'José Pérez' in text and 'Coinvestigador' in text and '2026-01-01' in text


def test_excel_keeps_formula_like_names_as_literal_text(client,calls):
    members(calls,[{'docente':'=1+1','rol':'Tutor','activo':True}])
    r=client.get('/investigacion/reportes/grupos/4/integrantes.xlsx')
    sheet=load_workbook(BytesIO(r.data),data_only=False).active
    assert sheet['A2'].value=='=1+1' and sheet['A2'].data_type=='s' and sheet['A2'].hyperlink is None


def test_articulation_download_and_screen_use_identical_filters_and_rows(client,calls):
    path='investigacion/reportes/articulacion?sedeId=4&lineaId=8'
    calls.responses['GET',path]=FakeResponse(200,[{'sede':'Sede prueba','grupo':'Grupo prueba','semillero':'Semillero prueba','docente':'José Pérez','rol':'Tutor','linea':'L8 · Línea prueba','ods':'ODS prueba','fecha_inicio':'2026-01-01','fecha_fin':None,'grupo_activo':True,'semillero_activo':True,'linea_activa':False,'vinculo_activo':True}])
    calls.responses['GET','investigacion/reportes/opciones']=FakeResponse(200,{'sedes':[],'lineas':[],'grupos':[]})
    page=client.get('/investigacion/reportes?sede_id=4&linea_id=8')
    export=client.get('/investigacion/reportes/articulacion.xlsx?sede_id=4&linea_id=8')
    assert page.status_code==export.status_code==200
    row=list(load_workbook(BytesIO(export.data)).active.values)[1]
    expected=('Sede prueba','Grupo prueba','Semillero prueba','José Pérez','Tutor','L8 · Línea prueba','ODS prueba','2026-01-01','—','Sí','Sí','No','Sí')
    assert row==expected
    class DataCells(HTMLParser):
        def __init__(self):super().__init__();self.values=[];self.active=False
        def handle_starttag(self,tag,attrs):self.active=tag=='td'
        def handle_endtag(self,tag):
            if tag=='td':self.active=False
        def handle_data(self,text):
            if self.active:self.values.append(text)
    parser=DataCells();parser.feed(page.get_data(as_text=True));assert tuple(parser.values)==expected
    assert len([c for c in calls if c['url'].endswith(path)])==2


def test_empty_reports_still_export_headers_and_clear_empty_pdf(client,calls):
    members(calls,[])
    r=client.get('/investigacion/reportes/grupos/4/integrantes.xlsx')
    assert len(list(load_workbook(BytesIO(r.data)).active.values))==1
    pdf=client.get('/investigacion/reportes/grupos/4/integrantes.pdf')
    assert 'No hay integrantes vinculados' in ''.join(p.extract_text() for p in PdfReader(BytesIO(pdf.data)).pages)


def test_read_only_user_can_export_without_any_mutation(client,calls):
    with client.session_transaction() as session:session['user']=dict(session['user'],roles=['Investigador'])
    members(calls)
    assert client.get('/investigacion/reportes/grupos/4/integrantes.xlsx').status_code==200
    assert all(c['method']=='GET' for c in calls)


@pytest.mark.parametrize('extension',['xlsx','pdf'])
def test_export_is_not_public(client,calls,extension):
    with client.session_transaction() as session:session.clear()
    assert client.get('/investigacion/reportes/grupos/4/integrantes.'+extension).status_code==302
    assert not [c for c in calls if '/reportes/' in c['url']]


def test_report_errors_are_not_downloaded_as_empty_success(client,calls):
    calls.responses['GET','investigacion/reportes/integrantes/4']=FakeResponse(500,{'mensaje':'No disponible'})
    r=client.get('/investigacion/reportes/grupos/4/integrantes.xlsx')
    assert r.status_code==500 and 'No disponible' in r.get_data(as_text=True)


def test_undefined_faculty_filter_is_rejected_without_ignoring_it(client,calls):
    r=client.get('/investigacion/reportes?facultad_id=3')
    assert r.status_code==400 and 'filtro solicitado' in r.get_data(as_text=True)
    assert not [c for c in calls if 'reportes/articulacion' in c['url']]


def test_excel_rejects_unsupported_control_text_instead_of_silently_changing_data(client,calls):
    members(calls,[{'docente':'dato\x01no permitido'}])
    assert client.get('/investigacion/reportes/grupos/4/integrantes.xlsx').status_code==422
