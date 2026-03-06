using System;
using System.Collections.Generic;

namespace App.Domain.Entities.Documentation
{
    public class DocumentationSection
    {
        public Guid Id { get; private set; }
        public string Title { get; private set; }
        public string Content { get; private set; }
        public int Order { get; private set; }
        public string SectionType { get; private set; }
        private readonly List<DocumentationTable> _tables = new List<DocumentationTable>();
        public IReadOnlyList<DocumentationTable> Tables => _tables.AsReadOnly();

        private DocumentationSection() { }

        public DocumentationSection(string title, string content, int order, string sectionType = "General")
        {
            Id = Guid.NewGuid();
            Title = title ?? throw new ArgumentNullException(nameof(title));
            Content = content ?? string.Empty;
            Order = order;
            SectionType = sectionType;
        }

        public void AddTable(DocumentationTable table)
        {
            if (table != null) _tables.Add(table);
        }

        public void UpdateContent(string content) => Content = content ?? string.Empty;
    }
}
