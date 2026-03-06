using System;
using System.IO;
using System.Text;
using App.Domain.Entities.Annexes;

namespace App.Application.Export
{
    public class CsvAnnexExporter : IAnnexExporter
    {
        private static readonly Encoding Utf8NoBom = new UTF8Encoding(false);

        public void ExportBeamAnnex(BeamDesignAnnex annex, Stream outputStream)
        {
            if (annex == null) throw new ArgumentNullException(nameof(annex));
            if (outputStream == null) throw new ArgumentNullException(nameof(outputStream));

            using var writer = new StreamWriter(outputStream, Utf8NoBom, 4096, leaveOpen: true);
            writer.WriteLine($"# {annex.Title}");
            writer.WriteLine($"# Generated: {annex.GeneratedAt:yyyy-MM-dd HH:mm}");
            writer.WriteLine($"# fc={annex.Fc}MPa, fy={annex.Fy}MPa");
            writer.WriteLine("ElementId,Story,Section,b(mm),h(mm),d(mm),Mu+(kN-m),Mu-(kN-m),Mn(kN-m),phiMn(kN-m),As_req(cm2),As_min(cm2),As_prov(cm2),Rebar,Vu(kN),Vc(kN),Vs(kN),Vn(kN),phiVn(kN),Stirrups,OK");
            foreach (var row in annex.Rows)
            {
                writer.WriteLine($"{row.ElementId},{row.StoryName},{row.Section},{row.bMm},{row.hMm},{row.dMm:F1}," +
                    $"{row.MuPositiveKNm:F2},{row.MuNegativeKNm:F2},{row.MnKNm:F2},{row.PhiMnKNm:F2}," +
                    $"{row.AsRequiredCm2:F2},{row.AsMinCm2:F2},{row.AsProvidedCm2:F2},{row.LongitudinalRebar}," +
                    $"{row.VuKN:F2},{row.VcKN:F2},{row.VsKN:F2},{row.VnKN:F2},{row.PhiVnKN:F2},{row.TransverseRebar},{(row.IsAdequate ? "OK" : "NG")}");
            }
        }

        public void ExportColumnAnnex(ColumnDesignAnnex annex, Stream outputStream)
        {
            if (annex == null) throw new ArgumentNullException(nameof(annex));
            if (outputStream == null) throw new ArgumentNullException(nameof(outputStream));

            using var writer = new StreamWriter(outputStream, Utf8NoBom, 4096, leaveOpen: true);
            writer.WriteLine($"# {annex.Title}");
            writer.WriteLine($"# Generated: {annex.GeneratedAt:yyyy-MM-dd HH:mm}");
            writer.WriteLine("ElementId,Story,Section,b(mm),h(mm),d(mm),Pu(kN),Mu2(kN-m),Mu3(kN-m),phiPn(kN),phiMn(kN-m),Rho,As(cm2),Rebar,Vu(kN),Vc(kN),Vs(kN),phiVn(kN),Hoops,InDiagram,OK");
            foreach (var row in annex.Rows)
            {
                writer.WriteLine($"{row.ElementId},{row.StoryName},{row.Section},{row.bMm},{row.hMm},{row.dMm:F1}," +
                    $"{row.PuKN:F2},{row.Mu2KNm:F2},{row.Mu3KNm:F2},{row.PhiPnKN:F2},{row.PhiMnKNm:F2}," +
                    $"{row.RhoProvided:F4},{row.AsProvidedCm2:F2},{row.LongitudinalRebar}," +
                    $"{row.VuKN:F2},{row.VcKN:F2},{row.VsKN:F2},{row.PhiVnKN:F2},{row.TransverseRebar}," +
                    $"{(row.IsInInteractionDiagram ? "YES" : "NO")},{(row.IsAdequate ? "OK" : "NG")}");
            }
        }

        public void ExportShearWallAnnex(ShearWallDesignAnnex annex, Stream outputStream)
        {
            if (annex == null) throw new ArgumentNullException(nameof(annex));
            if (outputStream == null) throw new ArgumentNullException(nameof(outputStream));

            using var writer = new StreamWriter(outputStream, Utf8NoBom, 4096, leaveOpen: true);
            writer.WriteLine($"# {annex.Title}");
            writer.WriteLine($"# Generated: {annex.GeneratedAt:yyyy-MM-dd HH:mm}");
            writer.WriteLine("ElementId,Story,L(m),t(mm),H(m),Pu(kN),Mu(kN-m),Vu(kN),phiMn(kN-m),Vc(kN),Vs(kN),phiVn(kN),rhoH,rhoV,Horiz,Vert,BoundaryReq,OK");
            foreach (var row in annex.Rows)
            {
                writer.WriteLine($"{row.ElementId},{row.StoryName},{row.LengthMeters:F2},{row.ThicknessMm:F0},{row.HeightMeters:F2}," +
                    $"{row.PuKN:F2},{row.MuKNm:F2},{row.VuKN:F2},{row.PhiMnKNm:F2}," +
                    $"{row.VcKN:F2},{row.VsKN:F2},{row.PhiVnKN:F2}," +
                    $"{row.RhoHorizontal:F4},{row.RhoVertical:F4},{row.HorizontalRebar},{row.VerticalRebar}," +
                    $"{(row.RequiresBoundaryElements ? "YES" : "NO")},{(row.IsAdequate ? "OK" : "NG")}");
            }
        }
    }
}
