using System;

namespace App.Domain.Entities.Design
{
    public class WallDesignData
    {
        public Guid Id { get; private set; }
        public string ElementId { get; set; }
        public string StoryName { get; set; }
        public double LengthMeters { get; set; }
        public double ThicknessMm { get; set; }
        public double HeightMeters { get; set; }
        public double CoverMm { get; set; }
        public double Fc { get; set; }
        public double Fy { get; set; }
        public double PuKN { get; set; }
        public double MuKNm { get; set; }
        public double VuKN { get; set; }
        public string LoadCombination { get; set; }

        public WallDesignData()
        {
            Id = Guid.NewGuid();
        }
    }
}
