using System;
using System.Collections.Generic;

namespace App.SAP2000.Adapters
{
    /// <summary>
    /// Centralizes all SAP2000 OAPI calls behind a thin, strongly-named API.
    /// Wraps the <c>dynamic</c> COM model so the rest of the adapter layer
    /// never needs to touch raw COM.
    /// In mock mode (SapModel == null) every mutating method returns success
    /// and every query returns plausible stub data.
    /// </summary>
    public sealed class SapModelFacade
    {
        private readonly SapConnectionService _conn;
        private dynamic? Model => _conn.SapModel;

        public SapModelFacade(SapConnectionService conn)
        {
            _conn = conn ?? throw new ArgumentNullException(nameof(conn));
        }

        // ── Helpers ─────────────────────────────────────────────────────────────

        private void Check(int ret, string where)
        {
            if (ret != 0)
                throw new InvalidOperationException($"{where} returned code {ret}");
        }

        // ── Load Patterns ───────────────────────────────────────────────────────

        public bool LoadPatterns_Add(string name, int type, double selfWeightMultiplier = 0, bool addCase = true)
        {
            if (Model == null) return true;
            try { return Model.LoadPatterns.Add(name, type, selfWeightMultiplier, addCase) == 0; }
            catch { return false; }
        }

        // ── Load Cases ──────────────────────────────────────────────────────────

        public bool LoadCases_StaticLinear_SetCase(string name)
        {
            if (Model == null) return true;
            try { return Model.LoadCases.StaticLinear.SetCase(name) == 0; }
            catch { return false; }
        }

        public bool LoadCases_ResponseSpectrum_SetCase(string name)
        {
            if (Model == null) return true;
            try { return Model.LoadCases.ResponseSpectrum.SetCase(name) == 0; }
            catch { return false; }
        }

        public bool LoadCases_ResponseSpectrum_SetDirComb(string name, int dirComb)
        {
            if (Model == null) return true;
            try { return Model.LoadCases.ResponseSpectrum.SetDirComb(name, dirComb) == 0; }
            catch { return false; }
        }

        public bool LoadCases_ResponseSpectrum_SetEccentricityOverride(string name, int diaphragm, double ecc,
            bool overrideActive)
        {
            if (Model == null) return true;
            try
            {
                return Model.LoadCases.ResponseSpectrum.SetDiaphragmEccentricityOverride(
                    name, diaphragm, ecc, overrideActive) == 0;
            }
            catch { return false; }
        }

        // ── Load Combinations ───────────────────────────────────────────────────

        public bool RespCombo_Add(string name, int comboType = 0)
        {
            if (Model == null) return true;
            try { return Model.RespCombo.Add(name, comboType) == 0; }
            catch { return false; }
        }

        public bool RespCombo_SetCaseList(string comboName, int caseType, string caseName, double factor)
        {
            if (Model == null) return true;
            try { return Model.RespCombo.SetCaseList(comboName, caseType, caseName, factor) == 0; }
            catch { return false; }
        }

        // ── Functions / Response Spectrum ────────────────────────────────────────

        public bool Func_FuncRS_SetUser(string name, IReadOnlyList<double> periods,
            IReadOnlyList<double> accels, double dampingRatio)
        {
            if (Model == null) return true;
            try
            {
                var pArr = new double[periods.Count];
                var aArr = new double[accels.Count];
                for (int i = 0; i < periods.Count; i++) pArr[i] = periods[i];
                for (int i = 0; i < accels.Count; i++) aArr[i] = accels[i];
                return Model.Func.FuncRS.SetUser(name, pArr.Length, pArr, aArr, dampingRatio) == 0;
            }
            catch { return false; }
        }

        // ── Mass Source ─────────────────────────────────────────────────────────

        public bool MassSource_SetDefault(bool includeElements, bool includeAdditionalMasses,
            bool includeLoads, int numberLoads, string[] loadPatterns, double[] scaleFactors)
        {
            if (Model == null) return true;
            try
            {
                return Model.PropMaterial.SetMassSource_1(
                    includeElements, includeAdditionalMasses, includeLoads,
                    numberLoads, loadPatterns, scaleFactors) == 0;
            }
            catch { return false; }
        }

