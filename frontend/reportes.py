"""Reportes académicos originales: datos de API y exportaciones sin credenciales ni fórmulas."""
from io import BytesIO
from pathlib import Path
from urllib.parse import urlencode
from xml.sax.saxutils import escape
from flask import Blueprint, render_template, request, send_file
from openpyxl import Workbook
from openpyxl.cell import WriteOnlyCell
from openpyxl.cell.cell import ILLEGAL_CHARACTERS_RE
import reportlab
from reportlab.lib import colors
from reportlab.lib.pagesizes import A4, landscape
from reportlab.lib.styles import ParagraphStyle
from reportlab.pdfbase import pdfmetrics
from reportlab.pdfbase.ttfonts import TTFont
from reportlab.platypus import Paragraph, SimpleDocTemplate, Table, TableStyle, Spacer


ARTICULATION_COLUMNS = [('sede','Sede'),('grupo','Grupo'),('semillero','Semillero'),('docente','Docente'),('rol','Rol'),('linea','Línea'),('ods','ODS'),('fecha_inicio','Inicio'),('fecha_fin','Fin'),('grupo_activo','Grupo activo'),('semillero_activo','Semillero activo'),('linea_activa','Línea activa'),('vinculo_activo','Vínculo activo')]
MEMBER_COLUMNS = [('docente','Docente'),('rol','Rol'),('fecha_inicio','Inicio'),('fecha_fin','Fin'),('activo','Vínculo activo')]
pdfmetrics.registerFont(TTFont('MiraVera',str(Path(reportlab.__file__).parent/'fonts/Vera.ttf')))


class ExportValidationError(ValueError):
    pass


def display(value):
    if value is None or value == '': return '—'
    if isinstance(value,bool): return 'Sí' if value else 'No'
    return str(value)


def cells(rows,columns):
    return [[display(row.get(key)) for key,_ in columns] for row in rows]


def excel(rows,columns,title):
    values_rows=[[label for _,label in columns],*cells(rows,columns)]
    # Validate before opening write-only XML streams, including rejected exports.
    for values in values_rows:
        for value in values:
            if len(value)>32767 or ILLEGAL_CHARACTERS_RE.search(value):
                raise ExportValidationError('El reporte contiene texto que Excel no admite. No se recortaron ni sustituyeron datos.')
    book=Workbook(write_only=True)
    sheet=book.create_sheet(title)
    for values in values_rows:
        row=[]
        for value in values:
            cell=WriteOnlyCell(sheet,value=value)
            # An academic name starting '=' is data, never an Excel formula.
            cell.data_type='s'
            row.append(cell)
        sheet.append(row)
    result=BytesIO();book.save(result);result.seek(0)
    return result


def pdf_members(rows,title):
    result=BytesIO()
    style=ParagraphStyle('academic',fontName='MiraVera',fontSize=8,leading=11,wordWrap='CJK')
    contents=[Paragraph(escape(title),style),Spacer(1,12)]
    table_rows=[[Paragraph(escape(label),style) for _,label in MEMBER_COLUMNS]]
    table_rows.extend([[Paragraph(escape(value),style) for value in row] for row in cells(rows,MEMBER_COLUMNS)])
    if not rows: contents.append(Paragraph('No hay integrantes vinculados.',style))
    table=Table(table_rows,repeatRows=1,colWidths=[240,160,95,95,100])
    table.setStyle(TableStyle([('BACKGROUND',(0,0),(-1,0),colors.HexColor('#eeeeee')),('GRID',(0,0),(-1,-1),0.4,colors.grey),('VALIGN',(0,0),(-1,-1),'TOP')]))
    contents.append(table)
    SimpleDocTemplate(result,pagesize=landscape(A4),leftMargin=24,rightMargin=24,topMargin=24,bottomMargin=24,title=title).build(contents)
    result.seek(0);return result


def register_reports(app,api_call,error_message):
    bp=Blueprint('reportes',__name__,url_prefix='/investigacion/reportes')

    @bp.errorhandler(ExportValidationError)
    def export_error(error):
        return render_template('error.html',status=422,message=str(error)),422

    def call(path,listed=False):
        response=api_call('GET','investigacion/reportes/'+path,expect_list=listed)
        if response is None:
            return None,(render_template('error.html',status=503,message='Los reportes no están disponibles. Intente de nuevo.'),503)
        if response.status_code!=200:
            return None,(render_template('error.html',status=response.status_code,message=error_message(response,'No fue posible consultar el reporte.')),response.status_code)
        return response.json(),None

    def articulation():
        if set(request.args)-{'sede_id','linea_id'}:
            return None,(render_template('error.html',status=400,message='El filtro solicitado no está definido para este reporte.'),400)
        query=urlencode({api:request.args[web] for web,api in [('sede_id','sedeId'),('linea_id','lineaId')] if request.args.get(web)})
        return call('articulacion'+('?' + query if query else ''),listed=True)

    @bp.get('')
    def index():
        rows,error=articulation()
        if error:return error
        options,error=call('opciones')
        if error:return error
        return render_template('investigacion/reportes.html',rows=cells(rows,ARTICULATION_COLUMNS),columns=ARTICULATION_COLUMNS,options=options,page_title='Articulación académica')

    @bp.get('/articulacion.xlsx')
    def articulation_excel():
        rows,error=articulation()
        if error:return error
        return send_file(excel(rows,ARTICULATION_COLUMNS,'Articulacion'),mimetype='application/vnd.openxmlformats-officedocument.spreadsheetml.sheet',as_attachment=True,download_name='articulacion-academica.xlsx')

    def members(grupo_id):
        return call('integrantes/'+str(grupo_id))

    @bp.get('/grupos/<int:grupo_id>/integrantes')
    def member_list(grupo_id):
        data,error=members(grupo_id)
        if error:return error
        return render_template('investigacion/integrantes.html',group=data['grupo'],rows=cells(data['integrantes'],MEMBER_COLUMNS),columns=MEMBER_COLUMNS,page_title='Integrantes del grupo')

    @bp.get('/grupos/<int:grupo_id>/integrantes.xlsx')
    def member_excel(grupo_id):
        data,error=members(grupo_id)
        if error:return error
        return send_file(excel(data['integrantes'],MEMBER_COLUMNS,'Integrantes'),mimetype='application/vnd.openxmlformats-officedocument.spreadsheetml.sheet',as_attachment=True,download_name=f'integrantes-grupo-{grupo_id}.xlsx')

    @bp.get('/grupos/<int:grupo_id>/integrantes.pdf')
    def member_pdf(grupo_id):
        data,error=members(grupo_id)
        if error:return error
        return send_file(pdf_members(data['integrantes'],'Integrantes · '+data['grupo']['nombre']),mimetype='application/pdf',as_attachment=True,download_name=f'integrantes-grupo-{grupo_id}.pdf')

    app.register_blueprint(bp)
