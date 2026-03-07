# Clases y Entidades del Sistema

## Índice de Clases

Esta referencia lista todas las clases, interfaces y enumeraciones del sistema organizadas por proyecto y namespace. Se incluye la firma de los miembros públicos más importantes.

---

## App.Domain

### Namespace: `App.Domain.Entities.Project`

#### `Project`
```csharp
public class Project
{
    public Guid Id { get; }
    public string Name { get; }
    public string Code { get; }
    public string Description { get; }
    public ProjectMetadata Metadata { get; }
    public List<ProjectRevision> Revisions { get; }
    public List<ProjectParticipant> Participants { get; }
    public DateTime CreatedAt { get; }
    public bool IsActive { get; set; }
}
```

#### `ProjectMetadata`
```csharp
public class ProjectMetadata
{
    public string Location { get; }
    public string Client { get; }
    public string DesignCode { get; }          // Ej: "ACI 318-19"
    public string StructuralSystem { get; }    // Ej: "Dual", "Muros"
    public int NumberOfStories { get; }
    public double TotalHeight { get; }         // metros
    public string BuildingUse { get; }         // Ej: "Vivienda", "Hospital"
}
```

#### `ProjectRevision`
```csharp
public class ProjectRevision
{
    public Guid Id { get; }
    public Guid ProjectId { get; }
    public int RevisionNumber { get; }
    public string Description { get; }
    public DateTime CreatedAt { get; }
    public string CreatedBy { get; }
    public List<RevisionNote> Notes { get; }
    public ApprovalRecord? Approval { get; }
}
```

#### `ProjectParticipant`
```csharp
public class ProjectParticipant
{
    public Guid Id { get; }
    public string Name { get; }
    public string Email { get; }
    public ProjectRoleType Role { get; }
    public DateTime AssignedAt { get; }
}
```

---

### Namespace: `App.Domain.Entities.Seismic`

#### `StructureOutputSnapshot`
```csharp
public class StructureOutputSnapshot
{
    public Guid Id { get; }
    public ModalDataSet ModalData { get; }
    public StoryDataSet StoryShears { get; }
    public DriftDataSet Drifts { get; }
    public BaseShearSummary BaseShear { get; }
    public GlobalSeismicSummary GlobalSummary { get; }
    public MassSummary MassData { get; }
    public DateTime ExtractedAt { get; }
}
```

#### `ModalResult`
```csharp
public class ModalResult
{
    public int ModeNumber { get; }
    public double Period { get; }               // segundos
    public double FrequencyHz { get; }          // Hz
    public double MassParticipationX { get; }   // fracción (0-1)
    public double MassParticipationY { get; }
    public double MassParticipationZ { get; }
    public double CumulativeMassX { get; }
    public double CumulativeMassY { get; }
}
```

#### `StoryResult`
```csharp
public class StoryResult
{
    public string StoryName { get; }
    public string LoadCase { get; }
    public double ShearX { get; }    // toneladas
    public double ShearY { get; }    // toneladas
    public double Elevation { get; } // metros
}
```

#### `DriftResult`
```csharp
public class DriftResult
{
    public string StoryName { get; }
    public string LoadCase { get; }
    public string Direction { get; }    // "X" o "Y"
    public double Drift { get; }        // adimensional (e.g. 0.005)
    public double DriftLimit { get; }   // límite normativo
    public bool IsWithinLimit { get; }  // Drift <= DriftLimit
}
```

#### `BaseShearSummary`
```csharp
public class BaseShearSummary
{
    public string LoadCombination { get; }
    public double VxTon { get; }    // toneladas
    public double VyTon { get; }    // toneladas
}
```

#### `GlobalSeismicSummary`
```csharp
public class GlobalSeismicSummary
{
    public double FundamentalPeriodX { get; }   // segundos
    public double FundamentalPeriodY { get; }   // segundos
    public double TotalMassTon { get; }         // toneladas
    public double BaseShearX { get; }           // toneladas
    public double BaseShearY { get; }           // toneladas
    public int TotalModes { get; }
    public double AccumulatedMassX { get; }     // fracción (objetivo: ≥ 0.90)
    public double AccumulatedMassY { get; }
}
```

