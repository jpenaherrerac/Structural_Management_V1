using System;
using System.Collections.Generic;
using System.Linq;
using App.Application.Interfaces;
using App.Domain.Entities.Seismic;

namespace App.Application.UseCases
{
    /// <summary>
    /// Computes story drifts from SAP2000 displacement data.
    /// Workflow:
    ///   1. Read displacements per story
    ///   2. Calculate elastic drift  = (Δ_i − Δ_{i-1}) / h
    ///   3. Apply factor R           → inelastic drift = elastic × 0.75 × R
    ///   4. Compare with code limit
    ///   5. Identify critical story
    /// </summary>
    public class StoryDriftCalculator
    {
        private readonly ISapAdapter _sapAdapter;

        public StoryDriftCalculator(ISapAdapter sapAdapter)
        {
            _sapAdapter = sapAdapter ?? throw new ArgumentNullException(nameof(sapAdapter));
        }

        /// <summary>
        /// Computes story drifts for the given parameters.
        /// Returns a DriftDataSet populated with results per story.
        /// </summary>
        public DriftDataSet Compute(StoryDriftCalculationParameters parameters)
        {
            if (parameters == null) throw new ArgumentNullException(nameof(parameters));

            var dataSet = new DriftDataSet { AllowableDrift = parameters.DriftLimit };

            // Get drift results from SAP2000 for X and Y load cases
            var driftsX = _sapAdapter.GetStoryDrifts(parameters.LoadCaseX).ToList();
            var driftsY = _sapAdapter.GetStoryDrifts(parameters.LoadCaseY).ToList();

            // Build a lookup from Y drifts keyed by story name
            var yLookup = driftsY.ToDictionary(d => d.StoryName, d => d, StringComparer.OrdinalIgnoreCase);

            foreach (var dx in driftsX)
            {
                double driftYValue = 0;
                double dispY = 0;
                if (yLookup.TryGetValue(dx.StoryName, out var dy))
                {
                    driftYValue = dy.DriftX; // DriftX from the Y load case is the Y-direction drift
                    dispY = dy.DisplacementX;
                }

                var result = new DriftResult(dx.StoryName, parameters.LoadCaseX, dx.DriftX, driftYValue)
                {
                    DisplacementX = dx.DisplacementX,
                    DisplacementY = dispY,
                    StoryHeightMeters = dx.StoryHeightMeters,
                    ReductionFactorR = parameters.ReductionFactorR,
                    AllowableDriftLimit = parameters.DriftLimit
                };

                dataSet.Add(result);
            }

            return dataSet;
        }

        /// <summary>
        /// Returns the name of the critical story (max inelastic drift).
        /// </summary>
        public static string GetCriticalStory(DriftDataSet dataSet)
        {
            if (dataSet == null || dataSet.Results.Count == 0) return string.Empty;
            var max = dataSet.Results.OrderByDescending(r => Math.Max(r.InelasticDriftX, r.InelasticDriftY)).First();
            return max.StoryName;
        }
    }
}