        // ── Constraints / Diaphragms ────────────────────────────────────────────

        public bool ConstraintDef_SetDiaphragm(string name, int axis = 0)
        {
            if (Model == null) return true;
            try { return Model.ConstraintDef.SetDiaphragm(name, axis) == 0; }
            catch { return false; }
        }

        public bool PointObj_SetConstraint(string pointName, string constraintName)
        {
            if (Model == null) return true;
            try { return Model.PointObj.SetConstraint(pointName, constraintName, 0) == 0; }
            catch { return false; }
        }

        // ── Section Cuts ────────────────────────────────────────────────────────

        public bool SectCut_SetByQuad(string name, string group,
            double x1, double y1, double z1,
            double x2, double y2, double z2,
            double x3, double y3, double z3,
            double x4, double y4, double z4)
        {
            if (Model == null) return true;
            try
            {
                return Model.SectCut.SetByQuad(name, group,
                    x1, y1, z1, x2, y2, z2,
                    x3, y3, z3, x4, y4, z4) == 0;
            }
            catch { return false; }
        }

        // ── Groups ──────────────────────────────────────────────────────────────

        public IReadOnlyList<string> GroupDef_GetNameList()
        {
            var result = new List<string>();
            if (Model == null)
            {
                result.AddRange(new[] { "Vigas_P1", "Vigas_P2", "Columnas_P1", "Columnas_P2" });
                return result;
            }
            try
            {
                int num = 0;
                string[] names = null;
                Model.GroupDef.GetNameList(ref num, ref names);
                if (names != null) result.AddRange(names);
            }
            catch { }
            return result;
        }

        public IReadOnlyList<string> GroupDef_GetAssignments(string groupName)
        {
            var result = new List<string>();
            if (Model == null)
            {
                for (int i = 1; i <= 3; i++) result.Add($"{groupName}_E{i}");
                return result;
            }
            try
            {
                int numObj = 0;
                int[] objectType = null;
                string[] objectName = null;
                Model.GroupDef.GetAssignments(groupName, ref numObj, ref objectType, ref objectName);
                if (objectName != null) result.AddRange(objectName);
            }
            catch { }
            return result;
        }

        // ── Selection ───────────────────────────────────────────────────────────

        /// <summary>
        /// Returns names of selected objects filtered by SAP type code.
        /// SAP2000 object type codes: 1=Point, 2=Frame, 3=Area, 4=Solid, 5=Link.
        /// </summary>
        public IReadOnlyList<string> SelectObj_GetSelectedByType(int sapObjectType)
        {
            var result = new List<string>();
            if (Model == null)
            {
                result.Add($"SEL{sapObjectType}_1");
                result.Add($"SEL{sapObjectType}_2");
                return result;
            }
            try
            {
                int numItems = 0;
                int[] objectType = null;
                string[] objectName = null;
                Model.SelectObj.GetSelected(ref numItems, ref objectType, ref objectName);
                if (objectName != null && objectType != null)
                {
                    for (int i = 0; i < numItems; i++)
                    {
                        if (objectType[i] == sapObjectType)
                            result.Add(objectName[i]);
                    }
                }
            }
            catch { }
            return result;
        }

        // ── Analysis ────────────────────────────────────────────────────────────

        public bool Analyze_RunAnalysis()
        {
            if (Model == null) return true;
            try { return Model.Analyze.RunAnalysis() == 0; }
            catch { return false; }
        }

        // ── Results: Section Cut ────────────────────────────────────────────────

