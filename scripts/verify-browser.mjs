import {mkdir, writeFile} from 'node:fs/promises';
import assert from 'node:assert/strict';

const baseUrl = process.env.MIRA_E2E_URL || 'http://127.0.0.1:5000';
const debugUrl = process.env.MIRA_BROWSER_DEBUG_URL || 'http://127.0.0.1:9224';

const target = await (await fetch(debugUrl + '/json/new?about:blank', {method: 'PUT'})).json();
const ws = new WebSocket(target.webSocketDebuggerUrl);
await new Promise((resolve, reject) => { ws.onopen = resolve; ws.onerror = reject; });
let seq = 0;
const pending = new Map();
const errors = [];
ws.onmessage = event => {
    const value = JSON.parse(event.data);
    if (value.id) {
        const request = pending.get(value.id);
        pending.delete(value.id);
        if (value.error) request.reject(value.error); else request.resolve(value.result);
    }
    if (value.method === 'Runtime.exceptionThrown') errors.push(value.params.exceptionDetails.text);
};
function send(method, params = {}) {
    return new Promise((resolve, reject) => {
        const id = ++seq;
        pending.set(id, {resolve, reject});
        ws.send(JSON.stringify({id, method, params}));
    });
}
async function evaluate(expression) {
    const result = await send('Runtime.evaluate', {expression, returnByValue: true, awaitPromise: true});
    if (result.exceptionDetails) throw new Error(result.exceptionDetails.text);
    return result.result.value;
}
async function until(expression) {
    const end = Date.now() + 25000;
    while (Date.now() < end) {
        if (await evaluate(expression)) return;
        await new Promise(resolve => setTimeout(resolve, 150));
    }
    throw new Error('Tiempo de espera: ' + expression);
}
async function go(path) {
    await send('Page.navigate', {url: baseUrl + path});
    await until(`location.pathname === ${JSON.stringify(path)} && document.readyState === 'complete'`);
}

await send('Page.enable');
await send('Runtime.enable');
await send('Emulation.setDeviceMetricsOverride', {width: 1440, height: 1000, deviceScaleFactor: 1, mobile: false});
const results = [];
await go('/area_aplicacion');
await until("document.querySelector('#catalog-results').textContent.includes('registros visibles')");
assert.equal(await evaluate("document.querySelector('.navbar-nav a[aria-current=page]').pathname"), '/area_aplicacion');
results.push('Navegacion activa y busqueda inicializadas en navegador real');
await evaluate("document.querySelector('#catalog-search').value = 'zz-sin-coincidencias-123'; document.querySelector('#catalog-search').dispatchEvent(new Event('input'))");
assert.equal(await evaluate("document.querySelector('#catalog-no-results').hidden"), false);
await evaluate("document.querySelector('#catalog-search').value = ''; document.querySelector('#catalog-search').dispatchEvent(new Event('input'))");
results.push('Busqueda de filas y estado sin coincidencias');
assert.equal(await evaluate("typeof bootstrap.Modal"), 'function');
await evaluate("document.querySelector('button[data-bs-toggle=modal]').click()");
await until("document.querySelector('.modal.show') !== null");
await until("bootstrap.Modal.getInstance(document.querySelector('.modal.show'))._isTransitioning === false");
await evaluate("document.querySelector('.modal.show button[data-bs-dismiss=modal]').click()");
await until("document.querySelector('.modal.show') === null");
results.push('Modal de retiro abre y cancela con Bootstrap real');

