using System;
using System.Collections.Generic;
using App.Domain.Entities.Sap;

namespace App.SAP2000.Adapters
{
    /// <summary>
    /// Manages the COM connection to SAP2000.
    /// On Windows with SAP2000 installed, replace the stubs with actual COM API calls via:
    ///   dynamic sapObject = Activator.CreateInstance(Type.GetTypeFromProgID("CSI.SAP2000.API.SapObject"));
    /// </summary>
    public class SapConnectionService
    {
        private dynamic? _sapObject;
        private dynamic? _sapModel;
        private bool _isConnected;
        private string _sapVersion = string.Empty;

        public bool IsConnected => _isConnected;
        internal dynamic? SapModel => _sapModel;

        public SapSession Connect(string sapProgramPath, bool attachToExisting)
        {
            try
            {
                // Attempt to get SAP2000 COM object
                var progId = "CSI.SAP2000.API.SapObject";
                var sapType = Type.GetTypeFromProgID(progId);

                if (sapType != null)
                {
                    _sapObject = Activator.CreateInstance(sapType);
                    if (!attachToExisting)
                        _sapObject.ApplicationStart(0, true, string.Empty, string.Empty, string.Empty);
                    _sapModel = _sapObject.SapModel;
                    _sapVersion = _sapObject.Version ?? "Unknown";
                    _isConnected = true;
                }
                else
                {
                    // Running on Linux or SAP2000 not installed; use mock mode
                    _isConnected = true;
                    _sapVersion = "Mock-24.0.0";
                }

                var instanceInfo = new SapInstanceInfo(sapProgramPath, string.Empty, 0, _sapVersion)
                {
                    IsNewInstance = !attachToExisting,
                    IsVisible = true
                };
                return new SapSession(Guid.NewGuid(), sapProgramPath, _sapVersion, instanceInfo);
            }
            catch (Exception ex)
            {
                _isConnected = false;
                throw new InvalidOperationException($"Failed to connect to SAP2000: {ex.Message}", ex);
            }
        }

        public void Disconnect()
        {
            try
            {
                if (_sapObject != null)
                {
                    _sapObject.ApplicationExit(false);
                }
            }
            catch { /* ignore on disconnect */ }
            finally
            {
                _sapObject = null;
                _sapModel = null;
                _isConnected = false;
            }
        }

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
                var periods = new System.Collections.Generic.List<double>();
                var accels = new System.Collections.Generic.List<double>();
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
