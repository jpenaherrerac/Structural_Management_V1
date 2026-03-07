#if SAP2000_LEGACY
using System;
using SAP2000v1;
using App.Infrastructure.Sap2000;

namespace App.Infrastructure.Sap2000.Motores
{
    /// <summary>
    /// Crea patrones de carga (cassette-driven).
    /// NOTA: Debe ejecutarse en STA.
    /// </summary>
    public sealed class LoadPatternEngine
    {
        private readonly SapModelFacade _facade;

        public LoadPatternEngine(SapModelFacade facade)
        {
            _facade = facade ?? throw new ArgumentNullException(nameof(facade));
        }

        public sealed class PatternSpec
        {
            public string Name { get; set; }
            public int Type { get; set; }
            public double SelfWtMult { get; set; }
            public bool AddAnalysisCase { get; set; }
        }

        public sealed class PatternsSpec
        {
            public PatternSpec[] Items { get; set; } = Array.Empty<PatternSpec>();
        }

        public void Apply(PatternsSpec spec)
        {
            if (spec == null || spec.Items == null || spec.Items.Length == 0) return;

            foreach (var p in spec.Items)
            {
                if (p == null || string.IsNullOrWhiteSpace(p.Name)) continue;
                _facade.LoadPatterns_Add(p.Name, (eLoadPatternType)p.Type, p.SelfWtMult, p.AddAnalysisCase);
            }
        }
    }
}
#endif
