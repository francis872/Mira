# GitHub Actions: bloqueo de cuenta comprobado

Fecha: 12 de noviembre de 2026. Repositorio público `francis872/Mira`, PR #7, commit publicado `630721a118f7cfa38093d1da8e885d502c889ddc`.

La [ejecución 43345193087](https://github.com/francis872/Mira/actions/runs/43345193087) creó los jobs `quality` y `containers`, pero ambos terminaron sin ejecutar pasos. Sus anotaciones dicen: «The job was not started because your account is locked due to a billing issue».

Se verificó que usan runners estándar ubuntu-24.04 y que el repositorio es público. Se consultaron los endpoints de facturación de la cuenta con la autenticación disponible; respondieron 404 y no proporcionaron información financiera ni una operación de desbloqueo. No se modificaron tarjetas, planes, presupuestos, pagos, visibilidad ni permisos. No se conoce ni se presupone un saldo pendiente.

GitHub documenta que una retención de autorización fallida puede bloquear también acceso gratuito, incluso sin una suscripción o saldo visibles. Si el panel no muestra un problema resoluble, indica solicitar revisión a soporte. [Procedimiento oficial](https://docs.github.com/es/billing/how-tos/troubleshooting/locked-account).

La validación local actual pasa 138 .NET, 135 Flask, 51 HTTP, once SQL y 51 Docker/Gunicorn con PostgreSQL compartido. No reemplaza una ejecución Actions aprobada. No se cambia el workflow para ocultar el bloqueo, no se publican estados verdes falsos y no se hace merge con este gate pendiente.

La comprobación posterior [43350345420](https://github.com/francis872/Mira/actions/runs/43350345420) en `e3f60e5e78381432c885e68cf8b24376c303aedb` repitió exactamente el bloqueo: ambos jobs sin pasos. No es un fallo reproducible del workflow.

## Solicitud preparada para revisión del titular

Cuenta: francis872. Repositorio: https://github.com/francis872/Mira. Ejecución afectada: enlace anterior. Dos jobs con cero pasos iniciados. Solicito revisar el bloqueo de facturación que impide ejecutar Actions en este repositorio público. Deseo mantener únicamente servicios gratuitos. Por favor, indicar el motivo verificable y la intervención necesaria para liberar el acceso gratuito; no autorizar nuevos cargos ni cambios de plan.

Este texto está preparado para el [portal oficial de soporte](https://support.github.com/), **no enviado**. No contiene credenciales ni datos de pago. El desbloqueo de cuenta requiere una acción del titular en el panel o de GitHub; modificar código del repositorio no lo elimina.
