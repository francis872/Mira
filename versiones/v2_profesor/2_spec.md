# MIRA — Versión 2 — Especificación funcional

## Alcance acumulativo
La V2 conserva la Entrega 1 (API REST y frontend para seis catálogos) y amplía la solución conforme a las indicaciones del profesor. Los requisitos de este documento se implementan y verifican, no se consideran completados solo por estar documentados.

## Requerimientos funcionales nuevos
- **RF-V2-FK-01:** toda clave foránea seleccionable se presenta con un control de selección que muestra valores comprensibles; nunca se pide teclear el identificador.
- **RF-V2-MD-01:** cada relación maestro-detalle identificada dispone de una operación atómica de alta mediante una rutina de PostgreSQL que inserta encabezado y todos los detalles. Quedan prohibidos INSERT separados desde la API para una misma operación de negocio.
- **RF-V2-MD-02:** modificación de maestro y colección completa de detalles mediante rutina de PostgreSQL, con control transaccional.
- **RF-V2-MD-03:** consulta individual y listado de maestro-detalle mediante rutinas de PostgreSQL; el resultado de datos compuestos se devuelve en JSONB.
- **RF-V2-MD-04:** eliminación o inactivación mediante rutina de PostgreSQL. Se respetará el borrado lógico cuando la metodología de la asignatura lo exija; confirmar la regla por entidad antes de implementar.
- **RF-V2-DB-01:** incorporar triggers únicamente donde existan invariantes, totales derivados o desnormalización controlada que los requieran.
- **RF-V2-DB-02:** crear vistas justificadas por consultas recurrentes y requerimientos reales.
- **RF-V2-SEC-01:** autenticar mediante usuario/correo y contraseña, guardando exclusivamente hashes bcrypt con factor de costo configurado y adecuado.
- **RF-V2-SEC-02:** credenciales ausentes/inválidas => HTTP 401; identidad autenticada sin permisos => HTTP 403.
- **RF-V2-SEC-03:** autorización basada en roles aplicada del lado del servidor. Evitar MD5/SHA1 como hashes de contraseña.

## Calidad y trazabilidad
- Mantener arquitectura Controller -> Service -> Repository -> PostgreSQL y principios SOLID.
- Validar claves foráneas y roles del lado del servidor, no solo en frontend.
- No construir relaciones maestro-detalle ficticias a partir de las seis tablas independientes de la Entrega 1: partir del modelo oficial de la siguiente entrega.
- La V2 requiere pruebas de atomicidad, integridad, permisos, resultados JSON y regresión de V1.
