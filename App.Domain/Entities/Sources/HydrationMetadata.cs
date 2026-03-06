using System;

namespace App.Domain.Entities.Sources
{
    public class HydrationMetadata
    {
        public string SapVersion { get; set; }
        public string ModelChecksum { get; set; }
        public int NumberOfStories { get; set; }
        public int NumberOfFrames { get; set; }
        public int NumberOfAreas { get; set; }
        public double TotalWeightKN { get; set; }
        public DateTime RunCompletedAt { get; set; }
        public bool AnalysisConverged { get; set; }
        public string AnalysisEngine { get; set; }
        public int NumberOfModes { get; set; }

        public HydrationMetadata() { }

        public HydrationMetadata(string sapVersion, string modelChecksum)
        {
            SapVersion = sapVersion;
            ModelChecksum = modelChecksum;
            RunCompletedAt = DateTime.UtcNow;
        }
    }
}
