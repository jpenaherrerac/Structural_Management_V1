using System;

namespace App.Domain.Entities.Sap
{
    public class SapInstanceInfo
    {
        public string ProgramPath { get; set; }
        public string ModelFilePath { get; set; }
        public int ProcessId { get; set; }
        public string SapVersion { get; set; }
        public bool IsNewInstance { get; set; }
        public bool IsVisible { get; set; }
        public DateTime LaunchedAt { get; set; }
        public string WindowTitle { get; set; }

        /// <summary>
        /// Human-readable label for UI display (e.g. "SAP2000 – Model1.sdb [PID 1234]").
        /// </summary>
        public string DisplayName =>
            string.IsNullOrWhiteSpace(WindowTitle)
                ? $"SAP2000 [PID {ProcessId}]"
                : $"{WindowTitle} [PID {ProcessId}]";

        public SapInstanceInfo() { WindowTitle = string.Empty; }

        public SapInstanceInfo(string programPath, string modelFilePath, int processId, string sapVersion)
        {
            ProgramPath = programPath ?? string.Empty;
            ModelFilePath = modelFilePath ?? string.Empty;
            ProcessId = processId;
            SapVersion = sapVersion ?? string.Empty;
            LaunchedAt = DateTime.UtcNow;
            WindowTitle = string.Empty;
        }
    }
}
