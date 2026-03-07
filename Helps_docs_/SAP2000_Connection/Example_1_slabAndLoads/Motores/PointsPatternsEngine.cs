#if SAP2000_LEGACY
using System;
using SAP2000v1;
using App.Infrastructure.Sap2000;

namespace App.Infrastructure.Sap2000.Motores
{
    /// <summary>
    /// Define patrones de puntos (Joint Patterns) necesarios para asignaciones de carga.
    /// </summary>
    public sealed class PointsPatternsEngine
    {
        private readonly SapModelFacade _facade;
        public PointsPatternsEngine(SapModelFacade facade)
        {
            _facade = facade ?? throw new ArgumentNullException(nameof(facade));
        }

        public sealed class PointPatternSpec
        {
            public string Name { get; set; }
            public int Restriction { get; set; }
            public double Magnitude { get; set; }
        }

        public sealed class PointPatternsSpec
        {
            public PointPatternSpec[] Items { get; set; } = Array.Empty<PointPatternSpec>();
        }

        public void Apply(PointPatternsSpec spec)
        {
            if (spec == null || spec.Items == null || spec.Items.Length == 0) return;
            var m = _facade.Raw;

            foreach (var p in spec.Items)
            {
                if (p == null || string.IsNullOrWhiteSpace(p.Name)) continue;

                int ret = m.PointObj.SetPatternByXYZ("ALL", p.Name, 0.0, 0.0, 1, p.Magnitude, eItemType.Group, p.Restriction, false);
                _facade.Check(ret, "PointObj.SetPatternByXYZ(" + p.Name + ")");
            }
        }
    }
}
#endif
