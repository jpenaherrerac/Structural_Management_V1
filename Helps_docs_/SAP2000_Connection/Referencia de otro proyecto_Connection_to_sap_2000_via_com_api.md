Descripción completa de la manera en que en otro  proyecto se conecta a sap2000 mediante su API. 

## Copilot

Voy a analizar el repositorio para entender cómo se establece la conexión con SAP2000 mediante su API COM. Déjame investigar el código. Perfecto, ahora déjame buscar más detalles sobre el archivo connection.cs completo y el SapProcessor.cs para darte una descripción más precisa: Perfecto.  Ahora voy a buscar cómo se usa desde la UI para tener el panorama completo del flujo de conexión: Perfecto, ahora tengo toda la información necesaria para crear una descripción completa y detallada.  Aquí está: 

---

# DESCRIPCIÓN COMPLETA DE LA CONEXIÓN A SAP2000 VÍA API COM CON STA ÚNICO en otro proyecto

## Contexto General del Proyecto:  jpenaherrerac/TowerLoadsProcessorToSAP

Este proyecto de C# se conecta a SAP2000 mediante su API COM utilizando un patrón de **Single-Threaded Apartment (STA) único con message loop nativo**. La arquitectura garantiza que todas las interacciones COM con SAP2000 ocurran en un hilo STA dedicado, evitando problemas de threading y cumpliendo con los requisitos de COM. 

---

## ARQUITECTURA DE CONEXIÓN:  2 CLASES PRINCIPALES

### **1. CLASE `SapStaHost` (archivo: `connection.cs`)**
Clase estática que proporciona la infraestructura genérica para ejecutar llamadas COM en un hilo STA dedicado. 

#### **1.1 Clase Anidada:  `StaComRunner`**
Es el componente central que implementa el hilo STA con message loop nativo. 

**Responsabilidades:**
- Crear y gestionar un hilo STA dedicado
- Inicializar COM con `CoInitializeEx` usando `COINIT_APARTMENTTHREADED`
- Implementar un message loop nativo de Win32 (sin dependencia de WinForms)
- Permitir invocación cross-thread de código COM de manera sincrónica
- Garantizar la liberación correcta de COM con `CoUninitialize`

**Miembros principales:**

```c#
public sealed class StaComRunner : IDisposable
{
    // Constantes para mensajes personalizados
    const int WM_USER = 0x0400;
    const int WM_INVOKE = WM_USER + 1;  // Mensaje personalizado para invocaciones

    // Estructuras Win32 para el message loop
    struct MSG { /* hwnd, message, wParam, lParam, time, pt */ }
    struct POINT { /* x, y */ }

    // P/Invoke declarations (DllImport)
    // - user32.dll: PeekMessage, GetMessage, TranslateMessage, DispatchMessage, PostThreadMessage
    // - kernel32.dll: GetCurrentThreadId
    // - ole32.dll: CoInitializeEx, CoUninitialize

    // Estado del runner
    readonly Thread staThread;          // El hilo STA dedicado
    uint threadId;                      // ID del hilo STA
    readonly AutoResetEvent ready;      // Sincronización:  espera a que el hilo esté listo
    bool disposed;                      // Control de disposición

    // Constructor, métodos Invoke, Dispose
}
```

**Clase auxiliar interna:**
```c#
class InvocationInfo
{
    public Action Action;                           // Acción a ejecutar en el hilo STA
    public ManualResetEventSlim Event;              // Sincronización de completitud
    public Exception Exception;                     // Captura de excepciones
}
```

---

### **2. CLASE `SapProcessor` (archivo: `SapProcessor.cs`)**
Encapsula la lógica de conexión COM a SAP2000, inicialización del modelo y operaciones básicas.

**Responsabilidades:**
- Conectar a una instancia existente de SAP2000 o crear una nueva
- Inicializar un modelo en blanco
- Exponer la API de SAP2000 (`cSapModel`) para operaciones posteriores
- Liberar correctamente los objetos COM

**Miembros COM principales:**
```c#
private cHelper helper;        // Helper COM de SAP2000
private cOAPI sapObject;       // Objeto principal de la aplicación SAP2000
private cSapModel sapModel;    // Modelo SAP2000 (el API real para trabajar)

public cSapModel SapModel => sapModel;  // Expuesto públicamente
```

**Métodos públicos clave:**
- `ConnectAndInit()`: Establece conexión e inicializa modelo
- `ReleaseCom()`: Libera referencias COM
- `UnlockAndRefreshView()` / `ShowSAP2000()` / `HideSAP2000()`: Control de UI
- `RunBasicAnalysis()`: Ejecuta análisis (opcional)

---

## FLUJO DETALLADO DE ESTABLECIMIENTO DE CONEXIÓN

### **FASE 1: INICIALIZACIÓN DEL HILO STA (Constructor de `StaComRunner`)**

**Ubicación:** `connection.cs`, líneas 67-75

```c#
public StaComRunner()
{
    // 1. Crear el hilo con IsBackground = true
    staThread = new Thread(Run) { IsBackground = true };
    
    // 2. CRÍTICO: Configurar como STA
    staThread.SetApartmentState(ApartmentState.STA);
    
    // 3. Iniciar el hilo
    staThread. Start();
    
    // 4. BLOQUEAR hasta que el hilo complete su inicialización COM
    ready. WaitOne();  // Espera a que Run() señalice ready. Set()
}
```

