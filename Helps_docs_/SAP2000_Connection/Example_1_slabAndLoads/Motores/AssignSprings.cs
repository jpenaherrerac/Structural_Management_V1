#if SAP2000_AVAILABLE
using System;
using SAP2000v1;

namespace App.Infrastructure.Sap2000.Motores
{
    /// <summary>
    /// Asigna resortes (balasto) a áreas.
    /// Implementación alineada al flujo del repo: seleccionar áreas y aplicar AreaObj.SetSpring con itemType=SelectedObjects.
    /// </summary>
    internal sealed class AssignSpringsEngine
    {
        private readonly SapModelFacade _facade;

        public AssignSpringsEngine(SapModelFacade facade)
        {
            _facade = facade ?? throw new ArgumentNullException(nameof(facade));
        }

        /// <summary>
        /// Asigna balasto (resortes) a todas las áreas del modelo.
        /// Nota: requiere que existan áreas previamente.
        /// </summary>
        /// <param name="coefBalasto">Coeficiente de balasto (kN/m/m^2) en unidades consistentes con el modelo SAP.</param>
        public void Execute(double coefBalasto)
        {
            // Basado en el flujo de referencia (TANK):
            // myType=1, simpleSpringType=2, face=-1, springLocalOneType=2, dir=0, outward=false, replace=true, cSys="Local"
            // itemType = SelectedObjects (para aplicar al set seleccionado)

            _facade.SelectObj_ClearSelection();
            _facade.SelectObj_All(false);

            double[] vec = new double[] { 0.0, 0.0, 0.0 };
            int myType = 1;
            double s = coefBalasto;
            int simpleSpringType = 2;
            string linkProp = string.Empty;
            int face = -1;
            int springLocalOneType = 2;
            int dir = 0;
            bool outward = false;
            double ang = 0.0;
            bool replace = true;
            string cSys = "Local";
            eItemType itemType = eItemType.SelectedObjects;

            _facade.AreaObj_SetSpring(
                name: string.Empty,
                myType: myType,
                s: s,
                simpleSpringType: simpleSpringType,
                linkProp: linkProp,
                face: face,
                springLocalOneType: springLocalOneType,
                dir: dir,
                outward: outward,
                ref vec,
                ang: ang,
                replace: replace,
                cSys: cSys,
                itemType: itemType);

            _facade.SelectObj_ClearSelection();
        }
    }
}
#else
namespace App.Infrastructure.Sap2000.Motores
{
    internal sealed class AssignSpringsEngine
    {
        public AssignSpringsEngine(SapModelFacade facade) { }
        public void Execute(double coefBalasto) => throw new System.NotSupportedException("SAP2000 not available.");
    }
}
#endif
