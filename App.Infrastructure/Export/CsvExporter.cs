using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using App.Domain.Entities.Documentation;

namespace App.Infrastructure.Export
{
    public class CsvExporter
    {
        private static readonly Encoding Utf8NoBom = new UTF8Encoding(false);

        public void ExportTable(DocumentationTable table, string filePath)
        {
            if (table == null) throw new ArgumentNullException(nameof(table));
            if (string.IsNullOrWhiteSpace(filePath)) throw new ArgumentNullException(nameof(filePath));

            Directory.CreateDirectory(Path.GetDirectoryName(filePath) ?? ".");
            using var writer = new StreamWriter(filePath, false, Utf8NoBom);
            writer.WriteLine(EscapeCsvRow(table.Headers));
            foreach (var row in table.Rows)
                writer.WriteLine(EscapeCsvRow(row));
        }

        public void ExportToStream(DocumentationTable table, Stream stream)
        {
            if (table == null) throw new ArgumentNullException(nameof(table));
            if (stream == null) throw new ArgumentNullException(nameof(stream));

            using var writer = new StreamWriter(stream, Utf8NoBom, 4096, leaveOpen: true);
            writer.WriteLine(EscapeCsvRow(table.Headers));
            foreach (var row in table.Rows)
                writer.WriteLine(EscapeCsvRow(row));
        }

        private static string EscapeCsvRow(IEnumerable<string> cells)
        {
            var sb = new StringBuilder();
            bool first = true;
            foreach (var cell in cells)
            {
                if (!first) sb.Append(',');
                first = false;
                var val = cell ?? string.Empty;
                if (val.Contains(',') || val.Contains('"') || val.Contains('\n'))
                    sb.Append('"').Append(val.Replace("\"", "\"\"")).Append('"');
                else
                    sb.Append(val);
            }
            return sb.ToString();
        }
    }
}
