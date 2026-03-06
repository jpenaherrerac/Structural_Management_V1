using System;

namespace App.Domain.Entities.Seismic
{
    public class StructureOutputSnapshot
    {
        public Guid Id { get; private set; }
        public Guid ProjectId { get; private set; }
        public string Label { get; private set; }
        public DateTime CapturedAt { get; private set; }
        public GlobalSeismicSummary GlobalSummary { get; set; }
        public ModalDataSet ModalData { get; set; }
        public StoryDataSet StoryData { get; set; }
        public DriftDataSet DriftData { get; set; }

        private StructureOutputSnapshot() { }

        public StructureOutputSnapshot(Guid projectId, string label)
        {
            Id = Guid.NewGuid();
            ProjectId = projectId;
            Label = label ?? throw new ArgumentNullException(nameof(label));
            CapturedAt = DateTime.UtcNow;
        }
    }
}