---

### Namespace: `App.Domain.Entities.Design`

#### `DesignSnapshot`
```csharp
public class DesignSnapshot
{
    public Guid Id { get; }
    public List<BeamDesignData> BeamDesigns { get; }
    public List<ColumnDesignData> ColumnDesigns { get; }
    public List<WallDesignData> WallDesigns { get; }
    public List<SlabDesignData> SlabDesigns { get; }
    public List<ElementForceRecord> ElementForces { get; }
    public DateTime ExtractedAt { get; }
}
```

#### `BeamDesignData`
```csharp
public class BeamDesignData
{
    public string ElementId { get; }
    public string SectionName { get; }
    public double WidthCm { get; }
    public double DepthCm { get; }
    public double AsTopPositive { get; }    // cm² - refuerzo superior
    public double AsTopNegative { get; }    // cm² - refuerzo inferior
    public double AsBot { get; }            // cm² - refuerzo inferior
    public double VuTon { get; }            // cortante último (ton)
    public string Station { get; }          // "Start", "Middle", "End"
    public string LoadCombo { get; }
}
```

#### `ColumnDesignData`
```csharp
public class ColumnDesignData
{
    public string ElementId { get; }
    public string SectionName { get; }
    public double AsTotalRequired { get; }  // cm² - área total requerida
    public double AsTotalProvided { get; }  // cm² - área total provista
    public double PuTon { get; }            // carga axial última (ton)
    public double MuMajorTonM { get; }      // momento último eje mayor (ton·m)
    public double MuMinorTonM { get; }      // momento último eje menor (ton·m)
    public string Station { get; }
    public string LoadCombo { get; }
}
```

#### `WallDesignData`
```csharp
public class WallDesignData
{
    public string ElementId { get; }
    public string PierName { get; }
    public double RhoHorizontal { get; }  // cuantía horizontal
    public double RhoVertical { get; }    // cuantía vertical
    public double VuTon { get; }          // cortante último (ton)
    public double MuTonM { get; }         // momento último (ton·m)
    public string LoadCombo { get; }
}
```

#### `ElementForceRecord`
```csharp
public class ElementForceRecord
{
    public string ElementId { get; }
    public string LoadCombo { get; }
    public string Station { get; }
    public double P { get; }     // fuerza axial (ton)
    public double V2 { get; }    // cortante local 2 (ton)
    public double V3 { get; }    // cortante local 3 (ton)
    public double T { get; }     // torsión (ton·m)
    public double M2 { get; }    // momento local 2 (ton·m)
    public double M3 { get; }    // momento local 3 (ton·m)
}
```

---

### Namespace: `App.Domain.Entities.Elements`

#### `StructuralElement` (clase base)
```csharp
public abstract class StructuralElement
{
    public string ElementId { get; }
    public ElementType Type { get; }
    public string Material { get; }
    public string SectionName { get; }
}
```

#### `Beam : StructuralElement`
```csharp
public class Beam : StructuralElement
{
    public double LengthM { get; }
    public double WidthCm { get; }
    public double DepthCm { get; }
    public bool IsCantilever { get; }
}
```

#### `Column : StructuralElement`
```csharp
public class Column : StructuralElement
{
    public double Width3Cm { get; }
    public double Width2Cm { get; }
    public double ClearHeightM { get; }
    public bool IsCircular { get; }
}
```

#### `ShearWall : StructuralElement`
```csharp
public class ShearWall : StructuralElement
{
    public double LengthCm { get; }
    public double ThicknessCm { get; }
    public double HeightM { get; }
    public bool HasConfinedEnds { get; }
}
```

---

### Namespace: `App.Domain.Entities.Sources`

