using System;

namespace App.Domain.Entities.Loads
{
    public class MassSourceDefinition
    {
        public Guid Id { get; private set; }
        public string Name { get; private set; }
        public bool IsDefault { get; private set; }
        public bool IncludeElementMasses { get; private set; }
        public bool IncludeAdditionalMasses { get; private set; }
        public bool IncludeLoadsAsLateral { get; private set; }

        private MassSourceDefinition() { }

        public MassSourceDefinition(string name, bool includeElementMasses = true,
            bool includeAdditionalMasses = true, bool includeLoadsAsLateral = false)
        {
            Id = Guid.NewGuid();
            Name = name ?? throw new ArgumentNullException(nameof(name));
            IncludeElementMasses = includeElementMasses;
            IncludeAdditionalMasses = includeAdditionalMasses;
            IncludeLoadsAsLateral = includeLoadsAsLateral;
        }

        public void SetAsDefault() => IsDefault = true;
    }
}
