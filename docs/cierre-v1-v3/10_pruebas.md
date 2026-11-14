# Pruebas actuales — Fase 6

SAPIENS está operativa en [https://sapiens-v1-v3-20261112.vercel.app](https://sapiens-v1-v3-20261112.vercel.app), con Flask en Vercel Hobby, API .NET en Render Free y PostgreSQL 16 en Neon Free. La URL Production aprobó **50 criterios funcionales (49 en la pasada completa y repetición puntual aprobada tras un timeout TLS)**, 14 comprobaciones de seguridad/operación y una revisión de navegador de escritorio, tableta y móvil emulados. GitHub Actions continúa bloqueado por la cuenta; el PR #7 permanece Draft y sin merge. V2 mantiene pendientes académicos, incluido C06. No se declara cerrada toda la misión ni certificada producción institucional.

138 .NET / 135 Flask / 51 HTTP / 11 SQL y repetición 51 HTTP Docker: cero fallos u omisiones. [Resultados locales](evidencias/fase6-20261112/resultados.json), [remotos y cronología de fallos](evidencias/fase6-20261112/remoto.json), [seguridad Production](evidencias/fase6-20261112/seguridad-production.json), [comandos](evidencias/fase6-20261112/comandos.json). No sumar las repeticiones como pruebas distintas.

Preview: 110 responsive, 38 axe sin infracciones y 2 incomplete, teclado y 51 zoom reales. Production: 11 responsive y 8 axe sin infracciones. JWT expiró esperando sus 60 s durante Preview contra la misma API; JWT 20 min normal restaurado. No se probaron dispositivos físicos, lector de pantalla, carga institucional ni recuperación periódica.

## Antecedente Fase 5: ejecutado el 7 de noviembre, consolidado el 12


Fecha del corte: 2026-11-12T19:47:19.807350-05:00. [Resultados y hashes de reportes crudos](evidencias/fase5-20261107/resultados.json), [nombres/outcomes](evidencias/fase5-20261107/casos.json), [código exacto](evidencias/fase5-20261107/version-fuentes.json). No sumar repeticiones Docker como casos distintos.

| Conjunto | Aprobadas / total | Fallidas | Omitidas |
|---|---:|---:|---:|
| .NET/PostgreSQL | 136 / 136 | 0 | 0 |
| Flask | 120 / 120 | 0 | 0 |
| HTTP real | 51 / 51 | 0 | 0 |
| SQL | 10 / 10 suites | 0 | 0 |
| HTTP Linux/Gunicorn | 51 / 51 | 0 | 0 |
| Responsive Edge | 110 combinaciones | 0 | 0 |
| axe-core | 38 revisiones | 0 infracciones | 2 incomplete |
| Teclado histórico | 10 grupos | 0 | 0 |
| Recorridos académicos adicionales | 5 grupos | 0 | 0 |
| Zoom real 100/200/400% | 51 comprobaciones | 0 | 0 |
| D01 integrado | 3 grupos | 0 | 0 |

Referencia Fase 4: 90 .NET / 98 Flask / 43 HTTP / 5 SQL. Ahora 46 .NET, 22 Flask, 8 HTTP y 5 suites SQL adicionales netas. Se mantienen las aserciones de seguridad y regresión; fixtures válidas incluyen nuevos campos/referencias. Se conservaron intentos fallidos por contrato de respuesta, código SQL, ruta de test y foco del modal; no se contaron como aprobados.

Entorno: PostgreSQL 16.15 exclusivo, .NET 10, venv limpio, Edge y Docker Linux; sin datos reales ni bases remotas. Comandos ejecutados y logs privados se identifican en [control de ejecución](evidencias/fase5-20261107/comandos.json). `dotnet test` sin MIRA_TEST_DB puede omitir integraciones: no equivale a este resultado. Nuevas suites: V1RequisitosOriginalesTests, V2RequisitosOriginalesTests, V2ReportesTests, V2MinimosLineaTests, V2EstadoLineaTests; test_v1_requisitos.py, test_v2_requisitos.py, test_reportes.py, test_fase5_academico.py y cinco SQL nuevos.

La prueba de modal encontró salida de Tab a la barra del navegador; se corrigió el ciclo solo en la confirmación nueva y se repitió. La comprobación real de sedes crea una sede mediante POST autenticado y verifica su aparición sin recargar el formulario por el mismo canal de avisos. Las exportaciones se leen como XLSX/PDF reales, comparan filas/filtros, prueban vacíos/errores y texto tipo fórmula como dato literal. La prueba concurrente garantiza que solo una de dos eliminaciones que vaciarían el mínimo puede confirmar.

[Upgrade/recuperación](evidencias/fase5-20261107/upgrade-recuperacion.json): fallo preflight sin cambios, 009–014 en base V3 representativa, preservación de todas las filas/columnas anteriores salvo rótulo económico documentado y 15 ODS añadidos; restauración separada y regresión de seguridad V3. No acredita ensayo con todos los datos reales.

Pendientes manuales: lector de pantalla, Safari/iOS y Android físicos, teclado virtual/táctil/orientación, contenido institucional largo, validación humana WCAG, expiración por espera real completa. El zoom real local ya se ejecutó; no confundirlo con pinch zoom. Actions remoto y producción Render/Neon/TLS/redeploy/carga no están validados.
