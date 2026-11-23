// Anonymous browser regression: detects a startup script that never reaches the network.
import assert from 'node:assert/strict';
import {mkdir, writeFile} from 'node:fs/promises';

const web = new URL(process.env.MIRA_E2E_URL);
const health = new URL(process.env.MIRA_RENDER_API_HEALTH_URL);
const debug = new URL(process.env.MIRA_BROWSER_DEBUG_URL || 'http://127.0.0.1:9226');
assert(web.protocol === 'https:' && process.env.MIRA_BROWSER_ALLOWED_ORIGIN === web.origin);
assert(health.protocol === 'https:' && !health.username && !health.password && !health.search && !health.hash);
assert(['127.0.0.1', 'localhost'].includes(debug.hostname));
const output = process.env.MIRA_EVIDENCE_DIRECTORY || '.local/render-startup-browser';
const report = {startedAtUtc: new Date().toISOString(), web: web.origin, health: health.href,
    status: 'running', requests: [], credentialsUsed: false, coldStartCertified: false};
const target = await (await fetch(new URL('/json/new?about:blank', debug), {method: 'PUT'})).json();
const socket = new WebSocket(target.webSocketDebuggerUrl);
await new Promise((resolve, reject) => {socket.onopen = resolve; socket.onerror = reject;});
let sequence = 0;
const pending = new Map(), tracked = new Map(), extra = new Map();
const flags = headers => {
    const keys = Object.keys(headers || {}).map(x => x.toLowerCase());
    return {authorization: keys.includes('authorization'), cookie: keys.includes('cookie'),
        referer: keys.includes('referer'), forwardingAssertion: keys.some(x => x.startsWith('x-mira'))};
};
socket.onmessage = event => {
    const value = JSON.parse(event.data), p = value.params;
    if (value.id) {const call = pending.get(value.id); pending.delete(value.id);
        if (call) value.error ? call.reject(new Error('CDP command failed')) : call.resolve(value.result);}
    if (value.method === 'Network.requestWillBeSent' && p.request.url === health.href) {
        const item = {method: p.request.method, body: !!p.request.hasPostData, ...flags(p.request.headers),
            ...extra.get(p.requestId)};
        report.requests.push(item); tracked.set(p.requestId, item);
    }
    if (value.method === 'Network.requestWillBeSentExtraInfo') {
        const info = {...flags(p.headers), extraHeadersObserved: true}; extra.set(p.requestId, info);
        if (tracked.has(p.requestId)) Object.assign(tracked.get(p.requestId), info);
    }
    if (value.method === 'Network.responseReceived' && tracked.has(p.requestId)) {
        Object.assign(tracked.get(p.requestId), {http: p.response.status,
            sameHealthUrl: p.response.url === health.href, tlsProtocol: p.response.securityDetails?.protocol});
    }
};
function send(method, params = {}) {
    return new Promise((resolve, reject) => {const id = ++sequence; pending.set(id, {resolve, reject});
        socket.send(JSON.stringify({id, method, params}));});
}
try {
    await send('Page.enable'); await send('Network.enable');
    await send('Page.navigate', {url: new URL('/login', web).href});
    const deadline = Date.now() + 90000;
    while (!report.requests.some(x => x.http === 200) && Date.now() < deadline)
        await new Promise(resolve => setTimeout(resolve, 200));
    assert.equal(report.requests.length, 1, 'Expected one automatic health request');
    const request = report.requests[0];
    assert.equal(request.http, 200); assert(request.sameHealthUrl && request.extraHeadersObserved);
    assert(request.method === 'GET' && !request.body && !request.authorization && !request.cookie && !request.referer && !request.forwardingAssertion);
    report.status = 'passed';
} catch (error) {report.status = 'failed'; report.errorType = error.name; process.exitCode = 1;}
finally {
    report.finishedAtUtc = new Date().toISOString(); await mkdir(output, {recursive: true});
    await writeFile(output + '/render-startup-browser.json', JSON.stringify(report, null, 2));
    await send('Page.close'); socket.close();
    console.log(JSON.stringify({status: report.status, anonymousHealthRequests: report.requests.length,
        coldStartCertified: false}));
}
