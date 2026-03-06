using System;
using System.Collections.Generic;

namespace App.Domain.Entities.Documentation
{
    public class DocumentationTable
    {
        public Guid Id { get; private set; }
        public string Title { get; private set; }
        public string Caption { get; private set; }
        public List<string> Headers { get; private set; }
        public List<List<string>> Rows { get; private set; }

        private DocumentationTable() { }

        public DocumentationTable(string title, IEnumerable<string> headers)
        {
            Id = Guid.NewGuid();
            Title = title ?? throw new ArgumentNullException(nameof(title));
            Headers = new List<string>(headers ?? Array.Empty<string>());
            Rows = new List<List<string>>();
        }

        public void AddRow(IEnumerable<string> cells)
        {
            Rows.Add(new List<string>(cells ?? Array.Empty<string>()));
        }

        public void SetCaption(string caption) => Caption = caption;
        public int RowCount => Rows.Count;
        public int ColumnCount => Headers.Count;
    }
}