        /// <summary>
        /// Gets section-cut analysis results. Returns arrays of force/moment data.
        /// </summary>
        public bool Results_SectionCutAnalysis(string sectionCutName,
            out int numResults, out double[] f1, out double[] f2, out double[] f3,
            out double[] m1, out double[] m2, out double[] m3)
        {
            numResults = 0; f1 = f2 = f3 = m1 = m2 = m3 = Array.Empty<double>();
            if (Model == null)
            {
                numResults = 1;
                f1 = new[] { 850.0 };
                f2 = new[] { 820.0 };
                f3 = new[] { 0.0 };
                m1 = m2 = m3 = new[] { 0.0 };
                return true;
            }
            try
            {
                // Set up results for the section cut name
                Model.Results.Setup.DeselectAllCasesAndCombosForOutput();
                string[] loadCases = null, stepTypes = null;
                double[] stepNums = null;
                double[] _f1 = null, _f2 = null, _f3 = null, _m1 = null, _m2 = null, _m3 = null;
                int ret = Model.Results.SectionCutAnalysis(sectionCutName,
                    ref numResults, ref loadCases, ref stepTypes, ref stepNums,
                    ref _f1, ref _f2, ref _f3, ref _m1, ref _m2, ref _m3);
                if (ret == 0 && _f1 != null)
                {
                    f1 = _f1; f2 = _f2; f3 = _f3;
                    m1 = _m1; m2 = _m2; m3 = _m3;
                    return true;
                }
            }
            catch { }
            return false;
        }

        // ── Results: Modal ──────────────────────────────────────────────────────

        public bool Results_ModalParticipatingMassRatios(
            out int numResults, out double[] periods, out double[] ux, out double[] uy,
            out double[] sumUx, out double[] sumUy)
        {
            numResults = 0;
            periods = ux = uy = sumUx = sumUy = Array.Empty<double>();
            if (Model == null)
            {
                numResults = 6;
                periods = new[] { 0.85, 0.82, 0.41, 0.38, 0.26, 0.24 };
                ux = new[] { 0.72, 0.02, 0.13, 0.01, 0.05, 0.01 };
                uy = new[] { 0.02, 0.73, 0.01, 0.14, 0.01, 0.04 };
                sumUx = new double[6]; sumUy = new double[6];
                double cx = 0, cy = 0;
                for (int i = 0; i < 6; i++) { cx += ux[i]; cy += uy[i]; sumUx[i] = cx; sumUy[i] = cy; }
                return true;
            }
            try
            {
                string[] loadCases = null, stepTypes = null;
                double[] stepNums = null, _period = null, _ux = null, _uy = null, _uz = null,
                    _sumUx = null, _sumUy = null, _sumUz = null,
                    _rx = null, _ry = null, _rz = null,
                    _sumRx = null, _sumRy = null, _sumRz = null;
                int ret = Model.Results.ModalParticipatingMassRatios(
                    ref numResults, ref loadCases, ref stepTypes, ref stepNums,
                    ref _period, ref _ux, ref _uy, ref _uz,
                    ref _sumUx, ref _sumUy, ref _sumUz,
                    ref _rx, ref _ry, ref _rz,
                    ref _sumRx, ref _sumRy, ref _sumRz);
                if (ret == 0 && _period != null)
                {
                    periods = _period; ux = _ux; uy = _uy;
                    sumUx = _sumUx; sumUy = _sumUy;
                    return true;
                }
            }
            catch { }
            return false;
        }

        // ── Results: Joint Displacement ─────────────────────────────────────────

        public bool Results_JointDisplacement(string pointName, string loadCase,
            out double u1, out double u2, out double u3)
        {
            u1 = u2 = u3 = 0;
            if (Model == null) { u1 = 0.005; u2 = 0.004; return true; }
            try
            {
                int numResults = 0;
                string[] obj = null, elm = null, loadCases = null, stepTypes = null;
                double[] stepNums = null, _u1 = null, _u2 = null, _u3 = null,
                    _r1 = null, _r2 = null, _r3 = null;
                Model.Results.Setup.SetCaseSelectedForOutput(loadCase);
                int ret = Model.Results.JointDispl(pointName, 0,
                    ref numResults, ref obj, ref elm, ref loadCases,
                    ref stepTypes, ref stepNums,
                    ref _u1, ref _u2, ref _u3, ref _r1, ref _r2, ref _r3);
                if (ret == 0 && numResults > 0 && _u1 != null)
                {
                    u1 = _u1[0]; u2 = _u2[0]; u3 = _u3[0];
                    return true;
                }
            }
            catch { }
            return false;
        }
    }
}
