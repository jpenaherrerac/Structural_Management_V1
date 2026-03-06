using System;
using System.Collections.Generic;
using App.Domain.Enums;

namespace App.Domain.Entities.Sources
{
    public class DesignSource
    {
        public Guid Id { get; private set; }
        public Guid ProjectId { get; private set; }
        public string Label { get; private set; }
        public HydrationPurpose Purpose { get; private set; }
        public DateTime HydratedAt { get; private set; }
        public string SapModelPath { get; private set; }
        public HydrationMetadata Metadata { get; private set; }
        public ExecutedCommandSet CommandSet { get; private set; }
        private readonly List<string> _elementIds = new List<string>();
        public IReadOnlyList<string> ElementIds => _elementIds.AsReadOnly();

        private DesignSource() { }

        public DesignSource(Guid projectId, string label, string sapModelPath, HydrationPurpose purpose)
        {
            Id = Guid.NewGuid();
            ProjectId = projectId;
            Label = label ?? throw new ArgumentNullException(nameof(label));
            SapModelPath = sapModelPath ?? throw new ArgumentNullException(nameof(sapModelPath));
            Purpose = purpose;
            HydratedAt = DateTime.UtcNow;
        }

        public void AttachMetadata(HydrationMetadata metadata)
        {
            Metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
        }

        public void AttachCommandSet(ExecutedCommandSet commandSet)
        {
            CommandSet = commandSet ?? throw new ArgumentNullException(nameof(commandSet));
        }

        public void AddElementId(string elementId)
        {
            if (!string.IsNullOrWhiteSpace(elementId))
                _elementIds.Add(elementId);
        }
    }
}
