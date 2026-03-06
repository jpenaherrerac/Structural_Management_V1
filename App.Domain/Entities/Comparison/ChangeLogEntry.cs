using System;
using App.Domain.Enums;

namespace App.Domain.Entities.Comparison
{
    public class ChangeLogEntry
    {
        public Guid Id { get; private set; }
        public Guid RevisionId { get; private set; }
        public Guid AuthorId { get; private set; }
        public string AuthorName { get; private set; }
        public ActionType Action { get; private set; }
        public string Description { get; private set; }
        public string AffectedComponent { get; private set; }
        public DateTime Timestamp { get; private set; }

        private ChangeLogEntry() { }

        public ChangeLogEntry(Guid revisionId, Guid authorId, string authorName,
            ActionType action, string description, string affectedComponent)
        {
            Id = Guid.NewGuid();
            RevisionId = revisionId;
            AuthorId = authorId;
            AuthorName = authorName ?? string.Empty;
            Action = action;
            Description = description ?? throw new ArgumentNullException(nameof(description));
            AffectedComponent = affectedComponent ?? string.Empty;
            Timestamp = DateTime.UtcNow;
        }
    }
}
