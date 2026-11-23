// One public health request per login page, without reading or forwarding the form.
(() => {
    const endpoint = document.currentScript?.dataset.apiHealthUrl;
    if (!endpoint) return;
    let target;
    try { target = new URL(endpoint); } catch { return; }
    if (target.protocol !== 'https:' || target.username || target.password) return;
    const controller = new AbortController();
    const timer = setTimeout(() => controller.abort(), 45000);
    fetch(target.href, {
        method: 'GET', mode: 'no-cors', credentials: 'omit',
        redirect: 'follow', cache: 'no-store', referrerPolicy: 'no-referrer',
        signal: controller.signal,
    }).catch(() => {}).finally(() => clearTimeout(timer));
})();
