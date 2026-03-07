# Partes y Componentes del Sistema

## Visión General de Componentes

El sistema está compuesto por **cinco proyectos** que se dividen en **veintidós componentes funcionales** claramente identificados. Esta sección describe cada parte del sistema, su responsabilidad y sus relaciones.

---

## 1. App.Domain — Dominio de Negocio

### 1.1 Entities/Project — Gestión de Proyectos

| Clase | Responsabilidad |
|---|---|
| `Project` | Raíz de agregado. Identidad única (`Guid`), nombre, código, lista de revisiones y participantes |
| `ProjectMetadata` | Información descriptiva: ubicación, cliente, código de diseño, sistema estructural, número de pisos, altura total, uso de edificación |
| `ProjectRevision` | Captura el estado del proyecto en un punto del tiempo: notas, fecha, autor |
| `RevisionNote` | Comentario anotado dentro de una revisión |
| `ProjectParticipant` | Persona asociada al proyecto con un rol definido (Ingeniero, Revisor, Aprobador) |
| `ApprovalRecord` | Registro formal de aprobación de una revisión |

### 1.2 Entities/Seismic — Resultados de Análisis Sísmico

| Clase | Responsabilidad |
|---|---|
| `StructureOutputSnapshot` | Agregado que consolida todos los resultados sísmicos extraídos de SAP2000 |
| `ModalDataSet` | Colección de resultados modales (periodos, participación de masa) |
| `ModalResult` | Modo individual: periodo T, masa participativa Mx/My/Mz |
| `StoryDataSet` | Colección de cortantes de historia por caso de carga |
| `StoryResult` | Cortante de historia individual: piso, caso de carga, Vx, Vy |
| `DriftDataSet` | Colección de derivas de entrepiso |
| `DriftResult` | Deriva individual: piso, dirección, deriva máxima, límite normativo |
| `BaseShearSummary` | Resumen de cortante basal: Vx, Vy por combinación de carga |
| `GlobalSeismicSummary` | Resumen global: periodo fundamental, masa total, cortantes basales |
| `MassSummary` | Distribución de masa por piso |

### 1.3 Entities/Design — Datos de Diseño Estructural

| Clase | Responsabilidad |
|---|---|
| `DesignSnapshot` | Agrega todos los resultados de diseño de un modelo SAP2000 |
| `BeamDesignData` | Datos de diseño de vigas: refuerzo requerido, capacidades Mn, φMn |
| `ColumnDesignData` | Datos de diseño de columnas: interacción P-M, refuerzo longitudinal |
| `WallDesignData` | Datos de diseño de muros de corte: refuerzo horizontal y vertical |
| `SlabDesignData` | Datos de diseño de losas: refuerzo por franja |
| `ElementForceRecord` | Fuerzas internas de elemento: N, V2, V3, M2, M3, T (todos los combos) |

### 1.4 Entities/Elements — Elementos Estructurales

| Clase | Responsabilidad |
|---|---|
| `StructuralElement` | Clase base abstracta: ID de elemento, tipo, material, sección |
| `Beam` | Viga: longitud, ancho, profundidad, indicador de ménsula |
| `Column` | Columna: dimensiones de sección, tipo (rectangular/circular), altura libre |
| `ShearWall` | Muro de corte: longitud, espesor, altura, extremos confinados |
| `Slab` | Losa: espesor, tipo (maciza/nervada/plana), luz libre |
| `StructuralModelDefinition` | Definición completa del modelo: lista de todos los elementos |

### 1.5 Entities/Loads — Cargas y Configuraciones

| Clase | Responsabilidad |
|---|---|
| `BuildingLoadConfiguration` | Configuración global de cargas: patrones, casos, combinaciones |
| `LoadPatternDefinition` | Patrón de carga: nombre, tipo (muerta, viva, sísmica), factor de auto-peso |
| `LoadCaseDefinition` | Caso de carga: tipo (estático/modal/espectral), parámetros |
| `LoadCombinationDefinition` | Combinación de carga: nombre, lista de casos con factores |
| `MassSourceDefinition` | Definición de fuente de masa: patrones de carga con factores de masa |
| `ResponseSpectrumDefinition` | Definición del espectro de respuesta: función de periodo-aceleración |
| `SeismicConfiguration` | Parámetros sísmicos del sitio: zona, suelo, factor de reducción R |
| `DiaphragmConstraintDefinition` | Restricción de diafragma rígido: nombre del piso, punto master |

