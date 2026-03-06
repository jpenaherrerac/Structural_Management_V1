using System;
using System.Collections.Generic;

namespace App.Domain.Entities.Loads
{
    public class BuildingLoadConfiguration
    {
        public Guid Id { get; private set; }
        public Guid ProjectId { get; private set; }
        public SeismicConfiguration SeismicConfig { get; set; }
        private readonly List<LoadPatternDefinition> _patterns = new List<LoadPatternDefinition>();
        public IReadOnlyList<LoadPatternDefinition> Patterns => _patterns.AsReadOnly();
        private readonly List<LoadCaseDefinition> _cases = new List<LoadCaseDefinition>();
        public IReadOnlyList<LoadCaseDefinition> Cases => _cases.AsReadOnly();
        private readonly List<LoadCombinationDefinition> _combinations = new List<LoadCombinationDefinition>();
        public IReadOnlyList<LoadCombinationDefinition> Combinations => _combinations.AsReadOnly();
        public MassSourceDefinition MassSource { get; set; }
        public ResponseSpectrumDefinition ResponseSpectrum { get; set; }
        public DateTime CreatedAt { get; private set; }
        public DateTime UpdatedAt { get; private set; }

        private BuildingLoadConfiguration() { }

        public BuildingLoadConfiguration(Guid projectId)
        {
            Id = Guid.NewGuid();
            ProjectId = projectId;
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }

        public void AddPattern(LoadPatternDefinition pattern)
        {
            if (pattern == null) throw new ArgumentNullException(nameof(pattern));
            _patterns.Add(pattern);
            UpdatedAt = DateTime.UtcNow;
        }

        public void AddCase(LoadCaseDefinition loadCase)
        {
            if (loadCase == null) throw new ArgumentNullException(nameof(loadCase));
            _cases.Add(loadCase);
            UpdatedAt = DateTime.UtcNow;
        }

        public void AddCombination(LoadCombinationDefinition combination)
        {
            if (combination == null) throw new ArgumentNullException(nameof(combination));
            _combinations.Add(combination);
            UpdatedAt = DateTime.UtcNow;
        }
    }
}