**¿Qué sucede aquí?**
1. Se crea un nuevo `Thread` marcado como background
2. Se establece su apartment state a **STA** (requisito para COM de SAP2000)
3. Se inicia el hilo que ejecutará el método `Run()`
4. El thread principal **se bloquea** esperando que el hilo STA complete su inicialización

---

### **FASE 2: EJECUCIÓN DEL MESSAGE LOOP EN EL HILO STA (Método `Run()`)**

**Ubicación:** `connection.cs`, líneas 77-121

```c#
void Run()
{
    // ===== PASO 1: INICIALIZAR COM =====
    CoInitializeEx(IntPtr.Zero, COINIT_APARTMENTTHREADED);
    
    // ===== PASO 2: FORZAR CREACIÓN DE MESSAGE QUEUE =====
    MSG msg;
    PeekMessage(out msg, IntPtr.Zero, 0, 0, PM_NOREMOVE);
    
    // ===== PASO 3: OBTENER ID DEL HILO Y SEÑALIZAR LISTO =====
    threadId = GetCurrentThreadId();
    ready.Set();  // ¡Desbloquea el constructor!
    
    // ===== PASO 4: MESSAGE LOOP ESTÁNDAR DE WIN32 =====
    while (true)
    {
        int res = GetMessage(out msg, IntPtr.Zero, 0, 0);
        if (res == 0) break;  // WM_QUIT
        if (res == -1) break; // Error
        
        // ===== PASO 5: MANEJO DE MENSAJE PERSONALIZADO WM_INVOKE =====
        if (msg.message == WM_INVOKE)
        {
            // wParam contiene un GCHandle a InvocationInfo
            var h = GCHandle.FromIntPtr(msg.wParam);
            try
            {
                var info = (InvocationInfo)h.Target;
                try
                {
                    info. Action();  // ¡EJECUTAR CÓDIGO COM AQUÍ!
                }
                catch (Exception ex)
                {
                    info.Exception = ex;  // Capturar excepción para rethrow
                }
                finally
                {
                    info.Event.Set();  // Señalizar completitud
                }
            }
            finally
            {
                h. Free();  // Liberar el GCHandle
            }
            continue;
        }
        
        // ===== PASO 6: PROCESAR MENSAJES ESTÁNDAR DE WINDOWS =====
        TranslateMessage(ref msg);
        DispatchMessage(ref msg);
    }
    
    // ===== PASO 7: CLEANUP AL SALIR =====
    CoUninitialize();
}
```

**Detalles críticos:**

1. **`CoInitializeEx(IntPtr.Zero, COINIT_APARTMENTTHREADED)`**: Inicializa COM en modo STA para este hilo
2. **`PeekMessage(... , PM_NOREMOVE)`**: Fuerza la creación de la message queue del hilo (requisito de Win32)
3. **`ready.Set()`**: Desbloquea el constructor, indicando que el hilo está listo para recibir mensajes
4. **Message Loop**: Bucle infinito que procesa mensajes Win32 estándar
5. **`WM_INVOKE` personalizado**: Mensajes de invocación cross-thread que ejecutan código COM
6. **`CoUninitialize()`**: Limpia COM al finalizar el hilo

---

### **FASE 3: INVOCACIÓN CROSS-THREAD (Método `Invoke(Action action)`)**

**Ubicación:** `connection.cs`, líneas 130-149

```c#
public void Invoke(Action action)
{
    if (disposed) throw new ObjectDisposedException(nameof(StaComRunner));
    
    // ===== PASO 1: CREAR INFO DE INVOCACIÓN =====
    var info = new InvocationInfo { Action = action };
    
    // ===== PASO 2: CREAR GCHANDLE PARA PASAR AL OTRO HILO =====
    var handle = GCHandle.Alloc(info);
    
    // ===== PASO 3: ENVIAR MENSAJE AL HILO STA =====
    bool posted = PostThreadMessage(
        threadId, 
        WM_INVOKE, 
        GCHandle.ToIntPtr(handle),  // wParam = GCHandle
        IntPtr. Zero
    );
    
    if (!posted)
    {
        handle.Free();
        throw new InvalidOperationException("Failed to post message to STA thread.");
    }
    
    // ===== PASO 4: BLOQUEAR HASTA QUE SE COMPLETE =====
    info. Event.Wait();  // Espera a que Run() ejecute y señalice
    
    // ===== PASO 5: RE-LANZAR EXCEPCIÓN SI HUBO ERROR =====
    if (info. Exception != null)
    {
        ExceptionDispatchInfo.Capture(info.Exception).Throw();
    }
}
```

**Flujo de comunicación cross-thread:**

```
Thread Principal (UI)          Thread STA (COM)
      │                              │
      │  Invoke(() => {... })         │
      ├─────────────────────────────>│
      │  PostThreadMessage           │
      │  (WM_INVOKE + GCHandle)      │
      │                              │
      │  info.Event.Wait()           │ GetMessage recibe WM_INVOKE
      │  [BLOQUEADO]                 │ Extrae GCHandle → InvocationInfo
      │                              │ Ejecuta info.Action()
      │                              │ Captura excepción (si hay)
      │                              │ info.Event.Set()
      │<─────────────────────────────│
      │  [DESBLOQUEADO]              │
      │  Re-lanza excepción          │
      │                              │
```

---

