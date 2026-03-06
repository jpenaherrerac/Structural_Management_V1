using System;

namespace App.Domain.Entities.Comparison
{
    public class SourceModelFingerprint
    {
        public Guid Id { get; private set; }
        public Guid SourceId { get; private set; }
        public string FilePath { get; private set; }
        public string Md5Hash { get; private set; }
        public long FileSizeBytes { get; private set; }
        public DateTime CapturedAt { get; private set; }
        public int NumberOfStories { get; set; }
        public int NumberOfElements { get; set; }
        public string SapVersion { get; set; }

        private SourceModelFingerprint() { }

        public SourceModelFingerprint(Guid sourceId, string filePath, string md5Hash, long fileSizeBytes)
        {
            Id = Guid.NewGuid();
            SourceId = sourceId;
            FilePath = filePath ?? throw new ArgumentNullException(nameof(filePath));
            Md5Hash = md5Hash ?? throw new ArgumentNullException(nameof(md5Hash));
            FileSizeBytes = fileSizeBytes;
            CapturedAt = DateTime.UtcNow;
        }

        public bool MatchesHash(string otherHash) =>
            string.Equals(Md5Hash, otherHash, StringComparison.OrdinalIgnoreCase);
    }
}
