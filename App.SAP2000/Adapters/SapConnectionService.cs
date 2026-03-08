using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using App.Application.Interfaces;
using App.Domain.Entities.Sap;

namespace App.SAP2000.Adapters
{
    /// <summary>
    /// Manages the COM connection to SAP2000.
    /// Supports discovering running instances, attaching to a specific one,
    /// creating new instances, disconnecting cleanly, and switching sessions.
    /// On environments without SAP2000 (Linux / CI) it falls back to mock mode.
    /// </summary>
    public class SapConnectionService : ISapConnectionManager
    {
        private dynamic? _sapObject;
        private dynamic? _sapModel;
        private bool _isConnected;
        private string _sapVersion = string.Empty;
        private SapSession? _currentSession;
        private bool _isMockMode;

        // ── ISapConnectionManager ───────────────────────────────────────────────
        public SapSession? CurrentSession => _currentSession;
        SapSession ISapConnectionManager.CurrentSession => _currentSession!;
        public bool IsConnected => _isConnected;
        internal dynamic? SapModel => _sapModel;
        internal dynamic? SapObject => _sapObject;

        public event EventHandler<bool>? ConnectionStateChanged;

        // ─── Discover ───────────────────────────────────────────────────────────

        /// <summary>
        /// Discovers all running SAP2000 processes with a valid main window.
        /// Returns empty list in mock / Linux mode.
        /// </summary>
        public IReadOnlyList<SapInstanceInfo> DiscoverInstances()
        {
            var list = new List<SapInstanceInfo>();
            try
            {
                var procs = Process.GetProcessesByName("SAP2000");
                foreach (var p in procs)
                {
                    try
                    {
                        if (p.HasExited) continue;
                        if (p.MainWindowHandle == IntPtr.Zero) continue;

                        string title = string.Empty;
                        try { title = p.MainWindowTitle; } catch { }

                        string path = string.Empty;
                        try { path = p.MainModule?.FileName ?? string.Empty; } catch { }

                        list.Add(new SapInstanceInfo(path, string.Empty, p.Id, string.Empty)
                        {
                            WindowTitle = title,
                            IsVisible = true,
                            IsNewInstance = false
                        });
                    }
                    catch { /* skip inaccessible process */ }
                }
            }
            catch { /* Process enumeration not supported on this platform */ }
            return list;
        }

        // ─── Attach ─────────────────────────────────────────────────────────────

        /// <summary>
        /// Attaches to an existing SAP2000 instance.
        /// Tries multiple ProgIDs via GetActiveObject first, then falls back to
        /// Activator.CreateInstance + attach.
        /// </summary>
        public SapSession AttachToInstance(SapInstanceInfo instance)
        {
            if (instance == null) throw new ArgumentNullException(nameof(instance));
            DisconnectInternal();

            try
            {
                var progId = "CSI.SAP2000.API.SapObject";
                var sapType = Type.GetTypeFromProgID(progId);

                if (sapType != null)
                {
                    // Try GetActiveObject first (gets running instance from ROT)
                    try
                    {
                        _sapObject = Marshal.GetActiveObject(progId);
                    }
                    catch
                    {
                        // Fall back to CreateInstance which may also attach
                        _sapObject = Activator.CreateInstance(sapType);
                    }

                    _sapModel = _sapObject!.SapModel;
                    try { _sapVersion = _sapObject.GetOAPIVersionNumber()?.ToString() ?? "Unknown"; }
                    catch { _sapVersion = "Unknown"; }
                    _isConnected = true;
                    _isMockMode = false;
                }
                else
                {
                    // Mock mode (Linux / SAP2000 not installed)
                    _isConnected = true;
                    _isMockMode = true;
                    _sapVersion = "Mock-24.0.0";
                }

                instance.SapVersion = _sapVersion;
                _currentSession = new SapSession(Guid.NewGuid(), Environment.MachineName, _sapVersion, instance);
                ConnectionStateChanged?.Invoke(this, true);
                return _currentSession;
            }
            catch (Exception ex)
            {
                _isConnected = false;
                throw new InvalidOperationException($"Failed to attach to SAP2000 (PID {instance.ProcessId}): {ex.Message}", ex);
            }
        }