### **FASE 4: CREACIÓN DEL RUNNER (Método Factory `SapStaHost. CreateRunner()`)**

**Ubicación:** `connection.cs`, líneas 181-184

```c#
public static StaComRunner CreateRunner()
{
    return new StaComRunner();  // Simple factory method
}
```

---

### **FASE 5: CONEXIÓN A SAP2000 (Método `SapProcessor.ConnectAndInit()`)**

**Ubicación:** `SapProcessor.cs`, líneas 71-75

```c#
public void ConnectAndInit()
{
    CreateHelperAndSapObject();  // ← FASE 6
    InitModel();                 // ← FASE 7
}
```

---

### **FASE 6: CREACIÓN DEL OBJETO SAP2000 (Método `CreateHelperAndSapObject()`)**

**Ubicación:** `SapProcessor.cs`, líneas 110-193

```c#
private void CreateHelperAndSapObject()
{
    // ===== PASO 1: CREAR cHelper =====
    try
    {
        helper = new Helper();  // Objeto COM helper de SAP2000
    }
    catch (Exception ex)
    {
        throw new Exception("No se pudo crear cHelper:  " + ex.Message, ex);
    }
    
    // ===== PASO 2: DETECTAR PROCESOS SAP2000 ACTIVOS =====
    var sapProcesses = Process.GetProcessesByName("SAP2000")
        .Where(p => ! p.HasExited && 
                    p.MainWindowHandle != IntPtr.Zero && 
                    ! string.IsNullOrWhiteSpace(p.MainWindowTitle))
        .ToArray();
    
    // ===== PASO 3: INTENTAR ADJUNTAR A INSTANCIA EXISTENTE =====
    string[] progIds = new[]
    {
        "CSI. SAP2000.API.SapObject",
        "Sap2000v1.SapObject",
        "CSI.SAP2000.SapObject"
    };
    
    cOAPI existing = null;
    
    // ===== PASO 3A:  Intentar con Marshal.GetActiveObject (ROT) =====
    foreach (var pid in progIds)
    {
        try
        {
            existing = (cOAPI)Marshal.GetActiveObject(pid);
            if (existing != null)
            {
                RaiseLog($"Adjuntado vía Marshal.GetActiveObject('{pid}')");
                break;
            }
        }
        catch (COMException) { /* Ignorar */ }
        catch (Exception ex)
        {
            Debug.WriteLine($"GetActiveObject('{pid}') error: {ex.Message}");
        }
    }
    
    // ===== PASO 3B: Intentar con Helper.GetObject =====
    if (existing == null)
    {
        foreach (var pid in progIds)
        {
            try
            {
                existing = helper.GetObject(pid);
                if (existing != null)
                {
                    RaiseLog($"Adjuntado vía Helper.GetObject('{pid}')");
                    break;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Helper.GetObject('{pid}') error: {ex.Message}");
            }
        }
    }
    
    // ===== PASO 4: SI HAY INSTANCIA EXISTENTE, USARLA =====
    if (existing != null)
    {
        sapObject = existing;
    }
    else
    {
        // ===== PASO 5: NO HAY INSTANCIA, CREAR UNA NUEVA =====
        try
        {
            sapObject = helper.CreateObjectProgID("CSI.SAP2000.API.SapObject");
        }
        catch (Exception ex)
        {
            throw new Exception("CreateObjectProgID falló: " + ex.Message, ex);
        }
        
        if (sapObject == null)
        {
            throw new Exception("CreateObjectProgID devolvió null.");
        }
        
        // ===== PASO 6: INICIAR LA APLICACIÓN SAP2000 =====
        int ret = sapObject.ApplicationStart(
            eUnits. N_m_C,  // Sistema de unidades:  Newton, metros, Celsius
            false,          // No visible inicialmente
            ""              // Sin archivo de configuración
        );
        RaiseLog($"SAP2000 iniciado => código {ret}");
    }
    
    // ===== PASO 7: OBTENER EL MODELO =====
    sapModel = sapObject.SapModel;
    
    // ===== PASO 8: LOG DE VERSIÓN =====
    try
    {
        RaiseLog($"Versión de API: {sapObject.GetOAPIVersionNumber()}");
    }
    catch { }
    
    RaiseLog("Conexión con SAP2000 establecida correctamente.");
}
```

**Estrategia de conexión multi-nivel:**

1. **Nivel 1**:  `Marshal.GetActiveObject()` - Busca en Running Object Table (ROT)
2. **Nivel 2**: `helper.GetObject()` - Método alternativo del helper COM
3. **Nivel 3**: `helper.CreateObjectProgID()` + `ApplicationStart()` - Crea nueva instancia

**ProgIDs intentados (en orden):**
- `CSI.SAP2000.API.SapObject` (versión moderna)
- `Sap2000v1.SapObject` (versión heredada)
- `CSI.SAP2000.SapObject` (alternativa)

---

### **FASE 7: INICIALIZACIÓN DEL MODELO (Método `InitModel()`)**

**Ubicación:** `SapProcessor.cs`, líneas 195-229

