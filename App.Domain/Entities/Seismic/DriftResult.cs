namespace App.Domain.Entities.Seismic
{
    public class DriftResult
    {
        /// <summary>E.030-2018 factor (0.75) for converting elastic → inelastic drift.</summary>
        private const double E030Factor = 0.75;

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

        // Inelastic drift = elastic drift × 0.75 × R  (per E.030-2018)
        public double InelasticDriftX => DriftX * E030Factor * ReductionFactorR;
        public double InelasticDriftY => DriftY * E030Factor * ReductionFactorR;

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
