using System;

namespace App.Domain.Entities.Design
{
    public class SlabDesignData
    {
        public Guid Id { get; private set; }
        public string ElementId { get; set; }
        public string StoryName { get; set; }
        public double ThicknessMm { get; set; }
        public double CoverMm { get; set; }
        public double Fc { get; set; }
        public double Fy { get; set; }
        public double MuXKNmPerM { get; set; }
        public double MuYKNmPerM { get; set; }
        public double VuKNPerM { get; set; }
        public string LoadCombination { get; set; }

        public SlabDesignData()
        {
            Id = Guid.NewGuid();
        }
    }
}