```c#
private void InitModel()
{
    if (sapModel == null)
    {
        throw new InvalidOperationException("SapModel no está inicializado.");
    }
    
    // ===== PASO 1: INICIALIZAR MODELO CON UNIDADES =====
    int retInit = sapModel.InitializeNewModel(eUnits.N_m_C);
    CheckRet(retInit, "InitializeNewModel");
    
    // ===== PASO 2: CREAR MODELO EN BLANCO =====
    int retNewBlank = sapModel.File.NewBlank();
    
    // ===== PASO 3: MANEJO DE FALLO (CON REINTENTO) =====
    if (retNewBlank != 0)
    {
        RaiseLog($"File.NewBlank retornó {retNewBlank}.  Reintentando con instancia nueva...");
        
        try
        {
            // Crear instancia completamente nueva
            var newSap = helper.CreateObjectProgID("CSI.SAP2000.API. SapObject");
            if (newSap == null)
            {
                throw new Exception("CreateObjectProgID devolvió null en reintento.");
            }
            
            int startRet = newSap.ApplicationStart(eUnits.N_m_C, false, "");
            RaiseLog($"Reintento:  ApplicationStart => código {startRet}");
            
            // Reemplazar sapObject y sapModel
            sapObject = newSap;
            sapModel = sapObject.SapModel;
            
            // Reintentar inicialización
            retInit = sapModel.InitializeNewModel(eUnits.N_m_C);
            CheckRet(retInit, "InitializeNewModel (reintento)");
            
            retNewBlank = sapModel.File.NewBlank();
            CheckRet(retNewBlank, "File.NewBlank (reintento)");
        }
        catch (Exception ex)
        {
            throw new Exception("Fallo al crear modelo en blanco tras reintento: " + ex. Message, ex);
        }
    }
    
    RaiseLog("Modelo en blanco inicializado correctamente.");
}
```

**Estrategia de resiliencia:**
- Si `NewBlank()` falla, crea una **instancia completamente nueva** de SAP2000
- Reintenta la inicialización con la nueva instancia
- Lanza excepción solo si el reintento también falla

---

### **FASE 8: LIBERACIÓN DE RECURSOS (Método `ReleaseCom()`)**

**Ubicación:** `SapProcessor.cs`, líneas 103-108

```c#
public void ReleaseCom()
{
    // Liberar en orden inverso a la creación
    if (sapModel != null)
    {
        try { Marshal.ReleaseComObject(sapModel); } catch { }
        sapModel = null;
    }
    
    if (sapObject != null)
    {
        try { Marshal.ReleaseComObject(sapObject); } catch { }
        sapObject = null;
    }
    
    if (helper != null)
    {
        try { Marshal.ReleaseComObject(helper); } catch { }
        helper = null;
    }
}
```

---

### **FASE 9: DISPOSICIÓN DEL RUNNER (Método `Dispose()` de `StaComRunner`)**

**Ubicación:** `connection.cs`, líneas 158-167

```c#
public void Dispose()
{
    if (disposed) return;
    disposed = true;
    
    // ===== PASO 1: ENVIAR WM_QUIT AL MESSAGE LOOP =====
    PostThreadMessage(threadId, 0x0012 /* WM_QUIT */, IntPtr.Zero, IntPtr. Zero);
    
    // ===== PASO 2: ESPERAR A QUE EL HILO TERMINE =====
    staThread.Join();
    
    // ===== PASO 3: LIMPIAR SINCRONIZACIÓN =====
    ready. Dispose();
}
```

---

## USO COMPLETO DESDE LA APLICACIÓN (Program.cs)

**Ubicación:** `Program.cs`, líneas 11-52

```c#
[STAThread]  // ← El thread principal también es STA (para WinForms)
static void Main()
{
    Application.EnableVisualStyles();
    Application.SetCompatibleTextRenderingDefault(false);
    
    SapStaHost. StaComRunner sapRunner = null;
    SapProcessor sapProcessor = null;
    
    try
    {
        // ===== 1. CREAR RUNNER STA DEDICADO =====
        sapRunner = SapStaHost.CreateRunner();
        
        // ===== 2. CONECTAR A SAP2000 EN EL HILO STA =====
        sapRunner. Invoke(() =>
        {
            sapProcessor = new SapProcessor();
            sapProcessor.ConnectAndInit();
            // NO mostrar SAP2000 todavía
        });
        
        // ===== 3. EJECUTAR LA UI =====
        Application.Run(new MainForm(resultado, sapRunner, sapProcessor));
    }
    catch (Exception ex)
    {
        MessageBox.Show(ex.ToString(), "Error al iniciar aplicación",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
    }
    finally
    {
        // ===== 4. CLEANUP =====
        if (sapRunner != null && sapProcessor != null)
        {
            try
            {
                sapRunner. Invoke(() => sapProcessor.ReleaseCom());
            }
            catch { }
        }
        sapRunner?.Dispose();
    }
}
```

---

## RESUMEN DE CLASES Y COMPONENTES

### **Cantidad de Clases:**
1. **`SapStaHost`** (estática) - Infraestructura STA
   - **`StaComRunner`** (anidada, sellada) - Runner del hilo STA
     - **`InvocationInfo`** (anidada privada) - Metadata de invocación
2. **`SapProcessor`** - Lógica de conexión COM a SAP2000

**Total: 2 clases principales + 2 clases auxiliares anidadas**

### **Objetos COM de SAP2000:**
1. **`cHelper`** - Helper para crear/obtener objetos COM
2. **`cOAPI`** (interface `sapObject`) - Objeto principal de aplicación
3. **`cSapModel`** (interface `sapModel`) - API del modelo

---

## DIAGRAMA DE FLUJO SIMPLIFICADO

