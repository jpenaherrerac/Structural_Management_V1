# Flujos de Trabajo

## Descripción General

Los flujos de trabajo de Structural Management V1 están organizados en torno a tres casos de uso principales: **Crear Proyecto**, **Hidratar Fuente Sísmica**, y **Hidratar Fuente de Diseño**. A partir de estos, se derivan flujos secundarios de exportación y comparación de revisiones.

---

## 1. Flujo de Inicio de la Aplicación

```
┌───────────────────────────────────────────────────────────────┐
│                        Program.cs                             │
│                                                               │
│  1. Crear IProjectRepository      → ProjectRepository         │
│  2. Crear ISeismicSourceRepository → SeismicSourceRepository  │
│  3. Crear IDesignSourceRepository → DesignSourceRepository    │
│  4. Crear ISapAdapter             → SapAdapter                │
│  5. Crear CreateProjectUseCase(projectRepo)                   │
│  6. Crear HydrateSeismicSourceUseCase(sap, seismicRepo)       │
│  7. Crear HydrateDesignSourceUseCase(sap, designRepo)         │
│  8. Application.Run(new MainForm(...))                        │
└───────────────────────────────────────────────────────────────┘
                           │
                           ↓
               ┌───────────────────────┐
               │   MainForm visible    │
               │  (estado inicial:     │
               │   SAP: Desconectado)  │
               └───────────────────────┘
```

**Resultado**: La interfaz principal está lista, todos los servicios instanciados, SAP2000 desconectado.

---

## 2. Flujo de Conexión a SAP2000

```
Usuario → Menú "Conectar" → menuConectarSAP_Click()
                                    │
                          _sapAdapter.Connect()
                                    │
                      ┌─────────────┴──────────────┐
                      │                            │
                   Éxito                         Fallo
                      │                            │
          ┌───────────┴───────────┐   ┌────────────┴────────────┐
          │  UpdateSapStatus()    │   │  MessageBox: Error       │
          │  Muestra: "SAP2000    │   │  de conexión             │
          │  v25.0.0 - Conectado" │   └─────────────────────────┘
          └───────────────────────┘
```

**Pre-condición**: SAP2000 v25 debe estar instalado y en ejecución.
**Post-condición**: `_sapAdapter.IsConnected == true`; barra de estado actualizada.

---

## 3. Flujo de Creación de Proyecto

```
Usuario → Menú "Archivo > Nuevo Proyecto"
                    │
          Abre NewProjectDialog
                    │
          Usuario completa campos:
          • Nombre (requerido)
          • Código (requerido, único)
          • Descripción
          • Ubicación, Cliente
          • Código de Diseño (ACI 318, etc.)
          • Sistema Estructural
          • N° Pisos, Altura total
          • Uso del Edificio
                    │
          Click "Aceptar"
                    │
     CreateProjectUseCase.Execute(...)
                    │
        ┌───────────┴──────────────┐
        │                         │
     Válido                   Inválido
        │                         │
  ProjectRepository.Add(project)  MessageBox: Error de validación
        │
  _activeProjectId = project.Id
        │
  UpdateStatusBar("Proyecto: CODIGO")
```

**Validaciones**:
1. Nombre no puede ser nulo o vacío
2. Código no puede ser nulo o vacío
3. El código debe ser único (verificado en `IProjectRepository.CodeExists`)

**Post-condición**: Proyecto activo establecido, `_activeProjectId` con valor válido.

---

## 4. Flujo de Apertura de Modelo SAP2000

```
Usuario → Menú "Archivo > Abrir"
                │
    OpenFileDialog (filtro: *.sdb, *.sap)
                │
    Usuario selecciona archivo
                │
    _sapAdapter.OpenModel(filePath)
                │
    ┌───────────┴──────────────┐
    │                         │
  Éxito                     Fallo
    │                         │
  Confirmar en statusbar    MessageBox: Error al abrir
  "Modelo: nombre.sdb"
```

**Pre-condición**: SAP2000 conectado (`IsConnected == true`).
**Post-condición**: Modelo SAP2000 abierto y listo para análisis.

---

## 5. Flujo de Hidratación Sísmica (HydrateSeismicSourceUseCase)

Este es el flujo principal de extracción de datos sísmicos:

