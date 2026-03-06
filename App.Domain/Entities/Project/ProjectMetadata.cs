using System;

namespace App.Domain.Entities.Project
{
    public class ProjectMetadata
    {
        public string Location { get; set; }
        public string Client { get; set; }
        public string ProjectManager { get; set; }
        public string StructuralSystem { get; set; }
        public int NumberOfStoreys { get; set; }
        public double TotalHeightMeters { get; set; }
        public string BuildingUse { get; set; }
        public string DesignCode { get; set; }
        public string SapModelPath { get; set; }
        public DateTime? DesignStartDate { get; set; }
        public DateTime? DesignEndDate { get; set; }

        public ProjectMetadata() { }

        public ProjectMetadata(string location, string client, string designCode)
        {
            Location = location ?? throw new ArgumentNullException(nameof(location));
            Client = client ?? throw new ArgumentNullException(nameof(client));
            DesignCode = designCode ?? throw new ArgumentNullException(nameof(designCode));
        }
    }
}