const marker = 'e2e-browser-' + Date.now();
await go('/area_aplicacion/nuevo');
await evaluate(`document.querySelector('[name=nombre]').value = ${JSON.stringify('  ' + marker + '  ')}; document.querySelector('form').requestSubmit()`);
await until("location.pathname === '/area_aplicacion' && document.readyState === 'complete'");
await until("document.querySelector('#catalog-results').textContent.includes('registros visibles')");
const key = await evaluate(`Array.from(document.querySelectorAll('tbody tr')).find(row => row.textContent.includes(${JSON.stringify(marker)})).querySelector('a[title=Editar]').pathname.split('/').pop()`);
assert.ok(key);
assert.equal(await evaluate(`Array.from(document.querySelectorAll('tbody tr')).find(row => row.textContent.includes(${JSON.stringify(marker)})).cells[1].textContent.trim()`), marker);
results.push('Alta desde formulario y valor normalizado por trigger visible en listado');
await go('/area_aplicacion/editar/' + key);
await evaluate(`document.querySelector('[name=nombre]').value = ${JSON.stringify('  ' + marker + '-edit  ')}; document.querySelector('form').requestSubmit()`);
await until("location.pathname === '/area_aplicacion' && document.readyState === 'complete'");
assert.ok(await evaluate(`document.querySelector('tbody').textContent.includes(${JSON.stringify(marker + '-edit')})`));
results.push('Edicion desde formulario persiste y vuelve al listado');
await evaluate(`Array.from(document.querySelectorAll('tbody tr')).find(row => row.textContent.includes(${JSON.stringify(marker)})).querySelector('button[data-bs-toggle=modal]').click()`);
await until("document.querySelector('.modal.show') !== null");
await until("bootstrap.Modal.getInstance(document.querySelector('.modal.show'))._isTransitioning === false");
await evaluate("document.querySelector('.modal.show form').requestSubmit()");
await until("!document.querySelector('.modal.show') && document.readyState === 'complete' && document.querySelector('.alert-success') !== null");
await until(`!document.querySelector('tbody').textContent.includes(${JSON.stringify(marker)})`);
results.push('Retiro confirmado desde modal excluye el registro del listado');

await mkdir('docs/evidencias', {recursive: true});
await go('/');
let capture = await send('Page.captureScreenshot', {format: 'png', captureBeyondViewport: true});
await writeFile('docs/evidencias/mira-v2-desktop.png', Buffer.from(capture.data, 'base64'));
await send('Emulation.setDeviceMetricsOverride', {width: 390, height: 844, deviceScaleFactor: 1, mobile: true});
await go('/area_aplicacion');
assert.equal(await evaluate('document.documentElement.scrollWidth <= innerWidth'), true);
await evaluate("document.querySelector('.navbar-toggler').click()");
await until("document.querySelector('#navbarMain').classList.contains('show')");
results.push('Viewport movil de 390px sin desbordamiento y menu desplegable funcional');
await until("!document.querySelector('#navbarMain').classList.contains('collapsing')");
await evaluate("document.querySelector('.navbar-toggler').click()");
await until("!document.querySelector('#navbarMain').classList.contains('show') && !document.querySelector('#navbarMain').classList.contains('collapsing')");
results.push('Menu movil se cierra y devuelve espacio al contenido');
capture = await send('Page.captureScreenshot', {format: 'png', captureBeyondViewport: true});
await writeFile('docs/evidencias/mira-v2-mobile.png', Buffer.from(capture.data, 'base64'));

