# Arquitectura del Sistema

## Resumen Ejecutivo

**Structural Management V1** implementa una **Arquitectura Limpia (Clean Architecture)** organizada en cinco proyectos .NET que siguen la regla de dependencia: las capas externas dependen de las internas, nunca al revés. El dominio de negocio es el núcleo inmutable del sistema; todo lo demás es un detalle de implementación.

---

## Diagrama de Capas

```
┌─────────────────────────────────────────────────────────────────┐
│                        PRESENTACIÓN                              │
│                       App.WinForms                               │
│  ┌─────────────────┐  ┌──────────────────┐  ┌────────────────┐  │
│  │   MainForm.cs   │  │ NewProjectDialog  │  │ E030UserControl│  │
│  │  (Ventana Ppal) │  │ (Crear Proyecto) │  │ (Parámetros    │  │
│  │                 │  │                  │  │  sísmicos E030)│  │
│  └─────────────────┘  └──────────────────┘  └────────────────┘  │
│  ┌─────────────────────────────────────────────────────────────┐ │
│  │              Program.cs (Inyección de Dependencias)          │ │
│  └─────────────────────────────────────────────────────────────┘ │
└──────────────────────────────┬──────────────────────────────────┘
                               │ usa
        ┌──────────────────────┼──────────────────────┐
        │                      │                      │
┌───────┴───────┐    ┌─────────┴──────┐   ┌───────────┴──────────┐
│ App.Application│   │  App.SAP2000   │   │  App.Infrastructure  │
│ (Casos de Uso) │   │  (Adaptador)   │   │  (Infraestructura)   │
│                │   │                │   │                      │
│ ■ CreateProject│   │ ■ SapAdapter   │   │ ■ ProjectRepository  │
│ ■ HydrateSeism.│   │ ■ SapConnection│   │ ■ SeismicSourceRepo  │
│ ■ HydrateDsgn. │   │ ■ SapStructure │   │ ■ DesignSourceRepo   │
│                │   │   OutputReader │   │ ■ CsvExporter        │
│ ■ ISapAdapter  │   │ ■ SapDesign    │   │ ■ XmlExporter        │
│ ■ IProjectRepo │   │   DataReader   │   │ ■ XlsExporter        │
│ ■ ISeismicRepo │   │                │   │                      │
│ ■ IDesignRepo  │   │                │   │                      │
│                │   │                │   │                      │
│ ■ BeamCalc.    │   │                │   │                      │
│ ■ ColumnCalc.  │   │                │   │                      │
│ ■ WallCalc.    │   │                │   │                      │
└───────┬───────┘    └─────────┬──────┘   └──────────────────────┘
        │                      │
        └──────────┬───────────┘
                   │ depende de
        ┌──────────┴──────────────────────────────────────────────┐
        │                     App.Domain                           │
        │                  (Núcleo del Sistema)                    │
        │                                                          │
        │  Entities: Project, SeismicSource, DesignSource,        │
        │  Beam, Column, ShearWall, ModalResult, DriftResult,     │
        │  BeamDesignData, ColumnDesignData, WallDesignData, ...  │
        │                                                          │
        │  Enums: ActionType, ElementType, ExportFormat, ...      │
        └──────────────────────────────────────────────────────────┘
                               ↑
                   Sin dependencias externas
```

---

## Proyectos del Sistema

### 1. App.Domain — El Núcleo

**Propósito**: Contiene las entidades de negocio puras, enumeraciones y reglas de dominio. No tiene dependencias hacia ningún otro proyecto del sistema.

**Principio rector**: Las entidades del dominio representan conceptos de ingeniería estructural, no artefactos de software.

```
App.Domain/
├── Entities/
│   ├── Annexes/        # Modelos de datos para anexos de diseño
│   ├── Comparison/     # Entidades para comparación de revisiones
│   ├── Design/         # Datos de diseño estructural
│   ├── Documentation/  # Estructura de reportes y documentación
│   ├── Elements/       # Elementos estructurales (viga, columna, muro, losa)
│   ├── Loads/          # Cargas, combinaciones, espectros
│   ├── Project/        # Proyecto, revisiones, participantes
│   ├── Sap/            # Sesión y referencia al modelo SAP2000
│   ├── Seismic/        # Resultados sísmicos (modal, historia, derivas)
│   └── Sources/        # Fuentes de datos (SeismicSource, DesignSource)
└── Enums/              # Tipos enumerados del dominio
```

**Dependencias**: Ninguna (proyecto base).

---

### 2. App.Application — Los Casos de Uso

**Propósito**: Orquesta los flujos de trabajo del sistema. Define las interfaces (contratos) que las capas externas deben implementar. Contiene las calculadoras de diseño y la lógica de exportación.

