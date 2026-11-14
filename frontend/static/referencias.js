"use strict";
document.addEventListener("DOMContentLoaded", () => {
    // Keep Tab inside confirmations even when the dialog is the last focusable
    // content in the document; focusin alone cannot intercept browser chrome.
    for (const modal of document.querySelectorAll("[data-line-confirmation]")) {
        modal.addEventListener("keydown", event => {
            if (event.key !== "Tab" || !modal.classList.contains("show")) return;
            const buttons = [...modal.querySelectorAll("button:not(:disabled)")].filter(button => button.getClientRects().length);
            if (!buttons.length) return;
            const first = buttons[0], last = buttons[buttons.length - 1];
            if (event.shiftKey && document.activeElement === first) {
                event.preventDefault(); last.focus();
            } else if (!event.shiftKey && document.activeElement === last) {
                event.preventDefault(); first.focus();
            }
        });
    }
    const selects = [...document.querySelectorAll("select[data-sede-options]")];
    const channel = typeof BroadcastChannel === "function" ? new BroadcastChannel("mira-sedes") : null;
    let refreshing = false;
    async function refresh() {
        if (!selects.length || refreshing || document.hidden) return;
        refreshing = true;
        try {
            for (const select of selects) {
                const response = await fetch(select.dataset.sedeOptions, {credentials: "same-origin", redirect: "error", headers: {Accept: "application/json"}});
                if (!response.ok) throw new Error("Sedes no disponibles");
                const data = await response.json();
                if (!Array.isArray(data.options)) throw new Error("Respuesta inválida");
                const value = select.value;
                const archived = select.dataset.archivedReference === value ? select.selectedOptions[0]?.textContent : null;
                const options = [new Option("Seleccione una opción", ""), ...data.options.map(row => new Option(row.label, String(row.id)))];
                if (archived && !data.options.some(row => String(row.id) === value)) options.push(new Option(archived, value));
                const same = options.length === select.options.length && options.every((row,index) => row.value === select.options[index].value && row.text === select.options[index].text);
                if (!same) {
                    select.replaceChildren(...options);
                    select.value = options.some(row => row.value === value) ? value : "";
                }
                const status = select.parentElement.querySelector("[data-sede-status]");
                if (status) status.textContent = value && !select.value ? "La sede seleccionada dejó de estar disponible. Seleccione una sede activa." : (!same ? "Lista de sedes actualizada." : "");
            }
        } catch {
            // Preserve current input when references cannot be refreshed; the API still validates on save.
            for (const select of selects) {
                const status = select.parentElement.querySelector("[data-sede-status]");
                if (status) status.textContent = "No fue posible actualizar las sedes. Intente de nuevo antes de guardar.";
            }
        } finally { refreshing = false; }
    }
    if (channel) channel.onmessage = event => { if (event.data?.source === "sedes") refresh(); };
    window.addEventListener("focus", refresh);
    document.addEventListener("visibilitychange", refresh);
    if (selects.length) setInterval(refresh, 15000);
    if (window.location.pathname === "/investigacion/sedes") channel?.postMessage({source: "sedes"});
});
