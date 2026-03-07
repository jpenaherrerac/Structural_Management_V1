#if SAP2000_AVAILABLE
using System;

namespace App.Infrastructure.Sap2000.Motores
{
    /// <summary>
    /// Crea el material y las propiedades de área (shells) necesarias para cimentaciones.
    /// Simplificado: 1 material (concreto fc=210 kgf/cm²), 3 propiedades:
    ///   - LOSA: espesor 0.70 m (losa general)
    ///   - PLATE: espesor 0.70 m (placas de cimentación)
    ///   - TRENCH: espesor 0.20 m (zanjas/trenches)
    /// 
    /// UNIDADES: El modelo SAP2000 está configurado en N, m, C.
    /// Por lo tanto:
    ///   - E (módulo de elasticidad) debe estar en N/m² = Pa
    ///   - fc (resistencia) debe estar en N/m² = Pa
    /// </summary>
    public sealed class MaterialAndPropertiesEngine
    {
        private readonly SapModelFacade _facade;

        public MaterialAndPropertiesEngine(SapModelFacade facade)
        {
            _facade = facade ?? throw new ArgumentNullException(nameof(facade));
        }

        /// <summary>
        /// Nombres de las propiedades shell creadas.
        /// </summary>
        public const string PropertyLosa = "LOSA";
        public const string PropertyPlate = "PLATE";
        public const string PropertyTrench = "TRENCH";

        /// <summary>
        /// Crea el material de concreto y las 3 propiedades shell estándar.
        /// </summary>
        /// <param name="fc_kgfcm2">Resistencia del concreto en kgf/cm² (default 210).</param>
        /// <param name="thicknessLosa">Espesor de losa en metros (default 0.70).</param>
        /// <param name="thicknessPlate">Espesor de plate en metros (default 0.70).</param>
        /// <param name="thicknessTrench">Espesor de trench en metros (default 0.20).</param>
        /// <returns>Nombre del material creado.</returns>
        public string Execute(
            double fc_kgfcm2 = 210.0,
            double thicknessLosa = 0.70,
            double thicknessPlate = 0.70,
            double thicknessTrench = 0.20)
        {
            // 1) Create concrete material
            // fc = 210 kgf/cm² ? 21 MPa
            string matName = $"CONC{(int)Math.Round(fc_kgfcm2)}";

            // Conversions for SAP2000 model in N, m, C:
            // 1 kgf/cm² = 0.0980665 MPa = 98066.5 Pa = 98066.5 N/m²
            // 1 MPa = 1,000,000 Pa = 1,000,000 N/m²
            // E (MPa) ? 4700 * sqrt(fc' MPa)
            
            double fc_MPa = fc_kgfcm2 * 0.0980665;
            double fc_Nm2 = fc_MPa * 1_000_000.0;  // Convert MPa to N/m² (Pa)
            
            // E = 4700 * sqrt(fc' MPa) in MPa, then convert to N/m²
            double E_MPa = 4700.0 * Math.Sqrt(Math.Max(0.0, fc_MPa));
            double E_Nm2 = E_MPa * 1_000_000.0;  // Convert MPa to N/m² (Pa)
            
            double nu = 0.20;  // Poisson ratio for concrete
            double alpha = 0.0000055;  // Thermal expansion coefficient (1/°C)

            // Create material
            _facade.PropMaterial_SetMaterial(matName, 2); // 2 = Concrete
            _facade.PropMaterial_SetMPIsotropic(matName, E_Nm2, nu, alpha);
            _facade.PropMaterial_SetOConcrete_1(
                matName,
                fc_Nm2,
                isLightweight: false,
                fcsFactor: 1.0,
                ssType: 2,       // Parametric - Mander
                ssHysType: 2,    // Takeda
                strainAtFc: 0.00192,
                strainUltimate: 0.005,
                finalSlope: -0.10);

            // 2) Create shell properties
            // shellType = 1 => Shell-Thin (membrane + bending)
            // matAngle = 0.0 => no rotation
            // thickMembrane = thickBending = thickness

            // LOSA - general slab
            _facade.PropArea_SetShell(PropertyLosa, 1, matName, 0.0, thicknessLosa, thicknessLosa);

            // PLATE - foundation plate zones
            _facade.PropArea_SetShell(PropertyPlate, 1, matName, 0.0, thicknessPlate, thicknessPlate);

            // TRENCH - trenches/zanjas
            _facade.PropArea_SetShell(PropertyTrench, 1, matName, 0.0, thicknessTrench, thicknessTrench);

            return matName;
        }

        /// <summary>
        /// Crea propiedades shell adicionales con espesores personalizados.
        /// </summary>
        public void CreateCustomProperty(string propName, string materialName, double thickness)
        {
            if (string.IsNullOrWhiteSpace(propName))
                throw new ArgumentNullException(nameof(propName));

            _facade.PropArea_SetShell(propName, 1, materialName, 0.0, thickness, thickness);
        }
    }
}
#else
namespace App.Infrastructure.Sap2000.Motores
{
    public sealed class MaterialAndPropertiesEngine
    {
   public const string PropertyLosa = "LOSA";
        public const string PropertyPlate = "PLATE";
  public const string PropertyTrench = "TRENCH";

        public MaterialAndPropertiesEngine(SapModelFacade facade) { }

        public string Execute(
  double fc_kgfcm2 = 210.0,
    double thicknessLosa = 0.70,
         double thicknessPlate = 0.70,
    double thicknessTrench = 0.20)
    => throw new System.NotSupportedException("SAP2000 not available.");

  public void CreateCustomProperty(string propName, string materialName, double thickness)
  => throw new System.NotSupportedException("SAP2000 not available.");
    }
}
#endif
