# Pruebas actuales — Fase 5

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