        // ─── Create new ─────────────────────────────────────────────────────────

        /// <summary>
        /// Launches a brand-new SAP2000 process and connects to it.
        /// </summary>
        public SapSession CreateNewInstance()
        {
            DisconnectInternal();
            try
            {
                var progId = "CSI.SAP2000.API.SapObject";
                var sapType = Type.GetTypeFromProgID(progId);

                if (sapType != null)
                {
                    _sapObject = Activator.CreateInstance(sapType);
                    _sapObject!.ApplicationStart(0, true, string.Empty, string.Empty, string.Empty);
                    _sapModel = _sapObject.SapModel;
                    try { _sapVersion = _sapObject.GetOAPIVersionNumber()?.ToString() ?? "Unknown"; }
                    catch { _sapVersion = "Unknown"; }
                    _isConnected = true;
                    _isMockMode = false;
                }
                else
                {
                    _isConnected = true;
                    _isMockMode = true;
                    _sapVersion = "Mock-24.0.0";
                }

                int pid = 0;
                try
                {
                    var procs = Process.GetProcessesByName("SAP2000").OrderByDescending(p => p.StartTime);
                    pid = procs.FirstOrDefault()?.Id ?? 0;
                }
                catch { }

                var info = new SapInstanceInfo(string.Empty, string.Empty, pid, _sapVersion)
                {
                    IsNewInstance = true,
                    IsVisible = true,
                    WindowTitle = "SAP2000 (New)"
                };
                _currentSession = new SapSession(Guid.NewGuid(), Environment.MachineName, _sapVersion, info);
                ConnectionStateChanged?.Invoke(this, true);
                return _currentSession;
            }
            catch (Exception ex)
            {
                _isConnected = false;
                throw new InvalidOperationException($"Failed to create new SAP2000 instance: {ex.Message}", ex);
            }
        }

        // ─── Disconnect ─────────────────────────────────────────────────────────

        /// <summary>
        /// Disconnects from the current session without killing SAP2000.
        /// </summary>
        public void Disconnect()
        {
            DisconnectInternal();
            ConnectionStateChanged?.Invoke(this, false);
        }

        private void DisconnectInternal()
        {
            // Release COM references but do NOT call ApplicationExit
            try
            {
                if (_sapModel != null && !_isMockMode)
                {
                    try { Marshal.ReleaseComObject(_sapModel); } catch { }
                }
            }
            catch { }
            finally
            {
                if (_sapObject != null && !_isMockMode)
                {
                    try { Marshal.ReleaseComObject(_sapObject); } catch { }
                }
                _sapObject = null;
                _sapModel = null;
                _isConnected = false;
                _currentSession?.Disconnect();
                _currentSession = null;
            }
        }

        // ─── Switch ─────────────────────────────────────────────────────────────

        public SapSession SwitchToInstance(SapInstanceInfo instance)
        {
            DisconnectInternal();
            return AttachToInstance(instance);
        }

        // ─── Legacy Connect (kept for backward compat) ─────────────────────────

        public SapSession Connect(string sapProgramPath, bool attachToExisting)
        {
            if (attachToExisting)
            {
                var instances = DiscoverInstances();
                if (instances.Count > 0)
                    return AttachToInstance(instances[0]);
                // No running instance found – fall through to create
            }
            return CreateNewInstance();
        }

        // ─── Convenience helpers (delegated by SapAdapter) ─────────────────────

        public string GetSapVersion() => _sapVersion;

        public bool OpenModel(string path)
        {
            if (_sapModel == null) return false;
            try { return _sapModel.File.OpenFile(path) == 0; }
            catch { return false; }
        }