### 1.6 Entities/Sources — Fuentes de Datos

| Clase | Responsabilidad |
|---|---|
| `SeismicSource` | Fuente de datos sísmicos: ID, proyecto asociado, snapshot de resultados, metadata |
| `DesignSource` | Fuente de datos de diseño: ID, proyecto asociado, snapshot de diseño, metadata |
| `HydrationMetadata` | Metadata de la hidratación: fecha, usuario, versión SAP2000, ruta del modelo |
| `ExecutedCommandSet` | Lista de trazas de comandos SAP2000 ejecutados durante la hidratación |

### 1.7 Entities/Annexes — Modelos de Anexos

| Clase | Responsabilidad |
|---|---|
| `BeamDesignAnnex` | Anexo de diseño de vigas: encabezado del proyecto + filas de cálculo |
| `BeamDesignReportRow` | Fila de reporte: ID, As,req, As,prov, φMn, verificación, estado |
| `ColumnDesignAnnex` | Anexo de diseño de columnas: encabezado + filas de interacción P-M |
| `ColumnDesignReportRow` | Fila de reporte: ID, Pu, Mu, φPn, φMn, verificación |
| `ShearWallDesignAnnex` | Anexo de diseño de muros: encabezado + filas de verificación |
| `ShearWallDesignReportRow` | Fila de reporte: ID, Vu, φVn, refuerzo horizontal/vertical, estado |

### 1.8 Entities/Comparison — Control de Revisiones

| Clase | Responsabilidad |
|---|---|
| `RevisionComparison` | Resultado de comparar dos fuentes de datos (RevisionA vs RevisionB) |
| `RevisionDifference` | Diferencia individual identificada entre revisiones |
| `ChangeLogEntry` | Entrada en el registro de cambios: elemento, campo, valor anterior/nuevo |
| `SourceModelFingerprint` | Huella digital del modelo: hash, número de elementos, estadísticas |

### 1.9 Entities/Sap — Sesión SAP2000

| Clase | Responsabilidad |
|---|---|
| `SapSession` | Estado de la sesión activa: conectado/desconectado, versión, instancia |
| `SapInstanceInfo` | Información de la instancia SAP2000: proceso, versión, directorio |
| `SapModelReference` | Referencia al modelo activo: ruta del archivo, nombre, estado |
| `SapCommandExecutionTrace` | Traza de ejecución: comando, resultado, código de error, duración |

### 1.10 Entities/Documentation — Estructura de Reportes

| Clase | Responsabilidad |
|---|---|
| `EngineeringReportPackage` | Paquete completo de reporte: encabezado, secciones, tablas, firma |
| `DocumentationSection` | Sección del reporte: título, contenido narrativo, subsecciones |
| `DocumentationTable` | Tabla de datos: encabezados de columna, filas de datos |
| `ExportJob` | Trabajo de exportación: formato destino, ruta, estado, progreso |

### 1.11 Enums — Tipos Enumerados

| Enumeración | Valores |
|---|---|
| `ActionType` | `Add`, `Update`, `Delete` — tipo de acción de auditoría |
| `AnalysisPurpose` | `Seismic`, `Design`, `Verification` — propósito del análisis |
| `ElementType` | `Beam`, `Column`, `ShearWall`, `Slab` — tipo de elemento estructural |
| `ExportFormat` | `CSV`, `XML`, `PDF`, `Excel` — formato de exportación |
| `HydrationPurpose` | `Seismic`, `Design` — tipo de datos a hidratar |
| `ProjectRoleType` | `Engineer`, `Reviewer`, `Approver` — rol en el proyecto |

---

## 2. App.Application — Casos de Uso e Interfaces

### 2.1 Interfaces — Contratos del Sistema

#### ISapAdapter
Contrato de 41 miembros que abstrae completamente la API COM de SAP2000:

```
Conexión:           Connect(), Disconnect(), GetSapVersion(), IsConnected
Modelo:             OpenModel(), SaveModel(), RunAnalysis(), RunDesign()
Bloqueo:            LockModel(), UnlockModel()
Consultas:          GetStoryNames(), GetFrameElementIds(), GetAreaElementIds()
Configuración:      DefineLoadPattern(), DefineLoadCase(), DefineLoadCombination()
                    DefineMassSource(), DefineResponseSpectrum(), AssignDiaphragm()
Resultados:         GetBaseShear(), GetStoryShears(), GetModalResults(), GetStoryDrifts()
                    GetFrameForces()
Diseño:             GetBeamDesignData(), GetColumnDesignData(), GetWallDesignData()
```

