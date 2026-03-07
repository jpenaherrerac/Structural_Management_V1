# Structural Management V1

> **Plataforma de gestión y análisis estructural integrada con SAP2000**

[![.NET Framework](https://img.shields.io/badge/.NET%20Framework-4.8-blue.svg)](https://dotnet.microsoft.com/download/dotnet-framework/net48)
[![C#](https://img.shields.io/badge/C%23-9.0-purple.svg)](https://docs.microsoft.com/en-us/dotnet/csharp/)
[![SAP2000](https://img.shields.io/badge/SAP2000-v25-orange.svg)](https://www.csiamerica.com/products/sap2000)
[![Architecture](https://img.shields.io/badge/Arquitectura-Clean%20Architecture-green.svg)](#arquitectura)

---

## ¿Qué es Structural Management V1?

**Structural Management V1** es una aplicación de escritorio Windows Forms desarrollada en C# 9.0 sobre .NET Framework 4.8, diseñada para la **gestión integral de proyectos de ingeniería estructural**. La plataforma actúa como capa de orquestación entre el ingeniero estructural y SAP2000, automatizando la extracción, procesamiento, almacenamiento y exportación de resultados de análisis y diseño sísmico.

El sistema integra los estándares peruanos de diseño sísmico (**NTE E030**) y las normativas de diseño en concreto (**ACI 318**), permitiendo a los equipos de ingeniería gestionar múltiples proyectos, revisiones y anexos de diseño desde un único entorno.

---

## Características Principales

| Característica | Descripción |
|---|---|
| 🏗️ **Gestión de Proyectos** | Creación, organización y seguimiento de proyectos estructurales con metadata completa |
| 🔗 **Integración SAP2000** | Conexión directa vía COM API para lectura de resultados de análisis y diseño |
| 🌍 **Análisis Sísmico** | Extracción automática de cortantes de historia, derivas, resultados modales |
| 📐 **Diseño Estructural** | Cálculo de vigas, columnas y muros de corte según ACI 318 |
| 📊 **Exportación** | Generación de reportes en CSV, XML y Excel (ClosedXML) |
| 📋 **Estándar E030** | Soporte nativo para parámetros sísmicos del Reglamento Nacional de Edificaciones |
| 🔄 **Control de Revisiones** | Trazabilidad de cambios entre revisiones del modelo estructural |
| 🏛️ **Arquitectura Limpia** | Diseño desacoplado que facilita mantenimiento y extensibilidad |

---

## Arquitectura General

El sistema sigue una **Arquitectura Limpia (Clean Architecture)** organizada en 5 proyectos:

```
┌─────────────────────────────────────────────────────────┐
│                    App.WinForms                          │
│         (Interfaz de Usuario · Windows Forms)            │
└─────────────────────┬───────────────────────────────────┘
                      │
        ┌─────────────┼─────────────┐
        │             │             │
┌───────┴──────┐ ┌────┴─────┐ ┌────┴────────┐
│App.Application│ │App.SAP2000│ │App.Infra    │
│ (Casos de Uso)│ │(Adaptador)│ │(Repositorios│
│  Calculadoras │ │ COM API   │ │ Exportadores│
└───────┬───────┘ └────┬─────┘ └─────────────┘
        │              │
        └──────┬────────┘
               │
       ┌───────┴───────┐
       │  App.Domain   │
       │  (Entidades   │
       │  Dominio)     │
       └───────────────┘
```

---

## Inicio Rápido

### Requisitos Previos

- Windows 10/11 (64 bits)
- [.NET Framework 4.8](https://dotnet.microsoft.com/download/dotnet-framework/net48)
- [SAP2000 v25](https://www.csiamerica.com/products/sap2000) instalado
- [Visual Studio 2022](https://visualstudio.microsoft.com/) (para desarrollo)

### Instalación y Compilación

```bash
# Clonar el repositorio
git clone https://github.com/jpenaherrerac/Structural_Management_V1.git
cd Structural_Management_V1

# Compilar (requiere .NET Framework 4.8)
dotnet build App.WinForms/App.WinForms.csproj
```

> **Nota**: Las advertencias relacionadas con `SAP2000v1.dll` son esperadas ya que la librería COM es externa y debe estar instalada localmente con SAP2000 v25.

### Ejecución

Abrir la solución `Structural_Management_V1.slnx` en Visual Studio 2022 y ejecutar el proyecto `App.WinForms`.

---

## Flujo de Trabajo Típico

```
1. Crear nuevo proyecto → NewProjectDialog
           ↓
2. Conectar a SAP2000 → Menú "Conectar"
           ↓
3. Abrir modelo SAP2000 → Menú "Archivo > Abrir"
           ↓
4. Ejecutar análisis/diseño en SAP2000
           ↓
5. Hidratar resultados sísmicos → HydrateSeismicSourceUseCase
           ↓
6. Hidratar resultados de diseño → HydrateDesignSourceUseCase
           ↓
7. Generar y exportar reportes → IAnnexExporter (CSV/XML/Excel)
```

---

## Estructura del Repositorio

```
Structural_Management_V1/
├── App.Domain/              # Entidades y reglas de negocio puras
│   ├── Entities/            # 56 entidades de dominio
│   └── Enums/               # 6 enumeraciones
├── App.Application/         # Casos de uso, interfaces, calculadoras
│   ├── Interfaces/          # ISapAdapter, IProjectRepository, ...
│   ├── UseCases/            # CreateProject, HydrateSeismic, ...
│   ├── Annexes/             # Calculadoras ACI 318
│   └── Export/              # Interfaces de exportación
├── App.Infrastructure/      # Implementaciones concretas
│   ├── Repositories/        # Repositorios en memoria
│   └── Export/              # CSV, XML, Excel exporters
├── App.SAP2000/             # Adaptador COM de SAP2000
│   └── Adapters/            # SapAdapter, SapConnectionService, ...
├── App.WinForms/            # Interfaz de usuario
│   ├── Forms/               # Diálogos y formularios
│   └── UserControls/        # Controles E030 y Espectro
├── DOCS/                    # 📚 Documentación especializada
│   ├── FILOSOFIA.md
│   ├── MISION_VISION_OBJETIVOS.md
│   ├── ARQUITECTURA.md
│   ├── PARTES_Y_COMPONENTES.md
│   ├── CLASES_Y_ENTIDADES.md
│   ├── FLUJOS_DE_TRABAJO.md
│   ├── ESTANDARES_INGENIERILES.md
│   ├── GUIA_DE_INICIO_RAPIDO.md
│   └── INTEGRACION_SAP2000.md
├── Helps_docs_/             # Documentación técnica auxiliar
└── Structural_Management_V1.slnx
```

---

## Documentación

La documentación especializada se encuentra en la carpeta [`DOCS/`](./DOCS/):

| Documento | Descripción |
|---|---|
| [Filosofía](./DOCS/FILOSOFIA.md) | Principios, valores y filosofía de diseño del sistema |
| [Misión, Visión y Objetivos](./DOCS/MISION_VISION_OBJETIVOS.md) | Propósito, dirección y metas del proyecto |
| [Arquitectura](./DOCS/ARQUITECTURA.md) | Arquitectura del sistema, patrones y decisiones de diseño |
| [Partes y Componentes](./DOCS/PARTES_Y_COMPONENTES.md) | Descripción detallada de cada módulo y componente |
| [Clases y Entidades](./DOCS/CLASES_Y_ENTIDADES.md) | Referencia completa de clases, entidades e interfaces |
| [Flujos de Trabajo](./DOCS/FLUJOS_DE_TRABAJO.md) | Diagramas y descripción de los flujos de datos y procesos |
| [Estándares de Ingeniería](./DOCS/ESTANDARES_INGENIERILES.md) | NTE E030, ACI 318 y normativas aplicadas |
| [Guía de Inicio Rápido](./DOCS/GUIA_DE_INICIO_RAPIDO.md) | Configuración del entorno y primeros pasos |
| [Integración SAP2000](./DOCS/INTEGRACION_SAP2000.md) | Guía de integración con la API COM de SAP2000 |

---

## Tecnologías Utilizadas

| Tecnología | Versión | Uso |
|---|---|---|
| .NET Framework | 4.8 | Plataforma base |
| C# | 9.0 | Lenguaje de programación |
| Windows Forms | - | Interfaz de usuario |
| SAP2000 COM API | v25 | Integración con SAP2000 |
| ClosedXML | Latest | Exportación a Excel |
| DocumentFormat.OpenXml | - | Soporte para formato xlsx |

---

## Estándares de Ingeniería Soportados

- **NTE E030** – Diseño Sismorresistente (Reglamento Nacional de Edificaciones, Perú)
- **ACI 318** – Código para Requisitos de Concreto Estructural (American Concrete Institute)

---

## Licencia

Este proyecto es de uso interno para proyectos de ingeniería estructural. Consultar con el autor para más información sobre términos de uso.

---

## Autor

**jpenaherrerac** – Ingeniero Estructural / Desarrollador

---

*Para documentación técnica detallada, consultar la carpeta [DOCS/](./DOCS/).*