using System;
using System.Collections.Generic;
using App.Domain.Entities.Loads;

namespace App.Application.UseCases
{
    /// <summary>
    /// Builds a default <see cref="BuildingLoadConfiguration"/> from seismic parameters.
    /// Used by "Cargas → Configurar Cargas" to create the standard E.030 load configuration
    /// that gets applied to SAP2000.
    /// </summary>
    public static class DefaultLoadConfigurationBuilder
    {
        /// <summary>
        /// Creates a complete BuildingLoadConfiguration including:
        ///   - Mass source
        ///   - Dead, Live, Seismic load patterns
        ///   - Modal, Response-spectrum, and static load cases
        ///   - Standard E.030 load combinations
        ///   - Response spectrum function
        /// </summary>
        public static BuildingLoadConfiguration Build(
            Guid projectId,
            SeismicConfiguration seismicConfig,
            IReadOnlyDictionary<string, double> seismicValues)
        {
            var cfg = new BuildingLoadConfiguration(projectId);
            cfg.SeismicConfig = seismicConfig;

            // ── Mass Source ──────────────────────────────────────────────────
            var ms = new MassSourceDefinition("MsSrc1", true, true);
            ms.SetAsDefault();
            cfg.MassSource = ms;

            // ── Load Patterns ───────────────────────────────────────────────
            cfg.AddPattern(new LoadPatternDefinition("Dead", "Dead", 1.0));
            cfg.AddPattern(new LoadPatternDefinition("Live", "Live", 0.0));
            cfg.AddPattern(new LoadPatternDefinition("Sx", "Quake", 0.0));
            cfg.AddPattern(new LoadPatternDefinition("Sy", "Quake", 0.0));

            // ── Response Spectrum Function ───────────────────────────────────
            var spectrum = new ResponseSpectrumDefinition("E030_Spectrum", 0.05);

            double Z = GetValue(seismicValues, "Z", 0.45);
            double U = GetValue(seismicValues, "U", 1.0);
            double S = GetValue(seismicValues, "S", 1.0);
            double TP = GetValue(seismicValues, "TP", 0.4);
            double TL = GetValue(seismicValues, "TL", 2.5);
            double Rx = GetValue(seismicValues, "R_x", 6.0);
            double Ry = GetValue(seismicValues, "R_y", 6.0);

            // Build spectrum points from 0.01s to 10s
            var periods = new List<double>();
            for (double t = 0.01; t <= 0.1; t += 0.01) periods.Add(t);
            for (double t = 0.15; t <= 1.0; t += 0.05) periods.Add(t);
            for (double t = 1.2; t <= 5.0; t += 0.2) periods.Add(t);
            for (double t = 5.5; t <= 10.0; t += 0.5) periods.Add(t);

            foreach (double t in periods)
            {
                double sa = ComputeSa(t, Z, U, S, TP, TL);
                spectrum.AddSpectrumPoint(t, sa);
            }

            cfg.ResponseSpectrum = spectrum;

            // ── Load Cases ──────────────────────────────────────────────────
            // Modal
            var modal = new LoadCaseDefinition("Modal", "Modal", "Eigen");
            modal.MarkAsModal();
            cfg.AddCase(modal);

            // Response spectrum X
            var sdx = new LoadCaseDefinition("Sdx", "ResponseSpectrum", "Steady");
            sdx.MarkAsSeismicResponseSpectrum();
            cfg.AddCase(sdx);

            // Response spectrum Y
            var sdy = new LoadCaseDefinition("Sdy", "ResponseSpectrum", "Steady");
            sdy.MarkAsSeismicResponseSpectrum();
            cfg.AddCase(sdy);

            // Static seismic
            var dead = new LoadCaseDefinition("Dead", "Linear Static", "Steady");
            dead.AddLoadPattern("Dead");
            cfg.AddCase(dead);

            var live = new LoadCaseDefinition("Live", "Linear Static", "Steady");
            live.AddLoadPattern("Live");
            cfg.AddCase(live);

            // ── Load Combinations (per E.030) ───────────────────────────────
            // 1.4D + 1.7L
            var combo1 = new LoadCombinationDefinition("Combo1", "Linear Add");
            combo1.AddCase("Dead", 1.4);
            combo1.AddCase("Live", 1.7);
            cfg.AddCombination(combo1);

            // 1.25(D+L) ± Sdx
            var combo2a = new LoadCombinationDefinition("Combo2a", "Linear Add");
            combo2a.AddCase("Dead", 1.25);
            combo2a.AddCase("Live", 1.25);
            combo2a.AddCase("Sdx", 1.0);
            cfg.AddCombination(combo2a);

            var combo2b = new LoadCombinationDefinition("Combo2b", "Linear Add");
            combo2b.AddCase("Dead", 1.25);
            combo2b.AddCase("Live", 1.25);
            combo2b.AddCase("Sdx", -1.0);
            cfg.AddCombination(combo2b);

            // 1.25(D+L) ± Sdy
            var combo3a = new LoadCombinationDefinition("Combo3a", "Linear Add");
            combo3a.AddCase("Dead", 1.25);
            combo3a.AddCase("Live", 1.25);
            combo3a.AddCase("Sdy", 1.0);
            cfg.AddCombination(combo3a);

            var combo3b = new LoadCombinationDefinition("Combo3b", "Linear Add");
            combo3b.AddCase("Dead", 1.25);
            combo3b.AddCase("Live", 1.25);
            combo3b.AddCase("Sdy", -1.0);
            cfg.AddCombination(combo3b);

            // 0.9D ± Sdx
            var combo4a = new LoadCombinationDefinition("Combo4a", "Linear Add");
            combo4a.AddCase("Dead", 0.9);
            combo4a.AddCase("Sdx", 1.0);
            cfg.AddCombination(combo4a);

            var combo4b = new LoadCombinationDefinition("Combo4b", "Linear Add");
            combo4b.AddCase("Dead", 0.9);
            combo4b.AddCase("Sdx", -1.0);
            cfg.AddCombination(combo4b);

            // 0.9D ± Sdy
            var combo5a = new LoadCombinationDefinition("Combo5a", "Linear Add");
            combo5a.AddCase("Dead", 0.9);
            combo5a.AddCase("Sdy", 1.0);
            cfg.AddCombination(combo5a);

            var combo5b = new LoadCombinationDefinition("Combo5b", "Linear Add");
            combo5b.AddCase("Dead", 0.9);
            combo5b.AddCase("Sdy", -1.0);
            cfg.AddCombination(combo5b);

            // Envelope
            var envelope = new LoadCombinationDefinition("Envolvente", "Envelope");
            envelope.AddCase("Combo1", 1.0);
            envelope.AddCase("Combo2a", 1.0);
            envelope.AddCase("Combo2b", 1.0);
            envelope.AddCase("Combo3a", 1.0);
            envelope.AddCase("Combo3b", 1.0);
            envelope.AddCase("Combo4a", 1.0);
            envelope.AddCase("Combo4b", 1.0);
            envelope.AddCase("Combo5a", 1.0);
            envelope.AddCase("Combo5b", 1.0);
            cfg.AddCombination(envelope);

            return cfg;
        }

        /// <summary>
        /// Spectral acceleration per E.030-2018: Sa = ZUCS/R
        /// where C depends on T, TP, TL.
        /// </summary>
        private static double ComputeSa(double T, double Z, double U, double S, double TP, double TL)
        {
            double C;
            if (T < TP)
                C = 2.5;
            else if (T < TL)
                C = 2.5 * TP / T;
            else
                C = 2.5 * TP * TL / (T * T);

            return Z * U * C * S;
        }

        private static double GetValue(IReadOnlyDictionary<string, double> vals, string key, double fallback)
        {
            if (vals != null && vals.TryGetValue(key, out var v) && v > 0) return v;
            return fallback;
        }
    }
}
