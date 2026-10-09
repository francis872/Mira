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

Verificación ejecutada en esta rama (contra PostgreSQL 16 desechable, API y Flask reales):

```powershell
# Primer administrador (una sola vez; nada se guarda en archivos)
$env:ConnectionStrings__PostgreSql = '<cadena>'
$env:MIRA_BOOTSTRAP_ADMIN_EMAIL = 'admin@su-dominio'
$env:MIRA_BOOTSTRAP_ADMIN_PASSWORD = '<≥12 caracteres, introducida por usted>'
dotnet MIRA.Api.dll --bootstrap-admin   # un segundo intento falla con código 1
Remove-Item Env:\MIRA_BOOTSTRAP_ADMIN_PASSWORD

# Pruebas
dotnet test MIRA.sln                      # 21 pruebas sin base de datos
$env:MIRA_TEST_DB = '<base de pruebas inicializada con database/init>'
dotnet test MIRA.sln                      # 47 pruebas: + procedimientos y regresión CRUD de V1 (datos con prefijo zz-reg-/sp-test-, eliminados al terminar)
cd frontend; pip install -r requirements-dev.txt; pytest -q   # 12 pruebas
```

Smoke HTTP verificado: sin token 401; credenciales inválidas 401; token basura 401; Investigador en escritura 403; usuario duplicado 409; rol inexistente 400; usuario inexistente 404; alta válida 201.
Pendientes: la base de pruebas SQL del dominio académico y la regresión manual en navegador de los 6 formularios V1.
