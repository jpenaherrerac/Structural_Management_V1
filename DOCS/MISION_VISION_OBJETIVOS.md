# Misión, Visión y Objetivos

## Misión

> **Empoderar a los equipos de ingeniería estructural con una plataforma de gestión que elimine la fricción entre el análisis computacional y la documentación técnica, garantizando trazabilidad, rigor normativo y eficiencia en cada proyecto.**

Structural Management V1 tiene como misión ser el punto de conexión entre el poder analítico de SAP2000 y la necesidad de organización, documentación y reporte que demanda la práctica profesional de la ingeniería estructural. El sistema no reemplaza al ingeniero ni al software de análisis; los complementa, automatizando las tareas repetitivas y liberando al profesional para que se concentre en lo que importa: **tomar decisiones de ingeniería informadas**.

---

## Visión

> **Ser la plataforma de referencia para la gestión de proyectos estructurales en el mercado hispanohablante, integrando las principales herramientas de análisis estructural bajo una arquitectura unificada, extensible y orientada al estándar de ingeniería local.**

### ¿A dónde va el sistema?

La visión de Structural Management V1 apunta a:

1. **Unificación de herramientas**: Un único entorno donde convivan SAP2000, ETABS, y otras plataformas de análisis bajo la misma interfaz de gestión.

2. **Normalización local**: Soporte nativo para los reglamentos de construcción latinoamericanos (NTE E030 del Perú, NTC-Sísmica de México, NSR-10 de Colombia), sin sacrificar compatibilidad con normas internacionales (ASCE 7, Eurocódigo 8).

3. **Colaboración en equipo**: Gestión multiusuario con control de revisiones, flujos de aprobación y registros de auditoría digital.

4. **Automatización inteligente**: Generación automática de memorias de cálculo, planos de diseño y reportes ejecutivos a partir de los datos extraídos de los modelos de análisis.

5. **Ecosistema abierto**: Arquitectura de plugins que permita a la comunidad de ingeniería agregar calculadoras, exportadores y adaptadores sin modificar el núcleo.

---

## Objetivos

### Objetivos Estratégicos (Largo Plazo)

| # | Objetivo | Indicador de Éxito |
|---|---|---|
| O1 | Soportar múltiples adaptadores de análisis estructural | SAP2000, ETABS, y al menos una herramienta open-source integradas |
| O2 | Persistencia en base de datos relacional | Migración de repositorios en memoria a SQL Server / SQLite |
| O3 | Generación automática de memorias de cálculo | Exportación a PDF con plantilla profesional |
| O4 | Soporte para múltiples normas sísmicas latinoamericanas | NTE E030, NTC-Sísmica, NSR-10 |
| O5 | Interfaz web complementaria | Dashboard de proyectos accesible desde navegador |

### Objetivos Tácticos (Mediano Plazo — V1.x)

| # | Objetivo | Estado |
|---|---|---|
| T1 | Integración estable con SAP2000 v25 via COM API | ✅ Implementado |
| T2 | Gestión completa del ciclo de vida de proyectos | ✅ Implementado |
| T3 | Extracción automatizada de resultados sísmicos | ✅ Implementado |
| T4 | Calculadoras de diseño ACI 318 para vigas, columnas y muros | ✅ Implementado |
| T5 | Exportación a CSV, XML y Excel | ✅ Implementado |
| T6 | Soporte para parámetros sísmicos NTE E030 | ✅ Implementado |
| T7 | Control de revisiones y comparación de modelos | ✅ Dominio implementado |
| T8 | Persistencia en base de datos | 🔄 Pendiente (actualmente en memoria) |
| T9 | Suite de pruebas automatizadas | 🔄 Pendiente |
| T10 | Módulo de reportes PDF | 🔄 Pendiente |

### Objetivos Operativos (Corto Plazo — V1.0)

| # | Objetivo | Descripción |
|---|---|---|
| Op1 | Estabilidad de la capa de integración SAP2000 | Sin fallos en la conexión COM durante sesiones de trabajo típicas |
| Op2 | Flujo completo de creación → análisis → exportación | El usuario puede ejecutar el flujo completo sin errores |
| Op3 | Documentación técnica completa | Todo el sistema documentado en la carpeta DOCS/ |
| Op4 | Código limpio y mantenible | Convenciones consistentes, sin deuda técnica acumulada |

---

## Propuesta de Valor

### Para el Ingeniero Estructural

- **Menos trabajo manual**: La extracción de resultados de SAP2000 es automática, no manual.
- **Menos errores**: Los datos fluyen directamente del modelo analítico al reporte, sin transcripción manual.
- **Más trazabilidad**: Cada dato tiene un origen identificable y verificable.
- **Cumplimiento normativo**: Los parámetros E030 y ACI 318 están integrados y validados.

### Para el Jefe de Proyecto

- **Visibilidad del estado**: Panel de control del proyecto con revisiones, participantes y estados.
- **Control de calidad**: Comparación entre revisiones para identificar cambios significativos.
- **Documentación lista**: Memorias de cálculo generadas automáticamente al cierre de revisiones.

### Para el Equipo de Desarrollo

- **Arquitectura sólida**: Clean Architecture facilita la incorporación de nuevos desarrolladores.
- **Extensibilidad**: Agregar nuevas funcionalidades no requiere modificar el código existente.
- **Independencia tecnológica**: El dominio de negocio no está acoplado a ningún framework o librería externa.

---

## Declaración de Propósito

Structural Management V1 existe porque los ingenieros estructurales merecen herramientas que estén a la altura de su trabajo. La ingeniería estructural es una disciplina exigente, con consecuencias reales en la seguridad de las personas. Este sistema es nuestra contribución a que esa disciplina se ejerza con mayor precisión, organización y confianza.

---

*Anterior: [Filosofía](./FILOSOFIA.md) | Siguiente: [Arquitectura](./ARQUITECTURA.md)*
