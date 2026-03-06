using System;
using System.Collections.Generic;

namespace App.Domain.Entities.Documentation
{
    public class EngineeringReportPackage
    {
        public Guid Id { get; private set; }
        public Guid ProjectId { get; private set; }
        public string Title { get; private set; }
        public string ProjectName { get; private set; }
        public string PreparedBy { get; private set; }
        public string ReviewedBy { get; private set; }
        public string ApprovedBy { get; private set; }
        public DateTime PreparedDate { get; private set; }
        public string RevisionLabel { get; private set; }
        private readonly List<DocumentationSection> _sections = new List<DocumentationSection>();
        public IReadOnlyList<DocumentationSection> Sections => _sections.AsReadOnly();

        private EngineeringReportPackage() { }

        public EngineeringReportPackage(Guid projectId, string title, string projectName)
        {
            Id = Guid.NewGuid();
            ProjectId = projectId;
            Title = title ?? throw new ArgumentNullException(nameof(title));
            ProjectName = projectName ?? throw new ArgumentNullException(nameof(projectName));
            PreparedDate = DateTime.UtcNow;
        }

        public void SetAuthors(string preparedBy, string reviewedBy, string approvedBy)
        {
            PreparedBy = preparedBy;
            ReviewedBy = reviewedBy;
            ApprovedBy = approvedBy;
        }

        public void AddSection(DocumentationSection section)
        {
            if (section != null) _sections.Add(section);
        }

        public void SetRevision(string label) => RevisionLabel = label;
    }
}
