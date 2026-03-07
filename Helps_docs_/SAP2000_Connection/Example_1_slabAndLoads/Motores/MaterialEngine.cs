#if SAP2000_LEGACY
using System;
using System.Collections.Generic;
using SAP2000v1;
using App.Infrastructure.Sap2000;

namespace App.Infrastructure.Sap2000.Motores
{
    public sealed class MaterialEngine
    {
        private readonly SapModelFacade _facade;
        private readonly Dictionary<string, MaterialDescriptor> _materials = new Dictionary<string, MaterialDescriptor>(StringComparer.OrdinalIgnoreCase);

        public MaterialEngine(SapModelFacade facade) { _facade = facade ?? throw new ArgumentNullException(nameof(facade)); }
        public IReadOnlyDictionary<string, MaterialDescriptor> Materials => _materials;

        public sealed class MaterialsDto { public UnitsDto Units { get; set; } public List<MaterialSpec> Materials { get; set; } = new List<MaterialSpec>(); }
        public sealed class UnitsDto { public string Stress { get; set; } public string Temp { get; set; } }
        public sealed class IsotropicDto { public double Nu { get; set; } public double Alpha { get; set; } }
        public sealed class ConcreteDto { public double fc { get; set; } public string fc_Units { get; set; } public bool ComputeEFromFc { get; set; } public bool IsLightweight { get; set; } public double fcsfactor { get; set; } public int SSType { get; set; } public int SSHysType { get; set; } public double StrainAtfc { get; set; } public double StrainUltimate { get; set; } public double FinalSlope { get; set; } }
        public sealed class MaterialSpec { public string Name { get; set; } public int Type { get; set; } public IsotropicDto Isotropic { get; set; } public ConcreteDto Concrete { get; set; } }

        public void Apply(MaterialsDto dto)
        {
            if (dto == null || dto.Materials == null || dto.Materials.Count == 0) return;
            var m = _facade.Raw;

            // Set model units to kN-m-C to match cassette defaults
            try { m.SetPresentUnits(eUnits.kN_m_C); } catch { }

            // Fetch existing material names once
            string[] existingNames = null; int count = 0;
            try { m.PropMaterial.GetNameList(ref count, ref existingNames); } catch { existingNames = new string[0]; }

            foreach (var mat in dto.Materials)
            {
                if (mat == null || string.IsNullOrWhiteSpace(mat.Name)) continue;
                var type = (eMatType)mat.Type;
                bool exists = existingNames != null && Array.Exists(existingNames, n => string.Equals(n, mat.Name, StringComparison.OrdinalIgnoreCase));

                if (!exists)
                {
                    int retMat = m.PropMaterial.SetMaterial(mat.Name, type, -1, string.Empty, string.Empty);
                    if (retMat != 0)
                    {
                        // Refresh list and re-check
                        existingNames = null; count = 0; try { m.PropMaterial.GetNameList(ref count, ref existingNames); } catch { }
                        exists = existingNames != null && Array.Exists(existingNames, n => string.Equals(n, mat.Name, StringComparison.OrdinalIgnoreCase));
                        if (!exists)
                        {
                            // Try alternate name with suffix
                            string alt = mat.Name + "_1";
                            try
                            {
                                int rAlt = m.PropMaterial.SetMaterial(alt, type, -1, string.Empty, string.Empty);
                                if (rAlt == 0) mat.Name = alt;
                            }
                            catch { }
                        }
                    }
                }

                // Proceed to set isotropic/concrete regardless of creation path
                double E_kN_m2 = 0.0;
                double nu = mat.Isotropic?.Nu ?? 0.2;
                double alpha = mat.Isotropic?.Alpha ?? 5.5e-6;

                if (type == eMatType.Concrete && mat.Concrete != null)
                {
                    double fc_kNm2 = ConvertFcTo_kNm2(mat.Concrete.fc, mat.Concrete.fc_Units);
                    if (mat.Concrete.ComputeEFromFc)
                    {
                        double fc_MPa = fc_kNm2 / 1000.0;
                        E_kN_m2 = 4700.0 * Math.Sqrt(Math.Max(0.0, fc_MPa)) * 1000.0;
                    }

                    int retIso = m.PropMaterial.SetMPIsotropic(mat.Name, E_kN_m2, nu, alpha);
                    _facade.Check(retIso, "PropMaterial.SetMPIsotropic(" + mat.Name + ")");

                    int retConc = m.PropMaterial.SetOConcrete_1(mat.Name, fc_kNm2, mat.Concrete.IsLightweight, mat.Concrete.fcsfactor, mat.Concrete.SSType, mat.Concrete.SSHysType, mat.Concrete.StrainAtfc, mat.Concrete.StrainUltimate, mat.Concrete.FinalSlope);
                    _facade.Check(retConc, "PropMaterial.SetOConcrete_1(" + mat.Name + ")");

                    _materials[mat.Name] = new MaterialDescriptor { Name = mat.Name, Type = type, E_kN_m2 = E_kN_m2, Nu = nu, Alpha = alpha, Fc_MPa = fc_kNm2 / 1000.0 };
                }
                else
                {
                    int retIso = m.PropMaterial.SetMPIsotropic(mat.Name, E_kN_m2, nu, alpha);
                    _facade.Check(retIso, "PropMaterial.SetMPIsotropic(" + mat.Name + ")");
                    _materials[mat.Name] = new MaterialDescriptor { Name = mat.Name, Type = type, E_kN_m2 = E_kN_m2, Nu = nu, Alpha = alpha };
                }
            }
        }

        private static double ConvertFcTo_kNm2(double fc, string units)
        {
            if (string.Equals(units, "kgf/cm2", StringComparison.OrdinalIgnoreCase)) { double fc_MPa = fc * 0.0980665; return fc_MPa * 1000.0; }
            if (string.Equals(units, "MPa", StringComparison.OrdinalIgnoreCase)) return fc * 1000.0;
            if (string.Equals(units, "kN/m2", StringComparison.OrdinalIgnoreCase)) return fc;
            return fc * 1000.0;
        }

        public sealed class MaterialDescriptor { public string Name { get; set; } public eMatType Type { get; set; } public double E_kN_m2 { get; set; } public double Nu { get; set; } public double Alpha { get; set; } public double? Fc_MPa { get; set; } }
    }
}
#endif
