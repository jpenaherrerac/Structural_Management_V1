using System;
using System.IO;
using App.Domain.Entities.Documentation;

namespace App.Infrastructure.Export
{
    /// <summary>
    /// XLS exporter stub. Requires NPOI or ClosedXML in a real implementation.
    /// Currently writes a CSV with .xls extension as a placeholder.
    /// </summary>
    public class XlsExporter
    {
        private readonly CsvExporter _csvFallback = new CsvExporter();

        public void ExportTable(DocumentationTable table, string filePath)
        {
            if (table == null) throw new ArgumentNullException(nameof(table));
            if (string.IsNullOrWhiteSpace(filePath)) throw new ArgumentNullException(nameof(filePath));

            // Stub: use CSV export as fallback until NPOI is added
            _csvFallback.ExportTable(table, filePath);
        }

        public void ExportReport(EngineeringReportPackage report, string filePath)
        {
            if (report == null) throw new ArgumentNullException(nameof(report));
            if (string.IsNullOrWhiteSpace(filePath)) throw new ArgumentNullException(nameof(filePath));

            Directory.CreateDirectory(Path.GetDirectoryName(filePath) ?? ".");

            // Stub: write each section's first table as CSV until NPOI/ClosedXML is wired up
            foreach (var section in report.Sections)
            {
                foreach (var table in section.Tables)
                {
                    var sectionPath = Path.Combine(
                        Path.GetDirectoryName(filePath) ?? ".",
                        $"{Path.GetFileNameWithoutExtension(filePath)}_{section.Order}_{table.Title}.csv");
                    _csvFallback.ExportTable(table, sectionPath);
                }
            }
        }
    }
}
