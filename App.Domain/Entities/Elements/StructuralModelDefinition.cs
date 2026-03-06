using System;
using System.Collections.Generic;
using System.Linq;
using App.Domain.Enums;

namespace App.Domain.Entities.Elements
{
    public class StructuralModelDefinition
    {
        public Guid Id { get; private set; }
        public Guid ProjectId { get; private set; }
        public string ModelName { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public DateTime UpdatedAt { get; private set; }
        private readonly List<StructuralElement> _elements = new List<StructuralElement>();
        public IReadOnlyList<StructuralElement> Elements => _elements.AsReadOnly();

        private StructuralModelDefinition() { }

        public StructuralModelDefinition(Guid projectId, string modelName)
        {
            Id = Guid.NewGuid();
            ProjectId = projectId;
            ModelName = modelName ?? throw new ArgumentNullException(nameof(modelName));
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }

        public void AddElement(StructuralElement element)
        {
            if (element == null) throw new ArgumentNullException(nameof(element));
            _elements.Add(element);
            UpdatedAt = DateTime.UtcNow;
        }

        public IEnumerable<Beam> GetBeams() => _elements.OfType<Beam>();
        public IEnumerable<Column> GetColumns() => _elements.OfType<Column>();
        public IEnumerable<ShearWall> GetShearWalls() => _elements.OfType<ShearWall>();
        public IEnumerable<Slab> GetSlabs() => _elements.OfType<Slab>();

        public IEnumerable<StructuralElement> GetByType(ElementType type) =>
            _elements.Where(e => e.Type == type);

        public int TotalElementCount => _elements.Count;
    }
}
