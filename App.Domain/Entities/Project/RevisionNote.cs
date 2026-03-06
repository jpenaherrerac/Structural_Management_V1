using System;

namespace App.Domain.Entities.Project
{
    public class RevisionNote
    {
        public Guid Id { get; private set; }
        public Guid RevisionId { get; private set; }
        public string Content { get; private set; }
        public Guid AuthorId { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public string Category { get; private set; }

        private RevisionNote() { }

        public RevisionNote(Guid revisionId, string content, Guid authorId, string category = "General")
        {
            Id = Guid.NewGuid();
            RevisionId = revisionId;
            Content = content ?? throw new ArgumentNullException(nameof(content));
            AuthorId = authorId;
            Category = category ?? "General";
            CreatedAt = DateTime.UtcNow;
        }

        public void UpdateContent(string content)
        {
            Content = content ?? throw new ArgumentNullException(nameof(content));
        }
    }
}
