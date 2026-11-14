import re
import uuid
import pytest
from test_local_platform import Browser, browser, row_for


def test_keyword_without_english_is_rejected_and_update_preserves_it(browser):
    marker = 'e2e-f5-keyword-' + uuid.uuid4().hex[:10]
    failed = browser.post('/termino_clave/crear', {'termino': marker}, '/termino_clave/nuevo')
    assert failed.status_code == 400
    assert 'inglés' in failed.text
    assert marker not in Browser().get('/termino_clave').text
    assert browser.post('/termino_clave/crear', {'termino': marker, 'termino_ingles': 'Verified equivalent'}, '/termino_clave/nuevo').status_code == 302
    try:
        failed = browser.post('/termino_clave/editar/' + marker, {'termino_ingles': '   '}, '/termino_clave/editar/' + marker)
        assert failed.status_code == 400
        assert 'Verified equivalent' in Browser().get('/termino_clave/editar/' + marker).text
    finally:
        browser.post('/termino_clave/eliminar/' + marker, {}, '/termino_clave')


@pytest.mark.parametrize('category', ['Económico', 'Economica', 'Otra'])
def test_ods_category_cannot_bypass_select_and_failed_update_keeps_value(browser, category):
    marker = 'e2e-f5-ods-' + uuid.uuid4().hex[:10]
    assert browser.post('/objetivo_desarrollo_sostenible/crear', {'nombre': marker, 'categoria': category}, '/objetivo_desarrollo_sostenible/nuevo').status_code == 400
    assert marker not in Browser().get('/objetivo_desarrollo_sostenible').text
    assert browser.post('/objetivo_desarrollo_sostenible/crear', {'nombre': marker, 'categoria': 'Social'}, '/objetivo_desarrollo_sostenible/nuevo').status_code == 302
    row = row_for(Browser().get('/objetivo_desarrollo_sostenible').text, marker)
    key = re.search(r'/objetivo_desarrollo_sostenible/editar/(\d+)', row).group(1)
    try:
        response = browser.post('/objetivo_desarrollo_sostenible/editar/' + key, {'nombre': marker, 'categoria': category}, '/objetivo_desarrollo_sostenible/editar/' + key)
        assert response.status_code == 400
        saved = row_for(Browser().get('/objetivo_desarrollo_sostenible').text, marker)
        assert 'Social' in saved and category not in saved
    finally:
        browser.post('/objetivo_desarrollo_sostenible/eliminar/' + key, {}, '/objetivo_desarrollo_sostenible')