```
Usuario → [Botón/Menú "Hidratar Resultados Sísmicos"]
                    │
     HydrateSeismicSourceUseCase.Execute(projectId)
                    │
          ┌─────────┴─────────────────────────────────┐
          │            ISapAdapter                     │
          │                                            │
          │  1. GetStoryNames()   → ["Piso 1"..."Piso N"]
          │  2. GetModalResults() → [ModalResult x n modos]
          │  3. GetStoryShears("Sismo X") → [StoryResult]
          │  4. GetStoryShears("Sismo Y") → [StoryResult]
          │  5. GetStoryDrifts("Sismo X") → [DriftResult]
          │  6. GetStoryDrifts("Sismo Y") → [DriftResult]
          │  7. GetBaseShear("Combo Sísmico") → BaseShearSummary
          └─────────────────────────────────────────────┘
                    │
          Construir StructureOutputSnapshot:
          • ModalDataSet (todos los modos)
          • StoryDataSet (cortantes X e Y)
          • DriftDataSet (derivas X e Y)
          • BaseShearSummary
          • GlobalSeismicSummary (periodo fund., masa total)
                    │
          Construir HydrationMetadata:
          • Fecha/hora de ejecución
          • Versión SAP2000
          • Ruta del modelo
          • ExecutedCommandSet (traza de comandos)
                    │
          Crear SeismicSource:
          • Id = Guid.NewGuid()
          • ProjectId = projectId
          • Snapshot = structureOutputSnapshot
          • Metadata = hydrationMetadata
                    │
          SeismicSourceRepository.Add(seismicSource)
                    │
          Retornar seismicSource.Id
```

**Datos extraídos por esta operación**:

| Dato | Descripción | Unidades |
|---|---|---|
| Periodos modales | T1, T2, ... Tn | segundos |
| Masa participativa | Mx, My por modo | fracción acumulada |
| Cortantes de historia | Vx, Vy por piso | toneladas |
| Derivas de entrepiso | Δ/h por piso y dirección | adimensional |
| Cortante basal | Vb,x y Vb,y | toneladas |

---

## 6. Flujo de Hidratación de Diseño (HydrateDesignSourceUseCase)

```
[Pre-condición: Diseño ejecutado en SAP2000]

HydrateDesignSourceUseCase.Execute(projectId)
                │
     ┌──────────┴──────────────────────────────────┐
     │           ISapAdapter                        │
     │                                              │
     │  1. GetFrameElementIds() → [IDs de marcos]   │
     │  2. GetBeamDesignData()  → [BeamDesignData]  │
     │  3. GetColumnDesignData()→ [ColumnDesignData]│
     │  4. GetWallDesignData()  → [WallDesignData]  │
     │  5. GetFrameForces(combo)→ [ElementForceRecord]
     └──────────────────────────────────────────────┘
                │
     Construir DesignSnapshot:
     • BeamDesigns   (todos los elementos viga)
     • ColumnDesigns (todos los elementos columna)
     • WallDesigns   (todos los muros de corte)
     • ElementForces (fuerzas internas por combinación)
                │
     Crear DesignSource:
     • Id, ProjectId, Snapshot, Metadata
                │
     DesignSourceRepository.Add(designSource)
```

---

## 7. Flujo de Generación de Anexo de Diseño (BeamDesignCalculator)

```
[Input: BeamDesignData de SAP2000, parámetros f'c, fy]

Para cada elemento viga en DesignSnapshot.BeamDesigns:

    BeamDesignCalculator.Calculate(data, fc, fy, b, d, s)
              │
    1. Calcular ρ_balance = 0.85·β1·(fc/fy)·(6120/(6120+fy))
    2. Calcular ρ_min = max(0.25√fc/fy, 1.4/fy)
    3. Calcular ρ_max = 0.75·ρ_balance
    4. Calcular As,req de Mu: As = Mu/(φ·fy·(d - a/2))
    5. Verificar: ρ_min ≤ ρ ≤ ρ_max
    6. Calcular Mn = As·fy·(d - As·fy/(1.7·fc·b))
    7. Verificar: φMn ≥ Mu
    8. Calcular Vc = 0.53√fc·b·d
    9. Calcular Vs = Av·fy·d/s
    10. Verificar: φ(Vc+Vs) ≥ Vu
              │
    → BeamDesignReportRow (adecuado/inadecuado)

Construir BeamDesignAnnex (encabezado + todas las filas)
              │
IAnnexExporter.ExportBeamAnnex(annex, outputStream)
              │
    ┌─────────┴──────────┐
    │                    │
 CsvAnnexExporter    XmlAnnexExporter
 (archivo .csv)      (archivo .xml)
```

