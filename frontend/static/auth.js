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
const confirmation = document.getElementById('confirm_password');
if (confirmation) {
    const password = document.getElementById('password');
    const validate = () => confirmation.setCustomValidity(confirmation.value && confirmation.value !== password.value ? 'Las contraseñas no coinciden.' : '');
    confirmation.addEventListener('input', validate);
    password.addEventListener('input', validate);
}
