using System;
using System.Collections.Generic;
using App.Domain.Enums;

namespace App.Domain.Entities.Project
{
    public class Project
    {
        public Guid Id { get; private set; }
        public string Name { get; private set; }
        public string Code { get; private set; }
        public string Description { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public DateTime UpdatedAt { get; private set; }
        public bool IsActive { get; private set; }
        public ProjectMetadata Metadata { get; private set; }
        private readonly List<ProjectRevision> _revisions = new List<ProjectRevision>();
        public IReadOnlyList<ProjectRevision> Revisions => _revisions.AsReadOnly();
        private readonly List<ProjectParticipant> _participants = new List<ProjectParticipant>();
        public IReadOnlyList<ProjectParticipant> Participants => _participants.AsReadOnly();

        private Project() { }

        public Project(string name, string code, string description, ProjectMetadata metadata)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Project name cannot be empty.", nameof(name));
            if (string.IsNullOrWhiteSpace(code))
                throw new ArgumentException("Project code cannot be empty.", nameof(code));

            Id = Guid.NewGuid();
            Name = name;
            Code = code;
            Description = description ?? string.Empty;
            Metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
            IsActive = true;
        }

        public void UpdateName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Project name cannot be empty.", nameof(name));
            Name = name;
            UpdatedAt = DateTime.UtcNow;
        }

        public void UpdateDescription(string description)
        {
            Description = description ?? string.Empty;
            UpdatedAt = DateTime.UtcNow;
        }

        public ProjectRevision AddRevision(string label, string notes, Guid authorId)
        {
            var revision = new ProjectRevision(Id, label, notes, authorId, _revisions.Count + 1);
            _revisions.Add(revision);
            UpdatedAt = DateTime.UtcNow;
            return revision;
        }

        public void AddParticipant(ProjectParticipant participant)
        {
            if (participant == null) throw new ArgumentNullException(nameof(participant));
            _participants.Add(participant);
            UpdatedAt = DateTime.UtcNow;
        }

        public void Deactivate()
        {
            IsActive = false;
            UpdatedAt = DateTime.UtcNow;
        }
    }
}
