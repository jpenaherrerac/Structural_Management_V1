using System;

namespace App.Domain.Entities.Sap
{
    public class SapSession
    {
        public Guid Id { get; private set; }
        public Guid ProjectId { get; private set; }
        public string MachineName { get; private set; }
        public string SapVersion { get; private set; }
        public DateTime ConnectedAt { get; private set; }
        public DateTime? DisconnectedAt { get; private set; }
        public bool IsActive => DisconnectedAt == null;
        public SapInstanceInfo InstanceInfo { get; private set; }

        private SapSession() { }

        public SapSession(Guid projectId, string machineName, string sapVersion, SapInstanceInfo instanceInfo)
        {
            Id = Guid.NewGuid();
            ProjectId = projectId;
            MachineName = machineName ?? Environment.MachineName;
            SapVersion = sapVersion ?? throw new ArgumentNullException(nameof(sapVersion));
            InstanceInfo = instanceInfo ?? throw new ArgumentNullException(nameof(instanceInfo));
            ConnectedAt = DateTime.UtcNow;
        }

        public void Disconnect()
        {
            DisconnectedAt = DateTime.UtcNow;
        }

        public TimeSpan GetSessionDuration()
        {
            var end = DisconnectedAt ?? DateTime.UtcNow;
            return end - ConnectedAt;
        }
    }
}
