using System;

namespace App.Domain.Entities.Loads
{
    public class LoadPatternDefinition
    {
        public Guid Id { get; private set; }
        public string Name { get; private set; }
        public string PatternType { get; private set; }
        public double SelfWeightMultiplier { get; private set; }
        public bool IsSeismic { get; private set; }
        public string DesignType { get; private set; }

        private LoadPatternDefinition() { }

        public LoadPatternDefinition(string name, string patternType, double selfWeightMultiplier = 0.0)
        {
            Id = Guid.NewGuid();
            Name = name ?? throw new ArgumentNullException(nameof(name));
            PatternType = patternType ?? throw new ArgumentNullException(nameof(patternType));
            SelfWeightMultiplier = selfWeightMultiplier;
        }

        public void MarkAsSeismic(string designType)
        {
            IsSeismic = true;
            DesignType = designType;
        }
    }
}
