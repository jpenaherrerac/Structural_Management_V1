using System;
using System.Collections.Generic;

namespace App.Domain.Entities.Comparison
{
    public class RevisionComparison
    {
        public Guid Id { get; private set; }
        public Guid ProjectId { get; private set; }
        public Guid BaseRevisionId { get; private set; }
        public Guid ComparedRevisionId { get; private set; }
        public DateTime ComparedAt { get; private set; }
        public string Summary { get; private set; }
        private readonly List<RevisionDifference> _differences = new List<RevisionDifference>();
        public IReadOnlyList<RevisionDifference> Differences => _differences.AsReadOnly();
        private readonly List<ChangeLogEntry> _changeLog = new List<ChangeLogEntry>();
        public IReadOnlyList<ChangeLogEntry> ChangeLog => _changeLog.AsReadOnly();

        private RevisionComparison() { }

        public RevisionComparison(Guid projectId, Guid baseRevisionId, Guid comparedRevisionId)
        {
            Id = Guid.NewGuid();
            ProjectId = projectId;
            BaseRevisionId = baseRevisionId;
            ComparedRevisionId = comparedRevisionId;
            ComparedAt = DateTime.UtcNow;
        }

        public void AddDifference(RevisionDifference diff)
        {
            if (diff != null) _differences.Add(diff);
        }

        public void AddChangeLogEntry(ChangeLogEntry entry)
        {
            if (entry != null) _changeLog.Add(entry);
        }

        public void SetSummary(string summary) => Summary = summary;

        public int TotalChanges => _differences.Count;
    }
}
