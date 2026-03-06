using System;
using App.Domain.Enums;

namespace App.Domain.Entities.Elements
{
    public class ShearWall : StructuralElement
    {
        public double LengthMeters { get; private set; }
        public double ThicknessMm { get; private set; }
        public double HeightMeters { get; private set; }
        public bool HasBoundaryElements { get; private set; }
        public double BoundaryWidthMm { get; private set; }
        public double BoundaryDepthMm { get; private set; }
        public string Orientation { get; private set; }

        private ShearWall() { }

        public ShearWall(string elementId, string label, double lengthMeters, double thicknessMm, double heightMeters)
            : base(elementId, label, ElementType.ShearWall)
        {
            LengthMeters = lengthMeters > 0 ? lengthMeters : throw new ArgumentException("Length must be positive.");
            ThicknessMm = thicknessMm > 0 ? thicknessMm : throw new ArgumentException("Thickness must be positive.");
            HeightMeters = heightMeters > 0 ? heightMeters : throw new ArgumentException("Height must be positive.");
        }

        public void SetBoundaryElements(double widthMm, double depthMm)
        {
            HasBoundaryElements = true;
            BoundaryWidthMm = widthMm;
            BoundaryDepthMm = depthMm;
        }

        public void SetOrientation(string orientation) => Orientation = orientation;

        public double GetAspectRatio() => HeightMeters / LengthMeters;

        public override string GetElementDescription() =>
            $"ShearWall {ElementId}: L={LengthMeters:F2}m, t={ThicknessMm}mm, H={HeightMeters:F2}m @ {StoryName}";
    }
}
