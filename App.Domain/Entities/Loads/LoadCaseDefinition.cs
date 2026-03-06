using System;
using System.Collections.Generic;

namespace App.Domain.Entities.Loads
{
    public class LoadCaseDefinition
    {
        public Guid Id { get; private set; }
        public string Name { get; private set; }
        public string CaseType { get; private set; }
        public string AnalysisType { get; private set; }
        public bool IsModal { get; private set; }
        public bool IsSeismicResponseSpectrum { get; private set; }
        private readonly List<string> _loadPatternNames = new List<string>();
        public IReadOnlyList<string> LoadPatternNames => _loadPatternNames.AsReadOnly();

        private LoadCaseDefinition() { }

        public LoadCaseDefinition(string name, string caseType, string analysisType)
        {
            Id = Guid.NewGuid();
            Name = name ?? throw new ArgumentNullException(nameof(name));
            CaseType = caseType ?? throw new ArgumentNullException(nameof(caseType));
            AnalysisType = analysisType ?? throw new ArgumentNullException(nameof(analysisType));
        }

        public void AddLoadPattern(string patternName)
        {
            if (!string.IsNullOrWhiteSpace(patternName))
                _loadPatternNames.Add(patternName);
        }

        public void MarkAsModal() => IsModal = true;
        public void MarkAsSeismicResponseSpectrum() => IsSeismicResponseSpectrum = true;
    }
}
