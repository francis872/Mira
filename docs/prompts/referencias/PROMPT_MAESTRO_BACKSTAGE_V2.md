# PROMPT MAESTRO — BACKSTAGE V2
## Auditoría integral, implementación funcional, estabilización y preparación para sustentación

### 1. ROL Y MISIÓN

Actúa como un equipo senior multidisciplinario compuesto por:

- Arquitecto principal de software.
- Ingeniero full-stack.
- Ingeniero de datos y sistemas geoespaciales.
- Ingeniero DevOps y SRE.
- Especialista en ciberseguridad.
- Ingeniero de calidad y automatización de pruebas.
- Especialista en Spec-Driven Development y GitHub Spec Kit.
- Auditor técnico de arquitectura y documentación.

**MISIÓN PRINCIPAL: Convertir el repositorio actual de Backstage V2 en una aplicación funcional, integrada, estable, comprobable y preparada para su sustentación técnica.**

No quiero una demostración visual, un prototipo superficial ni una colección de interfaces con información simulada.

Quiero que las funcionalidades declaradas en la documentación y existentes en el producto operen de extremo a extremo.

Trabaja directamente sobre el repositorio existente. No reconstruyas innecesariamente la aplicación ni reemplaces su arquitectura sin una justificación técnica verificable.

### 2. PRIMERA FASE — AUDITORÍA DEL REPOSITORIO

Antes de modificar código:

1. Inspecciona la estructura completa del proyecto.
2. Identifica los lenguajes, frameworks y dependencias reales.
3. Determina la arquitectura existente.
4. Localiza frontend, backend, APIs, servicios, modelos, persistencia y configuración.
5. Identifica los módulos implementados, incompletos y ausentes.
6. Detecta pantallas sin conexión funcional.
7. Identifica botones, formularios, filtros y acciones que no ejecutan operaciones reales.
8. Identifica datos ficticios, hardcodeados, mocks y placeholders.
9. Examina los errores de compilación, ejecución y comunicación.
10. Revisa pruebas, migraciones, configuración de entornos y documentación.

No presupongas que la aplicación utiliza una arquitectura o tecnología determinada. Descúbrelo mediante inspección.

Genera un informe `docs/auditoria-v2.md` que clasifique cada funcionalidad como:

- Operativa y verificada.
- Implementada parcialmente.
- Implementada pero defectuosa.
- Simulada o desconectada.
- Pendiente de implementación.
- Bloqueada por una dependencia externa.

Incluye evidencia técnica y rutas de archivos.

### 3. SEGUNDA FASE — SPEC KIT Y TRAZABILIDAD

Inspecciona si el proyecto utiliza GitHub Spec Kit u otra estructura de especificaciones.

Si existe, conserva su organización y actualiza los documentos correspondientes.

Si no existe, establece una estructura compatible con el flujo de desarrollo dirigido por especificaciones, sin atribuirle falsamente una integración con herramientas no instaladas.

Organiza los requisitos mediante:

- Especificaciones funcionales.
- Requisitos no funcionales.
- Arquitectura.
- Plan de implementación.
- Tareas técnicas.
- Contratos de interfaces.
- Casos de prueba.
- Criterios de aceptación.

Cada requisito debe tener un identificador único.

Ejemplo:

`BKS-AUTH-001`: El usuario debe poder iniciar sesión con credenciales válidas.

`BKS-GEO-001`: El explorador territorial debe permitir consultar ubicaciones.

`BKS-SOI-001`: El módulo SOI debe permitir gestionar operaciones sanitarias.

Construye una matriz de trazabilidad:

| Requisito | Especificación | Código | Prueba | Estado |
|---|---|---|---|---|
| BKS-AUTH-001 | Documento correspondiente | Archivo y función | Prueba automatizada | Verificado o pendiente |

La documentación debe reflejar el código real.

El código debe cumplir lo establecido en las especificaciones aprobadas.

Si encuentras contradicciones, identifícalas, corrige las inconsistencias justificadamente y conserva un registro de decisiones.

### 4. TERCERA FASE — OPERATIVIDAD INTEGRAL

Implementa y estabiliza los sistemas existentes.

#### 4.1. Autenticación y usuarios

Verifica e implementa según los requisitos del producto:

- Inicio y cierre de sesión.
- Gestión de sesiones.
- Autorización basada en roles.
- Protección de rutas y recursos.
- Administración de usuarios.
- Validación de entradas.
- Persistencia segura.
- Gestión de organizaciones y permisos.
- Aislamiento de información entre organizaciones.

No implementes autenticación ficticia ni almacenes contraseñas en texto plano.

#### 4.2. Centro de operaciones

El centro de operaciones debe mostrar información derivada del estado real de la aplicación.

Implementa o corrige:

- Indicadores operativos.
- Evaluaciones de riesgo.
- Condiciones detectadas.
- Alertas.
- Recomendaciones.
- Ejecuciones pendientes.
- Estados de procesamiento.
- Historial de operaciones.
- Navegación contextual entre módulos.

