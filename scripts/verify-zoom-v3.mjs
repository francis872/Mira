// Zoom real de página mediante preferencias de Chromium, sin emular DPR/viewport.
import {mkdir,mkdtemp,writeFile} from 'node:fs/promises';
import {spawn} from 'node:child_process';
import {resolve} from 'node:path';
import {createServer} from 'node:net';
import assert from 'node:assert/strict';

const base=process.env.MIRA_E2E_URL || 'http://127.0.0.1:5001';
const targetOrigin=new URL(base);
assert(['127.0.0.1','localhost'].includes(targetOrigin.hostname) ||
    (targetOrigin.protocol==='https:' && process.env.MIRA_BROWSER_ALLOWED_ORIGIN===targetOrigin.origin),
    'Configure explícitamente el origen HTTPS remoto autorizado o use loopback');
assert(process.env.MIRA_E2E_EMAIL && process.env.MIRA_E2E_PASSWORD,'Falta la identidad exclusiva de pruebas');
const output=process.env.MIRA_ZOOM_DIRECTORY || '.local/zoom-v3';
const edge=process.env.MIRA_BROWSER_EXECUTABLE || 'C:/Program Files (x86)/Microsoft/Edge/Application/msedge.exe';
const debugPort=Number(process.env.MIRA_ZOOM_DEBUG_PORT || 9228);
assert(Number.isInteger(debugPort) && debugPort>=1024 && debugPort<=65535,'Puerto de zoom inválido');
const debug='http://127.0.0.1:'+debugPort;
const delay=ms=>new Promise(r=>setTimeout(r,ms));
const results=[];
let baseline;
await mkdir(output,{recursive:true});
await mkdir('.local/zoom-profiles',{recursive:true});
const resultPath=`${output}/zoom-results.json`;
await writeFile(resultPath,JSON.stringify({status:'running',checks:[]}));

