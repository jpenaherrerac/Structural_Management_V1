using System;
using App.Domain.Enums;

namespace App.Domain.Entities.Elements
{
    public class Beam : StructuralElement
    {
        public double LengthMeters { get; private set; }
        public double WidthMm { get; private set; }
        public double DepthMm { get; private set; }
        public string StartJoint { get; private set; }
        public string EndJoint { get; private set; }
        public bool IsCantilever { get; private set; }
        public double ClearSpanMeters { get; private set; }

        private Beam() { }

        public Beam(string elementId, string label, double lengthMeters, double widthMm, double depthMm)
            : base(elementId, label, ElementType.Beam)
        {
            LengthMeters = lengthMeters > 0 ? lengthMeters : throw new ArgumentException("Length must be positive.");
            WidthMm = widthMm > 0 ? widthMm : throw new ArgumentException("Width must be positive.");
            DepthMm = depthMm > 0 ? depthMm : throw new ArgumentException("Depth must be positive.");
            ClearSpanMeters = lengthMeters;
        }

        public void SetJoints(string startJoint, string endJoint)
        {
            StartJoint = startJoint;
            EndJoint = endJoint;
        }

        public void MarkAsCantilever() => IsCantilever = true;

        public void SetClearSpan(double clearSpan)
        {
            ClearSpanMeters = clearSpan > 0 ? clearSpan : throw new ArgumentException("Clear span must be positive.");
        }

        public double GetAspectRatio() => DepthMm / WidthMm;

        public override string GetElementDescription() =>
            $"Beam {ElementId}: {WidthMm}x{DepthMm}mm, L={LengthMeters:F2}m @ {StoryName}";
    }
}
