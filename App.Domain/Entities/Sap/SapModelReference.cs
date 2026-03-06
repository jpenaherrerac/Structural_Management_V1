using System;

namespace App.Domain.Entities.Sap
{
    public class SapModelReference
    {
        public Guid Id { get; private set; }
        public Guid ProjectId { get; private set; }
        public string ModelFilePath { get; private set; }
        public string ModelName { get; private set; }
        public DateTime LastAccessedAt { get; private set; }
        public string ChecksumMd5 { get; private set; }
        public long FileSizeBytes { get; private set; }
        public bool IsLocked { get; private set; }

        private SapModelReference() { }

        public SapModelReference(Guid projectId, string modelFilePath, string modelName)
        {
            Id = Guid.NewGuid();
            ProjectId = projectId;
            ModelFilePath = modelFilePath ?? throw new ArgumentNullException(nameof(modelFilePath));
            ModelName = modelName ?? System.IO.Path.GetFileNameWithoutExtension(modelFilePath);
            LastAccessedAt = DateTime.UtcNow;
        }

        public void UpdateChecksum(string md5, long fileSizeBytes)
        {
            ChecksumMd5 = md5;
            FileSizeBytes = fileSizeBytes;
            LastAccessedAt = DateTime.UtcNow;
        }

        public void Lock() => IsLocked = true;
        public void Unlock() => IsLocked = false;
    }
}
