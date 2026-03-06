using System;
using System.IO;
using System.Xml;
using App.Domain.Entities.Documentation;

namespace App.Infrastructure.Export
{
    public class XmlExporter
    {
        public void ExportReport(EngineeringReportPackage report, string filePath)
        {
            if (report == null) throw new ArgumentNullException(nameof(report));
            if (string.IsNullOrWhiteSpace(filePath)) throw new ArgumentNullException(nameof(filePath));

            Directory.CreateDirectory(Path.GetDirectoryName(filePath) ?? ".");
            var settings = new XmlWriterSettings { Indent = true, Encoding = System.Text.Encoding.UTF8 };
            using var writer = XmlWriter.Create(filePath, settings);
            WriteReport(writer, report);
        }

        public void ExportReportToStream(EngineeringReportPackage report, Stream stream)
        {
            if (report == null) throw new ArgumentNullException(nameof(report));
            var settings = new XmlWriterSettings { Indent = true };
            using var writer = XmlWriter.Create(stream, settings);
            WriteReport(writer, report);
        }

        private static void WriteReport(XmlWriter writer, EngineeringReportPackage report)
        {
            writer.WriteStartDocument();
            writer.WriteStartElement("EngineeringReport");
            writer.WriteAttributeString("Id", report.Id.ToString());
            writer.WriteAttributeString("Title", report.Title);
            writer.WriteAttributeString("ProjectName", report.ProjectName);
            writer.WriteAttributeString("PreparedBy", report.PreparedBy ?? string.Empty);
            writer.WriteAttributeString("ReviewedBy", report.ReviewedBy ?? string.Empty);
            writer.WriteAttributeString("ApprovedBy", report.ApprovedBy ?? string.Empty);
            writer.WriteAttributeString("PreparedDate", report.PreparedDate.ToString("yyyy-MM-dd"));
            writer.WriteAttributeString("Revision", report.RevisionLabel ?? string.Empty);

            foreach (var section in report.Sections)
            {
                writer.WriteStartElement("Section");
                writer.WriteAttributeString("Title", section.Title);
                writer.WriteAttributeString("Order", section.Order.ToString());
                writer.WriteAttributeString("Type", section.SectionType);
                writer.WriteElementString("Content", section.Content);

                foreach (var table in section.Tables)
                {
                    writer.WriteStartElement("Table");
                    writer.WriteAttributeString("Title", table.Title);

                    writer.WriteStartElement("Headers");
                    foreach (var h in table.Headers)
                        writer.WriteElementString("Header", h);
                    writer.WriteEndElement();

                    writer.WriteStartElement("Rows");
                    foreach (var row in table.Rows)
                    {
                        writer.WriteStartElement("Row");
                        foreach (var cell in row)
                            writer.WriteElementString("Cell", cell);
                        writer.WriteEndElement();
                    }
                    writer.WriteEndElement();

                    writer.WriteEndElement();
                }
                writer.WriteEndElement();
            }

            writer.WriteEndElement();
            writer.WriteEndDocument();
        }
    }
}
