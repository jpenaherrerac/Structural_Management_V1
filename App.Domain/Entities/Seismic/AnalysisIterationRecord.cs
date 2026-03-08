using System;
using System.Collections.Generic;

namespace App.Domain.Entities.Seismic
{
    /// <summary>
    /// Tracks one cycle of the iterative seismic analysis workflow:
    ///   1. Configure parameters → 2. Apply to SAP2000 → 3. Run analysis →
    ///   4. Extract results → 5. Evaluate criteria → 6. Update parameters → repeat.
    /// </summary>
    public class AnalysisIterationRecord
    {
        public Guid Id { get; private set; }
        public Guid ProjectId { get; private set; }
        public int IterationNumber { get; private set; }
        public IterationPhase CurrentPhase { get; private set; }
        public DateTime StartedAt { get; private set; }
        public DateTime? CompletedAt { get; private set; }

        // ── Seismic parameter snapshot for this iteration ────────────────────
        public double ReductionFactorR_X { get; set; }
        public double ReductionFactorR_Y { get; set; }
        public double IrregularityIa_X { get; set; } = 1.0;
        public double IrregularityIp_X { get; set; } = 1.0;
        public double IrregularityIa_Y { get; set; } = 1.0;
        public double IrregularityIp_Y { get; set; } = 1.0;

        // ── Results summary ─────────────────────────────────────────────────
        public double? MaxDriftX { get; set; }
        public double? MaxDriftY { get; set; }
        public double? BaseShearX_kN { get; set; }
        public double? BaseShearY_kN { get; set; }
        public double? FundamentalPeriodX { get; set; }
        public double? FundamentalPeriodY { get; set; }
        public bool? AnalysisConverged { get; set; }
        public string Notes { get; set; }

        private readonly List<string> _phaseLog = new List<string>();
        public IReadOnlyList<string> PhaseLog => _phaseLog.AsReadOnly();

        private AnalysisIterationRecord() { }

        public AnalysisIterationRecord(Guid projectId, int iterationNumber)
        {
            Id = Guid.NewGuid();
            ProjectId = projectId;
            IterationNumber = iterationNumber;
            CurrentPhase = IterationPhase.ParametersConfigured;
            StartedAt = DateTime.UtcNow;
        }

        public void AdvanceTo(IterationPhase phase)
        {
            CurrentPhase = phase;
            _phaseLog.Add($"{DateTime.UtcNow:HH:mm:ss} → {phase}");
        }

        public void MarkCompleted()
        {
            CompletedAt = DateTime.UtcNow;
            AdvanceTo(IterationPhase.Completed);
        }

        public bool IsCompleted => CompletedAt.HasValue;
    }

    /// <summary>
    /// Phases of one iteration in the seismic analysis workflow.
    /// </summary>
    public enum IterationPhase
    {
        ParametersConfigured,
        LoadsApplied,
        AnalysisRunning,
        AnalysisCompleted,
        ResultsExtracted,
        CriteriaEvaluated,
        Completed
    }
}
