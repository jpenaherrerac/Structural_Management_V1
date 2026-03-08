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

        // ── Iteration support ───────────────────────────────────────────────

        /// <summary>Current iteration number in the analysis workflow.</summary>
        public int IterationNumber { get; set; } = 1;

        /// <summary>
        /// Updates this configuration based on extracted seismic results.
        /// Typically called after processing irregularity checks, drift results, etc.
        /// The user may adjust Ia, Ip irregularity factors or the reduction factor R
        /// which then requires re-applying the configuration to SAP2000.
        /// </summary>
        /// <param name="newIa_x">Updated height-irregularity factor X.</param>
        /// <param name="newIp_x">Updated plan-irregularity factor X.</param>
        /// <param name="newIa_y">Updated height-irregularity factor Y.</param>
        /// <param name="newIp_y">Updated plan-irregularity factor Y.</param>
        /// <param name="fundamentalPeriod">Measured fundamental period from analysis.</param>
        public void UpdateFromResults(
            double newIa_x, double newIp_x,
            double newIa_y, double newIp_y,
            double fundamentalPeriod)
        {
            IrregularityIa_X = newIa_x;
            IrregularityIp_X = newIp_x;
            IrregularityIa_Y = newIa_y;
            IrregularityIp_Y = newIp_y;
            MeasuredPeriod = fundamentalPeriod;
            IterationNumber++;
            ConfiguredAt = DateTime.UtcNow;
        }

        /// <summary>Height-irregularity factor X (default 1.0 = regular).</summary>
        public double IrregularityIa_X { get; set; } = 1.0;
        /// <summary>Plan-irregularity factor X (default 1.0 = regular).</summary>
        public double IrregularityIp_X { get; set; } = 1.0;
        /// <summary>Height-irregularity factor Y.</summary>
        public double IrregularityIa_Y { get; set; } = 1.0;
        /// <summary>Plan-irregularity factor Y.</summary>
        public double IrregularityIp_Y { get; set; } = 1.0;
        /// <summary>Measured fundamental period from the last analysis run.</summary>
        public double MeasuredPeriod { get; set; }

        /// <summary>
        /// Effective reduction factor in direction X: R = R0 × Ia × Ip.
        /// </summary>
        public double EffectiveR_X => ReductionFactor * IrregularityIa_X * IrregularityIp_X;

        /// <summary>
        /// Effective reduction factor in direction Y: R = R0 × Ia × Ip.
        /// </summary>
        public double EffectiveR_Y => ReductionFactor * IrregularityIa_Y * IrregularityIp_Y;
    }
}
