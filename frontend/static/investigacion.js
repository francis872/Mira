"use strict";
document.addEventListener("DOMContentLoaded", () => {
    const body = document.getElementById("group-details");
    if (!body) return;
    const status = document.getElementById("detail-status");
    const rows = () => [...body.querySelectorAll('tr[data-detail-row]')];
    const update = () => {
        const count = rows().length;
        document.getElementById('empty-details').hidden = count > 0;
        status.textContent = `${count} semilleros en esta ficha`;
    };
    document.getElementById("add-detail").addEventListener("click", () => {
        if (rows().length >= 200) { status.textContent = "Guarde como máximo 200 semilleros por ficha."; return; }
        body.append(document.getElementById("detail-template").content.cloneNode(true));
        body.lastElementChild.querySelector("input[name=detalle_nombre]").focus();
        update();
    });
    body.addEventListener("click", event => {
        const button = event.target.closest("[data-remove-detail]");
        if (button) {
            const row = button.closest("tr");
            const current = rows();
            const index = current.indexOf(row);
            const next = current[index + 1] || current[index - 1];
            row.remove();
            (next?.querySelector('input[name="detalle_nombre"]') || document.getElementById("add-detail")).focus();
            update();
        }
    });
    update();
});
