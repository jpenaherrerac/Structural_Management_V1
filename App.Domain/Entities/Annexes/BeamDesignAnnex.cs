using System;
using System.Collections.Generic;

namespace App.Domain.Entities.Annexes
{
    public class BeamDesignAnnex
    {
        public Guid Id { get; private set; }
        public Guid ProjectId { get; private set; }
        public string Title { get; private set; }
        public string DesignCode { get; private set; }
        public double Fc { get; set; }
        public double Fy { get; set; }
        public double PhiFlex { get; set; } = 0.9;
        public double PhiShear { get; set; } = 0.85;
        public DateTime GeneratedAt { get; private set; }
        private readonly List<BeamDesignReportRow> _rows = new List<BeamDesignReportRow>();
        public IReadOnlyList<BeamDesignReportRow> Rows => _rows.AsReadOnly();

        private BeamDesignAnnex() { }

        public BeamDesignAnnex(Guid projectId, string title, string designCode)
        {
            Id = Guid.NewGuid();
            ProjectId = projectId;
            Title = title ?? throw new ArgumentNullException(nameof(title));
            DesignCode = designCode ?? "ACI 318";
            GeneratedAt = DateTime.UtcNow;
        }

        public void AddRow(BeamDesignReportRow row)
        {
            if (row != null) _rows.Add(row);
        }
    }
}
