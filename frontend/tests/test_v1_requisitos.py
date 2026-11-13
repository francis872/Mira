from html.parser import HTMLParser
import pytest
from test_app import client, calls, FakeResponse


class Inputs(HTMLParser):
    def __init__(self):
        super().__init__()
        self.inputs = {}
        self.categories = []
        self.category_select = False

    def handle_starttag(self, tag, attrs):
        data = dict(attrs)
        if tag == 'input':
            self.inputs[data.get('name')] = data
        if tag == 'select':
            self.category_select = data.get('name') == 'categoria'
        if tag == 'option' and self.category_select and data.get('value'):
            self.categories.append(data['value'])

    def handle_endtag(self, tag):
        if tag == 'select':
            self.category_select = False


@pytest.mark.parametrize('editing', [False, True])
def test_keyword_english_is_required_on_both_forms(client, calls, editing):
    calls.responses[('GET', 'termino_clave/prueba')] = FakeResponse(200, {'termino': 'prueba', 'terminoIngles': 'test'})
    html = client.get('/termino_clave/editar/prueba' if editing else '/termino_clave/nuevo').get_data(as_text=True)
    fields = Inputs(); fields.feed(html)
    assert 'required' in fields.inputs['termino_ingles']
    assert 'Equivalente en inglés' in html


@pytest.mark.parametrize('editing', [False, True])
def test_ods_form_exposes_exactly_original_categories(client, calls, editing):
    calls.responses[('GET', 'objetivo_desarrollo_sostenible/7')] = FakeResponse(200, {'id': 7, 'nombre': 'Prueba', 'categoria': 'Económica'})
    html = client.get('/objetivo_desarrollo_sostenible/editar/7' if editing else '/objetivo_desarrollo_sostenible/nuevo').get_data(as_text=True)
    fields = Inputs(); fields.feed(html)
    assert fields.categories == ['Social', 'Económica', 'Ambiental']
    if editing:
        assert 'value="Económica" selected' in html


def test_rejected_english_preserves_spanish_and_error(client, calls):
    calls.responses[('POST', 'termino_clave')] = FakeResponse(400, {'mensaje': 'El equivalente en inglés es obligatorio.'})
    page = client.post('/termino_clave/crear', data={'termino': 'Contenido académico', 'termino_ingles': ''})
    assert page.status_code == 400
    assert 'Contenido académico' in page.get_data(as_text=True)
    assert 'El equivalente en inglés es obligatorio.' in page.get_data(as_text=True)