---

## 8. Flujo de Comparación de Revisiones

```
[Input: SeismicSource_A (revisión anterior) y SeismicSource_B (revisión nueva)]

RevisionComparison.Compare(sourceA, sourceB)
              │
    ┌─────────┴───────────────────────────────────────┐
    │  Para cada StoryResult en A y B:                 │
    │  • Si Vx difiere en >5%: RevisionDifference      │
    │                                                  │
    │  Para cada DriftResult en A y B:                 │
    │  • Si Deriva difiere en >5%: RevisionDifference  │
    │                                                  │
    │  Para cada ModalResult en A y B:                 │
    │  • Si T difiere en >3%: RevisionDifference       │
    └──────────────────────────────────────────────────┘
              │
    → RevisionComparison (lista de diferencias)
              │
    Presentar al usuario: tabla de cambios significativos
```

---

## 9. Flujo de Exportación Excel

```
[Input: DesignSource con resultados de diseño]

XlsExporter.Export(designSource, filePath)
              │
    ClosedXML: IXLWorkbook workbook = new XLWorkbook()
              │
    Hoja 1: "Vigas"
    • Encabezados: ID, Sección, As,req, As,prov, φMn, Vu, φVn, Estado
    • Filas: una por BeamDesignData
              │
    Hoja 2: "Columnas"
    • Encabezados: ID, Sección, As,tot, Pu, Mu, φPn, φMn, Estado
    • Filas: una por ColumnDesignData
              │
    Hoja 3: "Muros"
    • Encabezados: ID, Nombre, ρh, ρv, Vu, φVn, Estado
    • Filas: una por WallDesignData
              │
    workbook.SaveAs(filePath)
```

---

## 10. Flujo de Parámetros Sísmicos E030 (UserControl)

```
Usuario selecciona: Zona = Z3, Suelo = S2, Sistema = Dual

E030UserControl
    │
    ├── E030Tables.GetZoneFactor("Z3")    → Z = 0.35
    ├── E030Tables.GetSoilFactor("Z3","S2") → S = 1.20
    ├── E030Tables.GetTp("Z3","S2")       → Tp = 0.6 s
    ├── E030Tables.GetTl("Z3","S2")       → Tl = 2.0 s
    ├── E030Tables.GetR0("Dual")          → R0 = 7
    │
    Calcular Sa(T) para T = 0..4s:
    │  T < Tp:   Sa = Z·U·S·C / R     (zona plataforma: C=2.5)
    │  Tp≤T≤Tl:  Sa = Z·U·S·C / R     (C = 2.5·Tp/T)
    │  T > Tl:   Sa = Z·U·S·C / R     (C = 2.5·Tp·Tl/T²)
    │
    Disparar ValoresActualesChanged(this, new SeismicParametersEventArgs(params))
    │
EspectroUserControl recibe evento
    │
    Actualizar Chart 1: Sa(T) — Espectro de aceleración
    Actualizar Chart 2: Sd(T) — Espectro de desplazamiento
    Actualizar Chart 3: PSa    — Pseudoaceleración
```

---

## Resumen de Flujos Principales

| Flujo | Entrada | Salida | Clase principal |
|---|---|---|---|
| Inicio de app | - | MainForm visible | `Program` |
| Conectar SAP2000 | - | Conexión COM activa | `SapAdapter` |
| Crear proyecto | Formulario | `Guid` del proyecto | `CreateProjectUseCase` |
| Hidratar sísmica | `Guid` proyecto | `SeismicSource` en repo | `HydrateSeismicSourceUseCase` |
| Hidratar diseño | `Guid` proyecto | `DesignSource` en repo | `HydrateDesignSourceUseCase` |
| Calcular viga | `BeamDesignData` | `BeamDesignReportRow` | `BeamDesignCalculator` |
| Exportar CSV | `BeamDesignAnnex` | Archivo CSV | `CsvAnnexExporter` |
| Exportar XML | `BeamDesignAnnex` | Archivo XML | `XmlAnnexExporter` |
| Exportar Excel | `DesignSource` | Archivo .xlsx | `XlsExporter` |
| Parámetros E030 | Zona, Suelo, U, R | `Sa(T)` calculado | `E030Tables` + `E030UserControl` |

---

*Anterior: [Clases y Entidades](./CLASES_Y_ENTIDADES.md) | Siguiente: [Estándares de Ingeniería](./ESTANDARES_INGENIERILES.md)*
