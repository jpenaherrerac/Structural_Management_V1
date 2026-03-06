using System;
using System.Collections.Generic;
using System.Linq;

namespace App.Domain.Entities.Seismic
{
    public class ModalDataSet
    {
        public Guid Id { get; private set; }
        private readonly List<ModalResult> _results = new List<ModalResult>();
        public IReadOnlyList<ModalResult> Results => _results.AsReadOnly();

        public ModalDataSet()
        {
            Id = Guid.NewGuid();
        }

        public void Add(ModalResult result)
        {
            if (result != null) _results.Add(result);
        }

        public void AddRange(IEnumerable<ModalResult> results)
        {
            foreach (var r in results ?? Array.Empty<ModalResult>())
                Add(r);
        }

        public ModalResult? GetFundamentalMode() => _results.OrderBy(r => r.ModeNumber).FirstOrDefault();
        public double GetFundamentalPeriod() => GetFundamentalMode()?.Period ?? 0;
        public double GetSumModalMassX() => _results.Sum(r => r.ModalMassRatioX);
        public double GetSumModalMassY() => _results.Sum(r => r.ModalMassRatioY);
    }
}
