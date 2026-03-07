# Integración con SAP2000

## Introducción

La integración con SAP2000 es el núcleo operativo de Structural Management V1. Este documento describe en detalle cómo funciona la conexión, qué datos se extraen, cómo se mapean al dominio del sistema, y cómo extender o depurar la integración.

---

## Arquitectura de la Integración

La integración se basa en el principio de **aislamiento total**: ningún tipo del namespace `SAP2000v1` escapa del proyecto `App.SAP2000`. El resto del sistema no sabe que SAP2000 existe.

```
┌──────────────────────────────────────────────────────────────┐
│                    Sistema Interno                            │
│                                                              │
│  App.Application/Interfaces/ISapAdapter.cs                   │
│  ↑ Contrato abstracto (sin referencias a SAP2000)            │
│                                                              │
└──────────────────────────┬───────────────────────────────────┘
                           │ implementa
┌──────────────────────────┴───────────────────────────────────┐
│                      App.SAP2000                             │
│                  (Zona de aislamiento)                        │
│                                                              │
│  SapAdapter.cs              ← Fachada principal              │
│  SapConnectionService.cs    ← Conexión y operaciones         │
│  SapStructureOutputReader.cs ← Lectura de resultados         │
│  SapDesignDataReader.cs     ← Lectura de diseño              │
│                                                              │
└──────────────────────────┬───────────────────────────────────┘
                           │ usa directamente
┌──────────────────────────┴───────────────────────────────────┐
│                     SAP2000v1.dll                            │
│                    (COM API externa)                          │
│                                                              │
│  cSapModel, AnalysisResults, FrameObj, AreaObj, ...         │
│                                                              │
└──────────────────────────────────────────────────────────────┘
```

---

## Ciclo de Vida de la Conexión

### Estado: Desconectado (inicial)

```
SapAdapter._isConnected = false
ISapAdapter.IsConnected = false
UI: StatusBar muestra "SAP2000: No conectado"
```

### Operación: Connect()

```csharp
// Flujo interno de SapConnectionService.Connect():

1. Crear instancia COM de SAP2000
   cSapObject = new SAP2000v1.SapObject();

2. Inicializar la aplicación
   cSapObject.ApplicationStart();

3. Obtener referencia al modelo
   cSapModel = cSapObject.SapModel;

4. Inicializar el modelo
   cSapModel.InitializeNewModel(SAP2000v1.eUnits.kN_m_C);

5. Marcar como conectado
   _isConnected = true;
```

### Operación: OpenModel(filePath)

```csharp
// Flujo interno:

1. Verificar que esté conectado
   if (!_isConnected) throw new InvalidOperationException(...)

2. Abrir el archivo
   int ret = cSapModel.File.OpenFile(filePath);
   if (ret != 0) throw new SapException($"Error al abrir: {filePath}");

3. Ejecutar el modelo en modo activo
   cSapModel.SetModelIsLocked(false);
```

### Operación: RunAnalysis()

```csharp
// Flujo interno:

1. Desbloquear el modelo
   cSapModel.SetModelIsLocked(false);

2. Ejecutar todos los casos de carga
   cSapModel.Analyze.RunAnalysis();
   // Espera hasta que SAP2000 completa el análisis

3. El modelo queda bloqueado automáticamente tras el análisis
```

### Operación: Disconnect()

```csharp
// Flujo interno:

1. Liberar la referencia al modelo
   Marshal.ReleaseComObject(cSapModel);

2. Cerrar SAP2000 (opcional, configurable)
   cSapObject.ApplicationExit(false); // false = no guardar

3. Liberar objeto COM
   Marshal.ReleaseComObject(cSapObject);

4. Marcar como desconectado
   _isConnected = false;
```

---

## Extracción de Resultados Sísmicos

### GetModalResults()

Extrae los resultados del análisis modal (periodos, masas participativas):

```csharp
// Implementación en SapStructureOutputReader

int numberOfModes = 0;
string[] modeNames = null;
double[] periods = null;
double[] freqHz = null;
double[] massX = null;
double[] massY = null;
double[] massZ = null;
double[] cumMassX = null;
double[] cumMassY = null;

// Llamada a la API SAP2000
int ret = cSapModel.Results.ModalParticipatingMassRatios(
    ref numberOfModes, ref modeNames,
    ref periods, ref freqHz,
    ref massX, ref massY, ref massZ,
    ref cumMassX, ref cumMassY, ref _ /*cumMassZ*/);

// Mapeo a entidades del dominio
var results = new List<ModalResult>();
for (int i = 0; i < numberOfModes; i++)
{
    results.Add(new ModalResult(
        modeNumber:         i + 1,
        period:             periods[i],
        frequencyHz:        freqHz[i],
        massParticipationX: massX[i],
        massParticipationY: massY[i],
        massParticipationZ: massZ[i],
        cumulativeMassX:    cumMassX[i],
        cumulativeMassY:    cumMassY[i]
    ));
}
```

### GetStoryShears(combo)

Extrae cortantes de historia para un caso de carga:

