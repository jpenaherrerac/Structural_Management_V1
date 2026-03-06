using System;

namespace App.Domain.Entities.Loads
{
    public class SeismicConfiguration
    {
        public Guid Id { get; private set; }
        public Guid ProjectId { get; private set; }
        public string ZoneDesignation { get; set; }
        public double ZoneFactor { get; set; }
        public string SoilType { get; set; }
        public double SoilAmplificationFactor { get; set; }
        public string UsageCategory { get; set; }
        public double ImportanceFactor { get; set; }
        public double ReductionFactor { get; set; }
        public double Ct { get; set; }
        public double Alpha { get; set; }
        public double Tp { get; set; }
        public double Tl { get; set; }
        public double Fa { get; set; }
        public double Fd { get; set; }
        public double Fs { get; set; }
        public DateTime ConfiguredAt { get; private set; }

        private SeismicConfiguration() { }

        public SeismicConfiguration(Guid projectId)
        {
            Id = Guid.NewGuid();
            ProjectId = projectId;
            ConfiguredAt = DateTime.UtcNow;
            ReductionFactor = 6.0;
            ImportanceFactor = 1.0;
            Ct = 0.055;
            Alpha = 0.9;
        }

        public double ComputePeriod(double heightMeters)
        {
            return Ct * Math.Pow(heightMeters, Alpha);
        }

        public double ComputeBaseShear(double totalWeightKN, double period)
        {
            double sa = ComputeSpectralAcceleration(period);
            return sa * totalWeightKN / ReductionFactor;
        }

        public double ComputeSpectralAcceleration(double period)
        {
            if (period <= 0) throw new ArgumentException("Period must be positive.");
            double eta = Math.Max(0.1, DampingCorrectionFactor(0.05));
            double spectralPlateau = ZoneFactor * SoilAmplificationFactor * ImportanceFactor;
            if (period < Tp)
                return spectralPlateau * eta;
            else if (period < Tl)
                return spectralPlateau * eta * Tp / period;
            else
                return spectralPlateau * eta * Tp * Tl / (period * period);
        }

        private static double DampingCorrectionFactor(double damping)
        {
            return Math.Sqrt(10.0 / (5.0 + damping * 100));
        }
    }
}
