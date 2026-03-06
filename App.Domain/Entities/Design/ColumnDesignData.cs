using System;

namespace App.Domain.Entities.Design
{
    public class ColumnDesignData
    {
        public Guid Id { get; private set; }
        public string ElementId { get; set; }
        public string StoryName { get; set; }
        public string SectionName { get; set; }
        public double HeightMeters { get; set; }
        public double WidthMm { get; set; }
        public double DepthMm { get; set; }
        public double CoverMm { get; set; }
        public double Fc { get; set; }
        public double Fy { get; set; }
        public double PuKN { get; set; }
        public double Mu2KNm { get; set; }
        public double Mu3KNm { get; set; }
        public double VuKN { get; set; }
        public string LoadCombination { get; set; }

        public ColumnDesignData()
        {
            Id = Guid.NewGuid();
        }
    }
}