```
┌─────────────────────────────────────────────────────────────┐
│ INICIO:  Program.Main() [STAThread]                          │
└───────────────────────┬─────────────────────────────────────┘
                        │
                        ▼
┌─────────────────────────────────────────────────────────────┐
│ FASE 1-4: SapStaHost.CreateRunner()                         │
│   • new StaComRunner()                                      │
│   • Crear Thread STA                                        │
│   • Thread. Run() → CoInitializeEx + Message Loop            │
│   • Constructor. WaitOne() hasta ready. Set()                 │
└───────────────────────┬─────────────────────────────────────┘
                        │
                        ▼
┌─────────────────────────────────────────────────────────────┐
│ FASE 5-7: sapRunner.Invoke(() => {                          │
│   new SapProcessor().ConnectAndInit()                       │
│ })                                                           │
│                                                              │
│   CROSS-THREAD:                                              │
│   • PostThreadMessage(WM_INVOKE + GCHandle)                 │
│   • Invoke. Wait() [BLOQUEA]                                 │
│   • Message Loop recibe WM_INVOKE                           │
│   • Ejecuta:  CreateHelperAndSapObject()                     │
│     - new Helper()                                          │
│     - Marshal.GetActiveObject() o helper.CreateObjectProgID()│
│     - sapObject. ApplicationStart()                          │
│     - sapModel = sapObject.SapModel                         │
│   • Ejecuta: InitModel()                                    │
│     - sapModel. InitializeNewModel()                         │
│     - sapModel.File.NewBlank()                              │
│   • info.Event.Set() [DESBLOQUEA]                           │
└───────────────────────┬─────────────────────────────────────┘
                        │
                        ▼
┌─────────────────────────────────────────────────────────────┐
│ Application.Run(new MainForm(... , sapRunner, sapProcessor)) │
│   • MainForm usa sapRunner. Invoke() para todas las          │
│     operaciones COM posteriores                             │
└───────────────────────┬─────────────────────────────────────┘
                        │
                        ▼
┌─────────────────────────────────────────────────────────────┐
│ CLEANUP: finally                                            │
│   • sapRunner.Invoke(() => sapProcessor.ReleaseCom())       │
│     - Marshal.ReleaseComObject(sapModel)                    │
│     - Marshal.ReleaseComObject(sapObject)                   │
│     - Marshal.ReleaseComObject(helper)                      │
│   • sapRunner.Dispose()                                     │
│     - PostThreadMessage(WM_QUIT)                            │
│     - staThread.Join()                                      │
│     - CoUninitialize() en Run()                             │
└───────────────────────┬─────────────────────────────────────┘
                        │
                        ▼
                   [FIN]
```

---

## CARACTERÍSTICAS CLAVE DEL PATRÓN STA ÚNICO

1. **Thread Dedicado**: Un solo hilo STA para TODO el COM de SAP2000
2. **Message Loop Nativo**: Win32 puro (no WinForms `Application.DoEvents()`)
3. **Sincronización Robusta**: `AutoResetEvent` para inicialización, `ManualResetEventSlim` para invocaciones
4. **GCHandle Cross-Thread**: Paso seguro de objetos managed entre threads
5. **Preservación de Stack Traces**: `ExceptionDispatchInfo. Capture()` para excepciones
6. **Sin Race Conditions**: Toda interacción COM ocurre secuencialmente en un solo hilo
7. **Lifecycle Completo**: `CoInitializeEx` → Uso → `CoUninitialize`

---

## P/INVOKE UTILIZADAS (Win32 API)

**user32.dll:**
- `PeekMessage` - Crear message queue y peek sin remover
- `GetMessage` - Bloquear hasta recibir mensaje
- `TranslateMessage` - Traducir teclas virtuales
- `DispatchMessage` - Despachar mensaje a ventana
- `PostThreadMessage` - Enviar mensaje a thread por ID

**kernel32.dll:**
- `GetCurrentThreadId` - Obtener ID del thread actual

**ole32.dll:**
- `CoInitializeEx` - Inicializar COM en modo STA
- `CoUninitialize` - Desinicializar COM

---

## CÓDIGOS DE RETORNO IMPORTANTES

- **`ApplicationStart()`**: `0` = éxito
- **`InitializeNewModel()`**: `0` = éxito
- **`File.NewBlank()`**: `0` = éxito, `!= 0` = fallo (activa reintento)

---

Esta descripción completa documenta **exclusivamente el establecimiento de la conexión** entre la aplicación C# y SAP2000 vía COM con STA único y message loop nativo.  Todos los flujos posteriores de uso del API (creación de geometría, análisis, etc.) no están cubiertos aquí, pero utilizan el mismo patrón:  `sapRunner.Invoke(() => /* código COM */)`.

