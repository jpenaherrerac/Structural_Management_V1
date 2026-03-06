using System;
using System.Collections.Generic;

namespace App.Domain.Entities.Loads
{
    public class LoadCombinationDefinition
    {
        public Guid Id { get; private set; }
        public string Name { get; private set; }
        public string CombinationType { get; private set; }
        public string Notes { get; private set; }
        private readonly List<LoadCombinationCase> _cases = new List<LoadCombinationCase>();
        public IReadOnlyList<LoadCombinationCase> Cases => _cases.AsReadOnly();

        private LoadCombinationDefinition() { }

        public LoadCombinationDefinition(string name, string combinationType)
        {
            Id = Guid.NewGuid();
            Name = name ?? throw new ArgumentNullException(nameof(name));
            CombinationType = combinationType ?? "Linear Add";
        }

        public void AddCase(string caseName, double scaleFactor)
        {
            _cases.Add(new LoadCombinationCase(caseName, scaleFactor));
        }

        public void SetNotes(string notes) => Notes = notes;

        public class LoadCombinationCase
        {
            public string CaseName { get; }
            public double ScaleFactor { get; }
            public LoadCombinationCase(string caseName, double scaleFactor)
            {
                CaseName = caseName;
                ScaleFactor = scaleFactor;
            }
        }
    }
}
