using System;
using System.Collections.Generic;

namespace App.Domain.Entities.Sources
{
    public class ExecutedCommandSet
    {
        public Guid Id { get; private set; }
        public string SetName { get; private set; }
        public DateTime ExecutedAt { get; private set; }
        public double TotalDurationMs { get; private set; }
        private readonly List<string> _commandNames = new List<string>();
        public IReadOnlyList<string> CommandNames => _commandNames.AsReadOnly();
        private readonly List<string> _warnings = new List<string>();
        public IReadOnlyList<string> Warnings => _warnings.AsReadOnly();

        private ExecutedCommandSet() { }

        public ExecutedCommandSet(string setName)
        {
            Id = Guid.NewGuid();
            SetName = setName ?? throw new ArgumentNullException(nameof(setName));
            ExecutedAt = DateTime.UtcNow;
        }

        public void AddCommand(string commandName) => _commandNames.Add(commandName ?? string.Empty);
        public void AddWarning(string warning) => _warnings.Add(warning ?? string.Empty);
        public void SetDuration(double ms) => TotalDurationMs = ms;
    }
}
