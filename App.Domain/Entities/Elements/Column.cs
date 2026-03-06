using System;
using App.Domain.Enums;

namespace App.Domain.Entities.Elements
{
    public class Column : StructuralElement
    {
        public double HeightMeters { get; private set; }
        public double WidthMm { get; private set; }
        public double DepthMm { get; private set; }
        public bool IsCircular { get; private set; }
        public double DiameterMm { get; private set; }
        public string BaseJoint { get; private set; }
        public string TopJoint { get; private set; }
        public bool IsCornerColumn { get; private set; }
        public bool IsEdgeColumn { get; private set; }

        private Column() { }

        public Column(string elementId, string label, double heightMeters, double widthMm, double depthMm)
            : base(elementId, label, ElementType.Column)
        {
            HeightMeters = heightMeters > 0 ? heightMeters : throw new ArgumentException("Height must be positive.");
            WidthMm = widthMm > 0 ? widthMm : throw new ArgumentException("Width must be positive.");
            DepthMm = depthMm > 0 ? depthMm : throw new ArgumentException("Depth must be positive.");
        }

        public Column(string elementId, string label, double heightMeters, double diameterMm, bool circular)
            : base(elementId, label, ElementType.Column)
        {
            HeightMeters = heightMeters;
            DiameterMm = diameterMm;
            IsCircular = circular;
        }

        public void SetJoints(string baseJoint, string topJoint)
        {
            BaseJoint = baseJoint;
            TopJoint = topJoint;
        }

        public void MarkAsCorner() => IsCornerColumn = true;
        public void MarkAsEdge() => IsEdgeColumn = true;

        public double GetCrossSectionArea()
        {
            if (IsCircular)
                return Math.PI * DiameterMm * DiameterMm / 4.0;
            return WidthMm * DepthMm;
        }

        public override string GetElementDescription() =>
            IsCircular
                ? $"Column {ElementId}: D={DiameterMm}mm, H={HeightMeters:F2}m @ {StoryName}"
                : $"Column {ElementId}: {WidthMm}x{DepthMm}mm, H={HeightMeters:F2}m @ {StoryName}";
    }
}
