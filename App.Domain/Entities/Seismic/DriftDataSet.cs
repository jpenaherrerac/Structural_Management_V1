using System;
using System.Collections.Generic;
using System.Linq;

namespace App.Domain.Entities.Seismic
{
    public class DriftDataSet
    {
        public Guid Id { get; private set; }
        private readonly List<DriftResult> _results = new List<DriftResult>();
        public IReadOnlyList<DriftResult> Results => _results.AsReadOnly();
        public double AllowableDrift { get; set; } = 0.007;

        public DriftDataSet()
        {
            Id = Guid.NewGuid();
        }

        public void Add(DriftResult result)
        {
            if (result != null) _results.Add(result);
        }

        public void AddRange(IEnumerable<DriftResult> results)
        {
            foreach (var r in results ?? Array.Empty<DriftResult>())
                Add(r);
        }

        public double GetMaxInelasticDriftX() => _results.Max(r => r.InelasticDriftX);
        public double GetMaxInelasticDriftY() => _results.Max(r => r.InelasticDriftY);
        public bool HasExceedances() => _results.Any(r => r.ExceedsLimitX || r.ExceedsLimitY);
    }
}
