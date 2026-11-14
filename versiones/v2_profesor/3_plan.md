# Plan MIRA V2

1. Conservar la regresión y los contratos de seis catálogos V1.
2. Recuperar los dos SQL de `880ac75` y confirmar con el usuario su autoridad: realizado.
3. Especificar compatibilidad y alcance antes de implementar: `docs/integracion-modelo-oficial.md`.
4. Añadir el modelo completo sin reemplazar V1: migración 006.
5. Añadir funciones CRUD, vistas con etiquetas, puentes y ficha atómica JSONB: 007.
6. Integrar Controller → Service → Repository, once formularios y cinco asociaciones.
7. Verificar PostgreSQL 16.15, SQL original vs esquema, regresión, atomicidad, HTTP y Edge.
8. Respaldar y actualizar plataforma local; actualizar documentos; commit y push autorizados a la rama de trabajo.

Resultados: 47 .NET, 63 Flask/arquitectura, 46 HTTP y cuatro SQL aprobados sobre PostgreSQL 16.15; 14 controles Edge y 72 combinaciones página/ancho en plataforma habitual. C06 queda pendiente de aplicabilidad: SQL confirmado sin FK nullable. No se fusiona `main` ni se publica hosting.
