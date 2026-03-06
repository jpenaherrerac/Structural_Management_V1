using System;

namespace App.Domain.Entities.Sap
{
    public class SapCommandExecutionTrace
    {
        public Guid Id { get; private set; }
        public Guid SessionId { get; private set; }
        public string CommandName { get; private set; }
        public string Parameters { get; private set; }
        public int ReturnCode { get; private set; }
        public bool IsSuccess { get; private set; }
        public string ErrorMessage { get; private set; }
        public DateTime ExecutedAt { get; private set; }
        public double DurationMilliseconds { get; private set; }

        private SapCommandExecutionTrace() { }

        public SapCommandExecutionTrace(Guid sessionId, string commandName, string parameters)
        {
            Id = Guid.NewGuid();
            SessionId = sessionId;
            CommandName = commandName ?? throw new ArgumentNullException(nameof(commandName));
            Parameters = parameters ?? string.Empty;
            ExecutedAt = DateTime.UtcNow;
        }

        public void RecordSuccess(int returnCode, double durationMs)
        {
            ReturnCode = returnCode;
            IsSuccess = true;
            DurationMilliseconds = durationMs;
        }

        public void RecordFailure(int returnCode, string errorMessage, double durationMs)
        {
            ReturnCode = returnCode;
            IsSuccess = false;
            ErrorMessage = errorMessage ?? string.Empty;
            DurationMilliseconds = durationMs;
        }
    }
}
