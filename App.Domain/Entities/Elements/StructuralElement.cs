using System;
using App.Domain.Enums;

namespace App.Domain.Entities.Elements
{
    public abstract class StructuralElement
    {
        public Guid Id { get; protected set; }
        public string ElementId { get; protected set; }
        public string Label { get; protected set; }
        public ElementType Type { get; protected set; }
        public string StoryName { get; protected set; }
        public string SectionName { get; protected set; }
        public string MaterialName { get; protected set; }
        public bool IsActive { get; protected set; }
        public DateTime CreatedAt { get; protected set; }

        protected StructuralElement() { }

        protected StructuralElement(string elementId, string label, ElementType type)
        {
            Id = Guid.NewGuid();
            ElementId = elementId ?? throw new ArgumentNullException(nameof(elementId));
            Label = label ?? elementId;
            Type = type;
            IsActive = true;
            CreatedAt = DateTime.UtcNow;
        }

        public void SetSection(string sectionName)
        {
            SectionName = sectionName;
        }

        public void SetMaterial(string materialName)
        {
            MaterialName = materialName;
        }

        public void SetStory(string storyName)
        {
            StoryName = storyName;
        }

        public void Deactivate() => IsActive = false;

        public abstract string GetElementDescription();
    }
}