        public bool SaveModel()
        {
            if (_sapModel == null) return false;
            try { return _sapModel.File.Save() == 0; }
            catch { return false; }
        }

        public bool RunAnalysis()
        {
            if (_sapModel == null) return true; // mock success
            try { return _sapModel.Analyze.RunAnalysis() == 0; }
            catch { return false; }
        }

        public bool RunDesign()
        {
            if (_sapModel == null) return true; // mock success
            try
            {
                _sapModel.DesignConcrete.StartDesign();
                return true;
            }
            catch { return false; }
        }

        public bool SetModelLocked(bool locked)
        {
            if (_sapModel == null) return true;
            try { return _sapModel.SetModelIsLocked(locked) == 0; }
            catch { return false; }
        }

        public IEnumerable<string> GetStoryNames()
        {
            var result = new List<string>();
            if (_sapModel == null)
            {
                for (int i = 1; i <= 5; i++) result.Add($"PISO {i}");
                return result;
            }
            try
            {
                int num = 0;
                string[] names = null;
                double[] elevs = null, heights = null;
                bool[] isMasters = null;
                string[] masters = null;
                _sapModel.Story.GetStories(ref num, ref names, ref elevs, ref heights, ref isMasters, ref masters);
                if (names != null) result.AddRange(names);
            }
            catch { }
            return result;
        }

        public IEnumerable<string> GetFrameElementIds()
        {
            var result = new List<string>();
            if (_sapModel == null)
            {
                for (int i = 1; i <= 10; i++) result.Add($"B{i}");
                return result;
            }
            try
            {
                int num = 0;
                string[] names = null;
                _sapModel.FrameObj.GetNameList(ref num, ref names);
                if (names != null) result.AddRange(names);
            }
            catch { }
            return result;
        }

        public IEnumerable<string> GetAreaElementIds()
        {
            var result = new List<string>();
            if (_sapModel == null)
            {
                for (int i = 1; i <= 5; i++) result.Add($"W{i}");
                return result;
            }
            try
            {
                int num = 0;
                string[] names = null;
                _sapModel.AreaObj.GetNameList(ref num, ref names);
                if (names != null) result.AddRange(names);
            }
            catch { }
            return result;
        }

        // ─── Group operations ───────────────────────────────────────────────────

        public IEnumerable<string> GetGroupNames()
        {
            var result = new List<string>();
            if (_sapModel == null)
            {
                result.AddRange(new[] { "Vigas_P1", "Vigas_P2", "Columnas_P1", "Columnas_P2", "Muros_1_P1", "Losas_P1_1" });
                return result;
            }
            try
            {
                int num = 0;
                string[] names = null;
                _sapModel.GroupDef.GetNameList(ref num, ref names);
                if (names != null) result.AddRange(names);
            }
            catch { }
            return result;
        }

        public IEnumerable<string> GetGroupElements(string groupName)
        {
            var result = new List<string>();
            if (_sapModel == null)
            {
                for (int i = 1; i <= 3; i++) result.Add($"{groupName}_E{i}");
                return result;
            }
            try
            {
                int numObj = 0;
                int[] objectType = null;
                string[] objectName = null;
                _sapModel.GroupDef.GetAssignments(groupName, ref numObj, ref objectType, ref objectName);
                if (objectName != null) result.AddRange(objectName);
            }
            catch { }
            return result;
        }

        public IEnumerable<string> GetSelectedFrameIds()
        {
            var result = new List<string>();
            if (_sapModel == null)
            {
                result.AddRange(new[] { "B1", "B2" });
                return result;
            }
            try
            {
                int numItems = 0;
                int[] objectType = null;
                string[] objectName = null;
                _sapModel.SelectObj.GetSelected(ref numItems, ref objectType, ref objectName);
                if (objectName != null && objectType != null)
                {
                    for (int i = 0; i < numItems; i++)
                    {
                        // objectType 2 = Frame
                        if (objectType[i] == 2)
                            result.Add(objectName[i]);
                    }
                }
            }
            catch { }
            return result;
        }

