using System;
using System.Collections.Generic;

namespace App.Domain.Entities.Project
{
    public class ProjectRevision
    {
        public Guid Id { get; private set; }
        public Guid ProjectId { get; private set; }
        public int RevisionNumber { get; private set; }
        public string Label { get; private set; }
        public string Notes { get; private set; }
        public Guid AuthorId { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public bool IsApproved { get; private set; }
        private readonly List<RevisionNote> _additionalNotes = new List<RevisionNote>();
        public IReadOnlyList<RevisionNote> AdditionalNotes => _additionalNotes.AsReadOnly();
        private readonly List<ApprovalRecord> _approvals = new List<ApprovalRecord>();
        public IReadOnlyList<ApprovalRecord> Approvals => _approvals.AsReadOnly();

        private ProjectRevision() { }

        public ProjectRevision(Guid projectId, string label, string notes, Guid authorId, int revisionNumber)
        {
            Id = Guid.NewGuid();
            ProjectId = projectId;
            Label = label ?? throw new ArgumentNullException(nameof(label));
            Notes = notes ?? string.Empty;
            AuthorId = authorId;
            RevisionNumber = revisionNumber;
            CreatedAt = DateTime.UtcNow;
            IsApproved = false;
        }

        public void Approve(Guid approverId, string comment)
        {
            var record = new ApprovalRecord(Id, approverId, comment, approved: true);
            _approvals.Add(record);
            IsApproved = true;
        }

        public void Reject(Guid reviewerId, string comment)
        {
            var record = new ApprovalRecord(Id, reviewerId, comment, approved: false);
            _approvals.Add(record);
            IsApproved = false;
        }

        public void AddNote(RevisionNote note)
        {
            if (note == null) throw new ArgumentNullException(nameof(note));
            _additionalNotes.Add(note);
        }
    }
}
