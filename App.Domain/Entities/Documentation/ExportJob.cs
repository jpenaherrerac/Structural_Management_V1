using System;
using App.Domain.Enums;

namespace App.Domain.Entities.Documentation
{
    public class ExportJob
    {
        public Guid Id { get; private set; }
        public Guid ProjectId { get; private set; }
        public ExportFormat Format { get; private set; }
        public string OutputPath { get; private set; }
        public string Status { get; private set; }
        public string ErrorMessage { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public DateTime? CompletedAt { get; private set; }
        public bool IsCompleted => CompletedAt.HasValue;

        private ExportJob() { }

        public ExportJob(Guid projectId, ExportFormat format, string outputPath)
        {
            Id = Guid.NewGuid();
            ProjectId = projectId;
            Format = format;
            OutputPath = outputPath ?? throw new ArgumentNullException(nameof(outputPath));
            Status = "Pending";
            CreatedAt = DateTime.UtcNow;
        }

        public void MarkAsRunning() => Status = "Running";

        public void MarkAsCompleted()
        {
            Status = "Completed";
            CompletedAt = DateTime.UtcNow;
        }

        public void MarkAsFailed(string errorMessage)
        {
            Status = "Failed";
            ErrorMessage = errorMessage;
            CompletedAt = DateTime.UtcNow;
        }
    }
}
