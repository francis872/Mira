try {
    const version = await (await fetch('http://127.0.0.1:' + process.env.SAPIENS_EDGE_PORT + '/json/version', {signal: AbortSignal.timeout(2000)})).json();
    const socket = new WebSocket(version.webSocketDebuggerUrl);
    socket.onopen = () => socket.send(JSON.stringify({id: 1, method: 'Browser.close'}));
    const timer = setTimeout(() => socket.close(), 3000);
    socket.onclose = () => clearTimeout(timer);
    socket.onerror = () => {clearTimeout(timer); socket.close();};
} catch {}