#### IProjectRepository
CRUD completo para proyectos:
```
GetById(Guid), GetByCode(string), GetAll(), GetActive()
Add(Project), Update(Project), Delete(Guid)
Exists(Guid), CodeExists(string)
```

#### ISeismicSourceRepository / IDesignSourceRepository
Misma estructura para fuentes de datos sísmicos y de diseño:
```
GetById(), GetByProjectId(), GetLatestByProjectId()
Add(), Update(), Delete()
```

### 2.2 UseCases — Flujos de Trabajo

#### CreateProjectUseCase
- **Entrada**: nombre, código, descripción, ubicación, cliente, código de diseño, sistema estructural, pisos, altura, uso
- **Proceso**: Valida nombre/código requeridos, verifica unicidad del código, crea entidad `Project` con `ProjectMetadata`
- **Salida**: `Guid` del proyecto creado, o mensaje de error

#### HydrateSeismicSourceUseCase
- **Entrada**: `Guid` del proyecto activo
- **Proceso**: Conectado a SAP2000, lee resultados modales, cortantes de historia, derivas; crea `SeismicSource` + `StructureOutputSnapshot`
- **Salida**: ID de la `SeismicSource` creada

#### HydrateDesignSourceUseCase
- **Entrada**: `Guid` del proyecto activo
- **Proceso**: Lee datos de diseño de vigas, columnas y muros; lee fuerzas de elementos; crea `DesignSource` + `DesignSnapshot`
- **Salida**: ID de la `DesignSource` creada

### 2.3 Annexes — Calculadoras de Diseño

#### BeamDesignCalculator (ACI 318)

Calcula para cada viga los siguientes parámetros:

