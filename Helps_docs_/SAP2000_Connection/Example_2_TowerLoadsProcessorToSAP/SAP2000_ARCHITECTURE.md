# Arquitectura de Conexión e Interacción con SAP2000

## Índice

1. [Resumen Ejecutivo](#1-resumen-ejecutivo)
2. [Relación COM API con SAP2000](#2-relación-com-api-con-sap2000)
3. [Capa 1 — Hilo STA dedicado (`connection.cs`)](#3-capa-1--hilo-sta-dedicado-connectioncs)
4. [Capa 2 — Núcleo de Conexión (`SapProcessor.cs`)](#4-capa-2--núcleo-de-conexión-sapprocessorcs)
5. [Capa 3 — Herramientas de Modelo (`SAP/`)](#5-capa-3--herramientas-de-modelo-sap)
6. [Capa 4 — Orquestador (`SapModelBuilder`)](#6-capa-4--orquestador-sapmodelbuilder)
7. [Capa 5 — Servicio de UI (`RunSapExportService`)](#7-capa-5--servicio-de-ui-runsapexportservice)
8. [Flujo Completo: De `Program.Main` a SAP2000](#8-flujo-completo-de-programmain-a-sap2000)
9. [Diagrama de Capas](#9-diagrama-de-capas)
10. [Guía: Cómo reutilizar esta arquitectura en otro proyecto](#10-guía-cómo-reutilizar-esta-arquitectura-en-otro-proyecto)

---

## 1. Resumen Ejecutivo

Este proyecto centraliza **toda** la vinculación con SAP2000 en una arquitectura de **5 capas** bien definidas:

| Capa | Archivo(s) | Responsabilidad |
|------|-----------|-----------------|
| **1 — STA Thread** | `connection.cs` | Hilo COM dedicado con message loop nativo |
| **2 — Conexión** | `SapProcessor.cs` | Crear/adjuntar la instancia de SAP2000 y exponer `cSapModel` |
| **3 — Herramientas** | `SAP/SapGeometryBuilder.cs`, `SAP/SapMaterialSettings.cs`, `SAP/SapWindLoadSettings.cs` | Operaciones atómicas sobre el modelo (puntos, frames, materiales, cargas, restricciones) |
| **4 — Orquestador** | `SAP/SapModelBuilder.cs` | Pipeline secuencial de 8 pasos que invoca las herramientas en orden |
| **5 — Servicio UI** | `SAP/RunSapExportService.cs` | Conecta la UI (botón "Exportar a SAP2000") con el orquestador |

El principio fundamental es: **toda operación COM se ejecuta dentro de `_sapRunner.Invoke(() => { ... })`**, lo cual garantiza que siempre se ejecuta en el hilo STA correcto.

---

## 2. Relación COM API con SAP2000

### ¿Qué es la COM API de SAP2000?

SAP2000 expone su funcionalidad a través de una **interfaz COM (Component Object Model)** registrada en Windows. Esto significa que cualquier lenguaje que soporte COM (C#, VB, Python con comtypes, etc.) puede controlar SAP2000 programáticamente.

### Objetos COM principales

```
cHelper          ← Utilidad de la API para crear/buscar instancias
    │
    ▼
cOAPI (sapObject) ← La aplicación SAP2000 en sí
    │
    ▼
cSapModel        ← El modelo abierto dentro de SAP2000
    │
    ├── .File           (abrir, guardar, nuevo)
    ├── .PointObj       (crear puntos, asignar cargas, restricciones)
    ├── .FrameObj       (crear barras)
    ├── .PropMaterial   (crear/modificar materiales)
    ├── .PropFrame      (crear/modificar secciones)
    ├── .LoadPatterns   (crear patrones de carga)
    ├── .LoadCases      (crear/configurar casos de carga)
    ├── .Analyze        (crear modelo de análisis, ejecutar)
    ├── .Results        (leer reacciones, desplazamientos, etc.)
    ├── .View           (refrescar vista)
    └── .RespCombo      (combinaciones de carga)
```

### ProgIDs usadas para conectar

El proyecto intenta conectar usando tres ProgIDs en orden de prioridad:

```csharp
string[] progIds = new[] {
    "CSI.SAP2000.API.SapObject",   // ProgID oficial más reciente
    "Sap2000v1.SapObject",          // ProgID del interop assembly
    "CSI.SAP2000.SapObject"          // ProgID alternativa
};
```

### Estrategia de conexión (attach vs. create)

```
1. Marshal.GetActiveObject(progId)    → Adjuntarse a instancia existente (ROT)
2. helper.GetObject(progId)           → Segundo intento vía cHelper
3. helper.CreateObjectProgID(progId)  → Crear instancia nueva + ApplicationStart()
```

### Convención de retorno de la API

Todas las funciones de la API de SAP2000 retornan un `int`:
- **`0`** = éxito
- **`!= 0`** = error (código específico)

El proyecto centraliza la verificación con:

```csharp
private static void CheckRet(int ret, string where)
{
    if (ret != 0)
        throw new Exception($"{where} retornó código {ret}");
}
```

---

## 3. Capa 1 — Hilo STA dedicado (`connection.cs`)

### Problema que resuelve

SAP2000 es un servidor COM que requiere un hilo **STA (Single-Threaded Apartment)**. Si se crean o invocan objetos COM desde un hilo MTA o desde hilos diferentes, se producen errores de marshalling o deadlocks.

### Solución: `SapStaHost.StaComRunner`

```
┌──────────────────────────────────────────────────────────┐
│  Hilo STA dedicado (background thread)                    │
│                                                          │
│  1. CoInitializeEx(COINIT_APARTMENTTHREADED)             │
│  2. PeekMessage() → Crea la message queue                │
│  3. Message loop: GetMessage() → TranslateMessage() →     │
│     DispatchMessage()                                     │
│                                                          │
│  Mensajes especiales:                                     │
│    WM_INVOKE → Ejecuta el Action encapsulado             │
│    WM_QUIT   → Termina el loop                           │
└──────────────────────────────────────────────────────────┘
```

### Cómo funciona `Invoke()`

```csharp
// Desde cualquier hilo (UI, worker, etc.):
sapRunner.Invoke(() => {
    // Este código se ejecuta en el hilo STA
    sapModel.PointObj.AddCartesian(x, y, z, ref name, ...);
});
```

Internamente:
1. Crea un `InvocationInfo` con el `Action` y un `ManualResetEventSlim`
2. Envía un mensaje `WM_INVOKE` al hilo STA vía `PostThreadMessage`
3. El hilo STA recibe el mensaje, ejecuta el `Action`
4. Si hay excepción, la captura en `info.Exception`
5. Señala el evento (`info.Event.Set()`)
6. El hilo llamador espera (`info.Event.Wait()`) y re-lanza la excepción si la hubo

### Ejemplo mínimo de uso

```csharp
using (var runner = SapStaHost.CreateRunner())
{
    runner.Invoke(() => {
        var proc = new SapProcessor();
        proc.ConnectAndInit();
        proc.UnlockAndRefreshView();
        // ... operaciones con SAP2000 ...
        proc.ReleaseCom();
    });
}
```

### API pública de StaComRunner

| Método | Descripción |
|--------|-------------|
| `Invoke(Action)` | Ejecuta un delegate en el hilo STA (bloqueante) |
| `Invoke<T>(Func<T>)` | Ejecuta y retorna un valor desde el hilo STA |
| `Dispose()` | Envía `WM_QUIT`, espera fin del hilo, libera recursos |

---

## 4. Capa 2 — Núcleo de Conexión (`SapProcessor.cs`)

### Responsabilidad

Encapsula **todo** el ciclo de vida de la conexión COM con SAP2000:

```
ConnectAndInit()  →  CreateHelperAndSapObject() + InitModel()
                         │                           │
                         ▼                           ▼
                    Crear cHelper             InitializeNewModel()
                    Buscar/crear cOAPI        File.NewBlank()
                    Obtener cSapModel
```

### Campos COM internos

```csharp
private cHelper   helper;      // Utilidad del API para crear objetos
private cOAPI     sapObject;   // Aplicación SAP2000 (controla ventana, inicio, cierre)
private cSapModel sapModel;    // Modelo activo (donde se crean puntos, frames, etc.)

public cSapModel SapModel => sapModel;  // Expuesto para uso externo
```

### Métodos públicos

| Método | Qué hace |
|--------|----------|
| `ConnectAndInit()` | Conecta a SAP2000 existente o crea nueva instancia + modelo en blanco |
| `RunBasicAnalysis()` | Guarda → `Analyze.CreateAnalysisModel()` → `Analyze.RunAnalysis()` |
| `UnlockAndRefreshView()` | `Unhide()` + `View.RefreshView()` |
| `ShowSAP2000()` | Muestra la ventana de SAP2000 |
| `HideSAP2000()` | Oculta la ventana durante procesamiento |
| `ReleaseCom()` | `Marshal.ReleaseComObject()` para sapModel, sapObject, helper |
| `Log(string)` | Método estático para logging desde cualquier clase |

### Flujo detallado de `CreateHelperAndSapObject()`

```
1. Crear cHelper = new Helper()
2. Detectar procesos SAP2000 con ventana activa
3. Para cada ProgID conocida:
   a. Marshal.GetActiveObject(progId)  → ¿instancia existente?
   b. helper.GetObject(progId)         → ¿segundo intento?
4. Si no hay instancia existente:
   a. helper.CreateObjectProgID("CSI.SAP2000.API.SapObject")
   b. sapObject.ApplicationStart(eUnits.N_m_C, false, "")
5. sapModel = sapObject.SapModel
6. Log versión de API
```

### Sistema de logging

```csharp
public static event Action<string> LogMessage;  // La UI se suscribe

// Uso interno:
private static void RaiseLog(string message)
{
    Debug.WriteLine(message);
    LogMessage?.Invoke(message);
}

// Uso externo (desde otras clases):
SapProcessor.Log("Mi mensaje");
```

---

## 5. Capa 3 — Herramientas de Modelo (`SAP/`)

Cada herramienta recibe un `cSapModel` y ejecuta operaciones atómicas. Estas **no gestionan la conexión** — asumen que ya existe.

### `SapGeometryBuilder` — Geometría

```csharp
var geoBuilder = new SapGeometryBuilder(sapModel, logCallback);
```

| Método | Función SAP2000 que llama |
|--------|--------------------------|
| `CreatePoints(tower)` | `sapModel.PointObj.AddCartesian(x, y, z, ...)` |
| `CreateFrames(tower, puntosSAP)` | `sapModel.FrameObj.AddByCoord(xi,yi,zi, xj,yj,zj, ...)` |
| `CreateDefaultMaterial()` | `sapModel.PropMaterial.SetMaterial("Steel", eMatType.Steel, ...)` |
| `CreateDefaultAngleSection()` | `sapModel.PropFrame.SetAngle_1(name, mat, T3, T2, Tf, Tw, ...)` + `sapModel.PropFrame.SetModifiers(...)` |
| `AssignBaseRestraints(puntosSAP)` | `sapModel.PointObj.SetRestraint(name, restraints, ...)` |

### `SapMaterialSettings` — Configuración global (singleton)

```csharp
SapMaterialSettings.Current.MaterialName       // "Steel"
SapMaterialSettings.Current.FrameSectionName   // "L150x150x6.5"
SapMaterialSettings.Current.T3                 // 0.15 m
SapMaterialSettings.Current.T2                 // 0.15 m
SapMaterialSettings.Current.Tf                 // 0.008 m
SapMaterialSettings.Current.Tw                 // 0.008 m
SapMaterialSettings.Current.Modifiers          // [1,1,1,1,1,1,1,1]
```

### `SapWindLoadSettings` — Configuración de viento (singleton)

```csharp
SapWindLoadSettings.Current.DirAngles     // [0°, 90°]
SapWindLoadSettings.Current.WindSpeed     // m/s
SapWindLoadSettings.Current.ExposureType  // 2 = C
SapWindLoadSettings.Current.GustFactor    // 0.85
```

---

## 6. Capa 4 — Orquestador (`SapModelBuilder`)

### Pipeline de 8 pasos (dentro de `_sapRunner.Invoke()`)

```
_sapRunner.Invoke(() => {
    var sapModel = _sapProcessor.SapModel;

    // [1/8] Inicializar modelo
    sapModel.InitializeNewModel(eUnits.N_m_C);
    sapModel.File.NewBlank();

    // [2/8] Crear material y sección
    geoBuilder.CreateDefaultMaterial();
    geoBuilder.CreateDefaultAngleSection();

    // [3/8] Crear puntos
    puntosSAP = geoBuilder.CreatePoints(torre);

    // [3.5/8] Asignar restricciones de base
    geoBuilder.AssignBaseRestraints(puntosSAP);

    // [4/8] Crear frames
    framesCreados = geoBuilder.CreateFrames(torre, puntosSAP);

    // [5/8] Obtener datos transformados desde caché
    dtOutput = TransformOutputCache.GetOrAdd(...);

    // [6/8] Crear LoadPatterns y asignar cargas
    sapModel.LoadPatterns.Add(lp, eLoadPatternType.Other, ...);
    sapModel.PointObj.SetLoadForce(puntoSAP, loadPattern, ref loadValues, ...);
    // + Patrones de viento automáticos (ASCE 7-16)

    // [7/8] Guardar modelo
    sapModel.File.Save(modelPath);

    // [8/8] Ejecutar análisis y capturar reacciones
    sapModel.Analyze.CreateAnalysisModel();
    sapModel.Analyze.RunAnalysis();
    sapModel.Results.JointReact(...);
    sapModel.Results.BaseReactWithCentroid(...);
});
```

### Estructura del orquestador

```
SapModelBuilder
├── CreateModelsForWorkspace()          → Loop: Archivo → Torre → Bloque
│   └── CreateSingleModel()            → Pipeline de 8 pasos (dentro de Invoke)
├── CaptureBaseReactions()              → Lee reacciones por nodo base
├── CaptureBaseReactWithCentroid()      → Lee reacciones globales con centroide
├── CreateAutoWindLoadPatterns()        → Crea patrones WIND_0, WIND_90, etc.
├── BuildTipoColumnMap()                → Mapea columnas del DataTable
└── ReactionsByTower                    → Resultados acumulados por torre
```

---

## 7. Capa 5 — Servicio de UI (`RunSapExportService`)

Conecta el botón de la UI con todo el flujo:

```csharp
var service = new RunSapExportService(sapRunner, sapProcessor, transformGrid, switchToLogTab);
service.Run(uiOwner, resultado, generatedTowers);
```

### Flujo interno:

```
1. Validar datos (torres, conexión SAP, grid)
2. Solicitar carpeta de salida (FolderBrowserDialog)
3. Ocultar SAP2000 durante procesamiento
4. Crear SapModelBuilder(runner, processor, grid, log)
5. modelBuilder.CreateModelsForWorkspace(resultado, torres, outputDir)
6. Refrescar tab de resultados en MainForm
7. Mostrar SAP2000 al finalizar
```

---

## 8. Flujo Completo: De `Program.Main` a SAP2000

```
Program.Main()
│
├─ 1. SapStaHost.CreateRunner()           → Crea hilo STA + message loop
│
├─ 2. runner.Invoke(() => {
│      new SapProcessor().ConnectAndInit() → Conecta a SAP2000
│  })
│
├─ 3. new MainForm(resultado, runner, processor) → Inyecta dependencias
│      │
│      ├─ 4. Usuario hace click en "Exportar a SAP2000"
│      │
│      └─ 5. RunSapExportService.Run()
│             │
│             └─ 6. SapModelBuilder.CreateModelsForWorkspace()
│                    │
│                    └─ 7. _sapRunner.Invoke(() => {
│                           var sapModel = _sapProcessor.SapModel;
│                           │
│                           ├─ SapGeometryBuilder.CreatePoints()
│                           │   → sapModel.PointObj.AddCartesian()
│                           │
│                           ├─ SapGeometryBuilder.CreateFrames()
│                           │   → sapModel.FrameObj.AddByCoord()
│                           │
│                           ├─ sapModel.LoadPatterns.Add()
│                           ├─ sapModel.PointObj.SetLoadForce()
│                           ├─ sapModel.File.Save()
│                           ├─ sapModel.Analyze.RunAnalysis()
│                           └─ sapModel.Results.JointReact()
│                       })
│
└─ 8. finally: runner.Invoke(() => processor.ReleaseCom())
       runner.Dispose()
```

---

## 9. Diagrama de Capas

```
┌─────────────────────────────────────────────────────────────────────────┐
│                         APLICACIÓN (UI / Console)                       │
│  Program.Main() → MainForm → Botones/Menús                             │
└────────────────────────────────┬────────────────────────────────────────┘
                                 │
                                 ▼
┌─────────────────────────────────────────────────────────────────────────┐
│                    CAPA 5: SERVICIO UI                                   │
│  RunSapExportService                                                     │
│  - Valida datos, muestra diálogos, deshabilita UI                       │
│  - Delega al orquestador                                                │
└────────────────────────────────┬────────────────────────────────────────┘
                                 │
                                 ▼
┌─────────────────────────────────────────────────────────────────────────┐
│                    CAPA 4: ORQUESTADOR                                   │
│  SapModelBuilder.CreateModelsForWorkspace()                              │
│  - Loop: Archivo → Torre → Bloque                                       │
│  - Pipeline de 8 pasos dentro de _sapRunner.Invoke()                    │
│  - Acumula reacciones por torre                                          │
└────────────────────────────────┬────────────────────────────────────────┘
                                 │
                                 ▼
┌─────────────────────────────────────────────────────────────────────────┐
│                    CAPA 3: HERRAMIENTAS                                  │
│  SapGeometryBuilder  │ SapMaterialSettings  │ SapWindLoadSettings       │
│  - CreatePoints()    │ - MaterialName       │ - DirAngles               │
│  - CreateFrames()    │ - FrameSectionName   │ - WindSpeed               │
│  - CreateMaterial()  │ - T3, T2, Tf, Tw     │ - ExposureType            │
│  - AssignRestraints()│ - Modifiers          │ - GustFactor              │
│                      │                      │                            │
│  Cada herramienta recibe cSapModel y ejecuta operaciones atómicas       │
└────────────────────────────────┬────────────────────────────────────────┘
                                 │
                                 ▼
┌─────────────────────────────────────────────────────────────────────────┐
│                    CAPA 2: NÚCLEO DE CONEXIÓN                           │
│  SapProcessor                                                            │
│  - ConnectAndInit() → CreateHelperAndSapObject() + InitModel()          │
│  - Expone: SapModel (cSapModel)                                        │
│  - RunBasicAnalysis(), Show/Hide, ReleaseCom()                          │
│  - Logging centralizado vía evento estático LogMessage                  │
└────────────────────────────────┬────────────────────────────────────────┘
                                 │
                                 ▼
┌─────────────────────────────────────────────────────────────────────────┐
│                    CAPA 1: HILO STA                                      │
│  SapStaHost.StaComRunner                                                 │
│  - Hilo background con ApartmentState.STA                               │
│  - CoInitializeEx(COINIT_APARTMENTTHREADED)                             │
│  - Message loop nativo (GetMessage/DispatchMessage)                     │
│  - Invoke(Action) serializa llamadas COM al hilo STA                    │
└────────────────────────────────┬────────────────────────────────────────┘
                                 │
                                 ▼
┌─────────────────────────────────────────────────────────────────────────┐
│                    SAP2000 (COM Server)                                   │
│  cHelper → cOAPI (sapObject) → cSapModel                                │
│  ProgIDs: "CSI.SAP2000.API.SapObject"                                   │
│  Interop: SAP2000v1.dll                                                  │
└─────────────────────────────────────────────────────────────────────────┘
```

---

## 10. Guía: Cómo reutilizar esta arquitectura en otro proyecto

### Archivos mínimos necesarios (copiar y pegar)

Para reutilizar esta arquitectura en otro proyecto, necesitas copiar **solo 2 archivos**:

| Archivo | Qué contiene |
|---------|-------------|
| `connection.cs` | `SapStaHost` + `StaComRunner` (infraestructura STA genérica, ~185 líneas) |
| `SapProcessor.cs` | Conexión COM centralizada (~236 líneas) |

### Dependencias requeridas

```
- NuGet / Referencia COM: SAP2000v1 (interop assembly de CSI)
- .NET Framework 4.7.2+ (o .NET con soporte COM)
- System.Runtime.InteropServices
```

### Plantilla mínima para un nuevo proyecto

```csharp
// ═══════════════════════════════════════════════════════════
// PASO 1: Inicialización (en Main o al inicio de la app)
// ═══════════════════════════════════════════════════════════
SapStaHost.StaComRunner sapRunner = null;
SapProcessor sapProcessor = null;

try
{
    // Crear el hilo STA dedicado para COM
    sapRunner = SapStaHost.CreateRunner();

    // Conectar a SAP2000 (dentro del hilo STA)
    sapRunner.Invoke(() =>
    {
        sapProcessor = new SapProcessor();
        sapProcessor.ConnectAndInit();
    });

    // ═══════════════════════════════════════════════════════
    // PASO 2: Usar el modelo (siempre dentro de Invoke)
    // ═══════════════════════════════════════════════════════
    sapRunner.Invoke(() =>
    {
        var sapModel = sapProcessor.SapModel;

        // Ejemplo: crear un punto
        string name = "";
        int ret = sapModel.PointObj.AddCartesian(0, 0, 0, ref name);

        // Ejemplo: crear un frame
        string frameName = "";
        ret = sapModel.FrameObj.AddByCoord(0,0,0, 1,0,3, ref frameName);

        // Ejemplo: crear material
        ret = sapModel.PropMaterial.SetMaterial("Steel", eMatType.Steel, -1, "", "");

        // Ejemplo: crear sección
        ret = sapModel.PropFrame.SetAngle_1("L100x100x6.5", "Steel",
            0.1, 0.1, 0.0065, 0.0065, 0.0, -1, "", "");

        // Ejemplo: crear load pattern
        ret = sapModel.LoadPatterns.Add("DEAD", eLoadPatternType.Dead, 0, true);

        // Ejemplo: asignar carga
        double[] forces = { 0, 0, -10000, 0, 0, 0 };
        ret = sapModel.PointObj.SetLoadForce(name, "DEAD", ref forces, false, "Global");

        // Ejemplo: guardar
        ret = sapModel.File.Save(@"C:\MiModelo.sdb");

        // Ejemplo: analizar
        ret = sapModel.Analyze.CreateAnalysisModel();
        ret = sapModel.Analyze.RunAnalysis();

        // Ejemplo: leer resultados
        int nResults = 0;
        string[] obj = null, elm = null, loadCase = null, stepType = null;
        double[] stepNum = null, f1 = null, f2 = null, f3 = null;
        double[] m1 = null, m2 = null, m3 = null;
        ret = sapModel.Results.JointReact(name, eItemTypeElm.ObjectElm,
            ref nResults, ref obj, ref elm, ref loadCase, ref stepType,
            ref stepNum, ref f1, ref f2, ref f3, ref m1, ref m2, ref m3);
    });
}
finally
{
    // ═══════════════════════════════════════════════════════
    // PASO 3: Limpieza (siempre en finally)
    // ═══════════════════════════════════════════════════════
    if (sapRunner != null && sapProcessor != null)
    {
        try { sapRunner.Invoke(() => sapProcessor.ReleaseCom()); }
        catch { }
    }
    sapRunner?.Dispose();
}
```

### Patrón para crear herramientas reutilizables

Si quieres crear herramientas modulares (como `SapGeometryBuilder`), sigue este patrón:

```csharp
public class MiHerramientaSap
{
    private readonly cSapModel _sapModel;
    private readonly Action<string> _log;

    public MiHerramientaSap(cSapModel sapModel, Action<string> log = null)
    {
        _sapModel = sapModel ?? throw new ArgumentNullException(nameof(sapModel));
        _log = log ?? (msg => Debug.WriteLine(msg));
    }

    public void MiOperacion()
    {
        // Llamar funciones del API directamente
        string name = "";
        int ret = _sapModel.PointObj.AddCartesian(0, 0, 0, ref name);
        if (ret != 0)
            throw new Exception($"Error código {ret}");
        _log($"Punto creado: {name}");
    }
}

// Uso:
sapRunner.Invoke(() =>
{
    var herramienta = new MiHerramientaSap(sapProcessor.SapModel, Console.WriteLine);
    herramienta.MiOperacion();
});
```

### Patrón orquestador (pipeline secuencial)

```csharp
public class MiOrquestador
{
    private readonly SapStaHost.StaComRunner _runner;
    private readonly SapProcessor _processor;

    public MiOrquestador(SapStaHost.StaComRunner runner, SapProcessor processor)
    {
        _runner = runner;
        _processor = processor;
    }

    public void EjecutarPipeline()
    {
        _runner.Invoke(() =>
        {
            var model = _processor.SapModel;

            // Paso 1: Inicializar
            model.InitializeNewModel(eUnits.N_m_C);
            model.File.NewBlank();

            // Paso 2: Material y sección
            var geo = new MiHerramientaSap(model);
            geo.CrearMaterial();
            geo.CrearSeccion();

            // Paso 3: Geometría
            geo.CrearPuntos();
            geo.CrearFrames();

            // Paso 4: Cargas
            // ...

            // Paso 5: Guardar y analizar
            model.File.Save(@"C:\modelo.sdb");
            model.Analyze.RunAnalysis();

            // Paso 6: Resultados
            // ...
        });
    }
}
```

### Referencia rápida de funciones SAP2000 más usadas en este proyecto

| Categoría | Función API | Qué hace |
|-----------|------------|----------|
| **Conexión** | `new Helper()` | Crea el helper COM |
| | `Marshal.GetActiveObject(progId)` | Busca instancia existente en ROT |
| | `helper.CreateObjectProgID(progId)` | Crea nueva instancia |
| | `sapObject.ApplicationStart(units)` | Inicia la aplicación |
| | `sapObject.SapModel` | Obtiene el modelo activo |
| | `sapObject.GetOAPIVersionNumber()` | Versión de la API |
| **Modelo** | `sapModel.InitializeNewModel(units)` | Inicializa modelo nuevo |
| | `sapModel.File.NewBlank()` | Crea archivo en blanco |
| | `sapModel.File.Save(path)` | Guarda el modelo |
| | `sapModel.SetModelIsLocked(false)` | Desbloquea para edición |
| **Geometría** | `sapModel.PointObj.AddCartesian(x,y,z)` | Crea punto por coordenadas |
| | `sapModel.FrameObj.AddByCoord(...)` | Crea frame por coordenadas |
| **Materiales** | `sapModel.PropMaterial.SetMaterial(name, type)` | Crea material |
| | `sapModel.PropFrame.SetAngle_1(...)` | Crea sección de ángulo |
| | `sapModel.PropFrame.SetModifiers(name, mods)` | Asigna modificadores |
| **Restricciones** | `sapModel.PointObj.SetRestraint(name, values)` | Asigna apoyos |
| **Cargas** | `sapModel.LoadPatterns.Add(name, type)` | Crea patrón de carga |
| | `sapModel.PointObj.SetLoadForce(...)` | Asigna fuerza puntual |
| **Análisis** | `sapModel.Analyze.CreateAnalysisModel()` | Crea modelo de análisis |
| | `sapModel.Analyze.RunAnalysis()` | Ejecuta el análisis |
| | `sapModel.Analyze.SetRunCaseFlag(case, run)` | Activa/desactiva caso |
| **Resultados** | `sapModel.Results.Setup.SetCaseSelectedForOutput(case)` | Selecciona caso para output |
| | `sapModel.Results.JointReact(...)` | Lee reacciones en nodo |
| | `sapModel.Results.BaseReactWithCentroid(...)` | Reacciones globales con centroide |
| **Vista** | `sapObject.Unhide()` / `sapObject.Hide()` | Mostrar/ocultar ventana |
| | `sapModel.View.RefreshView()` | Refresca la vista |
| **Limpieza** | `Marshal.ReleaseComObject(obj)` | Libera referencia COM |

### Checklist para crear tu app de conexión SAP2000

- [ ] Copiar `connection.cs` (infraestructura STA)
- [ ] Copiar `SapProcessor.cs` (núcleo de conexión)
- [ ] Agregar referencia a `SAP2000v1` (interop assembly de CSI)
- [ ] En tu `Main()` o punto de entrada:
  - [ ] Crear `SapStaHost.CreateRunner()`
  - [ ] Dentro de `Invoke()`: crear `SapProcessor` + `ConnectAndInit()`
- [ ] Crear tus herramientas que reciban `cSapModel`
- [ ] Crear tu orquestador que use `_runner.Invoke()` para el pipeline
- [ ] En `finally`: `ReleaseCom()` + `Dispose()` del runner
