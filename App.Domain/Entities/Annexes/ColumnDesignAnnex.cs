using System;
using System.Collections.Generic;

namespace App.Domain.Entities.Annexes
{
    public class ColumnDesignAnnex
    {
        public Guid Id { get; private set; }
        public Guid ProjectId { get; private set; }
        public string Title { get; private set; }
        public string DesignCode { get; private set; }
        public double Fc { get; set; }
        public double Fy { get; set; }
        public double PhiFlexure { get; set; } = 0.65;
        public double PhiShear { get; set; } = 0.85;
        public DateTime GeneratedAt { get; private set; }
        private readonly List<ColumnDesignReportRow> _rows = new List<ColumnDesignReportRow>();
        public IReadOnlyList<ColumnDesignReportRow> Rows => _rows.AsReadOnly();

        private ColumnDesignAnnex() { }

        public ColumnDesignAnnex(Guid projectId, string title, string designCode)
        {
            Id = Guid.NewGuid();
            ProjectId = projectId;
            Title = title ?? throw new ArgumentNullException(nameof(title));
            DesignCode = designCode ?? "ACI 318";
            GeneratedAt = DateTime.UtcNow;
        }

        public void AddRow(ColumnDesignReportRow row)
        {
            if (row != null) _rows.Add(row);
        }
    }
}
