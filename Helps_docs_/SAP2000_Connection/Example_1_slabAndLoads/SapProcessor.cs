using System;
using System.Runtime.InteropServices;

#if SAP2000_AVAILABLE
using SAP2000v1;
#endif

namespace App.Infrastructure.Sap2000
{
    /// <summary>
    /// Strongly-typed SAP2000 COM processor (SAP2000v1 interop).
    /// Mirrors the known-good approach from the reference project.
    /// All calls must be executed on the STA runner thread.
    /// </summary>
    public sealed class SapProcessor
    {
#if SAP2000_AVAILABLE
        private cHelper helper;
        private cOAPI sapObject;
        private cSapModel sapModel;

        public cSapModel SapModel => sapModel;
#else
        public object SapModel => null;
#endif

        public string ConnectedProgId { get; private set; }

        public void ConnectAndInit(string units = null, bool visible = false)
        {
#if SAP2000_AVAILABLE
            CreateHelperAndSapObject(visible);
            InitModel();
#else
            throw new NotSupportedException("SAP2000 integration is not available. SAP2000v1.dll reference is missing.");
#endif
        }

#if SAP2000_AVAILABLE
        private void CreateHelperAndSapObject(bool visible)
        {
            try { helper = new Helper(); }
            catch (Exception ex) { throw new Exception("No se pudo crear cHelper: " + ex.Message, ex); }

            string[] progIds = new[]
            {
                "CSI.SAP2000.API.SapObject",
                "Sap2000v1.SapObject",
                "CSI.SAP2000.SapObject"
            };

            cOAPI existing = null;

            //1) ROT
            foreach (var pid in progIds)
            {
                try
                {
                    existing = (cOAPI)Marshal.GetActiveObject(pid);
                    if (existing != null)
                    {
                        ConnectedProgId = pid + " (GetActiveObject)";
                        break;
                    }
                }
                catch (COMException)
                {
                    // ignore
                }
                catch
                {
                    // ignore
                }
            }

            //2) Helper.GetObject
            if (existing == null)
            {
                foreach (var pid in progIds)
                {
                    try
                    {
                        existing = helper.GetObject(pid);
                        if (existing != null)
                        {
                            ConnectedProgId = pid + " (Helper.GetObject)";
                            break;
                        }
                    }
                    catch
                    {
                        // ignore
                    }
                }
            }

            if (existing != null)
            {
                sapObject = existing;
            }
            else
            {
                //3) Create new
                try
                {
                    sapObject = helper.CreateObjectProgID("CSI.SAP2000.API.SapObject");
                    ConnectedProgId = "CSI.SAP2000.API.SapObject (Helper.CreateObjectProgID)";
                }
                catch (Exception ex)
                {
                    throw new Exception("CreateObjectProgID falló: " + ex.Message, ex);
                }

                if (sapObject == null)
                    throw new Exception("CreateObjectProgID devolvió null.");

                // Start SAP2000
                int ret = sapObject.ApplicationStart(eUnits.N_m_C, visible, "");
                if (ret != 0)
                {
                    // still continue; some setups return nonzero but start
                }
            }

            sapModel = sapObject.SapModel;
            if (sapModel == null)
                throw new Exception("SapModel no está disponible (sapObject.SapModel devolvió null).");
        }

        private void InitModel()
        {
            if (sapModel == null)
                throw new InvalidOperationException("SapModel no está inicializado.");

            int retInit = sapModel.InitializeNewModel(eUnits.N_m_C);
            CheckRet(retInit, "InitializeNewModel");

            int retNewBlank = sapModel.File.NewBlank();
            if (retNewBlank != 0)
            {
                // Retry with new instance
                try
                {
                    var newSap = helper.CreateObjectProgID("CSI.SAP2000.API.SapObject");
                    if (newSap == null) throw new Exception("CreateObjectProgID devolvió null en reintento.");

                    int startRet = newSap.ApplicationStart(eUnits.N_m_C, false, "");
                    // swap
                    sapObject = newSap;
                    sapModel = sapObject.SapModel;
                    if (sapModel == null) throw new Exception("SapModel null en reintento.");

                    retInit = sapModel.InitializeNewModel(eUnits.N_m_C);
                    CheckRet(retInit, "InitializeNewModel (reintento)");

                    retNewBlank = sapModel.File.NewBlank();
                    CheckRet(retNewBlank, "File.NewBlank (reintento)");
                }
                catch (Exception ex)
                {
                    throw new Exception("Fallo al crear modelo en blanco tras reintento: " + ex.Message, ex);
                }
            }
        }

        private static void CheckRet(int ret, string where)
        {
            if (ret != 0)
                throw new Exception(where + " retornó código " + ret);
        }
#endif

        public void ReleaseCom()
        {
#if SAP2000_AVAILABLE
            if (sapModel != null) { try { Marshal.ReleaseComObject(sapModel); } catch { } sapModel = null; }
            if (sapObject != null) { try { Marshal.ReleaseComObject(sapObject); } catch { } sapObject = null; }
            if (helper != null) { try { Marshal.ReleaseComObject(helper); } catch { } helper = null; }
#endif
        }
    }
}
