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