import {mkdir, writeFile, readFile} from 'node:fs/promises';
import assert from 'node:assert/strict';

const base=process.env.MIRA_E2E_URL || 'http://127.0.0.1:5001';
const debug=process.env.MIRA_BROWSER_DEBUG_URL || 'http://127.0.0.1:9226';
const output='docs/evidencias/v3';
const axeScript=process.env.MIRA_AXE_SCRIPT ? await readFile(process.env.MIRA_AXE_SCRIPT,'utf8') : null;
const accessibility=[];
const keyboard=[];
await mkdir(output,{recursive:true});
let ready=false;
for(let i=0;i<100;i++) {
    try { await (await fetch(debug+'/json/version')).json(); ready=true; break; } catch {}
    await new Promise(resolve=>setTimeout(resolve,100));
}
assert(ready,'Edge no habilitó el puerto de pruebas');
const target=await (await fetch(debug+'/json/new?about:blank',{method:'PUT'})).json();
const ws=new WebSocket(target.webSocketDebuggerUrl);
await new Promise((resolve,reject)=>{ws.onopen=resolve;ws.onerror=reject;});
let seq=0; const pending=new Map(); const errors=[]; const results=[];
const loadedPages=new Set();
ws.onmessage=event=>{
    const data=JSON.parse(event.data);
    if(data.method==='Page.lifecycleEvent' && data.params.name==='load') loadedPages.add(data.params.loaderId);
    if(data.id) { const call=pending.get(data.id); if(!call)return; pending.delete(data.id); data.error?call.reject(data.error):call.resolve(data.result); }
    if(data.method==='Runtime.exceptionThrown') errors.push(data.params.exceptionDetails.text);
    if(data.method==='Log.entryAdded' && data.params.entry.level==='error') errors.push(data.params.entry.text);
};
function send(method,params={}) {return new Promise((resolve,reject)=>{const id=++seq;pending.set(id,{resolve,reject});ws.send(JSON.stringify({id,method,params}));});}
async function evaluate(expression) {
    const result=await send('Runtime.evaluate',{expression,returnByValue:true,awaitPromise:true});
    if(result.exceptionDetails)throw Error(result.exceptionDetails.text);
    return result.result.value;
}
async function until(expression) {
    const end=Date.now()+20000;
    while(Date.now()<end) {if(await evaluate(expression))return;await new Promise(resolve=>setTimeout(resolve,100));}
    throw Error('No se completó la condición: '+expression);
}
async function go(path) {
    const navigation=await send('Page.navigate',{url:base+path});
    const end=Date.now()+20000;
    while(navigation.loaderId && !loadedPages.has(navigation.loaderId) && Date.now()<end) await new Promise(resolve=>setTimeout(resolve,50));
    assert(!navigation.loaderId || loadedPages.has(navigation.loaderId),'No terminó la carga de '+path);
    await until(`location.pathname===${JSON.stringify(path)} && document.readyState==='complete'`);
}
async function width(value) {await send('Emulation.setDeviceMetricsOverride',{width:value,height:1000,deviceScaleFactor:1,mobile:value<768});}
async function key(name,code,virtual,modifiers=0) {
    await send('Input.dispatchKeyEvent',{type:'keyDown',key:name,code,windowsVirtualKeyCode:virtual,modifiers,...(name==='Enter'?{text:'\r'}:{})});
    await send('Input.dispatchKeyEvent',{type:'keyUp',key:name,code,windowsVirtualKeyCode:virtual,modifiers});
}
async function shot(name) {const result=await send('Page.captureScreenshot',{format:'png',captureBeyondViewport:false});await writeFile(`${output}/${name}.png`,Buffer.from(result.data,'base64'));}
async function check(path,value) {
    await width(value);await go(path);
    const state=await evaluate(`({overflow:document.documentElement.scrollWidth>innerWidth+1,
        unnamed:[...document.querySelectorAll('button')].filter(x=>!x.textContent.trim()&&!x.getAttribute('aria-label')&&!x.getAttribute('title')).length,
        main:!!document.querySelector('main'),
        icons:document.querySelectorAll('svg.mira-icon use').length})`);
    assert.equal(state.overflow,false,`${path} desborda a ${value}px`);
    assert.equal(state.unnamed,0,`${path} contiene botones sin nombre`);
    assert(state.main);
    if(state.icons) await until(`[...document.querySelectorAll('svg use')].some(x=>x.getBBox().width>0)`);
    results.push({path,width:value,...state});
    if (axeScript && (value===1440 || value===390)) {
        await evaluate(axeScript+';true');
        const report=await evaluate(`axe.run(document,{runOnly:{type:'tag',values:['wcag2a','wcag2aa','wcag21a','wcag21aa','wcag22aa']}}).then(r=>({passes:r.passes.length,incomplete:r.incomplete.length,manualReview:r.incomplete.map(v=>({id:v.id,nodes:v.nodes.map(n=>({target:n.target,summary:n.failureSummary}))})),violations:r.violations.map(v=>({id:v.id,impact:v.impact,description:v.description,nodes:v.nodes.map(n=>({target:n.target,summary:n.failureSummary}))}))}))`);
        accessibility.push({path,width:value,...report});
    }
}
try {
    await send('Page.enable'); await send('Page.setLifecycleEventsEnabled',{enabled:true}); await send('Runtime.enable'); await send('Log.enable');
    await check('/login',1440);await shot('login-desktop');
    await check('/login',390);await shot('login-mobile');
    await width(1440);
    assert(process.env.MIRA_E2E_EMAIL&&process.env.MIRA_E2E_PASSWORD,'Faltan credenciales aisladas');
    await evaluate(`document.getElementById('correo').value=${JSON.stringify(process.env.MIRA_E2E_EMAIL)};document.getElementById('password').value=${JSON.stringify(process.env.MIRA_E2E_PASSWORD)};document.querySelector('form').requestSubmit();true`);
    await until(`location.pathname==='/' && document.readyState==='complete'`);
    for(const value of [1440,1024,768,640,390,320]) {
        for(const path of ['/','/universidad','/universidad/nuevo','/investigacion','/investigacion/grupos_investigacion','/investigacion/grupos_investigacion/nuevo','/investigacion/asociaciones/grupo_lineas','/usuarios']) await check(path,value);
        await go('/');await shot(`dashboard-${value}`);
    }
    await width(390);await go('/');
    assert(await evaluate(`typeof bootstrap!=='undefined'`),'Bootstrap no está disponible; verificar CDN');
    await evaluate(`document.querySelector('.navbar-toggler').click();true`);
    await until(`document.querySelector('#navbarMain').classList.contains('show')`);
    await send('Input.dispatchKeyEvent',{type:'keyDown',key:'Tab',code:'Tab',windowsVirtualKeyCode:9});
    assert(await evaluate(`document.activeElement!==document.body`),'La navegación por teclado no tiene foco');
    await width(1440);await go('/universidad');
    assert(await evaluate(`!!document.querySelector('svg use')`));
    for (const path of ['/universidad','/area_conocimiento','/objetivo_desarrollo_sostenible','/area_aplicacion','/termino_clave','/linea_investigacion']) {
        await go(path);
        await evaluate(`window.modalTrigger=document.querySelector('[data-bs-toggle="modal"]');modalTrigger.focus();true`);
        await key('Enter','Enter',13);
        await until(`!!document.querySelector('.modal.show')`);
        // Bootstrap activa focusTrap al terminar la transición, después de .show.
        await until(`document.querySelector('.modal.show').contains(document.activeElement)`);
        assert(await evaluate(`(()=>{const m=document.querySelector('.modal.show');return m.getAttribute('aria-labelledby')&&!!document.getElementById(m.getAttribute('aria-labelledby'))?.textContent.trim()})()`),'Modal sin nombre accesible');
        for(let i=0;i<6;i++) {await key('Tab','Tab',9);assert(await evaluate(`document.querySelector('.modal.show').contains(document.activeElement)`),'Foco escapó del modal');}
        await key('Tab','Tab',9,8);
        assert(await evaluate(`document.querySelector('.modal.show').contains(document.activeElement)`));
        await key('Escape','Escape',27);
        await until(`!document.querySelector('.modal.show')`);
        await until(`document.activeElement===window.modalTrigger`);
        keyboard.push({path,modalName:true,tabTrap:true,escapeCloses:true,focusReturns:true});
    }
    await width(390);await go('/universidad');
    await evaluate(`document.querySelector('.table-responsive').focus();true`);
    await key('ArrowRight','ArrowRight',39);
    await until(`document.querySelector('.table-responsive').scrollLeft>0`);
    assert(await evaluate(`getComputedStyle(document.activeElement).outlineStyle!=='none'`));
    keyboard.push({path:'/universidad',width:390,tableKeyboardScroll:true,focusVisible:true});
    await go('/universidad/nuevo');
    await evaluate(`document.querySelector('form[action="/universidad/crear"]').requestSubmit();true`);
    assert(await evaluate(`document.activeElement.id==='nombre' && document.activeElement.validity.valueMissing`),'Validación no llevó el foco al primer requerido');
    keyboard.push({path:'/universidad/nuevo',requiredFieldFocus:true});
    await go('/investigacion/grupos_investigacion/nuevo');
    await evaluate(`document.getElementById('add-detail').focus();true`);
    await key('Enter','Enter',13);
    assert(await evaluate(`document.activeElement.name==='detalle_nombre'`));
    await key('Tab','Tab',9);await key('Tab','Tab',9);await key('Enter','Enter',13);
    assert(await evaluate(`document.querySelectorAll('#group-details tr').length===0 && document.activeElement.id==='add-detail'`));
    keyboard.push({path:'/investigacion/grupos_investigacion/nuevo',detailKeyboardAddRemove:true,focusPreserved:true});
    await width(1440);await go('/');
    await send('Emulation.setPageScaleFactor',{pageScaleFactor:2});
    assert(await evaluate(`visualViewport.scale>=1.99`));
    await send('Emulation.setPageScaleFactor',{pageScaleFactor:1});
    keyboard.push({path:'/',visualZoom:2,reflowCssWidths:[640,320],desktopBrowserZoomManuallyPending:true});
    assert.deepEqual(errors,[],'Excepciones JavaScript');
    await writeFile(`${output}/browser-results.json`,JSON.stringify({date:'2026-11-05',checks:results,consoleErrors:errors,mobileNavigation:true,keyboardFocus:true},null,2));
    await writeFile(`${output}/keyboard-results.json`,JSON.stringify({date:'2026-11-05',checks:keyboard,screenReaderTested:false},null,2));
    if(axeScript) {
        await writeFile(`${output}/accessibility-results.json`,JSON.stringify({date:'2026-11-05',tool:'axe-core 4.10.3',checks:accessibility},null,2));
        assert(accessibility.every(x=>x.violations.length===0),'axe encontró problemas; consultar accessibility-results.json');
    }
    console.log(`Navegador V3: ${results.length} combinaciones, menú móvil y foco aprobados; ${errors.length} excepciones JavaScript.`);
} finally {
    try { await send('Browser.close'); } catch {}
    ws.close();
}
