using System;
using System.Collections.Generic;
using System.Linq;

namespace App.Domain.Entities.Seismic
{
    public class StoryDataSet
    {
        public Guid Id { get; private set; }
        private readonly List<StoryResult> _results = new List<StoryResult>();
        public IReadOnlyList<StoryResult> Results => _results.AsReadOnly();
        public string Units { get; set; }

        public StoryDataSet()
        {
            Id = Guid.NewGuid();
            Units = "kN, m";
        }

        public void Add(StoryResult result)
        {
            if (result != null) _results.Add(result);
        }

        public void AddRange(IEnumerable<StoryResult> results)
        {
            foreach (var r in results ?? Array.Empty<StoryResult>())
                Add(r);
        }

        public StoryResult? GetByName(string storyName) =>
            _results.FirstOrDefault(r => r.StoryName == storyName);

        public double GetTotalWeight() => _results.Sum(r => r.WeightKN);
    }
}
