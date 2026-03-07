#if SAP2000_LEGACY
using System;
using SAP2000v1;
using App.Infrastructure.Sap2000;

namespace App.Infrastructure.Sap2000.Motores
{
    /// <summary>
    /// Asigna patrones de puntos y cargas a áreas/grupos.
    /// NOTA: Debe ejecutarse en STA.
    /// </summary>
    public sealed class PointsPatternsAssignLoadsEngine
    {
        private readonly SapModelFacade _facade;
        public PointsPatternsAssignLoadsEngine(SapModelFacade facade)
        {
            _facade = facade ?? throw new ArgumentNullException(nameof(facade));
        }

        public void Assign(double alt_liquido, double Rho_liqu_1, double PTS_base, double PT_base, double H, double h_SAL, double mo_Gamma, double t_TOP, double carga_acabado, double carga_viva)
        {
            var m = _facade.Raw;
            int restriction = 2;
            m.PointObj.SetPatternByXYZ("ALL", "presionagua", 0.0, 0.0, 1, -1 * alt_liquido, eItemType.Group, restriction, false);
            m.AreaObj.SetLoadSurfacePressure("SUMERGIDO", "CL", -1, (-1 * Rho_liqu_1), "presionagua", false, eItemType.Group);
            m.SelectObj.Group("All", true);
            m.SelectObj.PropertyArea("BASE", false);
            m.AreaObj.SetLoadSurfacePressure("", "CL", -1, Rho_liqu_1, "presionagua", false, eItemType.SelectedObjects);
            m.SelectObj.Group("All", true);
            int restriction2 = 1;
            m.PointObj.SetPatternByXYZ("ALL", "PresionSismo", 0.0, 0.0, 1, 0, eItemType.Group, restriction2, false);
            m.AreaObj.SetLoadSurfacePressure("CaraparasismoY", "ETsy", -2, PTS_base, "PresionSismo", false, eItemType.Group);
            m.AreaObj.SetLoadSurfacePressure("CaraparasismoX", "ETsx", -2, PTS_base, "PresionSismo", false, eItemType.Group);
            m.SelectObj.Group("All", true);
            int restriction4 = 2;
            m.PointObj.SetPatternByXYZ("ALL", "PresionSuelo", 0.0, 0.0, 1, -1 * (H + h_SAL), eItemType.Group, restriction4, false);
            m.AreaObj.SetLoadSurfacePressure("SUMERGIDO", "ET", -2, -1 * PT_base, "PresionSuelo", false, eItemType.Group);
            m.SelectObj.Group("All", true);
            m.SelectObj.PropertyArea("TOP", false);
            m.AreaObj.SetLoadUniform("", "CM", mo_Gamma * (h_SAL - 0.5 * t_TOP), 10, false, "Global", eItemType.SelectedObjects);
            m.SelectObj.Group("All", true);
            m.SelectObj.PropertyArea("TOP", false);
            m.AreaObj.SetLoadUniform("", "CM", carga_acabado, 10, false, "Global", eItemType.SelectedObjects);
            m.SelectObj.Group("All", true);
            m.SelectObj.PropertyArea("TOP", false);
            m.AreaObj.SetLoadUniform("", "CM", carga_viva, 10, false, "Global", eItemType.SelectedObjects);
            m.SelectObj.ClearSelection();
        }
    }
}
#endif
