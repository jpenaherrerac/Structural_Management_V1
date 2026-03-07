# Arquitectura Completa de Interacción con SAP2000

> **Objetivo**: Describir exhaustivamente cómo este proyecto se conecta y opera con SAP2000 vía COM API, de modo que el patrón pueda ser extraído, replicado y reutilizado en cualquier otro proyecto .NET que necesite interactuar con SAP2000.

---

## Índice

1. [Resumen Ejecutivo](#1-resumen-ejecutivo)
2. [Mapa de Archivos](#2-mapa-de-archivos)
3. [Capa 1 — STA Thread Host (SapStaHost)](#3-capa-1--sta-thread-host-sapstahost)
4. [Capa 2 — Procesador COM (SapProcessor)](#4-capa-2--procesador-com-sapprocessor)
5. [Capa 3 — Facade de Comandos (SapModelFacade)](#5-capa-3--facade-de-comandos-sapmodelfacade)
6. [Capa 4 — Motores de Ejecución (Engines)](#6-capa-4--motores-de-ejecución-engines)
7. [Capa 5 — Orquestación (SapOrchestrator / Sap2000Adapter / SapBranchExecutor)](#7-capa-5--orquestación)
8. [Capa 6 — UI (WinForms Tab Controller)](#8-capa-6--ui-winforms-tab-controller)
9. [Flujo Completo End-to-End](#9-flujo-completo-end-to-end)
10. [Diagrama de Capas y Dependencias](#10-diagrama-de-capas-y-dependencias)
11. [Compilación Condicional (SAP2000_AVAILABLE)](#11-compilación-condicional-sap2000_available)
12. [Guía para Reutilización en Otros Proyectos](#12-guía-para-reutilización-en-otros-proyectos)
13. [Prompt Generador para Nuevos Proyectos](#13-prompt-generador-para-nuevos-proyectos)

---

## 1. Resumen Ejecutivo

La interacción con SAP2000 en este proyecto sigue un patrón de **6 capas** claramente separadas (3 capas núcleo reutilizables + 3 capas de aplicación):

```
┌──────────────────────────────────────────────────────────────┐
│  UI (WinForms)  →  Botones: Conectar / Hidratar / Enviar    │
├──────────────────────────────────────────────────────────────┤
│  Orquestadores  →  SapOrchestrator / Sap2000Adapter /        │
│                    SapBranchExecutor                          │
├──────────────────────────────────────────────────────────────┤
│  Motores (Engines)  →  MaterialEngine, ShellAreasEngine,     │
│                        LoadCombinationsEngine, etc.           │
├──────────────────────────────────────────────────────────────┤
│  Facade  →  SapModelFacade (todos los comandos SAP)          │
├──────────────────────────────────────────────────────────────┤
│  Conexión COM  →  SapProcessor (conectar/inicializar)        │
│                    SapStaHost (hilo STA dedicado)             │
└──────────────────────────────────────────────────────────────┘
```

**Principio fundamental**: SAP2000 **no contiene lógica de negocio**. Toda decisión se toma antes; SAP2000 recibe datos ya procesados y simplemente los ejecuta.

---

## 2. Mapa de Archivos

Todos los archivos SAP2000 viven en `src/App.Infrastructure/Sap2000/`:

```
src/App.Infrastructure/Sap2000/
├── SapStaHost.cs              ← Capa 1: Hilo STA dedicado con message loop Win32
├── SapProcessor.cs            ← Capa 2: Conexión COM (connect, init, release)
├── SapModelFacade.cs          ← Capa 3: Facade con TODOS los comandos SAP2000
├── SapComDiagnostics.cs       ← Diagnóstico: verifica ProgIDs en el registry
├── BranchHydrationService.cs  ← Prepara datos (Nodes/Cells) antes de enviar a SAP
├── Sap2000Adapter.cs          ← Capa 5: Adapter público (Initialize/Send/Close)
├── SapOrchestrator.cs         ← Capa 5: Orquestador one-shot (connect-execute-release)
├── SapBranchExecutor.cs       ← Capa 5: Ejecutor detallado Branch→SAP2000
├── Motores/                   ← Capa 4: Engines especializados por tarea
│   ├── MaterialAndPropertiesEngine.cs
│   ├── ShellAreasCreatorEngine.cs
│   ├── SpecialPointsCreatorEngine.cs
│   ├── AssignSprings.cs
│   ├── PlateBodyConstraintsEngine.cs
│   ├── PlateCentroidLoadsEngine.cs
│   ├── LoadCombinationsEngine.cs
│   ├── UniformAreaLoadsEngine.cs
│   ├── GroupsCreationAndAssignEngine.cs
│   ├── LoadPatternEngine.cs
│   ├── LoadCaseEngine.cs
│   └── ... (otros engines)
└── Parameters/                ← Datos JSON de configuración
    ├── materials.json
    ├── area-properties.json
    ├── patterns.json
    ├── cases.json
    ├── combos.json
    └── ... (otros JSON)
```

---

## 3. Capa 1 — STA Thread Host (SapStaHost)

**Archivo**: `SapStaHost.cs`  
**Propósito**: SAP2000 es un servidor COM que requiere un hilo STA (Single-Threaded Apartment). Esta clase crea un hilo dedicado con un message loop nativo Win32 para todas las llamadas COM.

### ¿Por qué es necesario?

SAP2000 COM no es thread-safe. **Todas** las llamadas COM deben ejecutarse en el mismo hilo STA que creó los objetos COM. Si se llaman desde otro hilo, se producen errores o bloqueos.

### Estructura

```csharp
public static class SapStaHost
{
    public sealed class StaComRunner : IDisposable
    {
        // Ejecuta una acción en el hilo STA dedicado (bloqueante para el llamador)
        public void Invoke(Action action);
        
        // Versión con retorno de valor
        public T Invoke<T>(Func<T> func);
        
        // Libera el hilo STA
        public void Dispose();
    }
    
    // Factory method
    public static StaComRunner CreateRunner();
}
```

### Mecanismo interno

1. `CreateRunner()` crea un `StaComRunner` que lanza un hilo background con `ApartmentState.STA`.
2. El hilo ejecuta `CoInitializeEx(COINIT_APARTMENTTHREADED)` y entra en un **message loop** Win32 (`GetMessage`/`TranslateMessage`/`DispatchMessage`).
3. `Invoke(action)` envía un mensaje `WM_INVOKE` al hilo STA via `PostThreadMessage`, con un `GCHandle` a un `InvocationInfo`.
4. El hilo STA recibe el mensaje, ejecuta la acción, y señala `ManualResetEventSlim.Done`.
5. El llamador espera en `info.Done.Wait()`. Si hubo excepción, se re-lanza con `ExceptionDispatchInfo.Capture`.
6. `Dispose()` envía `WM_QUIT` para terminar el message loop y hace `Join(2000)` al hilo.

### P/Invoke utilizadas

| Función | Propósito |
|---------|-----------|
| `CoInitializeEx` | Inicializa COM en modo STA |
| `CoUninitialize` | Libera COM |
| `PeekMessage` | Fuerza creación de cola de mensajes |
| `GetMessage` | Espera y extrae mensajes |
| `PostThreadMessage` | Envía WM_INVOKE / WM_QUIT al hilo STA |
| `TranslateMessage` / `DispatchMessage` | Procesamiento estándar de mensajes Win32 |
| `GetCurrentThreadId` | Obtiene el ThreadId para PostThreadMessage |

### Uso

```csharp
using (var runner = SapStaHost.CreateRunner())
{
    runner.Invoke(() =>
    {
        // Todo código COM SAP2000 va aquí dentro
    });
}
```

---

## 4. Capa 2 — Procesador COM (SapProcessor)

**Archivo**: `SapProcessor.cs`  
**Propósito**: Encapsula la lógica de **conexión** a SAP2000, la **inicialización** del modelo, y la **liberación** de objetos COM.

### Miembros COM principales

```csharp
public sealed class SapProcessor
{
    private cHelper helper;        // Helper COM de SAP2000
    private cOAPI sapObject;       // Objeto principal de la aplicación SAP2000
    private cSapModel sapModel;    // Modelo SAP2000 (el API real)

    public cSapModel SapModel => sapModel;  // Expuesto públicamente
    public string ConnectedProgId { get; }  // Diagnóstico: qué ProgID se usó

    public void ConnectAndInit(string units = null, bool visible = false);
    public void ReleaseCom();
}
```

### Flujo de conexión (`ConnectAndInit`)

El método `ConnectAndInit()` ejecuta internamente dos fases:

#### Fase A: `CreateHelperAndSapObject(visible)`

Intenta conectar a SAP2000 en orden de prioridad:

```
1) ROT (Running Object Table) — Marshal.GetActiveObject(progId)
   → Intenta engancharse a una instancia YA abierta de SAP2000
   → Prueba 3 ProgIDs: CSI.SAP2000.API.SapObject, Sap2000v1.SapObject, CSI.SAP2000.SapObject

2) Helper.GetObject(progId)
   → Método alternativo del helper COM de SAP2000
   → Prueba los mismos 3 ProgIDs

3) Helper.CreateObjectProgID + ApplicationStart
   → Crea una instancia NUEVA de SAP2000
   → Usa CSI.SAP2000.API.SapObject
   → Llama sapObject.ApplicationStart(eUnits.N_m_C, visible, "")

Finalmente: sapModel = sapObject.SapModel
```

#### Fase B: `InitModel()`

```
1) sapModel.InitializeNewModel(eUnits.N_m_C)
2) sapModel.File.NewBlank()
   → Si NewBlank falla, reintenta creando nueva instancia completa
```

### ProgIDs conocidos de SAP2000

| ProgID | Descripción |
|--------|-------------|
| `CSI.SAP2000.API.SapObject` | Versión moderna (SAP2000 v20+) |
| `Sap2000v1.SapObject` | Versión heredada |
| `CSI.SAP2000.SapObject` | Alternativa |

### Liberación COM (`ReleaseCom`)

```csharp
public void ReleaseCom()
{
    Marshal.ReleaseComObject(sapModel);   sapModel = null;
    Marshal.ReleaseComObject(sapObject);  sapObject = null;
    Marshal.ReleaseComObject(helper);     helper = null;
}
```

> **Importante**: `ReleaseCom()` debe ejecutarse en el mismo hilo STA donde se crearon los objetos.

---

## 5. Capa 3 — Facade de Comandos (SapModelFacade)

**Archivo**: `SapModelFacade.cs`  
**Propósito**: Centraliza **todos** los comandos del API COM de SAP2000 en métodos tipados con verificación automática de código de retorno. Es la **biblioteca reutilizable** de operaciones SAP2000.

### Estructura

```csharp
public sealed class SapModelFacade
{
    private readonly cSapModel _sapModel;

    public SapModelFacade(cSapModel sapModel);
    public cSapModel Raw => _sapModel;  // Acceso directo si se necesita

    // Verificación universal de retorno
    public void Check(int ret, string where);
}
```

### Patrón de cada método

Cada método del Facade:
1. Recibe parámetros tipados
2. Llama al método COM correspondiente de `_sapModel`
3. Verifica el código de retorno con `Check(ret, "descripción")`
4. Si `ret != 0`, lanza `InvalidOperationException`

Ejemplo:

```csharp
public void PropArea_SetShell(string name, int shellType, string material, 
    double matAngle, double thickMembrane, double thickBending)
{
    int r = _sapModel.PropArea.SetShell(name, shellType, material, 
        matAngle, thickMembrane, thickBending);
    Check(r, $"PropArea.SetShell({name})");
}
```

### Categorías de comandos disponibles

| Categoría | Métodos Facade | API SAP2000 subyacente |
|-----------|---------------|------------------------|
| **Materiales** | `PropMaterial_SetMaterial`, `PropMaterial_SetMPIsotropic`, `PropMaterial_SetOConcrete_1` | `_sapModel.PropMaterial.*` |
| **Propiedades de Área (Shell)** | `PropArea_SetShell` | `_sapModel.PropArea.*` |
| **Puntos** | `PointObj_AddCartesian`, `PointObj_SetSpecialPoint`, `PointObj_SetRestraint`, `PointObj_SetSelected`, `PointObj_SetGroupAssign`, `PointObj_SetConstraint` | `_sapModel.PointObj.*` |
| **Áreas** | `AreaObj_AddByCoord`, `AreaObj_AddByPoint`, `AreaObj_ChangeName`, `AreaObj_SetProperty`, `AreaObj_SetGroupAssign`, `AreaObj_SetLoadUniform`, `AreaObj_SetSpring` | `_sapModel.AreaObj.*` |
| **Edición de Áreas** | `EditArea_Divide` | `_sapModel.EditArea.*` |
| **Load Patterns** | `LoadPatterns_Add` | `_sapModel.LoadPatterns.*` |
| **Load Cases (Static Linear)** | `LoadCases_StaticLinear_SetCase`, `_SetInitialCase`, `_SetLoads` | `_sapModel.LoadCases.StaticLinear.*` |
| **Load Cases (Static Nonlinear)** | `_SetCase`, `_SetInitialCase`, `_SetGeometricNonlinearity`, `_SetLoadApplication`, `_SetLoads`, `_SetMassSource`, `_SetModalCase`, `_SetResultsSaved`, `_SetSolControlParameters`, `_SetTargetForceParameters` | `_sapModel.LoadCases.StaticNonlinear.*` |
| **Load Cases (Modal Eigen)** | `_SetCase`, `_SetInitialCase`, `_SetLoads`, `_SetNumberModes`, `_SetParameters` | `_sapModel.LoadCases.ModalEigen.*` |
| **Load Cases (Modal Ritz)** | `_SetCase`, `_SetInitialCase`, `_SetLoads`, `_SetNumberModes` | `_sapModel.LoadCases.ModalRitz.*` |
| **Load Cases (Response Spectrum)** | `_SetCase`, `_SetDampConstant`, `_SetDampInterpolated`, `_SetDampOverrides`, `_SetDampProportional`, `_SetDiaphragmEccentricityOverride`, `_SetDirComb`, `_SetEccentricity`, `_SetLoads`, `_SetModalCase`, `_SetModalComb_1` | `_sapModel.LoadCases.ResponseSpectrum.*` |
| **Combinaciones** | `RespCombo_Add`, `RespCombo_SetCaseList` | `_sapModel.RespCombo.*` |
| **Constraints** | `ConstraintDef_SetBody` | `_sapModel.ConstraintDef.*` |
| **Grupos** | `GroupDef_SetGroup` | `_sapModel.GroupDef.*` |
| **Selección** | `SelectObj_ClearSelection`, `SelectObj_All`, `SelectObj_PropertyArea` | `_sapModel.SelectObj.*` |
| **Vista** | `View_RefreshView`, `SapObject_Hide`, `SapObject_Unhide` | `_sapModel.View.*` |

### Compilación condicional

El archivo tiene **dos versiones**:
- `#if SAP2000_AVAILABLE`: La versión real con llamadas COM
- `#else`: Stub que lanza `NotSupportedException` para cada método (permite compilar sin SAP2000v1.dll)

---

## 6. Capa 4 — Motores de Ejecución (Engines)

**Directorio**: `src/App.Infrastructure/Sap2000/Motores/`  
**Propósito**: Cada Engine encapsula una **tarea específica** de construcción del modelo SAP2000. Todos reciben el `SapModelFacade` y operan exclusivamente a través de él.

### Patrón común de un Engine

```csharp
public sealed class MiEngine
{
    private readonly SapModelFacade _facade;

    public MiEngine(SapModelFacade facade)
    {
        _facade = facade ?? throw new ArgumentNullException(nameof(facade));
    }

    public ResultType Execute(/* parámetros de dominio */)
    {
        // Usa _facade.MetodoX(), _facade.MetodoY(), etc.
        // Nunca accede directamente a cSapModel
    }
}
```

### Engines implementados y sus responsabilidades

| Engine | Responsabilidad |
|--------|----------------|
| **MaterialAndPropertiesEngine** | Crea el material concreto (fc→E→ν) y 3 propiedades shell (LOSA, PLATE, TRENCH) con espesores configurables |
| **SpecialPointsCreatorEngine** | Crea puntos especiales en centroides de placas (Z=0.10) |
| **ShellAreasCreatorEngine** | Crea áreas shell a partir de las celdas efectivas del Branch, clasificadas por tipo (General/Plate/Trench) |
| **AssignSpringsEngine** | Asigna resortes (balasto) a todas las áreas (selecciona todas → aplica AreaObj.SetSpring) |
| **PlateBodyConstraintsEngine** | Define y asigna constraints tipo Body por cada Plate (centroide + anchor points) |
| **PlateCentroidLoadsEngine** | Crea patrones de carga y asigna cargas puntuales a los centroides de las placas |
| **LoadCombinationsEngine** | Crea las combinaciones de carga (servicio S1-S11.2, últimas U1-U11.2, envolventes) |
| **UniformAreaLoadsEngine** | Asigna carga uniforme LC8 (carga viva) a todas las áreas |
| **GroupsCreationAndAssignEngine** | Crea grupos SAP2000 y asigna objetos a grupos |
| **LoadPatternEngine** | Crea load patterns individuales |
| **LoadCaseEngine** | Crea load cases |

### Ejemplo completo: AssignSpringsEngine

```csharp
internal sealed class AssignSpringsEngine
{
    private readonly SapModelFacade _facade;

    public AssignSpringsEngine(SapModelFacade facade)
    {
        _facade = facade;
    }

    public void Execute(double coefBalasto)
    {
        // 1. Limpiar selección
        _facade.SelectObj_ClearSelection();
        
        // 2. Seleccionar todas las áreas
        _facade.SelectObj_All(false);
        
        // 3. Aplicar resortes a la selección
        double[] vec = new double[] { 0.0, 0.0, 0.0 };
        _facade.AreaObj_SetSpring(
            name: "",
            myType: 1, s: coefBalasto, simpleSpringType: 2,
            linkProp: "", face: -1, springLocalOneType: 2,
            dir: 0, outward: false, ref vec, ang: 0.0,
            replace: true, cSys: "Local",
            itemType: eItemType.SelectedObjects);
        
        // 4. Limpiar selección
        _facade.SelectObj_ClearSelection();
    }
}
```

---

## 7. Capa 5 — Orquestación

Existen tres niveles de orquestación, cada uno para un caso de uso distinto:

### 7.1 SapOrchestrator — Ejecución one-shot

**Uso**: Conectar → Ejecutar snapshot → Liberar. Todo en un solo llamado.

```csharp
public sealed class SapOrchestrator
{
    public SapRunResult Run(SapBuildSnapshot snapshot, string saveAsPath = null, bool visible = false)
    {
        using (var runner = SapStaHost.CreateRunner())    // 1. Crear hilo STA
        {
            var proc = new SapProcessor();
            try
            {
                runner.Invoke(() =>
                {
                    proc.ConnectAndInit(visible: visible);  // 2. Conectar
                    _executor.Execute(proc, snapshot);       // 3. Ejecutar
                    if (saveAsPath != null)
                        proc.SapModel.File.Save(saveAsPath); // 4. Guardar
                });
                return SapRunResult.Success(proc.ConnectedProgId);
            }
            catch (Exception ex)
            {
                return SapRunResult.Failure(proc.ConnectedProgId, ex);
            }
            finally
            {
                runner.Invoke(proc.ReleaseCom);              // 5. Liberar COM
            }
        }
    }
}
```

### 7.2 Sap2000Adapter — Sesión persistente (Initialize/Send/Close)

**Uso**: Mantener la conexión abierta para múltiples operaciones.

```csharp
public class Sap2000Adapter : IDisposable
{
    // Estado de conexión persistente
    private StaComRunner _runner;
    private SapProcessor _processor;

    public bool IsConnected { get; }
    public string ConnectedProgId { get; }

    public void Initialize(bool visible = true);           // Conectar
    public SapRunResult SendSnapshot(snapshot, saveAsPath); // Enviar datos
    public void SaveModel(string filePath);                 // Guardar
    public void Close();                                    // Liberar
    
    // Atajo one-shot (usa SapOrchestrator internamente)
    public static SapRunResult RunComplete(snapshot, saveAsPath, visible);
}
```

### 7.3 SapBranchExecutor — Ejecutor detallado por pasos

**Uso**: Ejecuta la secuencia completa de construcción del modelo SAP2000 paso a paso, directamente desde un `BranchMeshSet` hidratado.

```csharp
public sealed class SapBranchExecutor
{
    public SapBranchExecuteResult Execute(SapProcessor proc, BranchMeshSet branch, 
        LoadExportData loadData = null, ExecuteOptions options = null)
    {
        // Step 0: Hidratar branch (asegurar Nodes/Cells/ReconstructedEntities)
        var hyd = BranchHydrationService.Hydrate(branch);
        
        var facade = new SapModelFacade(proc.SapModel);  // ← Crea el Facade
        
        facade.SapObject_Hide();  // Ocultar SAP para velocidad
        
        // Step 1: Material y propiedades shell
        var matEngine = new MaterialAndPropertiesEngine(facade);
        matEngine.Execute(fc, thicknessLosa, thicknessPlate, thicknessTrench);
        
        // Step 2: Puntos especiales (centroides de placas)
        var pointsEngine = new SpecialPointsCreatorEngine(facade);
        pointsEngine.CreatePlateCentroidsFromReconstructedEntities(branch);
        
        // Step 3: Áreas shell desde celdas
        var areasEngine = new ShellAreasCreatorEngine(facade);
        areasEngine.CreateFromEffectiveCells(branch, "LOSA", "PLATE", "TRENCH");
        
        // Step 4: Resortes (balasto) a todas las áreas
        var springsEngine = new AssignSpringsEngine(facade);
        springsEngine.Execute(coefBalasto);
        
        // Step 5: Restraints en puntos de contorno
        AssignBoundaryRestraintsToPoints(facade, boundaryPoints, "PuntosBASE");
        
        // Step 6: Body constraints por placa
        var bodyEngine = new PlateBodyConstraintsEngine(facade);
        bodyEngine.ExecuteWithAnchorPoints(branch, centroids, anchors);
        
        // Step 7: Load patterns + cargas puntuales
        var loadsEngine = new PlateCentroidLoadsEngine(facade);
        loadsEngine.CreateLoadPatterns(loadData);
        loadsEngine.AssignLoadsToPlatecentroids(loadData, branch);
        
        // Step 8: Combinaciones de carga
        var combosEngine = new LoadCombinationsEngine(facade);
        combosEngine.CreateAllCombinations();
        
        // Step 9: Carga uniforme LC8
        var uniformEngine = new UniformAreaLoadsEngine(facade);
        uniformEngine.AssignLC8UniformLoad(2000.0);
        
        facade.View_RefreshView(0, true);  // Refrescar vista
        facade.SapObject_Unhide();         // Mostrar SAP
    }
}
```

---

## 8. Capa 6 — UI (WinForms Tab Controller)

**Archivo**: `src/App.WinForms/Tabs/Sap2000/Sap2000TabController.cs`  
**Propósito**: La UI tiene 3 botones que invocan las capas inferiores:

### Flujo de interacción del usuario

```
┌────────────────────┐     ┌────────────────────┐     ┌────────────────────┐
│  [Conectar SAP2000]│ ──> │  [Hidratar Branch] │ ──> │  [Enviar a SAP2000]│
└────────────────────┘     └────────────────────┘     └────────────────────┘
```

### Botón "Conectar SAP2000"

```csharp
private void ConnectSap()
{
    _sapRunner = SapStaHost.CreateRunner();          // Crear hilo STA
    _sapRunner.Invoke(() =>
    {
        _sapProcessor = new SapProcessor();
        _sapProcessor.ConnectAndInit(visible: true); // Conectar
    });
}
```

### Botón "Hidratar Branch"

```csharp
private void HydrateBranch()
{
    var branch = GetSelectedBranch();
    var result = BranchHydrationService.Hydrate(branch);  // Preparar datos
}
```

### Botón "Enviar a SAP2000"

```csharp
private void ExecuteSap()
{
    var executor = new SapBranchExecutor();
    _sapRunner.Invoke(() =>
    {
        execResult = executor.Execute(_sapProcessor, branch, exportLoads);
    });
}
```

---

## 9. Flujo Completo End-to-End

```
                          HILO PRINCIPAL (UI)                    HILO STA (COM)
                          ─────────────────                      ──────────────

1. Click "Conectar"  ──> SapStaHost.CreateRunner()  ──────────> Crea hilo STA
                         runner.Invoke(...)  ────────────────── > new SapProcessor()
                                                                  .ConnectAndInit()
                                                                    ├── new Helper()
                                                                    ├── GetActiveObject() o CreateObjectProgID()
                                                                    ├── ApplicationStart()
                                                                    ├── sapModel = sapObject.SapModel
                                                                    ├── InitializeNewModel()
                                                                    └── File.NewBlank()
                         <── conexión establecida ◄────────────

2. Click "Hidratar"  ──> BranchHydrationService.Hydrate(branch)
                         (ejecuta en hilo principal - no necesita COM)
                         ├── RebuildReconstructedEntities
                         ├── RebuildNodes
                         ├── RebuildCells
                         └── PopulateBoundaryNodeIds

3. Click "Enviar"    ──> runner.Invoke(...)  ────────────────── > SapBranchExecutor.Execute()
                                                                    ├── SapModelFacade(proc.SapModel)
                                                                    ├── MaterialAndPropertiesEngine
                                                                    │     └── facade.PropMaterial_*()
                                                                    │     └── facade.PropArea_SetShell()
                                                                    ├── SpecialPointsCreatorEngine
                                                                    │     └── facade.PointObj_AddCartesian()
                                                                    ├── ShellAreasCreatorEngine
                                                                    │     └── facade.AreaObj_AddByPoint()
                                                                    ├── AssignSpringsEngine
                                                                    │     └── facade.AreaObj_SetSpring()
                                                                    ├── PlateBodyConstraintsEngine
                                                                    │     └── facade.ConstraintDef_SetBody()
                                                                    ├── PlateCentroidLoadsEngine
                                                                    │     └── facade.LoadPatterns_Add()
                                                                    ├── LoadCombinationsEngine
                                                                    │     └── facade.RespCombo_Add()
                                                                    ├── UniformAreaLoadsEngine
                                                                    │     └── facade.AreaObj_SetLoadUniform()
                                                                    └── facade.View_RefreshView()
                         <── resultado ◄───────────────────────

4. Cierre             ──> runner.Invoke(proc.ReleaseCom)
                         runner.Dispose()
```

---

## 10. Diagrama de Capas y Dependencias

```
┌─────────────────────────────────────────────────────────────────────────┐
│                         APP.WINFORMS (UI)                               │
│  Sap2000TabController                                                   │
│    ├── Tiene: StaComRunner _sapRunner                                   │
│    ├── Tiene: SapProcessor _sapProcessor                                │
│    └── Usa: SapBranchExecutor + BranchHydrationService                  │
├─────────────────────────────────────────────────────────────────────────┤
│                     APP.INFRASTRUCTURE.SAP2000                          │
│                                                                         │
│  ┌─────────────────┐    ┌─────────────────┐                             │
│  │ SapStaHost       │    │ SapProcessor    │                             │
│  │ (Hilo STA)       │    │ (Conexión COM)  │                             │
│  │ ┌─────────────┐  │    │                 │                             │
│  │ │StaComRunner  │  │    │ .SapModel ──────┼──────┐                     │
│  │ │ .Invoke()    │  │    │ .ConnectAndInit()│      │                     │
│  │ └─────────────┘  │    │ .ReleaseCom()   │      │                     │
│  └─────────────────┘    └─────────────────┘      │                     │
│                                                    │                     │
│  ┌─────────────────────────────────────────────────┤                     │
│  │ SapModelFacade                                  │                     │
│  │ (recibe cSapModel del SapProcessor)             │                     │
│  │                                                 │                     │
│  │  .PropMaterial_SetMaterial()                     │                     │
│  │  .PropArea_SetShell()                           │                     │
│  │  .PointObj_AddCartesian()                       │                     │
│  │  .AreaObj_AddByPoint()                          │                     │
│  │  .LoadPatterns_Add()                            │                     │
│  │  .RespCombo_Add()                               │                     │
│  │  ... (60+ métodos)                              │                     │
│  └─────────────────────────────────────────────────┘                     │
│           │                                                              │
│           ▼                                                              │
│  ┌──────────────────────────────────────────────┐                        │
│  │ MOTORES (cada uno recibe SapModelFacade)      │                        │
│  │  ├── MaterialAndPropertiesEngine              │                        │
│  │  ├── ShellAreasCreatorEngine                  │                        │
│  │  ├── SpecialPointsCreatorEngine               │                        │
│  │  ├── AssignSpringsEngine                      │                        │
│  │  ├── LoadCombinationsEngine                   │                        │
│  │  ├── PlateCentroidLoadsEngine                 │                        │
│  │  └── ...                                      │                        │
│  └──────────────────────────────────────────────┘                        │
│           │                                                              │
│           ▼                                                              │
│  ┌──────────────────────────────────────────────┐                        │
│  │ ORQUESTADORES (coordinan engines en secuencia)│                        │
│  │  ├── SapBranchExecutor (paso a paso detallado)│                        │
│  │  ├── SapOrchestrator (one-shot)               │                        │
│  │  └── Sap2000Adapter (sesión persistente)      │                        │
│  └──────────────────────────────────────────────┘                        │
└─────────────────────────────────────────────────────────────────────────┘
                    │
                    ▼
┌─────────────────────────────────────────────────────────────────────────┐
│                      SAP2000 (Proceso Externo)                          │
│                      COM Server (SAP2000v1.dll)                         │
│                      cSapModel → PropMaterial, PropArea, PointObj, etc.  │
└─────────────────────────────────────────────────────────────────────────┘
```

---

## 11. Compilación Condicional (SAP2000_AVAILABLE)

El proyecto usa la directiva `SAP2000_AVAILABLE` para compilar con o sin la referencia a `SAP2000v1.dll`:

```csharp
#if SAP2000_AVAILABLE
using SAP2000v1;
// Implementación real con llamadas COM
#else
// Stub que lanza NotSupportedException
#endif
```

**Archivos con compilación condicional**:
- `SapProcessor.cs`
- `SapModelFacade.cs`
- Todos los engines en `Motores/`
- `SapBranchExecutor.cs`

Esto permite que el proyecto compile y se ejecute en máquinas de desarrollo que no tienen SAP2000 instalado.

---

## 12. Guía para Reutilización en Otros Proyectos

### Archivos mínimos a copiar (núcleo reutilizable)

Para cualquier proyecto que necesite interactuar con SAP2000, se necesitan estos **3 archivos** como núcleo:

| # | Archivo | Propósito | Dependencias |
|---|---------|-----------|--------------|
| 1 | `SapStaHost.cs` | Hilo STA con message loop | Solo System (Win32 P/Invoke) |
| 2 | `SapProcessor.cs` | Conexión y lifecycle COM | SAP2000v1.dll (interop) |
| 3 | `SapModelFacade.cs` | Biblioteca de comandos | SAP2000v1.dll (interop) |

Opcionalmente:

| # | Archivo | Propósito |
|---|---------|-----------|
| 4 | `SapComDiagnostics.cs` | Diagnóstico de registro COM |
| 5 | `Sap2000Adapter.cs` + `SapOrchestrator.cs` | Orquestación ready-to-use |

### Pasos para integrar en un nuevo proyecto

1. **Agregar referencia COM**: Agregar `SAP2000v1` como referencia COM en el `.csproj`, o usar interop assembly generado.

2. **Copiar los 3 archivos núcleo** al proyecto destino, ajustando el namespace.

3. **Definir la constante de compilación** `SAP2000_AVAILABLE` en las configuraciones de build donde SAP2000 esté disponible.

4. **Crear engines** específicos para tu dominio. Cada engine recibe `SapModelFacade` y llama sus métodos.

5. **Uso básico**:

```csharp
using (var runner = SapStaHost.CreateRunner())
{
    var proc = new SapProcessor();

    runner.Invoke(() =>
    {
        // Conectar
        proc.ConnectAndInit(visible: true);

        // Crear facade
        var facade = new SapModelFacade(proc.SapModel);

        // Usar facade directamente o a través de engines
        facade.PropMaterial_SetMaterial("CONC210", 2);
        facade.PropArea_SetShell("LOSA", 1, "CONC210", 0.0, 0.70, 0.70);
        
        // ... más operaciones ...
        
        facade.View_RefreshView();
    });

    // Liberar COM en el hilo STA
    runner.Invoke(proc.ReleaseCom);
}
```

### Patrón recomendado para crear nuevos Engines

```csharp
public sealed class MiNuevoEngine
{
    private readonly SapModelFacade _facade;

    public MiNuevoEngine(SapModelFacade facade)
    {
        _facade = facade ?? throw new ArgumentNullException(nameof(facade));
    }

    public void Execute(/* parámetros específicos del dominio */)
    {
        // Siempre usar _facade, nunca acceder a cSapModel directamente
        _facade.PropMaterial_SetMaterial(...);
        _facade.AreaObj_AddByPoint(...);
        // etc.
    }
}
```

### Patrón recomendado para un nuevo Orquestador

```csharp
public sealed class MiOrquestador
{
    public void Run(MisDatos datos, bool visible = true)
    {
        using (var runner = SapStaHost.CreateRunner())
        {
            var proc = new SapProcessor();
            try
            {
                runner.Invoke(() =>
                {
                    proc.ConnectAndInit(visible: visible);
                    var facade = new SapModelFacade(proc.SapModel);

                    // Ejecutar engines en secuencia
                    new Engine1(facade).Execute(datos.Parte1);
                    new Engine2(facade).Execute(datos.Parte2);
                    new Engine3(facade).Execute(datos.Parte3);

                    facade.View_RefreshView();
                });
            }
            finally
            {
                try { runner.Invoke(proc.ReleaseCom); } catch { }
            }
        }
    }
}
```

---

## 13. Prompt Generador para Nuevos Proyectos

Puedes usar el siguiente prompt para generar una nueva app de conexión con SAP2000 basada en esta arquitectura:

---

> **Prompt para crear una nueva app de conexión SAP2000:**
> 
> Necesito crear una aplicación .NET Framework 4.8 (WinForms) que se conecte a SAP2000 vía COM API siguiendo este patrón de arquitectura:
> 
> **ARQUITECTURA DE 3 CAPAS NÚCLEO (reutilizables tal cual) + capas de aplicación:**
> 
> La arquitectura completa tiene 6 capas: 3 núcleo (SapStaHost, SapProcessor, SapModelFacade) que se copian directo, y 3 de aplicación (Engines, Orquestadores, UI) que se adaptan al dominio.
> 
> **Capa 1 - SapStaHost.cs**: Clase estática con una inner class `StaComRunner` (IDisposable). Crea un hilo background con `ApartmentState.STA` que ejecuta un message loop Win32 nativo (GetMessage/TranslateMessage/DispatchMessage). Expone `Invoke(Action)` e `Invoke<T>(Func<T>)` que envían mensajes WM_INVOKE vía `PostThreadMessage` al hilo STA, esperan la ejecución y re-lanzan excepciones con `ExceptionDispatchInfo`. Usa P/Invoke para `CoInitializeEx`, `CoUninitialize`, `GetCurrentThreadId`, `PeekMessage`, `GetMessage`, `PostThreadMessage`, `TranslateMessage`, `DispatchMessage`. El factory method es `SapStaHost.CreateRunner()`.
> 
> **Capa 2 - SapProcessor.cs**: Clase sealed que encapsula `cHelper`, `cOAPI sapObject`, `cSapModel sapModel`. Método `ConnectAndInit()` que: (1) crea `new Helper()`, (2) intenta `Marshal.GetActiveObject` con 3 ProgIDs (CSI.SAP2000.API.SapObject, Sap2000v1.SapObject, CSI.SAP2000.SapObject), (3) intenta `helper.GetObject(progId)`, (4) si nada funciona usa `helper.CreateObjectProgID("CSI.SAP2000.API.SapObject")` + `sapObject.ApplicationStart(eUnits.N_m_C, visible, "")`. Luego obtiene `sapModel = sapObject.SapModel`, inicializa con `InitializeNewModel(eUnits.N_m_C)` y `File.NewBlank()`. Expone `SapModel` y `ReleaseCom()` que llama `Marshal.ReleaseComObject` en orden inverso.
> 
> **Capa 3 - SapModelFacade.cs**: Clase sealed que recibe `cSapModel` y expone métodos tipados para cada operación SAP2000. Cada método llama al API COM correspondiente y verifica `ret != 0` lanzando excepción si falla. Ejemplo: `PropArea_SetShell(name, shellType, material, matAngle, thickMembrane, thickBending)`. Usar compilación condicional `#if SAP2000_AVAILABLE` para la versión real y `#else` para stubs.
> 
> **USO:** Crear engines que reciban `SapModelFacade` y encapsulen tareas específicas (crear materiales, crear áreas, asignar cargas, etc.). Un orquestador crea el runner, conecta, ejecuta engines en secuencia, y libera COM. Todo dentro de `runner.Invoke(() => { ... })`.
> 
> **Mi proyecto necesita hacer:** [DESCRIBIR AQUÍ LO QUE TU PROYECTO NECESITA HACER CON SAP2000]
> 
> Genera los 3 archivos núcleo (SapStaHost.cs, SapProcessor.cs, SapModelFacade.cs) con los métodos del facade que necesite mi proyecto, y los engines específicos para mi dominio.

---

### Resumen de los conceptos clave para copiar

| Concepto | Qué copiar | Por qué |
|----------|-----------|---------|
| **Hilo STA** | `SapStaHost.cs` (completo) | Requisito COM de SAP2000. Copiar tal cual. |
| **Conexión COM** | `SapProcessor.cs` (completo) | Lógica de conexión robusta con fallbacks. Copiar tal cual. |
| **Facade** | `SapModelFacade.cs` (adaptar métodos) | Agregar/quitar métodos según tu proyecto. |
| **Engines** | Crear nuevos | Cada engine es específico de tu dominio. |
| **Orquestador** | Adaptar `SapOrchestrator.cs` | Coordina la secuencia de engines. |
| **Diagnóstico** | `SapComDiagnostics.cs` (opcional) | Útil para depuración. |

---

## Códigos de Retorno SAP2000

Todos los métodos del API COM de SAP2000 retornan `int`:

| Código | Significado |
|--------|-------------|
| `0` | Éxito |
| `!= 0` | Error (el número específico varía por método) |

El Facade verifica esto automáticamente con `Check(ret, where)`.

---

## Referencias Internas

- `docs/Referencia de otro proyecto_Connection_to_sap_2000_via_com_api.md` — Análisis detallado del proyecto de referencia original (TowerLoadsProcessorToSAP)
- `docs/IMPLEMENTACION_SAP2000.md` — Estado de implementación V0.4
- `docs/09-sap2000-build-snapshot.md` — Especificación del snapshot
- `docs/COMPLETE_PROJECT_GUIDE.md` — Guía completa del proyecto
