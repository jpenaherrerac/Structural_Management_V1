using System;
using System.Collections.Generic;
using System.Linq;
using App.Application.Interfaces;
using App.Domain.Entities.Loads;
using App.Domain.Entities.Seismic;

namespace App.Application.UseCases
{
    // ═════════════════════════════════════════════════════════════════════════
    // Request / Response
    // ═════════════════════════════════════════════════════════════════════════

    public class ApplySeismicConfigurationRequest
    {
        public Guid ProjectId { get; set; }
        public BuildingLoadConfiguration LoadConfig { get; set; }
        public int NumberOfStories { get; set; }
        public double[] StoryHeights { get; set; }

        /// <summary>Current iteration number (1-based).</summary>
        public int IterationNumber { get; set; } = 1;
    }

    public class ApplySeismicConfigurationResponse
    {
        public bool Success { get; set; }
        public string ErrorMessage { get; set; }
        public AnalysisIterationRecord Iteration { get; set; }

        /// <summary>Commands that were sent to SAP2000 during this apply step.</summary>
        public IReadOnlyList<string> CommandsExecuted { get; set; } = Array.Empty<string>();

        public static ApplySeismicConfigurationResponse Ok(AnalysisIterationRecord iter, IReadOnlyList<string> cmds) =>
            new ApplySeismicConfigurationResponse { Success = true, Iteration = iter, CommandsExecuted = cmds };

        public static ApplySeismicConfigurationResponse Fail(string message) =>
            new ApplySeismicConfigurationResponse { Success = false, ErrorMessage = message };
    }

    // ═════════════════════════════════════════════════════════════════════════
    // Use Case — "Cargas → Configurar Cargas" (1st routine)
    // ═════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Applies a complete seismic load configuration to SAP2000:
    ///   • Mass Source
    ///   • Load Patterns
    ///   • Response Spectrum Functions (X and Y)
    ///   • Load Cases (including modal and response-spectrum cases)
    ///   • Load Combinations
    ///   • Diaphragm constraints (one per story)
    ///   • Assigns diaphragm to slabs per story
    /// This is the first routine in the iterative seismic analysis workflow.
    /// </summary>
    public class ApplySeismicConfigurationUseCase
    {
        private readonly ISapAdapter _sapAdapter;
        private readonly IProjectRepository _projectRepository;

        public ApplySeismicConfigurationUseCase(
            ISapAdapter sapAdapter,
            IProjectRepository projectRepository)
        {
            _sapAdapter = sapAdapter ?? throw new ArgumentNullException(nameof(sapAdapter));
            _projectRepository = projectRepository ?? throw new ArgumentNullException(nameof(projectRepository));
        }

        public ApplySeismicConfigurationResponse Execute(ApplySeismicConfigurationRequest request)
        {
            if (request == null)
                return ApplySeismicConfigurationResponse.Fail("Request cannot be null.");

            if (request.LoadConfig == null)
                return ApplySeismicConfigurationResponse.Fail("LoadConfig cannot be null.");

            if (!_sapAdapter.IsConnected)
                return ApplySeismicConfigurationResponse.Fail("SAP2000 is not connected.");

            if (!_projectRepository.Exists(request.ProjectId))
                return ApplySeismicConfigurationResponse.Fail($"Project {request.ProjectId} not found.");

            var commands = new List<string>();
            var iteration = new AnalysisIterationRecord(request.ProjectId, request.IterationNumber);

            try
            {
                var cfg = request.LoadConfig;

                // ── 1. Mass Source ───────────────────────────────────────────
                if (cfg.MassSource != null)
                {
                    _sapAdapter.DefineMassSource(
                        cfg.MassSource.Name,
                        cfg.MassSource.IncludeElementMasses,
                        cfg.MassSource.IncludeAdditionalMasses);
                    commands.Add($"DefineMassSource({cfg.MassSource.Name})");
                }

                // ── 2. Load Patterns ────────────────────────────────────────
                foreach (var pat in cfg.Patterns)
                {
                    _sapAdapter.DefineLoadPattern(pat.Name, pat.PatternType, pat.SelfWeightMultiplier);
                    commands.Add($"DefineLoadPattern({pat.Name})");
                }

                // ── 3. Response Spectrum Functions ───────────────────────────
                if (cfg.ResponseSpectrum != null)
                {
                    var points = cfg.ResponseSpectrum.SpectrumPoints
                        .Select(p => (p.Period, p.Acceleration));
                    _sapAdapter.DefineResponseSpectrum(
                        cfg.ResponseSpectrum.Name,
                        cfg.ResponseSpectrum.DampingRatio,
                        points);
                    commands.Add($"DefineResponseSpectrum({cfg.ResponseSpectrum.Name})");
                }

                // ── 4. Load Cases ───────────────────────────────────────────
                foreach (var lc in cfg.Cases)
                {
                    _sapAdapter.DefineLoadCase(lc.Name, lc.CaseType, lc.AnalysisType);
                    commands.Add($"DefineLoadCase({lc.Name})");
                }

                // ── 5. Load Combinations ────────────────────────────────────
                foreach (var combo in cfg.Combinations)
                {
                    var cases = combo.Cases.Select(c => (c.CaseName, c.ScaleFactor));
                    _sapAdapter.DefineLoadCombination(combo.Name, combo.CombinationType, cases);
                    commands.Add($"DefineLoadCombination({combo.Name})");
                }

                // ── 6. Diaphragm Constraints ────────────────────────────────
                int nStories = request.NumberOfStories;
                for (int i = 1; i <= nStories; i++)
                {
                    string dName = $"Diafragma_{i}";
                    _sapAdapter.DefineDiaphragmConstraint(dName);
                    commands.Add($"DefineDiaphragmConstraint({dName})");
                }

                // ── 7. Assign diaphragm to slab points per story ────────────
                var storyNames = _sapAdapter.GetStoryNames().ToList();
                for (int i = 0; i < Math.Min(nStories, storyNames.Count); i++)
                {
                    string dName = $"Diafragma_{i + 1}";
                    _sapAdapter.AssignDiaphragm(storyNames[i], dName, true);
                    commands.Add($"AssignDiaphragm({storyNames[i]}, {dName})");
                }

                // Record seismic parameters used in this iteration
                if (cfg.SeismicConfig != null)
                {
                    iteration.ReductionFactorR_X = cfg.SeismicConfig.ReductionFactor;
                    iteration.ReductionFactorR_Y = cfg.SeismicConfig.ReductionFactor;
                }

                iteration.AdvanceTo(IterationPhase.LoadsApplied);
                return ApplySeismicConfigurationResponse.Ok(iteration, commands);
            }
            catch (Exception ex)
            {
                return ApplySeismicConfigurationResponse.Fail($"Error applying configuration: {ex.Message}");
            }
        }
    }
}
