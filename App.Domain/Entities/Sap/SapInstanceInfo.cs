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

        public SapInstanceInfo() { }

        public SapInstanceInfo(string programPath, string modelFilePath, int processId, string sapVersion)
        {
            ProgramPath = programPath ?? string.Empty;
            ModelFilePath = modelFilePath ?? string.Empty;
            ProcessId = processId;
            SapVersion = sapVersion ?? string.Empty;
            LaunchedAt = DateTime.UtcNow;
        }
    }
}
