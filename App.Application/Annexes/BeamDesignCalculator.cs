using System;
using App.Domain.Entities.Design;
using App.Domain.Entities.Annexes;

namespace App.Application.Annexes
{
    /// <summary>
    /// Calculates beam flexural and shear design per ACI 318.
    /// </summary>
    public class BeamDesignCalculator
    {
        private const double PhiFlexure = 0.90;
        private const double PhiShear = 0.85;

        public BeamDesignReportRow Calculate(BeamDesignData data)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));

            double b = data.WidthMm;
            double h = data.DepthMm;
            double cover = data.CoverMm > 0 ? data.CoverMm : 40.0;
            double d = h - cover - 8.0;   // effective depth (assume 8mm stirrup radius approx)
            double fc = data.Fc;           // MPa
            double fy = data.Fy;           // MPa

            double muMax = Math.Max(Math.Abs(data.MuPositiveKNm),
                Math.Max(Math.Abs(data.MuNegativeStartKNm), Math.Abs(data.MuNegativeEndKNm)));

            double muNm = muMax * 1e6; // kN-m to N-mm

            double rhoBalance = (0.85 * 0.85 * fc / fy) * (600.0 / (600.0 + fy));
            double rhoMax = 0.75 * rhoBalance;
            double rhoMin = Math.Max(0.25 * Math.Sqrt(fc) / fy, 1.4 / fy);

            double Rn = muNm / (PhiFlexure * b * d * d);
            double rhoRequired = (0.85 * fc / fy) * (1.0 - Math.Sqrt(1.0 - 2.0 * Rn / (0.85 * fc)));
            double rhoDesign = Math.Max(rhoRequired, rhoMin);
            rhoDesign = Math.Min(rhoDesign, rhoMax);

            double asRequired = rhoRequired * b * d / 100.0;    // cm²
            double asMin = rhoMin * b * d / 100.0;               // cm²
            double asDesign = rhoDesign * b * d / 100.0;         // cm²

            double Mn = rhoDesign * (b * d * d) * fy * (1.0 - rhoDesign * fy / (1.7 * fc)) / 1e6;
            double phiMn = PhiFlexure * Mn;

            // Shear design
            double vuN = data.VuKN * 1000.0;
            double Vc = 0.17 * Math.Sqrt(fc) * b * d;
            double Vs = (vuN / PhiShear - Vc);
            if (Vs < 0) Vs = 0;
            double Vn = Vc + Vs;
            double phiVn = PhiShear * Vn;

            bool isAdequate = phiMn >= muMax && phiVn >= data.VuKN * 1000.0;

            return new BeamDesignReportRow
            {
                ElementId = data.ElementId,
                StoryName = data.StoryName,
                Section = data.SectionName,
                bMm = b,
                hMm = h,
                dMm = d,
                MuPositiveKNm = data.MuPositiveKNm,
                MuNegativeKNm = Math.Max(Math.Abs(data.MuNegativeStartKNm), Math.Abs(data.MuNegativeEndKNm)),
                MnKNm = Mn,
                PhiMnKNm = phiMn,
                AsRequiredCm2 = Math.Round(asRequired, 2),
                AsMinCm2 = Math.Round(asMin, 2),
                AsProvidedCm2 = Math.Round(asDesign, 2),
                LongitudinalRebar = SuggestRebar(asDesign),
                VuKN = data.VuKN,
                VcKN = Math.Round(Vc / 1000.0, 2),
                VsKN = Math.Round(Vs / 1000.0, 2),
                VnKN = Math.Round(Vn / 1000.0, 2),
                PhiVnKN = Math.Round(phiVn / 1000.0, 2),
                TransverseRebar = SuggestStirrup(Vs, b),
                IsAdequate = isAdequate,
                Notes = isAdequate ? "OK" : "CHECK REQUIRED"
            };
        }

        private static string SuggestRebar(double asCm2)
        {
            if (asCm2 <= 3.14) return "2Ø14";
            if (asCm2 <= 5.09) return "2Ø18";
            if (asCm2 <= 6.28) return "2Ø20";
            if (asCm2 <= 10.18) return "2Ø25";
            if (asCm2 <= 15.70) return "2Ø32";
            return $"As={asCm2:F1}cm² (verify)";
        }

        private static string SuggestStirrup(double vsN, double bMm)
        {
            if (vsN <= 0) return "Ø8@200mm";
            double sRequired = (0.785 * 2 * 280e6 * (bMm * 0.9)) / vsN;
            int spacing = (int)Math.Min(200, Math.Max(75, sRequired));
            return $"Ø8@{spacing}mm";
        }
    }
}
