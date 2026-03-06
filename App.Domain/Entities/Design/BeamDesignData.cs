using System;

namespace App.Domain.Entities.Design
{
    public class BeamDesignData
    {
        public Guid Id { get; private set; }
        public string ElementId { get; set; }
        public string StoryName { get; set; }
        public string SectionName { get; set; }
        public double LengthMeters { get; set; }
        public double WidthMm { get; set; }
        public double DepthMm { get; set; }
        public double CoverMm { get; set; }
        public double Fc { get; set; }
        public double Fy { get; set; }
        public double MuPositiveKNm { get; set; }
        public double MuNegativeStartKNm { get; set; }
        public double MuNegativeEndKNm { get; set; }
        public double VuKN { get; set; }
        public string LoadCombination { get; set; }

        public BeamDesignData()
        {
            Id = Guid.NewGuid();
        }
    }
}