```csharp
// Los cortantes de historia se obtienen como resultados de sección de piso

int numberOfItems = 0;
string[] storyNames = null;
string[] loadCases = null;
double[] Vx = null;
double[] Vy = null;

int ret = cSapModel.Results.StoryForces(combo,
    eItemTypeElm.ObjectElm,
    ref numberOfItems, ref storyNames, ref loadCases,
    ref _ /*FX*/, ref _ /*FY*/, ref _ /*FZ*/,
    ref _ /*MX*/, ref _ /*MY*/, ref _ /*MZ*/,
    ref Vx, ref Vy);

// Mapeo a entidades del dominio
var results = new List<StoryResult>();
for (int i = 0; i < numberOfItems; i++)
{
    results.Add(new StoryResult(
        storyName: storyNames[i],
        loadCase:  loadCases[i],
        shearX:    Vx[i],
        shearY:    Vy[i]
    ));
}
```

### GetStoryDrifts(combo)

```csharp
// Derivas calculadas desde los desplazamientos de piso

int numberOfItems = 0;
string[] storyNames = null;
string[] labels = null;
string[] loadCases = null;
double[] driftX = null;
double[] driftY = null;

int ret = cSapModel.Results.StoryDrifts(combo,
    ref numberOfItems, ref storyNames, ref labels, ref loadCases,
    ref driftX, ref driftY, ref _ /*driftZ*/,
    ref _ /*dispX*/, ref _ /*dispY*/);
```

---

## Extracción de Resultados de Diseño

### GetBeamDesignData()

Los resultados de diseño de vigas se obtienen de la verificación de diseño de SAP2000:

```csharp
// Para cada elemento de marco tipo viga:

int numberOfItems = 0;
string[] frameNames = null;
double[] stations = null;
string[] loadCombos = null;
double[] asTopPos = null;   // As superior positivo (cm²)
double[] asTopNeg = null;   // As superior negativo (cm²)
double[] asBot = null;      // As inferior (cm²)
double[] Vu = null;         // Cortante último (ton)

int ret = cSapModel.DesignConcrete.GetSummaryResultsBeam(
    frameNames[i], ref numberOfItems, ref stations,
    ref loadCombos, ref asTopPos, ref asTopNeg,
    ref asBot, ref Vu, ...);
```

### GetFrameForces(combo)

```csharp
// Fuerzas internas por elemento de marco

int numberOfItems = 0;
string[] objNames = null;
string[] loadCases = null;
double[] P = null;    // Fuerza axial (ton)
double[] V2 = null;   // Cortante local 2 (ton)
double[] V3 = null;   // Cortante local 3 (ton)
double[] T = null;    // Torsión (ton·m)
double[] M2 = null;   // Momento local 2 (ton·m)
double[] M3 = null;   // Momento local 3 (ton·m)

int ret = cSapModel.Results.FrameForce(
    combo, eItemTypeElm.ObjectElm,
    ref numberOfItems, ref objNames, ref _ /*elmNames*/,
    ref _ /*pointNames*/, ref loadCases,
    ref P, ref V2, ref V3, ref T, ref M2, ref M3);
```

---

## Definición de Configuraciones en SAP2000

### DefineLoadPattern(pattern)

```csharp
// Crea o modifica un patrón de carga en SAP2000

cSapModel.LoadPatterns.Add(
    pattern.Name,
    (eLoadPatternType)pattern.PatternType,
    pattern.SelfWeightFactor,
    addLoadCase: true);
```

### DefineResponseSpectrum(spectrum)

```csharp
// Define una función espectral en SAP2000

// 1. Crear la función espectral
cSapModel.Func.FuncRS.SetUser(
    spectrum.Name,
    spectrum.PeriodValues.ToArray(),
    spectrum.AccelerationValues.ToArray(),
    dampingRatio: 0.05);

// 2. Crear el caso de carga espectral
cSapModel.LoadCases.ResponseSpectrum.SetCase(spectrum.LoadCaseName);
cSapModel.LoadCases.ResponseSpectrum.SetLoads(
    spectrum.LoadCaseName,
    numberOfLoads: 2,
    loadTypes: new[] { "Accel", "Accel" },
    loadNames: new[] { "U1", "U2" },
    funcs: new[] { spectrum.Name, spectrum.Name },
    scaleFactor: new[] { spectrum.ScaleFactorX, spectrum.ScaleFactorY });
```

### AssignDiaphragm(diaphragm)

```csharp
// Asignar restricción de diafragma rígido a todos los puntos de un piso

// 1. Obtener todos los joints del piso
int nPoints = 0;
string[] pointNames = null;
cSapModel.PointObj.GetNameListOnStory(diaphragm.StoryName,
    ref nPoints, ref pointNames);

// 2. Asignar diafragma a cada punto
foreach (var pt in pointNames)
{
    cSapModel.PointObj.SetDiaphragm(pt, diaphragm.DiaphragmName);
}
```

---

## Manejo de Errores

Todos los métodos de la API SAP2000 retornan un código entero:
- `0` = Éxito
- `!=0` = Error

El adaptador verifica estos códigos y lanza excepciones de dominio:

