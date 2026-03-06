using System;
using App.Domain.Enums;

namespace App.Domain.Entities.Elements
{
    public class Slab : StructuralElement
    {
        public double ThicknessMm { get; private set; }
        public double AreaSqM { get; private set; }
        public string SlabType { get; private set; }
        public bool IsTwoWay { get; private set; }
        public double Lx { get; private set; }
        public double Ly { get; private set; }

        private Slab() { }

        public Slab(string elementId, string label, double thicknessMm, double lx, double ly)
            : base(elementId, label, ElementType.Slab)
        {
            ThicknessMm = thicknessMm > 0 ? thicknessMm : throw new ArgumentException("Thickness must be positive.");
            Lx = lx;
            Ly = ly;
            AreaSqM = lx * ly;
            IsTwoWay = (ly / lx) <= 2.0;
            SlabType = IsTwoWay ? "Two-Way" : "One-Way";
        }

        public void SetSlabType(string type) => SlabType = type;

        public override string GetElementDescription() =>
            $"Slab {ElementId}: t={ThicknessMm}mm, {Lx:F2}x{Ly:F2}m ({SlabType}) @ {StoryName}";
    }
}
