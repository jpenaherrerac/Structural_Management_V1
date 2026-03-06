using System;
using System.Collections.Generic;

namespace App.Domain.Entities.Design
{
    public class DesignSnapshot
    {
        public Guid Id { get; private set; }
        public Guid ProjectId { get; private set; }
        public string Label { get; private set; }
        public DateTime CapturedAt { get; private set; }
        private readonly List<BeamDesignData> _beams = new List<BeamDesignData>();
        public IReadOnlyList<BeamDesignData> Beams => _beams.AsReadOnly();
        private readonly List<ColumnDesignData> _columns = new List<ColumnDesignData>();
        public IReadOnlyList<ColumnDesignData> Columns => _columns.AsReadOnly();
        private readonly List<WallDesignData> _walls = new List<WallDesignData>();
        public IReadOnlyList<WallDesignData> Walls => _walls.AsReadOnly();
        private readonly List<SlabDesignData> _slabs = new List<SlabDesignData>();
        public IReadOnlyList<SlabDesignData> Slabs => _slabs.AsReadOnly();
        private readonly List<ElementForceRecord> _forces = new List<ElementForceRecord>();
        public IReadOnlyList<ElementForceRecord> Forces => _forces.AsReadOnly();

        private DesignSnapshot() { }

        public DesignSnapshot(Guid projectId, string label)
        {
            Id = Guid.NewGuid();
            ProjectId = projectId;
            Label = label ?? throw new ArgumentNullException(nameof(label));
            CapturedAt = DateTime.UtcNow;
        }

        public void AddBeam(BeamDesignData data) => _beams.Add(data);
        public void AddColumn(ColumnDesignData data) => _columns.Add(data);
        public void AddWall(WallDesignData data) => _walls.Add(data);
        public void AddSlab(SlabDesignData data) => _slabs.Add(data);
        public void AddForce(ElementForceRecord record) => _forces.Add(record);
    }
}
