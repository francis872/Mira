"use strict";

document.addEventListener("DOMContentLoaded", () => {
    const section = window.location.pathname.split("/")[1];
    document.querySelectorAll(".navbar-nav .nav-link").forEach(link => {
        if (new URL(link.href).pathname.split("/")[1] === section) {
            link.setAttribute("aria-current", "page");
        }
    });

    const search = document.getElementById("catalog-search");
    const summary = document.getElementById("catalog-results");
    const empty = document.getElementById("catalog-no-results");
    const body = document.querySelector("table tbody");
    if (search && body) {
        search.value = new URL(window.location.href).searchParams.get("q") || "";
        const rows = Array.from(body.rows).filter(row => row.cells.length > 1);
        const normalize = value => value.normalize("NFD").replace(/[\u0300-\u036f]/g, "").toLocaleLowerCase("es");
        const headers=body.closest('table').querySelectorAll('thead th');
        const hasActions=headers.length>0 && normalize(headers[headers.length-1].textContent.trim())==='acciones';
        let page=Math.max(1,Number(new URL(window.location.href).searchParams.get('page'))||1);
        const pageSize=25;
        const pager=document.createElement('nav');pager.id='catalog-pagination';pager.setAttribute('aria-label','Páginas del listado');pager.className='list-pagination';
        const previous=document.createElement('button'),next=document.createElement('button'),state=document.createElement('span');
        previous.type=next.type='button';previous.className=next.className='btn btn-sm btn-outline-secondary';previous.textContent='Anterior';next.textContent='Siguiente';state.setAttribute('aria-live','polite');
        pager.append(previous,state,next);summary.insertAdjacentElement('afterend',pager);
        const syncUrl=()=>{const url=new URL(window.location.href);if(search.value.trim())url.searchParams.set('q',search.value.trim());else url.searchParams.delete('q');if(page>1)url.searchParams.set('page',String(page));else url.searchParams.delete('page');history.replaceState(null,'',url);document.querySelectorAll('[data-filter-search]').forEach(x=>{x.value=search.value.trim();});document.querySelectorAll('[data-search-export]').forEach(link=>{const target=new URL(link.href);if(search.value.trim())target.searchParams.set('q',search.value.trim());else target.searchParams.delete('q');target.searchParams.delete('page');link.href=target.href;});};
        const filter = () => {
            const query = normalize(search.value.trim());
            const matching = rows.filter(row => {
                // El texto del modal no forma parte de la búsqueda del registro.
                const cells=Array.from(row.cells);
                const text = (hasActions?cells.slice(0,-1):cells).map(cell => cell.textContent).join(" ");
                return normalize(text).includes(query);
            });
            const pages=Math.max(1,Math.ceil(matching.length/pageSize));page=Math.min(page,pages);
            const visible=new Set(matching.slice((page-1)*pageSize,page*pageSize));rows.forEach(row=>{row.hidden=!visible.has(row);});
            summary.textContent = `${matching.length} de ${rows.length} registros encontrados · ${visible.size} en esta página`;
            if(empty)empty.hidden = rows.length === 0 || matching.length > 0;
            pager.hidden=matching.length<=pageSize;previous.disabled=page<=1;next.disabled=page>=pages;state.textContent=`Página ${page} de ${pages}`;
        };
        search.addEventListener("input", () => {
            page=1;filter();syncUrl();
        });
        previous.addEventListener('click',()=>{page--;filter();syncUrl();});next.addEventListener('click',()=>{page++;filter();syncUrl();});
        filter();
    }

    document.querySelectorAll('form[method]').forEach(form => {
        if (form.method.toLowerCase() !== 'post') return;
        form.addEventListener("submit", event => {
            if (form.dataset.submitting === "true") { event.preventDefault(); return; }
            if (!form.checkValidity()) return;
            form.dataset.submitting = "true";
            const status = document.getElementById("form-status");
            if (status) { status.hidden = false; status.textContent = "Procesando solicitud…"; }
            form.querySelectorAll('button[type="submit"]').forEach(button => { button.disabled = true; });
        });
    });
});

window.addEventListener("pageshow", () => {
    document.querySelectorAll("form[data-submitting]").forEach(form => {
        delete form.dataset.submitting;
        form.querySelectorAll('button[type="submit"]').forEach(button => { button.disabled = false; });
    });
    const status = document.getElementById("form-status");
    if (status) status.hidden = true;
});
