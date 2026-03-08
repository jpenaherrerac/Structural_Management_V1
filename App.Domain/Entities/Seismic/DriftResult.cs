namespace App.Domain.Entities.Seismic
{
    public class DriftResult
    {
        /// <summary>Default inelastic multiplier (0.75 × R) for backward compat.</summary>
        private const double DefaultInelasticMultiplier = 0.75;

        /// <summary>Default allowable drift per E.030-2018 for concrete.</summary>
        private const double DefaultDriftLimit = 0.007;

        public string StoryName { get; set; }
        public string LoadCase { get; set; }
        public double DriftX { get; set; }
        public double DriftY { get; set; }
        public double DisplacementX { get; set; }
        public double DisplacementY { get; set; }
        public double StoryHeightMeters { get; set; }

        /// <summary>Control point name (center-of-mass node for the diaphragm).</summary>
        public string ControlPoint { get; set; }

        /// <summary>Seismic reduction factor R applied to convert elastic→inelastic.</summary>
        public double ReductionFactorR { get; set; } = 6.0;

        /// <summary>Allowable drift limit (Δ/h) from code.</summary>
        public double AllowableDriftLimit { get; set; } = DefaultDriftLimit;

        // Elastic drift accessors
        public double ElasticDriftX => DriftX;
        public double ElasticDriftY => DriftY;

        // Inelastic drift = elastic drift × R  (per E.030 simplified)
        public double InelasticDriftX => DriftX * DefaultInelasticMultiplier * ReductionFactorR;
        public double InelasticDriftY => DriftY * DefaultInelasticMultiplier * ReductionFactorR;

        public bool ExceedsLimitX => InelasticDriftX > AllowableDriftLimit;
        public bool ExceedsLimitY => InelasticDriftY > AllowableDriftLimit;

        public DriftResult() { }

        public DriftResult(string storyName, string loadCase, double driftX, double driftY)
        {
            StoryName = storyName;
            LoadCase = loadCase;
            DriftX = driftX;
            DriftY = driftY;
        }
    }
}
