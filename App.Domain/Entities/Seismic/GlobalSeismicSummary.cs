using System;
using System.Collections.Generic;

namespace App.Domain.Entities.Seismic
{
    public class GlobalSeismicSummary
    {
        public Guid Id { get; private set; }
        public double TotalStructuralWeightKN { get; set; }
        public double FundamentalPeriodX { get; set; }
        public double FundamentalPeriodY { get; set; }
        public double StaticBaseShearX { get; set; }
        public double StaticBaseShearY { get; set; }
        public double DynamicBaseShearX { get; set; }
        public double DynamicBaseShearY { get; set; }
        public double ScaleFactorX { get; set; }
        public double ScaleFactorY { get; set; }
        public double ModalMassParticipationX { get; set; }
        public double ModalMassParticipationY { get; set; }
        private readonly List<BaseShearSummary> _baseShears = new List<BaseShearSummary>();
        public IReadOnlyList<BaseShearSummary> BaseShears => _baseShears.AsReadOnly();
        private readonly List<MassSummary> _masses = new List<MassSummary>();
        public IReadOnlyList<MassSummary> Masses => _masses.AsReadOnly();

        public GlobalSeismicSummary()
        {
            Id = Guid.NewGuid();
        }

        public void AddBaseShear(BaseShearSummary summary) => _baseShears.Add(summary);
        public void AddMass(MassSummary summary) => _masses.Add(summary);

        public double GetMinimumScaleFactor() => 0.85;
        public bool NeedsScaling => ScaleFactorX < GetMinimumScaleFactor() || ScaleFactorY < GetMinimumScaleFactor();
    }
}
