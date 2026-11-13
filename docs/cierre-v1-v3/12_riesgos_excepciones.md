# Riesgos vigentes después de Fase 5

V2 sigue sin cierre: HU03-CA03 (perfil propio), HU05-CA02 (exactamente un líder por periodo), HU06-CA02 (periodo académico) y HU10-CA02/03 (Facultad/filtro completo). HU01-DESCRIPCION requiere aceptar tres roles cerrados frente a creación dinámica. No hay dispensas inventadas.

C06 permanece pendiente: «En el SQL confirmado hay 22 columnas FK públicas NOT NULL. ¿Ratifica que C06 no aplica a este modelo o identifica una relación opcional concreta? Indicar tabla/columna, cardinalidad, documento fuente y aceptación escrita». No se modificó nulabilidad.

Datos heredados: 009 aborta si falta inglés V1 o hay categorías incompatibles; no convertir desconocidos automáticamente. 010 preserva código/inglés/descripción faltantes con CHECK NOT VALID; completar y validar antes de operar esas filas. 012 no inventa mínimos de líneas viejas sin relaciones. 014 preserva IDs/rótulos antiguos y añade solo objetivos oficiales ausentes; no deduplica variantes arbitrarias institucionales.

Reportes reúnen filas en memoria y generan archivos al solicitarse; no se midió carga/volumen institucional. La UI usa avisos entre pestañas y sondeo visible de 15 segundos, sin garantía de actualización instantánea durante fallos de red. Se conserva CDN Bootstrap y alcance de sesión de una instancia.

Seguridad: bcrypt/JWT/roles/revocación/CSRF/D01/workflow preservados y probados localmente. El logout durante caída API limpia sesión local pero no puede confirmar revocación remota. OSV/NuGet son consultas fechadas, no auditoría de todo el SO/base. Dispositivos reales y lector de pantalla pendientes; cero infracciones axe no certifica WCAG.

CI01 ya no bloquea la configuración local: Fase 4 preservada y revalidada. Sigue pendiente publicación autorizada y ejecución Actions real; no confundir con verde remoto. Producción Render/Neon/TLS/pooler/disco/redeploy/carga no probada. No desplegar automáticamente.

---

## Corte previo conservado como antecedente

# Riesgos, defectos y excepciones abiertas

Registro de este corte; P0 bloquea producción, P1 bloquea el cierre/entrega correspondiente y P2 requiere mejora o decisión delimitada. Prioridades son propuestas, no aceptación formal del docente. La matriz conserva cada criterio original y evidencia.

