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
        const rows = Array.from(body.rows).filter(row => row.cells.length > 1);
        const normalize = value => value.normalize("NFD").replace(/[\u0300-\u036f]/g, "").toLocaleLowerCase("es");
        const filter = () => {
            const query = normalize(search.value.trim());
            let visible = 0;
            rows.forEach(row => {
                // El texto del modal no forma parte de la búsqueda del registro.
                const text = Array.from(row.cells).slice(0, -1).map(cell => cell.textContent).join(" ");
                row.hidden = !normalize(text).includes(query);
                if (!row.hidden) visible += 1;
            });
            summary.textContent = `${visible} de ${rows.length} registros visibles`;
            empty.hidden = rows.length === 0 || visible > 0;
        };
        search.addEventListener("input", filter);
        filter();
    }

    document.querySelectorAll('form[method="POST"]').forEach(form => {
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
