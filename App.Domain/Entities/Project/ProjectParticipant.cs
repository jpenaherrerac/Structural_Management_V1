using System;
using App.Domain.Enums;

namespace App.Domain.Entities.Project
{
    public class ProjectParticipant
    {
        public Guid Id { get; private set; }
        public Guid ProjectId { get; private set; }
        public Guid UserId { get; private set; }
        public string UserName { get; private set; }
        public string Email { get; private set; }
        public ProjectRoleType Role { get; private set; }
        public DateTime JoinedAt { get; private set; }
        public bool IsActive { get; private set; }

        private ProjectParticipant() { }

        public ProjectParticipant(Guid projectId, Guid userId, string userName, string email, ProjectRoleType role)
        {
            Id = Guid.NewGuid();
            ProjectId = projectId;
            UserId = userId;
            UserName = userName ?? throw new ArgumentNullException(nameof(userName));
            Email = email ?? throw new ArgumentNullException(nameof(email));
            Role = role;
            JoinedAt = DateTime.UtcNow;
            IsActive = true;
        }

        public void ChangeRole(ProjectRoleType newRole)
        {
            Role = newRole;
        }

        public void Deactivate() => IsActive = false;
    }
}
