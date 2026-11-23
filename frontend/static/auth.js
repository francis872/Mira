'use strict';
document.querySelectorAll('[data-password-toggle]').forEach(button => {
    button.addEventListener('click', () => {
        const input = document.getElementById(button.dataset.passwordToggle);
        const visible = input.type === 'password';
        input.type = visible ? 'text' : 'password';
        button.setAttribute('aria-pressed', String(visible));
        button.setAttribute('aria-label', visible ? 'Ocultar contraseña' : 'Mostrar contraseña');
    });
});
const accessForm = document.getElementById('access-form');
if (accessForm) {
    const submit = accessForm.querySelector('button[type="submit"]');
    const label = submit.querySelector('[data-submit-label]');
    const status = document.getElementById('access-processing');
    accessForm.addEventListener('submit', event => {
        if (accessForm.dataset.submitting === 'true') { event.preventDefault(); return; }
        if (!accessForm.checkValidity()) return;
        accessForm.dataset.submitting = 'true';
        accessForm.setAttribute('aria-busy', 'true');
        submit.disabled = true;
        label.textContent = 'Verificando acceso…';
        status.hidden = false;
    });
    window.addEventListener('pageshow', () => {
        delete accessForm.dataset.submitting;
        accessForm.removeAttribute('aria-busy');
        submit.disabled = false;
        label.textContent = 'Iniciar sesión';
        status.hidden = true;
    });
}