/////////////////////////
Pendiente por documentar:
1.	Agregar una sección “Checklist de prerequisitos”
•	SAP2000 instalado (versión/bitness).
•	API/OAPI habilitada (qué confirmar en SAP/instalación).
•	Referencia a SAP2000v1.dll (ruta típica) y Embed Interop Types = False.
•	Arquitectura del ejecutable (x64 vs AnyCPU vs x86) y cómo verificarlo.
•	Requisito de hilo STA + message loop.
2.	Separar el documento en 3 capas (para que se entienda el “por qué”)
•	Infra STA (SapStaHost/StaComRunner): objetivo, por qué message loop, por qué CoInitializeEx.
•	Conexión COM (SapProcessor): estrategia de attach/create y manejo de errores.
•	Uso desde UI: patrón recomendado (runner.Invoke(...)), ciclo de vida y cleanup.
3.	Añadir una sección “Diagnóstico / Troubleshooting”
•	Síntomas comunes y causa:
•	“no encuentra ProgID”: COM no registrado / bitness mismatch.
•	“MarshalByRefObject sin SapModel”: proxy/no RCW, o se obtuvo el objeto equivocado.
•	COMException específicas y qué significan.
•	Script/steps para validar registro COM (por ejemplo con Registry.ClassesRoot).
•	Recomendación: loggear ProgID usado, versión API GetOAPIVersionNumber, y ruta de SAP2000v1.dll.
4.	Explicar mejor el ciclo de vida COM
•	Crear runner una vez, reutilizarlo.
•	Crear SapProcessor dentro del runner.
•	Liberar COM en orden inverso + dispose del runner.
•	Qué NO hacer: instanciar COM en el UI thread y usarlo en otro hilo.
5.	Ajustar/estandarizar el código del documento
•	Formatear consistentemente (bloques por archivo, regiones, comentarios).
•	Marcar “V0 / MVP” vs “Producción” (ej. reconexión, múltiples modelos).
•	Incluir variantes: conectar a instancia existente vs crear nueva.
6.	Agregar un diagrama simple del flujo
•	UI thread -> runner.Invoke -> STA COM thread -> SAP2000 OAPI -> return.

EJEMPLO de CLASES:

SapProcessor.cs ->
/////////////////////////////////////////////////////////////////////////
using SAP2000v1;
using System;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Linq;

// Encapsula conexión COM, inicialización básica, análisis opcional y release con SAP2000
public class SapProcessor
{
 // Evento de log para que la UI pueda mostrar mensajes en tiempo real
 public static event Action<string> LogMessage;

 private static void RaiseLog(string message)
 {
 Debug.WriteLine(message);
 var handler = LogMessage;
 if (handler != null)
 handler(message);
 }

 /// <summary>
/// Método público para enviar mensajes al log desde otras clases.
  /// </summary>
 public static void Log(string message)
{
   RaiseLog(message);
}

 /// <summary>
 /// Oculta la ventana de SAP2000 para evitar distracciones y mejorar rendimiento.
 /// </summary>
 public void HideSAP2000()
 {
     if (sapObject == null) return;
     try
     {
   sapObject.Hide();
      RaiseLog("[SapProcessor] SAP2000 oculto");
 }
     catch (Exception ex)
 {
   Debug.WriteLine($"[SapProcessor] Error al ocultar SAP2000: {ex.Message}");
  }
 }

 /// <summary>
 /// Muestra la ventana de SAP2000 y refresca la vista.
 /// </summary>
 public void ShowSAP2000()
 {
     if (sapObject == null || sapModel == null) return;
     try
     {
         sapObject.Unhide();
         sapModel.View.RefreshView();
         RaiseLog("[SapProcessor] SAP2000 visible");
 }
     catch (Exception ex)
     {
         Debug.WriteLine($"[SapProcessor] Error al mostrar SAP2000: {ex.Message}");
   }
 }
 // COM objects
 private cHelper helper;
 private cOAPI sapObject;
 private cSapModel sapModel;

 public cSapModel SapModel => sapModel;

 // Conecta a una instancia existente de SAP2000 (si hay) o crea una nueva, y prepara un modelo en blanco
 public void ConnectAndInit()
 {
 CreateHelperAndSapObject();
 InitModel();
 }

 // Ejecuta un análisis muy básico sobre el modelo actual (opcional)
 public void RunBasicAnalysis()
 {
 if (sapModel == null) throw new InvalidOperationException("SapModel no está inicializado.");

 int retAnaly;
 var savePath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "ModeloArbolDeCargas.sdb");
 retAnaly = sapModel.File.Save(savePath);
 RaiseLog($"[SapProcessor] Modelo guardado en: {savePath} (ret={retAnaly})");

 retAnaly = sapModel.Analyze.CreateAnalysisModel();
 RaiseLog($"[SapProcessor] CreateAnalysisModel => {retAnaly}");