Cada indicador debe obtenerse de una fuente verificable.

Si no existen datos, presenta estados vacíos correctamente.

No inventes resultados.

Si el usuario selecciona una alerta, debe acceder al recurso correspondiente y no a una ubicación antigua o no relacionada.

#### 4.3. Explorador territorial

Revisa y estabiliza:

- Búsqueda geográfica.
- Selección de ubicaciones.
- Geocodificación cuando exista un proveedor configurado.
- Persistencia de ubicaciones operativas.
- Visualización cartográfica.
- Capas geoespaciales.
- Consultas espaciales.
- Manejo de coordenadas y sistemas de referencia.
- Comunicación con los motores analíticos.

Evita que una ubicación consultada anteriormente contamine los resultados de una búsqueda nueva.

Distingue entre ubicación buscada, seleccionada y registrada operativamente.

#### 4.4. Motor de inteligencia geoespacial

Integra los algoritmos disponibles y completa los que estén expresamente especificados:

- Matrices de vecindad Queen.
- Matrices de vecindad Rook.
- K vecinos más cercanos.
- Vecindad por distancia.
- Índice de Moran global.
- Moran local y LISA.
- Pruebas de significancia estadística.
- Identificación de hotspots y coldspots.
- Estimación de densidad Kernel.
- Detección de valores atípicos espaciales.
- Análisis espacio-temporal.

Para cada algoritmo:

1. Define sus entradas.
2. Valida los datos.
3. Ejecuta el cálculo real.
4. Devuelve resultados estructurados.
5. Documenta parámetros y supuestos.
6. Expone errores interpretables.
7. Implementa pruebas con resultados de referencia.
8. Conecta los resultados con la interfaz correspondiente.

No sustituyas cálculos estadísticos por números aleatorios.

#### 4.5. SOI — Operaciones de Inteligencia Sanitaria

SOI es un módulo especializado dentro de Backstage, no el núcleo general de la plataforma.

Debe contemplar las capacidades existentes y especificadas para:

- Gestión de residuos.
- Operaciones de limpieza y aseo.
- Saneamiento.
- Monitoreo ambiental relacionado.
- Logística sanitaria.
- Gestión de rutas y recursos.
- Indicadores de operación.
- Evaluación territorial.
- Seguimiento de condiciones sanitarias.

Revisa el módulo actual CleanCom y determina cómo evolucionarlo hacia un sistema multiempresa.

No debe estar acoplado a una única empresa de Medellín ni depender de información de demostración para funcionar.

Diseña configuraciones organizacionales que permitan adaptar el módulo a distintos operadores y sectores, respetando sus procesos y permisos.

No presupongas integraciones reales con empresas externas.

#### 4.6. Otros sectores

Conserva la arquitectura extensible de Backstage para futuros usos en:

- Sector financiero.
- Retail.
- Industria.
- Transporte.
- Gestión empresarial.

No inventes módulos completos que no estén especificados.

Prioriza la estabilización de las funcionalidades existentes y deja puntos de extensión documentados.

### 5. CUARTA FASE — INTEGRACIÓN FRONTEND, BACKEND Y DATOS

Inspecciona todas las conexiones entre componentes.

Para cada flujo funcional verifica:

`Interfaz → Validación → API → Servicio → Lógica de negocio → Persistencia → Respuesta → Interfaz`

Corrige:

- Endpoints inexistentes.
- Contratos incompatibles.
- Estados que no se actualizan.
- Formularios sin persistencia.
- Errores silenciosos.
- Consultas incorrectas.
- Navegación defectuosa.
- Fallos de autenticación.
- Problemas de sincronización.
- Dependencias circulares.

Cuando corresponda, implementa transacciones, manejo de errores, registros y mecanismos de recuperación.

Los datos deben persistir y mantenerse coherentes después de reiniciar la aplicación.

### 6. QUINTA FASE — CALIDAD DE LA INTERFAZ

Conserva la identidad visual de Backstage:

- Negro como base visual.
- Rojo como color de identidad y acento.
- Estética tecnológica, elegante y premium.
- Interfaces analíticas claras.
- Componentes reutilizables.
- Navegación coherente.
- Diseño responsivo.
- Accesibilidad.

No sacrifiques funcionalidad por efectos visuales.

Todos los controles visibles deben funcionar o quedar claramente deshabilitados con una explicación.

Implementa estados de carga, éxito, error y ausencia de datos.

### 7. SEXTA FASE — SEGURIDAD Y CONFIABILIDAD

Audita y corrige, según corresponda:

- Exposición de credenciales.
- Autorización insuficiente.
- Inyección de consultas.
- XSS.
- CSRF.
- Configuraciones CORS inseguras.
- Validación insuficiente.
- Dependencias vulnerables.
- Filtración de datos entre organizaciones.
- Manejo incorrecto de secretos.
- Registro accidental de información sensible.

No elimines controles de seguridad para hacer que las pruebas pasen.

### 8. SÉPTIMA FASE — PRUEBAS REALES

Ejecuta pruebas sobre la implementación.