| Parámetro | Descripción |
|---|---|
| ρ_balance | Cuantía balanceada |
| ρ_min | Cuantía mínima (0.0033 para f'c=280 kg/cm²) |
| ρ_max | Cuantía máxima (0.75·ρ_balance) |
| As,req | Área de acero requerida (cm²) |
| As,prov | Área de acero provista (cm²) |
| Mn | Momento nominal (ton·m) |
| φMn | Momento resistente de diseño (φ=0.90) |
| Vc | Resistencia al corte del concreto (ton) |
| Vs | Resistencia al corte del acero (ton) |
| φVn | Cortante resistente de diseño (φ=0.85) |

#### ColumnDesignCalculator (ACI 318)
- Diagramas de interacción P-M
- Verificación para cada combinación de (Pu, Mu) del análisis

#### ShearWallDesignCalculator (ACI 318)
- Refuerzo horizontal y vertical mínimo
- Verificación de cortante último Vu ≤ φVn
- Revisión de extremos confinados

### 2.4 Export — Exportadores de Aplicación

#### IAnnexExporter (Interfaz)
```
ExportBeamAnnex(BeamDesignAnnex annex, Stream output)
ExportColumnAnnex(ColumnDesignAnnex annex, Stream output)
ExportShearWallAnnex(ShearWallDesignAnnex annex, Stream output)
```

#### CsvAnnexExporter
Implementación CSV: una fila por elemento, separador por comas, encabezado en primera fila.

#### XmlAnnexExporter
Implementación XML: estructura jerárquica con nodos por sección y elemento.

---

## 3. App.Infrastructure — Infraestructura

### 3.1 Repositorios en Memoria

| Clase | Almacenamiento | Claves de búsqueda |
|---|---|---|
| `ProjectRepository` | `Dictionary<Guid, Project>` | Por ID, por código |
| `SeismicSourceRepository` | `Dictionary<Guid, SeismicSource>` | Por ID, por proyecto |
| `DesignSourceRepository` | `Dictionary<Guid, DesignSource>` | Por ID, por proyecto |

### 3.2 Exportadores de Infraestructura

| Clase | Librería | Formatos |
|---|---|---|
| `CsvExporter` | BCL (.NET) | CSV |
| `XmlExporter` | `System.Xml` | XML |
| `XlsExporter` | ClosedXML | .xlsx (Excel) |

---

## 4. App.SAP2000 — Adaptador COM

### 4.1 SapAdapter
- Implementa `ISapAdapter` completo
- Delega operaciones de conexión a `SapConnectionService`
- Delega lectura sísmica a `SapStructureOutputReader`
- Delega lectura de diseño a `SapDesignDataReader`
- No expone ningún tipo del namespace `SAP2000v1` hacia el exterior

### 4.2 SapConnectionService
- Gestiona el ciclo de vida de la conexión COM: inicializar, conectar, desconectar
- Operaciones sobre el modelo: abrir, guardar, ejecutar análisis, ejecutar diseño
- Definiciones de cargas: patrones, casos, combinaciones, masa, espectro, diafragmas
- Consultas de estructura: pisos, IDs de elementos

### 4.3 SapStructureOutputReader
- `ReadBaseShear()` → `BaseShearSummary`
- `ReadStoryShears()` → `IList<StoryResult>`
- `ReadModalResults()` → `IList<ModalResult>`
- `ReadStoryDrifts()` → `IList<DriftResult>`
- `ReadMassSummary()` → `MassSummary`

### 4.4 SapDesignDataReader
- `ReadBeamDesignData()` → `IList<BeamDesignData>`
- `ReadColumnDesignData()` → `IList<ColumnDesignData>`
- `ReadWallDesignData()` → `IList<WallDesignData>`
- `ReadFrameForces()` → `IList<ElementForceRecord>`

---

## 5. App.WinForms — Interfaz de Usuario

### 5.1 Program.cs — Raíz de Composición
- Único punto de creación del árbol de dependencias
- Instancia todos los repositorios, adaptadores y casos de uso
- Pasa dependencias al `MainForm` por constructor

### 5.2 MainForm — Ventana Principal

**Menús**:
- **Archivo**: Nuevo Proyecto, Abrir Modelo, Guardar Modelo, Salir
- **Conectar**: Conectar SAP2000, Desconectar SAP2000

**Barra de Estado**:
- Indicador del proyecto activo
- Indicador de conexión SAP2000 (versión)

**Métodos principales**:
- `UpdateSapStatus()` — Actualiza indicadores de conexión
- `menuNuevoProyecto_Click()` — Abre `NewProjectDialog`
- `menuAbrir_Click()` — Abre diálogo de archivo SAP2000
- `menuConectarSAP_Click()` — Conecta/desconecta SAP2000

### 5.3 NewProjectDialog — Diálogo de Nuevo Proyecto

**Campos del formulario**:
- Nombre del proyecto (requerido)
- Código del proyecto (requerido, único)
- Descripción
- Ubicación
- Cliente
- Código de diseño (ACI 318, etc.)
- Sistema estructural (marcos, muros, dual)
- Número de pisos
- Altura total (m)
- Uso del edificio (vivienda, comercial, hospital, etc.)

### 5.4 SeismicityForm — Formulario de Sismicidad
Formulario especializado para configurar y visualizar los parámetros sísmicos del proyecto. Integra los controles `E030UserControl` y `EspectroUserControl`.

### 5.5 E030UserControl — Control de Parámetros E030
- Permite seleccionar: Zona sísmica (Z1-Z4), Tipo de suelo (S0-S3), Factor de uso U
- Calcula automáticamente: TP, TL, Sa, parámetros de irregularidad
- Emite evento `ValoresActualesChanged` al cambiar cualquier parámetro
- Expone `GetValoresActuales()` → `Dictionary<string, double>`

### 5.6 E030Tables — Tablas de Valores E030
Clase estática con métodos de consulta para los parámetros del Reglamento Nacional de Edificaciones E030:
- Factores de zona Z por zona (Z1=0.10, Z2=0.25, Z3=0.35, Z4=0.45)
- Parámetros de suelo Tp, TL, S por tipo (S0, S1, S2, S3)
- Factor de reducción R0 por sistema estructural
- Factores de irregularidad en planta y en altura

### 5.7 EspectroUserControl — Visualización del Espectro
- Muestra 3 gráficas: espectro Sa(T), espectro Sd(T), pseudoaceleración
- Se actualiza dinámicamente al cambiar los parámetros en `E030UserControl`
- Implementa la forma del espectro de diseño E030: zona plana, zona de decaimiento

---

*Anterior: [Arquitectura](./ARQUITECTURA.md) | Siguiente: [Clases y Entidades](./CLASES_Y_ENTIDADES.md)*
