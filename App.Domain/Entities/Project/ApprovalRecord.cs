using System;

namespace App.Domain.Entities.Project
{
    public class ApprovalRecord
    {
        public Guid Id { get; private set; }
        public Guid RevisionId { get; private set; }
        public Guid ReviewerId { get; private set; }
        public bool IsApproved { get; private set; }
        public string Comment { get; private set; }
        public DateTime RecordedAt { get; private set; }

        private ApprovalRecord() { }

        public ApprovalRecord(Guid revisionId, Guid reviewerId, string comment, bool approved)
        {
            Id = Guid.NewGuid();
            RevisionId = revisionId;
            ReviewerId = reviewerId;
            Comment = comment ?? string.Empty;
            IsApproved = approved;
            RecordedAt = DateTime.UtcNow;
        }
    }
}
