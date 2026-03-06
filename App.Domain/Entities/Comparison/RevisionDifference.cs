using System;

namespace App.Domain.Entities.Comparison
{
    public class RevisionDifference
    {
        public Guid Id { get; private set; }
        public string Category { get; private set; }
        public string ElementId { get; private set; }
        public string FieldName { get; private set; }
        public string BaseValue { get; private set; }
        public string ComparedValue { get; private set; }
        public double PercentChange { get; private set; }
        public bool IsSignificant { get; private set; }

        private RevisionDifference() { }

        public RevisionDifference(string category, string elementId, string fieldName,
            string baseValue, string comparedValue)
        {
            Id = Guid.NewGuid();
            Category = category ?? throw new ArgumentNullException(nameof(category));
            ElementId = elementId ?? string.Empty;
            FieldName = fieldName ?? throw new ArgumentNullException(nameof(fieldName));
            BaseValue = baseValue ?? string.Empty;
            ComparedValue = comparedValue ?? string.Empty;

            if (double.TryParse(baseValue, out double bv) && double.TryParse(comparedValue, out double cv) && bv != 0)
            {
                PercentChange = (cv - bv) / Math.Abs(bv) * 100.0;
                IsSignificant = Math.Abs(PercentChange) > 5.0;
            }
        }
    }
}
