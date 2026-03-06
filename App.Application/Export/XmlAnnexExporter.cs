using System;
using System.IO;
using System.Xml;
using App.Domain.Entities.Annexes;

namespace App.Application.Export
{
    public class XmlAnnexExporter : IAnnexExporter
    {
        public void ExportBeamAnnex(BeamDesignAnnex annex, Stream outputStream)
        {
            if (annex == null) throw new ArgumentNullException(nameof(annex));
            var settings = new XmlWriterSettings { Indent = true };
            using var writer = XmlWriter.Create(outputStream, settings);
            writer.WriteStartDocument();
            writer.WriteStartElement("BeamDesignAnnex");
            writer.WriteAttributeString("Title", annex.Title);
            writer.WriteAttributeString("GeneratedAt", annex.GeneratedAt.ToString("yyyy-MM-dd HH:mm"));
            writer.WriteAttributeString("fc", annex.Fc.ToString("F1"));
            writer.WriteAttributeString("fy", annex.Fy.ToString("F1"));
            foreach (var row in annex.Rows)
            {
                writer.WriteStartElement("Beam");
                writer.WriteAttributeString("id", row.ElementId);
                writer.WriteAttributeString("story", row.StoryName);
                writer.WriteAttributeString("section", row.Section);
                writer.WriteAttributeString("b", row.bMm.ToString());
                writer.WriteAttributeString("h", row.hMm.ToString());
                writer.WriteAttributeString("MuPlus", row.MuPositiveKNm.ToString("F2"));
                writer.WriteAttributeString("MuMinus", row.MuNegativeKNm.ToString("F2"));
                writer.WriteAttributeString("phiMn", row.PhiMnKNm.ToString("F2"));
                writer.WriteAttributeString("AsReq", row.AsRequiredCm2.ToString("F2"));
                writer.WriteAttributeString("AsProv", row.AsProvidedCm2.ToString("F2"));
                writer.WriteAttributeString("Rebar", row.LongitudinalRebar);
                writer.WriteAttributeString("Vu", row.VuKN.ToString("F2"));
                writer.WriteAttributeString("phiVn", row.PhiVnKN.ToString("F2"));
                writer.WriteAttributeString("Stirrups", row.TransverseRebar);
                writer.WriteAttributeString("OK", row.IsAdequate ? "true" : "false");
                writer.WriteEndElement();
            }
            writer.WriteEndElement();
            writer.WriteEndDocument();
        }

        public void ExportColumnAnnex(ColumnDesignAnnex annex, Stream outputStream)
        {
            if (annex == null) throw new ArgumentNullException(nameof(annex));
            var settings = new XmlWriterSettings { Indent = true };
            using var writer = XmlWriter.Create(outputStream, settings);
            writer.WriteStartDocument();
            writer.WriteStartElement("ColumnDesignAnnex");
            writer.WriteAttributeString("Title", annex.Title);
            writer.WriteAttributeString("GeneratedAt", annex.GeneratedAt.ToString("yyyy-MM-dd HH:mm"));
            foreach (var row in annex.Rows)
            {
                writer.WriteStartElement("Column");
                writer.WriteAttributeString("id", row.ElementId);
                writer.WriteAttributeString("story", row.StoryName);
                writer.WriteAttributeString("Pu", row.PuKN.ToString("F2"));
                writer.WriteAttributeString("Mu2", row.Mu2KNm.ToString("F2"));
                writer.WriteAttributeString("Mu3", row.Mu3KNm.ToString("F2"));
                writer.WriteAttributeString("phiPn", row.PhiPnKN.ToString("F2"));
                writer.WriteAttributeString("phiMn", row.PhiMnKNm.ToString("F2"));
                writer.WriteAttributeString("Rho", row.RhoProvided.ToString("F4"));
                writer.WriteAttributeString("Rebar", row.LongitudinalRebar);
                writer.WriteAttributeString("Hoops", row.TransverseRebar);
                writer.WriteAttributeString("InDiagram", row.IsInInteractionDiagram ? "true" : "false");
                writer.WriteAttributeString("OK", row.IsAdequate ? "true" : "false");
                writer.WriteEndElement();
            }
            writer.WriteEndElement();
        }

        public void ExportShearWallAnnex(ShearWallDesignAnnex annex, Stream outputStream)
        {
            if (annex == null) throw new ArgumentNullException(nameof(annex));
            var settings = new XmlWriterSettings { Indent = true };
            using var writer = XmlWriter.Create(outputStream, settings);
            writer.WriteStartDocument();
            writer.WriteStartElement("ShearWallDesignAnnex");
            writer.WriteAttributeString("Title", annex.Title);
            writer.WriteAttributeString("GeneratedAt", annex.GeneratedAt.ToString("yyyy-MM-dd HH:mm"));
            foreach (var row in annex.Rows)
            {
                writer.WriteStartElement("ShearWall");
                writer.WriteAttributeString("id", row.ElementId);
                writer.WriteAttributeString("story", row.StoryName);
                writer.WriteAttributeString("L", row.LengthMeters.ToString("F2"));
                writer.WriteAttributeString("t", row.ThicknessMm.ToString("F0"));
                writer.WriteAttributeString("phiMn", row.PhiMnKNm.ToString("F2"));
                writer.WriteAttributeString("phiVn", row.PhiVnKN.ToString("F2"));
                writer.WriteAttributeString("BoundaryReq", row.RequiresBoundaryElements ? "true" : "false");
                writer.WriteAttributeString("OK", row.IsAdequate ? "true" : "false");
                writer.WriteEndElement();
            }
            writer.WriteEndElement();
        }
    }
}
