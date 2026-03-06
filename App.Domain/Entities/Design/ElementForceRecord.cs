using System;

namespace App.Domain.Entities.Design
{
    public class ElementForceRecord
    {
        public Guid Id { get; private set; }
        public string ElementId { get; set; }
        public string StoryName { get; set; }
        public string LoadCombination { get; set; }
        public double P { get; set; }
        public double V2 { get; set; }
        public double V3 { get; set; }
        public double T { get; set; }
        public double M2 { get; set; }
        public double M3 { get; set; }
        public string Location { get; set; }
        public string Units { get; set; }

        public ElementForceRecord()
        {
            Id = Guid.NewGuid();
            Units = "kN, kN-m";
        }

        public double GetAxialForce() => P;
        public double GetMaxShear() => Math.Max(Math.Abs(V2), Math.Abs(V3));
        public double GetMaxMoment() => Math.Max(Math.Abs(M2), Math.Abs(M3));
    }
}
