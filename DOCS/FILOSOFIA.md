# Filosofía de Structural Management V1

## Introducción

**Structural Management V1** nació de una premisa fundamental: la ingeniería estructural de calidad exige tanto rigor técnico como claridad organizacional. En un entorno donde los ingenieros trabajan con modelos analíticos complejos, múltiples revisiones y equipos multidisciplinarios, la ausencia de una plataforma de gestión estructurada genera errores, pérdida de trazabilidad y retrabajos costosos.

Este sistema fue concebido para resolver ese problema con una filosofía clara y principios técnicos sólidos.

---

## Principios Filosóficos Fundamentales

### 1. 🏛️ El Dominio del Negocio es Sagrado

La ingeniería estructural tiene su propio lenguaje: vigas, columnas, cortantes, derivas, espectros. El sistema está diseñado para que **el código hable el mismo idioma que el ingeniero**. Las clases del dominio no son abstracciones técnicas; son representaciones directas de los conceptos que los ingenieros usan a diario.

> *"El código debe leer como el problema que resuelve, no como la solución técnica que lo implementa."*

### 2. 🔒 Independencia de Herramientas Externas

SAP2000 es una herramienta poderosa, pero es externa al sistema. La filosofía del proyecto establece que **ninguna decisión de diseño interno debe depender de SAP2000**. La integración existe en una capa de adaptación aislada, de modo que el día que cambie la herramienta de análisis (SAP2000 v26, ETABS, OpenSees), el resto del sistema permanezca intacto.

> *"El sistema depende de abstracciones, no de implementaciones."*

### 3. 🔄 Trazabilidad Total del Conocimiento

Cada resultado de análisis tiene una historia: ¿de qué modelo proviene?, ¿qué comandos se ejecutaron?, ¿cuándo se realizó el análisis? La filosofía de **hidratación de fuentes** garantiza que cada conjunto de datos lleva consigo su metadata de origen, haciendo posible auditar y comparar revisiones con precisión.

### 4. 📐 Rigor Normativo sin Rigidez

El sistema aplica normas de ingeniería (ACI 318, NTE E030) con precisión, pero sin anclar la arquitectura a una norma específica. Los parámetros sísmicos, las combinaciones de carga y los factores de diseño son configurables, no codificados en piedra.

### 5. 🧩 Separación de Responsabilidades

Cada capa del sistema tiene **una única razón para cambiar**:
- El dominio cambia cuando cambia la ingeniería.
- Los casos de uso cambian cuando cambian los flujos de trabajo.
- La infraestructura cambia cuando cambia la tecnología de persistencia.
- La interfaz cambia cuando cambia la experiencia del usuario.

Ninguna capa contamina a otra.

### 6. 🚀 Extensibilidad como Virtud

El sistema fue diseñado para crecer. Agregar un nuevo formato de exportación (PDF, IFC), un nuevo repositorio (base de datos SQL), o un nuevo adaptador de análisis (ETABS) no requiere modificar el núcleo del sistema. Las interfaces son los puntos de extensión.

---

## Valores del Proyecto

| Valor | Manifestación en el Código |
|---|---|
| **Claridad** | Nombres de clases y métodos que describen intención, no implementación |
| **Cohesión** | Cada clase tiene una responsabilidad única y bien definida |
| **Desacoplamiento** | Las dependencias fluyen hacia adentro, nunca hacia afuera del dominio |
| **Verificabilidad** | El comportamiento del sistema es predecible y comprobable |
| **Sostenibilidad** | El código puede ser mantenido y extendido por cualquier desarrollador familiarizado con los conceptos |

---

## El Modelo Mental del Sistema

Imaginemos el sistema como una **mesa de trabajo de ingeniería**:

```
┌──────────────────────────────────────────────────────────┐
│                    Mesa de Trabajo                        │
│                   (App.WinForms)                         │
│                                                          │
│  ┌──────────────┐    ┌──────────────┐                    │
│  │   SAP2000    │    │  Repositorios│                    │
│  │ (Herramienta)│    │  (Archiveros)│                    │
│  └──────────────┘    └──────────────┘                    │
│         │                   │                            │
│  ┌──────┴───────────────────┴──────────────────────────┐ │
│  │          Reglas de Trabajo (App.Application)        │ │
│  │   "Cómo se crea un proyecto, cómo se hidrata,..."   │ │
│  └─────────────────────────────────────────────────────┘ │
│         │                                                │
│  ┌──────┴──────────────────────────────────────────────┐ │
│  │        Conceptos de Ingeniería (App.Domain)          │ │
│  │  "Qué es una viga, una columna, un espectro,..."     │ │
│  └─────────────────────────────────────────────────────┘ │
└──────────────────────────────────────────────────────────┘
```

---

## Filosofía de Diseño de Software Aplicada

### Clean Architecture (Robert C. Martin)

El proyecto adopta los principios de **Arquitectura Limpia**:

1. **Regla de Dependencia**: Las dependencias del código fuente solo pueden apuntar hacia adentro (hacia el dominio).
2. **Independencia de Frameworks**: El dominio y los casos de uso no dependen de Windows Forms, SAP2000 o ClosedXML.
3. **Independencia de la Base de Datos**: Los repositorios son intercambiables sin afectar la lógica de negocio.
4. **Independencia de la UI**: La lógica de negocio funciona independientemente de la interfaz gráfica.
5. **Testabilidad**: Cada capa puede probarse en aislamiento.

### Domain-Driven Design (Eric Evans)

El dominio del negocio guía el diseño:

- **Entidades con identidad**: `Project`, `SeismicSource`, `DesignSource` tienen identidad única por `Guid`.
- **Agregados**: `Project` es la raíz de agregado que controla sus revisiones y participantes.
- **Lenguaje Ubicuo**: Los términos del dominio (`Hydration`, `Seismic Source`, `Design Snapshot`) se usan consistentemente en código y documentación.

---

## Filosofía ante la Evolución

El sistema fue nombrado **V1** intencionalmente. Es una versión 1 que establece los fundamentos. Las decisiones tomadas aquí —la arquitectura, las abstracciones, el modelo de dominio— son inversiones a largo plazo que hacen posible una V2, V3, y más allá sin necesidad de reescrituras.

> *"El mejor código es el que no necesitas reescribir."*

---

*Siguiente: [Misión, Visión y Objetivos](./MISION_VISION_OBJETIVOS.md)*