#### `SeismicSource`
```csharp
public class SeismicSource
{
    public Guid Id { get; }
    public Guid ProjectId { get; }
    public StructureOutputSnapshot Snapshot { get; }
    public HydrationMetadata Metadata { get; }
    public ExecutedCommandSet CommandTrace { get; }
    public DateTime CreatedAt { get; }
}
```

#### `DesignSource`
```csharp
public class DesignSource
{
    public Guid Id { get; }
    public Guid ProjectId { get; }
    public DesignSnapshot Snapshot { get; }
    public HydrationMetadata Metadata { get; }
    public ExecutedCommandSet CommandTrace { get; }
    public DateTime CreatedAt { get; }
}
```

#### `HydrationMetadata`
```csharp
public class HydrationMetadata
{
    public DateTime ExecutedAt { get; }
    public string ExecutedBy { get; }
    public string SapVersion { get; }       // Ej: "SAP2000 v25.0.0"
    public string ModelFilePath { get; }    // ruta completa del .sdb
    public string ModelFileName { get; }
    public HydrationPurpose Purpose { get; }
}
```

---

### Namespace: `App.Domain.Enums`

```csharp
public enum ActionType     { Add, Update, Delete }
public enum AnalysisPurpose { Seismic, Design, Verification }
public enum ElementType    { Beam, Column, ShearWall, Slab }
public enum ExportFormat   { CSV, XML, PDF, Excel }
public enum HydrationPurpose { Seismic, Design }
public enum ProjectRoleType { Engineer, Reviewer, Approver }
```

---

## App.Application

### Namespace: `App.Application.Interfaces`

#### `ISapAdapter`
```csharp
public interface ISapAdapter
{
    // Estado
    bool IsConnected { get; }

    // Conexión
    void Connect();
    void Disconnect();
    string GetSapVersion();

    // Modelo
    void OpenModel(string filePath);
    void SaveModel();
    void RunAnalysis();
    void RunDesign();
    void LockModel();
    void UnlockModel();

    // Consultas de estructura
    IList<string> GetStoryNames();
    IList<string> GetFrameElementIds();
    IList<string> GetAreaElementIds();

    // Configuración
    void DefineLoadPattern(LoadPatternDefinition pattern);
    void DefineLoadCase(LoadCaseDefinition loadCase);
    void DefineLoadCombination(LoadCombinationDefinition combo);
    void DefineMassSource(MassSourceDefinition massSource);
    void DefineResponseSpectrum(ResponseSpectrumDefinition spectrum);
    void AssignDiaphragm(DiaphragmConstraintDefinition diaphragm);

    // Resultados sísmicos
    BaseShearSummary GetBaseShear(string combo);
    IList<StoryResult> GetStoryShears(string combo);
    IList<ModalResult> GetModalResults();
    IList<DriftResult> GetStoryDrifts(string combo);

    // Resultados de elementos
    IList<ElementForceRecord> GetFrameForces(string combo);

    // Resultados de diseño
    IList<BeamDesignData> GetBeamDesignData();
    IList<ColumnDesignData> GetColumnDesignData();
    IList<WallDesignData> GetWallDesignData();
}
```

#### `IProjectRepository`
```csharp
public interface IProjectRepository
{
    Project? GetById(Guid id);
    Project? GetByCode(string code);
    IList<Project> GetAll();
    IList<Project> GetActive();
    void Add(Project project);
    void Update(Project project);
    void Delete(Guid id);
    bool Exists(Guid id);
    bool CodeExists(string code);
}
```

#### `ISeismicSourceRepository`
```csharp
public interface ISeismicSourceRepository
{
    SeismicSource? GetById(Guid id);
    IList<SeismicSource> GetByProjectId(Guid projectId);
    SeismicSource? GetLatestByProjectId(Guid projectId);
    void Add(SeismicSource source);
    void Update(SeismicSource source);
    void Delete(Guid id);
}
```