**Principio rector**: Los casos de uso son la razón de existir del sistema. Todo lo demás les sirve a ellos.

```
App.Application/
├── Interfaces/
│   ├── ISapAdapter.cs              # Contrato de integración con SAP2000
│   ├── IProjectRepository.cs       # Contrato de persistencia de proyectos
│   ├── ISeismicSourceRepository.cs # Contrato de persistencia sísmica
│   └── IDesignSourceRepository.cs  # Contrato de persistencia de diseño
├── UseCases/
│   ├── CreateProjectUseCase.cs         # Crear nuevo proyecto
│   ├── HydrateSeismicSourceUseCase.cs  # Extraer resultados sísmicos de SAP2000
│   └── HydrateDesignSourceUseCase.cs   # Extraer resultados de diseño de SAP2000
├── Annexes/
│   ├── BeamDesignCalculator.cs         # Cálculo de vigas (ACI 318)
│   ├── ColumnDesignCalculator.cs       # Cálculo de columnas (ACI 318)
│   └── ShearWallDesignCalculator.cs    # Cálculo de muros de corte (ACI 318)
└── Export/
    ├── IAnnexExporter.cs       # Interfaz de exportación de anexos
    ├── CsvAnnexExporter.cs     # Implementación CSV
    └── XmlAnnexExporter.cs     # Implementación XML
```

**Dependencias**: `App.Domain`

---

### 3. App.Infrastructure — La Infraestructura

**Propósito**: Implementaciones concretas de los repositorios y exportadores definidos como interfaces en la capa de aplicación. Actualmente usa almacenamiento en memoria, preparado para migración a base de datos.

```
App.Infrastructure/
├── Repositories/
│   ├── ProjectRepository.cs        # Dictionary<Guid, Project> en memoria
│   ├── SeismicSourceRepository.cs  # Almacenamiento en memoria
│   └── DesignSourceRepository.cs   # Almacenamiento en memoria
└── Export/
    ├── CsvExporter.cs   # Exportación CSV general
    ├── XmlExporter.cs   # Exportación XML general
    └── XlsExporter.cs   # Exportación Excel (ClosedXML)
```

**Dependencias**: `App.Application`, `App.Domain`

---

### 4. App.SAP2000 — El Adaptador

**Propósito**: Encapsula completamente la integración con SAP2000 a través de su API COM. Ningún tipo específico de SAP2000 escapa de esta capa; el resto del sistema no sabe que SAP2000 existe.

```
App.SAP2000/
└── Adapters/
    ├── SapAdapter.cs               # Implementa ISapAdapter, delega a servicios
    ├── SapConnectionService.cs     # Gestión de conexión y operaciones COM
    ├── SapStructureOutputReader.cs # Lectura de resultados sísmicos
    └── SapDesignDataReader.cs      # Lectura de resultados de diseño
```

**Dependencias**: `App.Application`, `App.Domain`, `SAP2000v1.dll` (COM)

---

### 5. App.WinForms — La Presentación

**Propósito**: Interfaz gráfica de usuario. Es el punto de entrada de la aplicación y la raíz de composición (Composition Root) donde se ensambla el árbol de dependencias.

```
App.WinForms/
├── Program.cs          # Raíz de composición + punto de entrada
├── MainForm.cs         # Formulario principal
├── Forms/
│   ├── NewProjectDialog.cs     # Diálogo de nuevo proyecto
│   ├── SeismicityForm.cs       # Formulario de análisis sísmico
│   └── *.Designer.cs           # Definición visual (auto-generado)
└── UserControls/
    ├── E030/
    │   ├── E030UserControl.cs           # Control parámetros sísmicos E030
    │   ├── E030Tables.cs                # Tablas de valores E030
    │   └── SeismicParametersEventArgs.cs
    └── Espectro/
        ├── EspectroUserControl.cs       # Visualización espectro de respuesta
        └── EspectroUserControl.Designer.cs
```

**Dependencias**: `App.Application`, `App.Infrastructure`, `App.SAP2000`

---

## Patrones de Diseño Aplicados

### Repository Pattern
```
IProjectRepository (App.Application/Interfaces)
        ↑ implementa
ProjectRepository (App.Infrastructure/Repositories)
```
Abstrae la persistencia. Cambiar de memoria a SQL Server solo requiere una nueva implementación de `IProjectRepository`.

### Adapter Pattern
```
ISapAdapter (App.Application/Interfaces)
     ↑ implementa
SapAdapter (App.SAP2000/Adapters)
     │ delega a
SapConnectionService + SapStructureOutputReader + SapDesignDataReader
```
Protege al sistema del API COM de SAP2000. Si SAP2000 cambia o se reemplaza, solo cambia `App.SAP2000`.