        public IEnumerable<string> GetSelectedAreaIds()
        {
            var result = new List<string>();
            if (_sapModel == null)
            {
                result.AddRange(new[] { "A1", "A2" });
                return result;
            }
            try
            {
                int numItems = 0;
                int[] objectType = null;
                string[] objectName = null;
                _sapModel.SelectObj.GetSelected(ref numItems, ref objectType, ref objectName);
                if (objectName != null && objectType != null)
                {
                    for (int i = 0; i < numItems; i++)
                    {
                        // objectType 3 = Area
                        if (objectType[i] == 3)
                            result.Add(objectName[i]);
                    }
                }
            }
            catch { }
            return result;
        }

        // ─── Definitions ────────────────────────────────────────────────────────

        public bool DefineLoadPattern(string name, string patternType, double selfWeightMultiplier)
        {
            if (_sapModel == null) return true;
            try
            {
                int typeVal = ConvertLoadPatternType(patternType);
                return _sapModel.LoadPatterns.Add(name, typeVal, selfWeightMultiplier, true) == 0;
            }
            catch { return false; }
        }

        public bool DefineLoadCase(string name, string caseType, string analysisType)
        {
            if (_sapModel == null) return true;
            try { return _sapModel.LoadCases.StaticLinear.SetCase(name) == 0; }
            catch { return false; }
        }

        public bool DefineLoadCombination(string name, string combinationType,
            IEnumerable<(string caseName, double factor)> cases)
        {
            if (_sapModel == null) return true;
            try
            {
                _sapModel.RespCombo.Add(name, 0);
                foreach (var (caseName, factor) in cases)
                    _sapModel.RespCombo.SetCaseList(name, 0, caseName, factor);
                return true;
            }
            catch { return false; }
        }

        public bool DefineMassSource(string name, bool includeElements, bool includeAdditionalMasses)
        {
            if (_sapModel == null) return true;
            try { return _sapModel.PropMaterial.SetMassSource(name, includeElements, includeAdditionalMasses, false, 0, null, null) == 0; }
            catch { return false; }
        }

        public bool DefineResponseSpectrum(string name, double dampingRatio,
            IEnumerable<(double period, double accel)> points)
        {
            if (_sapModel == null) return true;
            try
            {
                var periods = new List<double>();
                var accels = new List<double>();
                foreach (var (p, a) in points) { periods.Add(p); accels.Add(a); }
                return _sapModel.Func.FuncRS.SetUser(name, periods.Count, periods.ToArray(), accels.ToArray(), dampingRatio) == 0;
            }
            catch { return false; }
        }

        public bool AssignDiaphragm(string storyName, string diaphragmName, bool isRigid)
        {
            if (_sapModel == null) return true;
            try { return _sapModel.Diaphragm.SetDiaphragm(diaphragmName, isRigid ? 1 : 2) == 0; }
            catch { return false; }
        }

        public bool DefineDiaphragmConstraint(string diaphragmName)
        {
            if (_sapModel == null) return true;
            try { return _sapModel.ConstraintDef.SetDiaphragm(diaphragmName, 0) == 0; }
            catch { return false; }
        }

        public bool AssignPointConstraint(string pointName, string constraintName)
        {
            if (_sapModel == null) return true;
            try { return _sapModel.PointObj.SetConstraint(pointName, constraintName) == 0; }
            catch { return false; }
        }

        private static int ConvertLoadPatternType(string type)
        {
            return type?.ToUpperInvariant() switch
            {
                "DEAD" => 1,
                "LIVE" => 3,
                "WIND" => 12,
                "SEISMIC" or "QUAKE" => 5,
                "SNOW" => 7,
                _ => 1
            };
        }
    }
}
