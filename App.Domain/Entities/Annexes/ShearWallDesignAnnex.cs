using System;
using System.Collections.Generic;

namespace App.Domain.Entities.Annexes
{
    public class ShearWallDesignAnnex
    {
        public Guid Id { get; private set; }
        public Guid ProjectId { get; private set; }
        public string Title { get; private set; }
        public string DesignCode { get; private set; }
        public double Fc { get; set; }
        public double Fy { get; set; }
        public double PhiFlexure { get; set; } = 0.9;
        public double PhiShear { get; set; } = 0.75;
        public DateTime GeneratedAt { get; private set; }
        private readonly List<ShearWallDesignReportRow> _rows = new List<ShearWallDesignReportRow>();
        public IReadOnlyList<ShearWallDesignReportRow> Rows => _rows.AsReadOnly();

        private ShearWallDesignAnnex() { }

        public ShearWallDesignAnnex(Guid projectId, string title, string designCode)
        {
            Id = Guid.NewGuid();
            ProjectId = projectId;
            Title = title ?? throw new ArgumentNullException(nameof(title));
            DesignCode = designCode ?? "ACI 318";
            GeneratedAt = DateTime.UtcNow;
        }

        public void AddRow(ShearWallDesignReportRow row)
        {
            if (row != null) _rows.Add(row);
        }
    }
}