| ID local | Hallazgo / estado | Impacto y evidencia | Responsable sugerido / condición de cierre |
|---|---|---|---|
| D01 | Corregido localmente y revalidado | Orígenes independientes, firma/cuotas/falsificación comprobados; aún no publicado | Equipo; publicar solo tras aprobación y CI. Validar proxy real antes de producción |
| B01 | No reproducido en el corte actual | Build inicial con caché y reconstrucción final --no-cache --pull correcta; 43 E2E repetidos sobre imágenes frescas; no certifica red futura | Operación; conservar logs actuales y repetir en entorno de entrega |
| C06 | Pendiente de aceptación docente P1 | 22 FK públicas, cero opcionales, sin nueva dispensa/modelo localizado | Docente/equipo; acta de no aplicabilidad o fuente de relación opcional y cambio aprobado |
| CI01 | Bloqueo técnico P1, sin corregir | LOGIN_FORWARDING_KEY ausente en quality; Compose exit1, con fixture exit0 | Equipo/propietario; autorizar una línea, repetir configuración y después ejecutar Actions autorizado |
| H-CAT | Diferencias históricas V1 P1 | Categoría ODS libre, inglés opcional; contraejemplos y Service/DDL | Docente/equipo; decidir alcance o aprobar correcciones y tests |
| H-MODELO | Diferencias históricas V2 P1 | Código sede ausente, sede inactiva aceptada, cero/líderes fechados superpuestos, descripción NULL, línea inactiva asignable, línea sin ODS/disciplina | Docente/equipo; alcance formal o nuevas migraciones/reglas aprobadas; no editar 001–008 |
| H-UI | Parciales históricos P1/P2 | Sin autoservicio docente, multiselección, confirmación de puentes, resumen de líneas; búsqueda no sustituye filtros por tutor | Docente/equipo; priorizar y autorizar, sin implementar durante este cierre |
| H-REPORTES | No implementado P1 si se mantiene alcance original | Exportaciones integrantes/consolidadas y reporte unificado con Facultad no existen | Docente; definir entrega exigible y aprobar alcance. No inventar que forma parte de V4 |
| H-AMBIGUOS | Decisiones semánticas P2 | Tutor duplica intervalo exacto: rechazado; solapados distintos: admitidos. Periodo académico expresado en fechas. Estado línea solo permite retiro. Selectores refrescan al cargar | Docente/equipo; aclarar significado antes de nuevas reglas |
| AUTH-ROLE | Limitación del alcance P1 académico | Roles cerrados en V3, asignación sí; descripción original pedía crear roles | Docente/equipo; aceptar alcance fijo o aprobar diseño; no agregar roles sin políticas |
| SEC-LOGOUT | Riesgo residual P1 operativo | API caída: borra local, advierte; JWT aún200 tras retorno; explícita revocación luego401 | Operación/seguridad; aceptar/documentar o autorizar solución adicional; no prometer revocación inexistente |
| SEC-LIMIT | Límite por instancia P1 antes de escala | NAT puede compartir cuota; memoria reinicia con API, no distribuida | Operación; validar topología/carga y diseñar solución compartida si se autoriza escala |
| SEC-SESS | Filesystem privado y una instancia P0 cloud | CacheLib requiere archivos confiables; disco/redeploy/permisos reales pendientes | Operación; prueba aislada, volumen privado UID10001 y política de recuperación |
| SEC-SUPPLY | Alcance parcial de auditoría P1/P2 | OSV18/NuGet sin avisos consultados; no prueba ausencia universal, ni CVE del SO base | Seguridad; auditoría OS/capas/digests antes de producción |
| A11Y | Manual pendiente P1/P2 | Edge50, axe18 cero infracciones/incomplete, teclado10 y zoom27 aprobados; lectores/físicos no probados | QA con usuario asistivo; tareas documentadas en guía de pruebas, sin afirmar WCAG |
| CLOUD | Producción no validada P0 | Render/Neon/TLS/privilegios/pooler/redeploy/restore/carga no ejecutados | Propietario/operador; autorizar solo entorno de pruebas, presupuesto y pruebas específicas |
| DOC-ORIGEN | Fuentes del curso incompletas P1 académico | VERSION_2.md/mapa/nombres oficiales faltantes; no acta docente | Docente/equipo; aportar fuentes o aceptar formalmente la consolidación disponible |
| PUBLICACION | Cambios locales sin publicar P1 entrega | HEAD remoto sigue3105273; PR #7 Draft y sin runs; nuevos documentos no están en GitHub | Propietario; aprobación explícita de grupos, commits/push y revisión; no merge automático |

No se detectaron secretos reales en los candidatos revisados ni en el contenido inspeccionado de /app de las imágenes; el informe de seguridad final documenta alcance y heurísticas. Fixtures de tests/CI son públicas y desechables, nunca credenciales productivas. No se publican .env, .local, sesiones, logs privados ni documentos privados nuevos.

Los cambios funcionales pendientes requieren aprobación. Este registro no autoriza implementarlos ni inventa nuevas relaciones para satisfacer una rúbrica. Las reglas actuales derivadas del SQL confirmado se preservan, aunque difieran del DOCX original. Un cierre académico necesitará resolver esa contradicción, no ocultarla con porcentajes de tests.
