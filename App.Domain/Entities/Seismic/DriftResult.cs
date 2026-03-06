namespace App.Domain.Entities.Seismic
{
    public class DriftResult
    {
        public string StoryName { get; set; }
        public string LoadCase { get; set; }
        public double DriftX { get; set; }
        public double DriftY { get; set; }
        public double DisplacementX { get; set; }
        public double DisplacementY { get; set; }
        public double StoryHeightMeters { get; set; }
        public double InelasticDriftX => DriftX * 0.75;
        public double InelasticDriftY => DriftY * 0.75;
        public bool ExceedsLimitX => InelasticDriftX > 0.007;
        public bool ExceedsLimitY => InelasticDriftY > 0.007;

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
