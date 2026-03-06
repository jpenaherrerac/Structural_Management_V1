namespace App.Domain.Entities.Seismic
{
    public class StoryResult
    {
        public string StoryName { get; set; }
        public int StoryLevel { get; set; }
        public double ElevationMeters { get; set; }
        public double HeightMeters { get; set; }
        public double ShearX { get; set; }
        public double ShearY { get; set; }
        public double MomentX { get; set; }
        public double MomentY { get; set; }
        public double WeightKN { get; set; }
        public string LoadCase { get; set; }

        public StoryResult() { }

        public StoryResult(string storyName, int level, double elevation)
        {
            StoryName = storyName;
            StoryLevel = level;
            ElevationMeters = elevation;
        }
    }
}
