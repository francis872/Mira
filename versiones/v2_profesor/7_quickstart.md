# Guía para ejecutar y verificar V2

## Requisitos
.NET SDK compatible con MIRA.Api/MIRA.Api.csproj, PostgreSQL, y el frontend Flask de V1. Revisar README y docker-compose.yml antes de iniciar.

1. Clonar francis872/Mira y usar feature/v2-profesor.
2. Levantar servicios según la configuración de Docker del proyecto; copiar .env.example a un .env local, nunca publicar secretos.
3. Aplicar schema y seeds de V1; comprobar los seis catálogos.
4. Cuando existan migraciones V2, aplicarlas en orden en una base de prueba.
5. Verificar alta, modificación, consulta, listado y eliminación/inactivación de cada maestro-detalle.
6. Provocar fallo deliberado en un detalle y comprobar que no se guardó el maestro.
7. Verificar que ninguna FK se digite manualmente; probar rechazos por FK inválida.
8. Validar respuestas 401 y 403 por separado y comprobar hash bcrypt.
9. Ejecutar pruebas de regresión de V1 antes de solicitar merge.

Los pasos 4-8 son criterios pendientes, no funcionalidades certificadas.