### Use Case Pattern
Cada flujo de trabajo es una clase independiente con un único método de entrada:
```csharp
CreateProjectUseCase.Execute(name, code, ...) → Guid
HydrateSeismicSourceUseCase.Execute(projectId) → SeismicSourceId
HydrateDesignSourceUseCase.Execute(projectId) → DesignSourceId
```

### Strategy Pattern
```
IAnnexExporter (App.Application/Export)
    ↑ implementan
CsvAnnexExporter   (App.Application/Export)
XmlAnnexExporter   (App.Application/Export)
XlsExporter        (App.Infrastructure/Export)
```
Permite agregar nuevos formatos de exportación (PDF, IFC) sin modificar código existente.

### Dependency Injection (Manual)
La composición del sistema ocurre en `Program.cs`:
```csharp
var projectRepo     = new ProjectRepository();
var seismicRepo     = new SeismicSourceRepository();
var designRepo      = new DesignSourceRepository();
var sapAdapter      = new SapAdapter();
var createProject   = new CreateProjectUseCase(projectRepo);
var hydrateSeismic  = new HydrateSeismicSourceUseCase(sapAdapter, seismicRepo);
var hydrateDesign   = new HydrateDesignSourceUseCase(sapAdapter, designRepo);

Application.Run(new MainForm(sapAdapter, createProject, hydrateSeismic, hydrateDesign));
```

---

## Flujo de Datos Principal

```
┌─────────┐    ┌──────────────┐    ┌─────────────┐    ┌────────────┐
│ Usuario │───>│  MainForm    │───>│  UseCase    │───>│ SAP2000    │
│         │    │ (Presenta)   │    │ (Orquesta)  │    │ (Analiza)  │
└─────────┘    └──────────────┘    └──────┬──────┘    └────────────┘
                                          │                  │
                                   ┌──────┴──────┐           │
                                   │  Dominio    │<──────────┘
                                   │ (Entidades) │
                                   └──────┬──────┘
                                          │
                                   ┌──────┴──────┐
                                   │Repositorios │
                                   │ (Persiste)  │
                                   └─────────────┘
```

---

## Regla de Dependencia

```
App.WinForms
    ↓ depende de
App.Application  ←→  App.SAP2000  ←→  App.Infrastructure
    ↓ depende de
App.Domain
    ↓
(nada — núcleo del sistema)
```

**Regla crítica**: Las flechas de dependencia siempre apuntan hacia `App.Domain`. Ningún proyecto del dominio conoce la existencia de Windows Forms, SAP2000 o ClosedXML.

---

## Tecnologías por Capa

| Capa | Proyecto | Tecnologías |
|---|---|---|
| Presentación | App.WinForms | Windows Forms, .NET Framework 4.8 |
| Aplicación | App.Application | C# 9.0, .NET Framework 4.8 |
| Infraestructura | App.Infrastructure | ClosedXML, DocumentFormat.OpenXml |
| Adaptador SAP2000 | App.SAP2000 | SAP2000v1.dll (COM), Microsoft.CSharp |
| Dominio | App.Domain | C# 9.0 puro (sin dependencias) |

---

## Decisiones de Arquitectura (ADR)

### ADR-001: Repositorios en Memoria

**Decisión**: Implementar repositorios en memoria (`Dictionary<Guid, T>`) en la versión 1.

**Razón**: Simplifica el desarrollo inicial y permite validar los flujos de trabajo sin overhead de base de datos. La interfaz `IProjectRepository` garantiza que la migración a SQL sea no disruptiva.

**Consecuencia**: Los datos no persisten entre sesiones. Aceptable para V1.

### ADR-002: Inyección de Dependencias Manual

**Decisión**: No usar un contenedor IoC (Autofac, Unity, MS DI) en V1.

**Razón**: El grafo de dependencias es pequeño y estable. Un contenedor IoC agregaría complejidad sin beneficio proporcional.

**Consecuencia**: `Program.cs` asume responsabilidad de composición. Migrar a un contenedor en V2 es directo.

### ADR-003: COM API en Capa Aislada

**Decisión**: Confinar toda interacción con SAP2000 COM API en `App.SAP2000`.

**Razón**: Las APIs COM son verbosas, inestables entre versiones y no testeables. Aislarlas protege el resto del sistema.

**Consecuencia**: `App.SAP2000` es la única capa que requiere SAP2000 instalado para compilar.

---

*Anterior: [Misión, Visión y Objetivos](./MISION_VISION_OBJETIVOS.md) | Siguiente: [Partes y Componentes](./PARTES_Y_COMPONENTES.md)*