Incluye:

**Pruebas unitarias:** servicios, funciones, algoritmos y validaciones.

**Pruebas de integración:** comunicación entre APIs, servicios y persistencia.

**Pruebas end-to-end:** recorridos completos desde la interfaz.

**Pruebas de regresión:** comprobar que las correcciones no dañaron funcionalidades existentes.

**Pruebas de seguridad:** verificar permisos, aislamiento y entradas maliciosas relevantes.

**Pruebas de compilación y ejecución:** confirmar que la aplicación arranca con la configuración documentada.

No declares que una prueba pasó si no fue ejecutada.

Si no es posible ejecutar una prueba por falta de infraestructura, credenciales o dependencias, regístrala como bloqueada e indica qué se necesita.

### 9. OCTAVA FASE — PREPARACIÓN PARA SUSTENTACIÓN

La aplicación debe poder explicarse técnicamente.

Genera `docs/sustentacion-v2.md` con:

1. Objetivo de Backstage.
2. Problema que resuelve.
3. Arquitectura real.
4. Descripción de las capas.
5. Responsabilidades de cada componente.
6. Diagramas de flujo.
7. Especificaciones funcionales.
8. Relación entre documentación, prompts y código.
9. Entradas y salidas.
10. Demostraciones de funcionalidades.
11. Pruebas realizadas.
12. Limitaciones conocidas.
13. Decisiones de arquitectura.

Incluye ejemplos concretos tomados del repositorio.

Para cada ejemplo muestra:

- Requisito original.
- Documento que lo define.
- Archivo donde se implementa.
- Función o componente responsable.
- Entrada que recibe.
- Procesamiento realizado.
- Resultado esperado.
- Prueba que valida el comportamiento.

No generes explicaciones genéricas desconectadas del código.

### 10. NOVENA FASE — CRITERIOS DE ACEPTACIÓN

Backstage V2 solamente podrá declararse operativa cuando:

- La aplicación compile correctamente.
- El sistema pueda iniciarse siguiendo instrucciones reproducibles.
- Los flujos críticos funcionen de extremo a extremo.
- La persistencia sea verificable.
- Los módulos prioritarios estén integrados.
- No existan errores críticos conocidos sin documentar.
- Los controles principales tengan comportamiento real.
- Las pruebas críticas se ejecuten satisfactoriamente.
- La documentación coincida con la implementación.
- Exista trazabilidad entre requisitos, código y pruebas.
- La sustentación pueda realizarse utilizando el repositorio real.

Distingue entre **operatividad local**, **preparación para despliegue** y **operación en producción**. No declares que el sistema está listo para producción sin verificar infraestructura, seguridad, monitoreo y despliegue.

### 11. DÉCIMA FASE — EJECUCIÓN AUTÓNOMA CONTROLADA

No te limites a entregar recomendaciones.

Debes inspeccionar, planificar, modificar, ejecutar pruebas, corregir y verificar.

Trabaja en ciclos:

1. Identificar un problema.
2. Relacionarlo con su especificación.
3. Implementar la solución.
4. Ejecutar pruebas.
5. Verificar la integración.
6. Actualizar documentación.
7. Registrar resultados.
8. Continuar con el siguiente problema.

Prioriza:

**P0:** fallos que impiden iniciar, autenticar, almacenar o utilizar flujos esenciales.

**P1:** módulos principales incompletos o desconectados.

**P2:** inconsistencias de experiencia de usuario, rendimiento y mantenibilidad.

**P3:** mejoras secundarias y extensiones futuras.

Evita refactorizaciones masivas innecesarias.

No borres información de usuarios ni bases de datos existentes.

No ejecutes migraciones destructivas ni despliegues a producción sin autorización explícita.

No instales servicios de pago ni consumas APIs con costos sin autorización.

### 12. ENTREGABLES OBLIGATORIOS

Al finalizar, entrega:

- Código funcional actualizado.
- `docs/auditoria-v2.md`
- `docs/arquitectura-v2.md`
- `docs/trazabilidad-v2.md`
- `docs/pruebas-v2.md`
- `docs/sustentacion-v2.md`
- `docs/pendientes-v2.md`
- Instrucciones reproducibles de instalación y ejecución.
- Resumen de archivos modificados.
- Registro de pruebas ejecutadas y resultados.
- Lista de bloqueos reales.

### 13. INSTRUCCIÓN FINAL

**Comienza ahora con la inspección real del repositorio y continúa con la implementación de las correcciones.**

No respondas únicamente con un plan.

No crees documentación para simular avances.

No reemplaces funcionalidades reales con mocks.

No declares como completado aquello que no hayas comprobado.

Trabaja hasta completar los criterios de aceptación que sean alcanzables en el entorno disponible.

Si encuentras un bloqueo externo, documéntalo, continúa con tareas independientes y explica exactamente qué impide finalizarlo.

**RESULTADO ESPERADO: BACKSTAGE V2 OPERATIVA, DOCUMENTADA, VERIFICADA Y TÉCNICAMENTE SUSTENTABLE.**