const catalogs = ['area_aplicacion', 'area_conocimiento', 'linea_investigacion', 'objetivo_desarrollo_sostenible', 'termino_clave', 'universidad'];
const routes = ['/', ...catalogs.flatMap(catalog => ['/' + catalog, '/' + catalog + '/nuevo'])];
for (const width of [390, 768, 1024, 1440]) {
    await send('Emulation.setDeviceMetricsOverride', {width, height: 1000, deviceScaleFactor: 1, mobile: width < 768});
    for (const path of routes) {
        await go(path);
        assert.equal(await evaluate('document.documentElement.scrollWidth <= innerWidth'), true, `${path} en ${width}px`);
        if (width >= 992) {
            assert.ok(await evaluate("document.querySelector('.mira-sidebar').getBoundingClientRect().right <= document.querySelector('.mira-workspace').getBoundingClientRect().left + 1"), 'Barra lateral sin superponer el contenido');
        }
    }
}
results.push('52 combinaciones de pagina y viewport verificadas sin desbordamiento ni superposicion lateral');
await go('/universidad/nuevo');
capture = await send('Page.captureScreenshot', {format: 'png', captureBeyondViewport: true});
await writeFile('docs/evidencias/mira-v2-form.png', Buffer.from(capture.data, 'base64'));
await go('/');
await send('Page.bringToFront');
await evaluate("document.querySelector('.project-status summary').focus()");
await send('Input.dispatchKeyEvent', {type: 'keyDown', key: 'Enter', code: 'Enter', windowsVirtualKeyCode: 13, text: '\r', unmodifiedText: '\r'});
await send('Input.dispatchKeyEvent', {type: 'keyUp', key: 'Enter', code: 'Enter', windowsVirtualKeyCode: 13});
await until("document.querySelector('.project-status').open");
assert.equal(await evaluate("document.querySelector('.project-status').open"), true);
results.push('Estado del proyecto desplegable accesible mediante teclado');
// Official research model: selectors, a real multi-row sheet and association conflicts.
await send('Emulation.setDeviceMetricsOverride', {width: 1440, height: 1000, deviceScaleFactor: 1, mobile: false});
const researchMarker = 'e2e-browser-research-' + Date.now();
await go('/investigacion/sedes/nuevo');
await evaluate(`document.querySelector('[name=nombre]').value=${JSON.stringify(researchMarker)}; document.querySelector('[name=ciudad]').value='Medellín'; document.querySelector('form').requestSubmit()`);
await until("location.pathname === '/investigacion/sedes' && document.readyState === 'complete'");
const sedeId = await evaluate(`Array.from(document.querySelectorAll('tbody tr')).find(row=>row.textContent.includes(${JSON.stringify(researchMarker)})).querySelector('a').pathname.split('/').pop()`);
await go('/investigacion/grupos_investigacion/nuevo');
assert.equal(await evaluate("document.querySelector('[name=sede_id]').tagName"), 'SELECT');
await evaluate(`document.querySelector('[name=sede_id]').value=${JSON.stringify(sedeId)}; document.querySelector('[name=gruptac_url]').value='https://example.org/browser'; document.querySelector('#add-detail').click(); document.querySelector('#add-detail').click();`);
assert.equal(await evaluate("document.querySelector('[name=detalle_nombre]').getBoundingClientRect().width > 100"), true);
await evaluate(`Array.from(document.querySelectorAll('[name=detalle_nombre]')).forEach((field,index)=>field.value=${JSON.stringify(researchMarker)}+'-'+index); document.querySelector('#group-sheet').requestSubmit()`);
await until("/^\\/investigacion\\/grupos_investigacion\\/editar\\/\\d+$/.test(location.pathname) && document.readyState === 'complete' && document.querySelector('.alert-success') !== null");
const grupoId=await evaluate("location.pathname.split('/').pop()");
assert.equal(await evaluate("document.querySelectorAll('#group-details tr').length"), 2);
assert.equal(await evaluate("document.querySelectorAll('#group-details [name=grupo_id]').length"), 0);
await mkdir('docs/evidencias',{recursive:true});
capture=await send('Page.captureScreenshot',{format:'png',captureBeyondViewport:true});
await writeFile('docs/evidencias/mira-v2-grupo-semilleros.png',Buffer.from(capture.data,'base64'));
results.push('SQL oficial: sede seleccionada de la API, dos semilleros creados y recuperados en una misma ficha');
await evaluate("document.querySelector('[name=detalle_nombre]').value+='-editado'; document.querySelectorAll('[data-remove-detail]')[1].click(); document.querySelectorAll('.alert').forEach(alert=>alert.remove()); document.querySelector('#group-sheet').requestSubmit()");
await until("document.readyState === 'complete' && document.querySelector('.alert-success') !== null && document.querySelectorAll('#group-details tr').length === 1");
assert.ok(await evaluate("document.querySelector('[name=detalle_nombre]').value.endsWith('-editado')"));
results.push('Ficha real: editar un detalle y retirar otro conserva el estado persistido al guardar');
await go('/investigacion/lineas_investigacion/nuevo');
await evaluate(`document.querySelector('[name=codigo]').value=${JSON.stringify(researchMarker)}; document.querySelector('[name=nombre]').value=${JSON.stringify(researchMarker)}; document.querySelector('form').requestSubmit()`);
await until("location.pathname === '/investigacion/lineas_investigacion' && document.readyState === 'complete'");
const lineaId=await evaluate(`Array.from(document.querySelectorAll('tbody tr')).find(row=>row.textContent.includes(${JSON.stringify(researchMarker)})).querySelector('a').pathname.split('/').pop()`);
await go('/investigacion/asociaciones/grupo_lineas');
await evaluate(`document.querySelector('[name=grupo_id]').value=${JSON.stringify(grupoId)}; document.querySelector('[name=linea_id]').value=${JSON.stringify(lineaId)}; document.querySelector('form').requestSubmit()`);
await until("document.readyState === 'complete' && document.querySelector('.alert-success') !== null");
await evaluate(`document.querySelector('[name=grupo_id]').value=${JSON.stringify(grupoId)}; document.querySelector('[name=linea_id]').value=${JSON.stringify(lineaId)}; document.querySelector('form').requestSubmit()`);
await until("document.readyState === 'complete' && document.querySelector('.alert-danger') !== null");
assert.ok(await evaluate("document.querySelector('.alert-danger').textContent.includes('Ya existe')"));
const associationRow=await evaluate(`Array.from(document.querySelectorAll('tbody tr')).findIndex(row=>row.textContent.includes(${JSON.stringify(researchMarker)}))`);
assert.ok(associationRow>=0);
await evaluate(`document.querySelectorAll('tbody tr')[${associationRow}].querySelector('form').requestSubmit()`);
await until("document.readyState === 'complete' && document.querySelector('.alert-success') !== null");
results.push('Puente real: asignacion desde desplegables, duplicado explicado en español y retiro desde la tabla');
const researchRoutes=['/investigacion','/investigacion/grupos_investigacion/editar/'+grupoId,'/investigacion/docentes/nuevo','/investigacion/semilleros/nuevo','/investigacion/asociaciones/grupo_lineas'];
for(const width of [390,768,1024,1440]) {
    await send('Emulation.setDeviceMetricsOverride',{width,height:1000,deviceScaleFactor:1,mobile:width<768});
    for(const path of researchRoutes) {
        await go(path);
        assert.equal(await evaluate('document.documentElement.scrollWidth <= innerWidth'),true,`${path} en ${width}px`);
    }
}
results.push('20 combinaciones adicionales de investigacion y ancho, incluida ficha editable movil sin desbordamiento');
await go('/investigacion/grupos_investigacion/editar/'+grupoId);
await evaluate("document.querySelectorAll('[data-remove-detail]').forEach(button=>button.click()); document.querySelector('#group-sheet').requestSubmit()");
await until("document.readyState === 'complete' && document.querySelector('.alert-success') !== null && document.querySelectorAll('#group-details tr').length === 0");
// Cleanup uses CSRF-protected forms in the same browser, and only the just-created records.
for (const [resource,id] of [['grupos_investigacion',grupoId],['lineas_investigacion',lineaId],['sedes',sedeId]]) {
    await go('/investigacion/'+resource);
    await evaluate(`document.querySelector('form[action$="/retirar/${id}"]').requestSubmit()`);
    await until("document.readyState === 'complete' && document.querySelector('.alert-success') !== null");
}
assert.deepEqual(errors, []);
await writeFile('.local/browser-check.json', JSON.stringify({results, errors}, null, 2));
console.log(JSON.stringify({checks: results.length, results, errors}, null, 2));
ws.close();
