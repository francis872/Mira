# MANUAL DE IDENTIDAD VISUAL Y MARCA CORPORATIVA — MIRA (`MANUAL_DE_MARCA.md`)

> **Marca:** MIRA — Módulo de Investigación para la Gestión de Proyectos Académicos  
> **Institución:** Universidad de San Buenaventura Medellín  
> **Archivo Técnico Vinculado:** [`frontend/static/css/marca.css`](frontend/static/css/marca.css)

---

## 1. Propósito de la Marca e Identidad Corporativa

**MIRA** es la plataforma institucional de gestión, articulación y analítica científica que conecta a docentes investigadores, grupos de investigación, semilleros, líneas estratégicas, áreas OCDE y Objetivos de Desarrollo Sostenible (ODS). Su identidad visual transmite **rigor académico, trazabilidad científica, solidez tecnológica y accesibilidad universal**.

---

## 2. El Logosímbolo y sus Versiones Autorizadas

El logosímbolo de MIRA está compuesto por dos elementos indivisibles:
1. **Isotipo Académico:** El birrete de posgrado e investigación (`bi-mortarboard-fill`) inscrito en un contenedor cuadrado de bordes suavizados (`border-radius: 8px`).
2. **Logotipo Tipográfico:** La sigla **MIRA** en caja alta con tipografía `Plus Jakarta Sans ExtraBold (800)` y espaciado `+0.4px`.

### Versiones Oficiales del Logosímbolo
| Versión | Uso Autorizado | Configuración Visual |
| :--- | :--- | :--- |
| **Versión Principal (Sobre Fondo Oscuro / Vino)** | Barra de navegación superior (`navbar-mira`) y cabeceras institucionales. | Isotipo en contenedor blanco `#FFFFFF` con icono en Rojo Institucional `#7F1D1D` + texto **MIRA** en blanco `#FFFFFF`. |
| **Versión Positiva (Sobre Fondo Claro)** | Documentos, reportes impresos y tarjetas sobre superficie `#FFFFFF` o `#F1F5F9`. | Isotipo en contenedor `#7F1D1D` con icono `#FFFFFF` + texto **MIRA** en Pizarra Nocturno `#0F172A`. |
| **Versión Monocromática** | Pie de página (`mira-footer`), sellos de agua y entornos de una sola tinta. | Todo el conjunto en blanco `#FFFFFF` (sobre fondo oscuro) o `#0F172A` (sobre fondo blanco). |

---

## 3. Paleta Cromática Oficial y Verificación de Contraste (WCAG 2.1 AA)

Todos los colores de la aplicación están declarados como variables CSS en [`frontend/static/css/marca.css`](frontend/static/css/marca.css). Ninguna vista utiliza colores arbitrarios fuera de esta tabla:

| Variable en `marca.css` | Nombre del Color | Hexadecimal | RGB | Relación de Contraste (WCAG AA) | Rol en la Interfaz |
| :--- | :--- | :---: | :---: | :---: | :--- |
| `--mira-primary` | **Rojo Institucional Profundo** | `#7F1D1D` | `rgb(127, 29, 29)` | **8.12 : 1** sobre `#FFFFFF` (Cumple AAA) | Color primario de marca, cabeceras, botones principales y activos. |
| `--mira-primary-dark` | **Borgoña Académico** | `#450A0A` | `rgb(69, 10, 10)` | **14.8 : 1** sobre `#FFFFFF` (Cumple AAA) | Gradientes profundos en Hero Banner y barra de navegación. |
| `--mira-secondary` | **Azul Pizarra Nocturno** | `#0F172A` | `rgb(15, 23, 42)` | **17.4 : 1** sobre `#FFFFFF` (Cumple AAA) | Encabezados de tablas (`thead`), títulos `h1–h6` y pie de página. |
| `--mira-accent` | **Ámbar Académico** | `#D97706` | `rgb(217, 119, 6)` | **3.10 : 1** sobre `#FFFFFF` (*Uso restringido*) | **Regla de contraste:** No se usa para texto pequeño sobre blanco; se emplea como fondo de botones/badges con texto oscuro `#0F172A` (**8.9 : 1** Cumple AAA) o iconos sobre fondo oscuro. |
| `--mira-bg` | **Gris Azulado Claro** | `#F1F5F9` | `rgb(241, 245, 249)` | Fondo base | Superficie general de descanso visual de la aplicación. |
| `--mira-surface` | **Blanco Puro** | `#FFFFFF` | `rgb(255, 255, 255)` | Superficie | Tarjetas de módulos, paneles KPI, formularios y tablas. |
| `--mira-text-body` | **Gris Pizarra Cuerpo** | `#334155` | `rgb(51, 65, 85)` | **9.35 : 1** sobre `#FFFFFF` (Cumple AAA) | Párrafos, celdas de datos y etiquetas de formularios. |

---

## 4. Tipografías Autorizadas

1. **Tipografía de Títulos e Identidad (`--mira-font-sans`):**
   - **`Plus Jakarta Sans`** (Pesos `600 SemiBold`, `700 Bold`, `800 ExtraBold`): Utilizada en el logotipo MIRA, encabezados `h1` a `h5`, valores numéricos de tarjetas KPI y títulos de secciones.
2. **Tipografía de Lectura y Tablas de Datos:**
   - **`Inter`** (Pesos `400 Regular`, `500 Medium`, `600 SemiBold`): Optimizada para alta legibilidad en tablas extensas, selectores desplegables, formularios y navegación.
3. **Tipografía Monoespaciada (`--mira-font-mono`):**
   - **`JetBrains Mono`**: Reservada exclusivamente para códigos GrupLAC (`COL0012345`), identificadores técnicos, rutas de API y bloques de código.

---

## 5. Tamaño Mínimo y Área de Reserva

- **Tamaño Mínimo Digital:** El isotipo nunca debe representarse en pantalla con una altura inferior a **`32 px × 32 px`**, y el conjunto completo nunca debe ser menor a **`110 px` de ancho**, garantizando la legibilidad del birrete y de la sigla MIRA en dispositivos móviles.
- **Área de Reserva (Zona de Exclusión):** Alrededor del logosímbolo debe respetarse un margen libre mínimo equivalente a **`0.5X`** (donde `X` es la altura del contenedor del isotipo, es decir, mínimo `16 px` libres de otros textos o bordes).

---

## 6. Usos Incorrectos Prohibidos

1. **Prohibido distorsionar las proporciones:** No estirar horizontal ni verticalmente el isotipo ni el texto MIRA.
2. **Prohibido alterar los colores de marca:** No aplicar colores chillones (verde neón, fucsia, cian puro) fuera de las variables declaradas en `marca.css`.
3. **Prohibido usar texto blanco sobre fondo Ámbar (`#D97706`) en tamaños pequeños:** Por norma de contraste WCAG AA, sobre fondo ámbar o amarillo siempre debe emplearse texto oscuro (`#0F172A`).
4. **Prohibido usar valores hexadecimales sueltos en las plantillas HTML:** Todo estilo personalizado debe invocar `var(--mira-...)` desde `frontend/static/css/marca.css`.