#### `IDesignSourceRepository`
```csharp
public interface IDesignSourceRepository
{
    DesignSource? GetById(Guid id);
    IList<DesignSource> GetByProjectId(Guid projectId);
    DesignSource? GetLatestByProjectId(Guid projectId);
    void Add(DesignSource source);
    void Update(DesignSource source);
    void Delete(Guid id);
}
```

---

### Namespace: `App.Application.UseCases`

#### `CreateProjectUseCase`
```csharp
public class CreateProjectUseCase
{
    public CreateProjectUseCase(IProjectRepository repository) { }

    // Retorna el Guid del proyecto creado, o null si hay error
    public Guid? Execute(
        string name, string code, string description,
        string location, string client, string designCode,
        string structuralSystem, int numberOfStories,
        double totalHeight, string buildingUse,
        out string? errorMessage);
}
```

#### `HydrateSeismicSourceUseCase`
```csharp
public class HydrateSeismicSourceUseCase
{
    public HydrateSeismicSourceUseCase(
        ISapAdapter sapAdapter,
        ISeismicSourceRepository repository) { }

    // Retorna el Guid de la SeismicSource creada
    public Guid Execute(Guid projectId);
}
```

#### `HydrateDesignSourceUseCase`
```csharp
public class HydrateDesignSourceUseCase
{
    public HydrateDesignSourceUseCase(
        ISapAdapter sapAdapter,
        IDesignSourceRepository repository) { }

    // Retorna el Guid de la DesignSource creada
    public Guid Execute(Guid projectId);
}
```

---

### Namespace: `App.Application.Annexes`

#### `BeamDesignCalculator`
```csharp
public class BeamDesignCalculator
{
    // Constantes ACI 318
    public const double PhiFlexure = 0.90;
    public const double PhiShear   = 0.85;

    public BeamDesignReportRow Calculate(
        BeamDesignData data,
        double fcKgCm2,       // f'c (kg/cm²)
        double fyKgCm2,       // fy (kg/cm²)
        double bCm,           // ancho (cm)
        double dCm,           // peralte efectivo (cm)
        double stiSeparation  // separación de estribos (cm)
    );
}
```

#### `ColumnDesignCalculator`
```csharp
public class ColumnDesignCalculator
{
    public ColumnDesignReportRow Calculate(
        ColumnDesignData data,
        double fcKgCm2,
        double fyKgCm2
    );
}
```

#### `ShearWallDesignCalculator`
```csharp
public class ShearWallDesignCalculator
{
    public ShearWallDesignReportRow Calculate(
        WallDesignData data,
        double fcKgCm2,
        double fyKgCm2
    );
}
```

---

### Namespace: `App.Application.Export`

#### `IAnnexExporter`
```csharp
public interface IAnnexExporter
{
    void ExportBeamAnnex(BeamDesignAnnex annex, Stream output);
    void ExportColumnAnnex(ColumnDesignAnnex annex, Stream output);
    void ExportShearWallAnnex(ShearWallDesignAnnex annex, Stream output);
}
```

---

## App.SAP2000

### Namespace: `App.SAP2000.Adapters`

#### `SapAdapter : ISapAdapter`
Implementación completa de `ISapAdapter`. Delega internamente a:
- `SapConnectionService` para conexión, modelo y configuración
- `SapStructureOutputReader` para resultados sísmicos
- `SapDesignDataReader` para resultados de diseño

#### `SapConnectionService`
```csharp
internal class SapConnectionService
{
    // Conexión
    public void Connect();
    public void Disconnect();
    public bool IsConnected { get; }
    public string GetVersion();

    // Modelo
    public void OpenModel(string filePath);
    public void SaveModel();
    public void RunAnalysis();
    public void RunDesign();

    // Consultas
    public IList<string> GetStoryNames();
    public IList<string> GetFrameElementIds();
    public IList<string> GetAreaElementIds();
}
```

#### `SapStructureOutputReader`
```csharp
internal class SapStructureOutputReader
{
    public BaseShearSummary ReadBaseShear(string combo);
    public IList<StoryResult> ReadStoryShears(string combo);
    public IList<ModalResult> ReadModalResults();
    public IList<DriftResult> ReadStoryDrifts(string combo);
    public MassSummary ReadMassSummary();
}
```

