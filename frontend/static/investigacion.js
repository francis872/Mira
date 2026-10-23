"use strict";
document.addEventListener("DOMContentLoaded", () => {
    const body = document.getElementById("group-details");
    if (!body) return;
    const status = document.getElementById("detail-status");
    const update = () => { status.textContent = `${body.rows.length} semilleros en esta ficha`; };
    document.getElementById("add-detail").addEventListener("click", () => {
        if (body.rows.length >= 200) { status.textContent = "Guarde como máximo 200 semilleros por ficha."; return; }
        body.append(document.getElementById("detail-template").content.cloneNode(true));
        body.lastElementChild.querySelector("input[name=detalle_nombre]").focus();
        update();
    });
    body.addEventListener("click", event => {
        const button = event.target.closest("[data-remove-detail]");
        if (button) { button.closest("tr").remove(); update(); }
    });
    update();
});
