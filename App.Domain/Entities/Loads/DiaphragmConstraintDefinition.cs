using System;

namespace App.Domain.Entities.Loads
{
    public class DiaphragmConstraintDefinition
    {
        public Guid Id { get; private set; }
        public string Name { get; private set; }
        public string StoryName { get; private set; }
        public string ConstraintType { get; private set; }
        public bool IsRigid { get; private set; }

        private DiaphragmConstraintDefinition() { }

        public DiaphragmConstraintDefinition(string name, string storyName, bool isRigid = true)
        {
            Id = Guid.NewGuid();
            Name = name ?? throw new ArgumentNullException(nameof(name));
            StoryName = storyName ?? throw new ArgumentNullException(nameof(storyName));
            IsRigid = isRigid;
            ConstraintType = isRigid ? "Rigid" : "SemiRigid";
        }
    }
}