```csharp
private void CheckResult(int ret, string operationName)
{
    if (ret != 0)
    {
        throw new SapOperationException(
            $"SAP2000 retornó error {ret} en operación: {operationName}");
    }
}
```

### Tabla de Códigos de Error Comunes

| Código | Significado | Solución |
|---|---|---|
| 0 | Éxito | - |
| 1 | Error genérico | Verificar parámetros |
| -1 | Objeto no encontrado | Verificar nombre del elemento |
| 2 | Modelo bloqueado | Desbloquear antes de modificar |
| 4 | No hay resultados | Ejecutar el análisis primero |

---

## Traza de Comandos (ExecutedCommandSet)

Cada operación de hidratación registra los comandos ejecutados:

```csharp
public class SapCommandExecutionTrace
{
    public string CommandName { get; }       // Ej: "Results.ModalParticipatingMassRatios"
    public DateTime ExecutedAt { get; }
    public int ReturnCode { get; }           // 0 = éxito
    public double DurationMs { get; }        // tiempo de ejecución
    public string? ErrorMessage { get; }     // si ret != 0
}
```

Esta información se almacena en `ExecutedCommandSet` dentro de cada `SeismicSource` o `DesignSource`, permitiendo auditar exactamente qué se ejecutó y cuándo.

---

## Configuración Avanzada

### Cambiar el sistema de unidades

Por defecto, el adaptador trabaja con **kN, m, °C**. Para cambiar:

```csharp
// En SapConnectionService.Connect():
cSapModel.InitializeNewModel(SAP2000v1.eUnits.tonf_m_C);  // ton-fuerza, metros
```

### Seleccionar casos de carga para hidratación

Los nombres de los casos de carga para extracción de cortantes y derivas son configurables. Por defecto se usan patrones comunes:

```csharp
// Casos sísmicos típicos esperados:
var seismicCasesX = new[] { "SISMO X", "SPECX", "ESPECTRO X", "SX" };
var seismicCasesY = new[] { "SISMO Y", "SPECY", "ESPECTRO Y", "SY" };
```

El adaptador intenta cada nombre en orden hasta encontrar uno que exista en el modelo.

---

## Prerrequisitos del Modelo SAP2000

Para que la hidratación funcione correctamente, el modelo SAP2000 debe cumplir con:

### Para análisis sísmico:
- ✅ Análisis modal configurado (mínimo 12 modos o suficientes para ≥90% masa)
- ✅ Casos de carga espectral definidos (nombre reconocible: SISMO X, ESPECTRO X, etc.)
- ✅ Análisis ejecutado exitosamente (sin errores)
- ✅ Diafragmas asignados por piso (para cortantes de historia)

### Para diseño estructural:
- ✅ Diseño de marcos de concreto ejecutado
- ✅ Combinaciones de carga con sismo definidas
- ✅ Parámetros de diseño configurados (f'c, fy, φ)

### Nombres de pisos:
- Los pisos deben tener nombres definidos en SAP2000 (Story 1, Piso 1, etc.)
- Se extraen automáticamente con `GetStoryNames()`

---

## Depuración de la Integración

### Verificar la conexión COM

```csharp
// En la consola inmediata de Visual Studio durante depuración:
_sapAdapter.IsConnected  // debe ser true
_sapAdapter.GetSapVersion()  // debe retornar "SAP2000 v25.x.x"
```

### Registrar todas las operaciones

Activar el logging de `ExecutedCommandSet` para ver qué comandos se ejecutaron y su resultado:

```csharp
var seismicSource = seismicSourceRepository.GetLatestByProjectId(projectId);
foreach (var trace in seismicSource.CommandTrace.Traces)
{
    Console.WriteLine($"{trace.CommandName}: {trace.ReturnCode} ({trace.DurationMs:F0}ms)");
}
```

### Errores frecuentes y soluciones

| Error | Causa | Solución |
|---|---|---|
| `COMException: RPC server unavailable` | SAP2000 cerrado inesperadamente | Reconectar: `Connect()` |
| `InvalidOperationException: Not connected` | Operación sin conexión activa | Llamar `Connect()` primero |
| `SapOperationException: Error 4` | No hay resultados disponibles | Ejecutar análisis en SAP2000 |
| `NullReferenceException en cSapModel` | Modelo no inicializado | Llamar `OpenModel()` primero |
| Arrays vacíos en resultados | Nombre de caso de carga incorrecto | Verificar nombre del combo en SAP2000 |

---

## Referencias

- [SAP2000 API Documentation](https://www.csiamerica.com/products/sap2000/watch-and-learn#documentation) — Manual oficial de la API COM
- [ACI 318-19](https://www.concrete.org/store/productdetail.aspx?ItemID=31819) — Código para Concreto Estructural
- [NTE E030](https://www.sencico.gob.pe/publicaciones.php?id=287) — Diseño Sismorresistente del RNE
- [ClosedXML](https://github.com/ClosedXML/ClosedXML) — Librería para generación de Excel

---

*Anterior: [Guía de Inicio Rápido](./GUIA_DE_INICIO_RAPIDO.md) | Índice: [README](../README.md)*