 retAnaly = sapModel.Analyze.RunAnalysis();
 RaiseLog($"[SapProcessor] RunAnalysis => {retAnaly}");
 }

 // Muestra la ventana de SAP2000 y refresca la vista
 public void UnlockAndRefreshView()
 {
 if (sapObject == null || sapModel == null) return;
 try { sapObject.Unhide(); } catch { }
 try { sapModel.View.RefreshView(); } catch { }
 }

 // Libera referencias COM. Debe llamarse cuando ya no se usa SAP2000 desde este proceso
 public void ReleaseCom()
 {
 if (sapModel != null) { try { Marshal.ReleaseComObject(sapModel); } catch { } sapModel = null; }
 if (sapObject != null) { try { Marshal.ReleaseComObject(sapObject); } catch { } sapObject = null; }
 if (helper != null) { try { Marshal.ReleaseComObject(helper); } catch { } helper = null; }
 }

 private void CreateHelperAndSapObject()
 {
 try { helper = new Helper(); }
 catch (Exception ex) { throw new Exception("No se pudo crear cHelper: " + ex.Message, ex); }

 // Detectar procesos SAP2000 abiertos (con ventana principal válida)
 var sapProcessesRaw = Process.GetProcessesByName("SAP2000");
 var sapProcesses = sapProcessesRaw
 .Where(p =>
 {
 try { return !p.HasExited && p.MainWindowHandle != IntPtr.Zero && !string.IsNullOrWhiteSpace(p.MainWindowTitle); }
 catch { return false; }
 })
 .ToArray();

 if (sapProcesses.Length ==0)
 {
 RaiseLog("[SapProcessor] No se encontraron ventanas activas de SAP2000. Se intentará adjuntar vía ROT y, si no, crear una nueva.");
 }

 // Intentar adjuntar a instancia existente usando distintas ProgID conocidas
 string[] progIds = new[] { "CSI.SAP2000.API.SapObject", "Sap2000v1.SapObject", "CSI.SAP2000.SapObject" };
 cOAPI existing = null;
 foreach (var pid in progIds)
 {
 try
 {
 existing = (cOAPI)Marshal.GetActiveObject(pid);
 if (existing != null)
 {
 RaiseLog($"[SapProcessor] Adjuntado a instancia existente de SAP2000 vía Marshal.GetActiveObject('{pid}').");
 break;
 }
 }
 catch (COMException)
 {
 // Ignorar y probar siguiente
 }
 catch (Exception ex)
 {
 Debug.WriteLine($"[SapProcessor] Aviso: Marshal.GetActiveObject('{pid}') lanzó excepción: {ex.Message}");
 }
 }

 if (existing == null)
 {
 foreach (var pid in progIds)
 {
 try
 {
 existing = helper.GetObject(pid);
 if (existing != null)
 {
 RaiseLog($"[SapProcessor] Adjuntado a instancia existente vía Helper.GetObject('{pid}').");
 break;
 }
 }
 catch (Exception ex)
 {
 Debug.WriteLine($"[SapProcessor] Aviso: Helper.GetObject('{pid}') lanzó excepción: {ex.Message}");
 }
 }
 }

 if (existing != null)
 {
 sapObject = existing;
 try { /*sapObject.Hide();*/ } catch { }
 }
 else
 {
 // No hay instancia existente, crear una nueva
 try { sapObject = helper.CreateObjectProgID("CSI.SAP2000.API.SapObject"); }
 catch (Exception ex) { throw new Exception("CreateObjectProgID falló: " + ex.Message, ex); }
 if (sapObject == null) throw new Exception("CreateObjectProgID devolvió null.");
 int ret = sapObject.ApplicationStart(eUnits.N_m_C, false, "");
 RaiseLog($"[SapProcessor] SAP2000 iniciado => código {ret}");
 try { /*sapObject.Hide();*/ } catch { }
 }

 sapModel = sapObject.SapModel;
 try { RaiseLog($"[SapProcessor] Versión de API: {sapObject.GetOAPIVersionNumber()}"); } catch { }
 RaiseLog("[SapProcessor] Conexión con SAP2000 establecida correctamente.");
 }

 private void InitModel()
 {
 if (sapModel == null) throw new InvalidOperationException("SapModel no está inicializado.");

 // Inicializa y crea modelo en blanco. Si falla, reintenta creando una nueva instancia dedicada.
 int retInit = sapModel.InitializeNewModel(eUnits.N_m_C);
 CheckRet(retInit, "InitializeNewModel");
 int retNewBlank = sapModel.File.NewBlank();
 if (retNewBlank !=0)
 {
 RaiseLog($"[SapProcessor] Aviso: File.NewBlank retornó código {retNewBlank}. Se intentará iniciar instancia nueva y reintentar.");
 // Intentar una instancia nueva dedicada desde helper
 try
 {
 var newSap = helper.CreateObjectProgID("CSI.SAP2000.API.SapObject");
 if (newSap == null) throw new Exception("CreateObjectProgID devolvió null en reintento.");
 int startRet = newSap.ApplicationStart(eUnits.N_m_C, false, "");
 RaiseLog($"[SapProcessor] Reintento: ApplicationStart => código {startRet}");
 // Cambiar sapObject/sapModel a la nueva instancia
 sapObject = newSap;
 sapModel = sapObject.SapModel;
 // Reintentar inicialización y NewBlank
 retInit = sapModel.InitializeNewModel(eUnits.N_m_C);
 CheckRet(retInit, "InitializeNewModel (reintento)");
 retNewBlank = sapModel.File.NewBlank();
 CheckRet(retNewBlank, "File.NewBlank (reintento)");
 }
 catch (Exception ex)
 {
 // Propagar con contexto original
 throw new Exception("Fallo al crear modelo en blanco tras reintento: " + ex.Message, ex);
 }
 }
 RaiseLog("[SapProcessor] Modelo en blanco inicializado correctamente.");
 }

 private static void CheckRet(int ret, string where)
 {
 if (ret !=0)
 throw new Exception($"{where} retornó código {ret}");
 }
}

////////////////////////////////////////////////////////////
Connection.cs-->
////////////////////////
using SAP2000v1;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.ExceptionServices;
using System.Threading;