#### `SapDesignDataReader`
```csharp
internal class SapDesignDataReader
{
    public IList<BeamDesignData> ReadBeamDesignData();
    public IList<ColumnDesignData> ReadColumnDesignData();
    public IList<WallDesignData> ReadWallDesignData();
    public IList<ElementForceRecord> ReadFrameForces(string combo);
}
```

---

## App.WinForms

### Namespace: `App.WinForms`

#### `Program` (static)
```csharp
static class Program
{
    [STAThread]
    static void Main();  // Composition Root
}
```

#### `MainForm : Form`
```csharp
public partial class MainForm : Form
{
    // Dependencias inyectadas
    private readonly ISapAdapter _sapAdapter;
    private readonly CreateProjectUseCase _createProject;
    private readonly HydrateSeismicSourceUseCase _hydrateSeismic;
    private readonly HydrateDesignSourceUseCase _hydrateDesign;

    // Estado
    private Guid? _activeProjectId;

    public MainForm(
        ISapAdapter sapAdapter,
        CreateProjectUseCase createProject,
        HydrateSeismicSourceUseCase hydrateSeismic,
        HydrateDesignSourceUseCase hydrateDesign);

    // Métodos UI
    private void UpdateSapStatus();
    private void menuNuevoProyecto_Click(object sender, EventArgs e);
    private void menuAbrir_Click(object sender, EventArgs e);
    private void menuGuardar_Click(object sender, EventArgs e);
    private void menuConectarSAP_Click(object sender, EventArgs e);
    private void menuDesconectarSAP_Click(object sender, EventArgs e);
    private void menuSalir_Click(object sender, EventArgs e);
}
```

### Namespace: `App.WinForms.UserControls.E030`

#### `E030UserControl : UserControl`
```csharp
public partial class E030UserControl : UserControl
{
    // Evento disparado al cambiar cualquier parámetro
    public event EventHandler<SeismicParametersEventArgs>? ValoresActualesChanged;

    // Retorna los parámetros sísmicos actuales
    public Dictionary<string, double> GetValoresActuales();
}
```

#### `E030Tables` (static)
```csharp
public static class E030Tables
{
    public static double GetZoneFactor(string zone);        // Z1→0.10, Z4→0.45
    public static double GetSoilFactor(string zone, string soil);  // S
    public static double GetTp(string zone, string soil);   // periodo de plataforma (s)
    public static double GetTl(string zone, string soil);   // periodo de corte largo (s)
    public static double GetR0(string structuralSystem);    // factor básico de reducción
    public static double GetIrregularityFactor(string type, string irregularity);
}
```

#### `SeismicParametersEventArgs : EventArgs`
```csharp
public class SeismicParametersEventArgs : EventArgs
{
    public Dictionary<string, double> Parameters { get; }
}
```

---

## Diagrama de Dependencias entre Clases

```
Program.cs
    │ crea e inyecta
    ├──> ProjectRepository ──────────────> IProjectRepository
    ├──> SeismicSourceRepository ────────> ISeismicSourceRepository
    ├──> DesignSourceRepository ─────────> IDesignSourceRepository
    ├──> SapAdapter ─────────────────────> ISapAdapter
    ├──> CreateProjectUseCase(projectRepo)
    ├──> HydrateSeismicSourceUseCase(sap, seismicRepo)
    ├──> HydrateDesignSourceUseCase(sap, designRepo)
    └──> MainForm(sap, create, hydrateSeismic, hydrateDesign)
              │ usa
              ├──> NewProjectDialog
              ├──> SeismicityForm
              │         └──> E030UserControl
              │         └──> EspectroUserControl
              └──> [otros formularios futuros]
```

---

*Anterior: [Partes y Componentes](./PARTES_Y_COMPONENTES.md) | Siguiente: [Flujos de Trabajo](./FLUJOS_DE_TRABAJO.md)*