try {
for(const factor of [1,2,4]) {
    const probe=createServer();
    await new Promise((r,e)=>{probe.once('error',e);probe.listen(debugPort,'127.0.0.1',r)});
    await new Promise(r=>probe.close(r));
    const directory=await mkdtemp(resolve('.local/zoom-profiles/profile-'));
    await mkdir(directory+'/Default');
    // Partición por defecto: clave x para la ruta relativa vacía (Chromium).
    await writeFile(directory+'/Default/Preferences',JSON.stringify({partition:{default_zoom_level:{x:Math.log(factor)/Math.log(1.2)}}}));
    const child=spawn(edge,['--headless=new','--enable-automation','--disable-gpu','--no-first-run',
        '--no-default-browser-check','--remote-debugging-address=127.0.0.1','--remote-debugging-port='+debugPort,
        '--window-size=1440,1000','--user-data-dir='+directory,'about:blank'],{windowsHide:true,stdio:'ignore'});
    let launchError;
    child.once('error',error=>{launchError=error});
    let ws;
    let owned=false;
    let sequence=0;
    try {
        let target;
        for(let i=0;i<120;i++) {
            if(launchError)throw Error('No se pudo ejecutar el navegador configurado');
            try {target=await (await fetch(debug+'/json/new?about:blank',{method:'PUT'})).json();break}
            catch {await delay(100)}
        }
        assert(target,'No inició el navegador exclusivo de zoom');
        ws=new WebSocket(target.webSocketDebuggerUrl);
        await new Promise((r,e)=>{ws.onopen=r;ws.onerror=e});
        const pending=new Map();
        const loaded=new Set();
        const errors=[];
        ws.onmessage=event=>{
            const data=JSON.parse(event.data);
            if(data.id) {const call=pending.get(data.id);if(call){pending.delete(data.id);data.error?call.reject(data.error):call.resolve(data.result)}}
            if(data.method==='Page.lifecycleEvent' && data.params.name==='load')loaded.add(data.params.loaderId);
            if(data.method==='Runtime.exceptionThrown')errors.push(data.params.exceptionDetails.text);
        };
        const send=(method,params={})=>new Promise((resolve,reject)=>{const id=++sequence;pending.set(id,{resolve,reject});ws.send(JSON.stringify({id,method,params}))});
        const evaluate=async expression=>{
            const result=await send('Runtime.evaluate',{expression,returnByValue:true,awaitPromise:true});
            assert(!result.exceptionDetails,'Excepción de JavaScript durante zoom');
            return result.result.value;
        };
        const command=await send('Browser.getBrowserCommandLine');
        assert(command.arguments.includes('--user-data-dir='+directory),'El puerto pertenece a otro perfil');
        owned=true;
        await send('Page.enable');await send('Page.setLifecycleEventsEnabled',{enabled:true});await send('Runtime.enable');
        async function go(path) {
            const navigation=await send('Page.navigate',{url:base+path});
            const end=Date.now()+20000;
            while(!loaded.has(navigation.loaderId) && Date.now()<end)await delay(50);
            assert(loaded.has(navigation.loaderId),'No cargó '+path);
            assert(await evaluate(`location.pathname===${JSON.stringify(path)}`));
        }
        async function check(path) {
            await go(path);
            const state=await evaluate(`({innerWidth,outerWidth,dpr:devicePixelRatio,visualScale:visualViewport.scale,
                overflow:document.documentElement.scrollWidth>innerWidth+1,main:!!document.querySelector('main')})`);
            if(!baseline)baseline=state;
            const measured=state.dpr/baseline.dpr;
            assert(Math.abs(measured-factor)<.02,'No se aplicó el zoom real solicitado');
            assert(Math.abs(state.innerWidth*factor-baseline.innerWidth)<5,'No cambió el viewport CSS conforme al zoom');
            assert.equal(state.outerWidth,baseline.outerWidth);
            assert.equal(state.visualScale,1,'Se aplicó pinch zoom en vez de zoom de página');
            assert(!state.overflow && state.main,path+' desborda con zoom '+factor);
            const controlVisible=await evaluate(`(()=>{
                const visible=selector=>[...document.querySelectorAll(selector)].find(x=>{
                    if(x.closest('details:not([open]), [hidden], [inert], .modal:not(.show)'))return false;
                    const r=x.getBoundingClientRect();return r.width>0 && r.height>0;
                });
                const control=visible('main form button[type="submit"]:not(:disabled)') ||
                    visible('main input:not([type="hidden"]):not(:disabled)') || visible('main a');
                if(!control)return {visible:true,control:null};
                control.scrollIntoView({block:'center',inline:'nearest',behavior:'instant'});control.focus({preventScroll:true});
                const r=control.getBoundingClientRect();
                return {visible:r.width>0 && r.height>0 && r.left>=-1 && r.right<=innerWidth+1 && r.top>=-1 && r.bottom<=innerHeight+1,
                    control:control.tagName,focused:document.activeElement===control,rect:r.toJSON(),height:innerHeight};
            })()`);
            assert(controlVisible.visible,'Control inaccesible al desplazarse con zoom: '+path+' '+JSON.stringify(controlVisible));
            assert(controlVisible.control===null || controlVisible.focused,'No recibió foco el control visible');
            results.push({path,requestedZoom:factor,measuredZoom:measured,programmaticFocusAndScroll:controlVisible.control!==null,...state});
            if(['/login','/','/investigacion/grupos_investigacion/nuevo'].includes(path)) {
                const shot=await send('Page.captureScreenshot',{format:'png',captureBeyondViewport:false});
                await writeFile(`${output}/zoom-${factor}-${path==='/login'?'login':path==='/'?'inicio':'ficha'}.png`,Buffer.from(shot.data,'base64'));
            }
        }
        await check('/login');
        await evaluate(`document.getElementById('correo').value=${JSON.stringify(process.env.MIRA_E2E_EMAIL)};document.getElementById('password').value=${JSON.stringify(process.env.MIRA_E2E_PASSWORD)};document.querySelector('form').requestSubmit();true`);
        const end=Date.now()+20000;
        while(!await evaluate(`location.pathname==='/' && document.readyState==='complete'`) && Date.now()<end)await delay(100);
        assert(await evaluate(`location.pathname==='/'`),'No inició la sesión de prueba');
        for(const path of ['/','/universidad','/universidad/nuevo','/investigacion','/investigacion/grupos_investigacion',
            '/investigacion/grupos_investigacion/nuevo','/investigacion/asociaciones/grupo_lineas','/usuarios'])await check(path);
        for(const path of ['/termino_clave/nuevo','/objetivo_desarrollo_sostenible/nuevo','/investigacion/sedes/nuevo',
            '/investigacion/palabras_clave/nuevo','/investigacion/lineas_investigacion/nuevo','/investigacion/lineas_investigacion/inactivas',
            '/investigacion/semilleros','/investigacion/reportes'])await check(path);
        assert.deepEqual(errors,[]);
    } finally {
        if(owned && ws?.readyState===WebSocket.OPEN) {
            ws.send(JSON.stringify({id:++sequence,method:'Browser.close'}));
            await delay(300);
        }
        ws?.close();
        if(child.exitCode===null)child.kill();
        if(owned) {
            // Browser.close can return before Chromium releases its socket on
            // Windows. Wait for our port; never terminate a different process.
            let released=false;
            for(let attempt=0;attempt<100;attempt++) {
                const listener=createServer();
                try {
                    await new Promise((r,e)=>{listener.once('error',e);listener.listen(debugPort,'127.0.0.1',r)});
                    await new Promise(r=>listener.close(r));released=true;break;
                } catch {await delay(100);}
            }
            assert(released,'El navegador propio no liberó su puerto de zoom');
        }
    }
}
} catch(error) {
    await writeFile(resultPath,JSON.stringify({status:'failed',checks:results,error:error.message},null,2));
    throw error;
}
await writeFile(resultPath,JSON.stringify({status:'passed',date:new Date().toLocaleDateString('en-CA',{timeZone:'America/Bogota'}),
    mode:'Chromium profile page zoom; no Emulation overrides; visualViewport.scale=1',checks:results,
    physicalDevicesTested:false,screenReaderTested:false},null,2));
console.log(`Zoom real: ${results.length} comprobaciones al 100/200/400 %, sin overflow del documento.`);
