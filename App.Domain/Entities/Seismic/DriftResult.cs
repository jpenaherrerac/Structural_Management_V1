namespace App.Domain.Entities.Seismic
{
    public class DriftResult
    {
        /// <summary>NEC-SE-DS / E030: inelastic drift = elastic drift × 0.75 × R / I (simplified to 0.75 for R=6, I=1).</summary>
        private const double InelasticDriftMultiplier = 0.75;

        /// <summary>NEC-SE-DS Table 6: maximum allowable inelastic inter-story drift = 0.007 for concrete buildings.</summary>
        private const double DriftLimit = 0.007;

        public string StoryName { get; set; }
        public string LoadCase { get; set; }
        public double DriftX { get; set; }
        public double DriftY { get; set; }
        public double DisplacementX { get; set; }
        public double DisplacementY { get; set; }
        public double StoryHeightMeters { get; set; }
        public double InelasticDriftX => DriftX * InelasticDriftMultiplier;
        public double InelasticDriftY => DriftY * InelasticDriftMultiplier;
        public bool ExceedsLimitX => InelasticDriftX > DriftLimit;
        public bool ExceedsLimitY => InelasticDriftY > DriftLimit;

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
