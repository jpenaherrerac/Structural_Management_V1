using System;

namespace App.Domain.Entities.Seismic
{
    public class MassSummary
    {
        public string StoryName { get; set; }
        public double MassX { get; set; }
        public double MassY { get; set; }
        public double MassZ { get; set; }
        public double CenterOfMassX { get; set; }
        public double CenterOfMassY { get; set; }
        public double MassRotationalZ { get; set; }

        public MassSummary() { }

        public MassSummary(string storyName, double massX, double massY)
        {
            StoryName = storyName ?? throw new ArgumentNullException(nameof(storyName));
            MassX = massX;
            MassY = massY;
            MassZ = 0;
        }
    }
}