// Infraestructura genérica para ejecutar llamadas COM (SAP2000) en un hilo STA dedicado con message loop.
// Se reutiliza en este proyecto para mantener todas las interacciones con SAP2000 en un solo hilo STA.
public static class SapStaHost
{
 // STA runner implemented with a native message loop (no WinForms dependency).
 // Purpose: run all COM interactions on a dedicated STA thread with a message loop
 // so SAP2000 COM objects are created/released on an STA thread.
 public sealed class StaComRunner : IDisposable
 {
 const int WM_USER =0x0400;
 const int WM_INVOKE = WM_USER +1;

 [StructLayout(LayoutKind.Sequential)]
 struct POINT { public int x; public int y; }

 [StructLayout(LayoutKind.Sequential)]
 struct MSG
 {
 public IntPtr hwnd;
 public uint message;
 public IntPtr wParam;
 public IntPtr lParam;
 public uint time;
 public POINT pt;
 }

 [DllImport("user32.dll", SetLastError = true)]
 static extern bool PeekMessage(out MSG lpMsg, IntPtr hWnd, uint wMsgFilterMin, uint wMsgFilterMax, uint wRemoveMsg);

 [DllImport("user32.dll", SetLastError = true)]
 static extern int GetMessage(out MSG lpMsg, IntPtr hWnd, uint wMsgFilterMin, uint wMsgFilterMax);

 [DllImport("user32.dll")]
 static extern bool TranslateMessage([In] ref MSG lpMsg);

 [DllImport("user32.dll")]
 static extern IntPtr DispatchMessage([In] ref MSG lpMsg);

 [DllImport("user32.dll", SetLastError = true)]
 static extern bool PostThreadMessage(uint idThread, uint Msg, IntPtr wParam, IntPtr lParam);

 [DllImport("kernel32.dll")]
 static extern uint GetCurrentThreadId();

 [DllImport("ole32.dll")]
 static extern int CoInitializeEx(IntPtr pvReserved, uint dwCoInit);

 [DllImport("ole32.dll")]
 static extern void CoUninitialize();

 const uint COINIT_APARTMENTTHREADED =0x2;
 const uint PM_NOREMOVE =0x0000;

 readonly Thread staThread;
 uint threadId;
 readonly AutoResetEvent ready = new AutoResetEvent(false);
 bool disposed;

 public StaComRunner()
 {
 // Start STA thread and wait until its COM and message queue are ready.
 staThread = new Thread(Run) { IsBackground = true };
 staThread.SetApartmentState(ApartmentState.STA);
 staThread.Start();
 // Wait for thread to initialize COM and message queue
 ready.WaitOne();
 }

 void Run()
 {
 // Initialize COM for STA explicitly
 CoInitializeEx(IntPtr.Zero, COINIT_APARTMENTTHREADED);

 // Force creation of message queue for this thread
 MSG msg;
 PeekMessage(out msg, IntPtr.Zero,0,0, PM_NOREMOVE);

 threadId = GetCurrentThreadId();
 ready.Set();

 // Standard message loop
 while (true)
 {
 int res = GetMessage(out msg, IntPtr.Zero,0,0);
 if (res ==0)
 break; // WM_QUIT
 if (res == -1)
 break; // error

 if (msg.message == WM_INVOKE)
 {
 // wParam contains a GCHandle to an InvocationInfo
 var h = GCHandle.FromIntPtr(msg.wParam);
 try
 {
 var info = (InvocationInfo)h.Target;
 try
 { info.Action(); }
 catch (Exception ex) { info.Exception = ex; }
 finally
 { info.Event.Set(); }
 }
 finally { h.Free(); }

 continue;
 }

 TranslateMessage(ref msg);
 DispatchMessage(ref msg);
 }

 CoUninitialize();
 }

 class InvocationInfo
 {
 public Action Action;
 public ManualResetEventSlim Event = new ManualResetEventSlim(false);
 public Exception Exception;
 }

 public void Invoke(Action action)
 {
 if (disposed) throw new ObjectDisposedException(nameof(StaComRunner));
 var info = new InvocationInfo { Action = action };
 var handle = GCHandle.Alloc(info);
 bool posted = PostThreadMessage(threadId, WM_INVOKE, GCHandle.ToIntPtr(handle), IntPtr.Zero);
 if (!posted)
 {
 handle.Free();
 throw new InvalidOperationException("Failed to post message to STA thread.");
 }

 // Wait for completion
 info.Event.Wait();
 if (info.Exception != null)
 {
 // Re-throw original exception preserving stack to reveal root cause
 ExceptionDispatchInfo.Capture(info.Exception).Throw();
 }
 }

 public T Invoke<T>(Func<T> func)
 {
 T result = default;
 Invoke(() => { result = func(); });
 return result;
 }

 public void Dispose()
 {
 if (disposed) return;
 disposed = true;
 // Post WM_QUIT to end message loop
 PostThreadMessage(threadId,0x0012 /* WM_QUIT */, IntPtr.Zero, IntPtr.Zero);
 // Wait for thread to exit
 staThread.Join();
 ready.Dispose();
 }
 }

 // Método de ayuda de alto nivel para usar SapProcessor desde un hilo STA dedicado.
 // Ejemplo de uso desde la UI:
 // using (var runner = SapStaHost.CreateRunner()) {
 // runner.Invoke(() => {
 // var proc = new SapProcessor();
 // proc.ConnectAndInit();
 // proc.UnlockAndRefreshView();
 // ...
 // proc.ReleaseCom();
 // });
 // }
 public static StaComRunner CreateRunner()
 {
 return new StaComRunner();
 }
}

    
