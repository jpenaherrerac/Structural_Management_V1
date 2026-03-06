using System;
using System.Collections.Generic;
using App.Domain.Entities.Design;
using App.Domain.Entities.Annexes;

namespace App.Application.Annexes
{
    /// <summary>
    /// Calculates column axial-flexure interaction and shear design per ACI 318.
    /// </summary>
    public class ColumnDesignCalculator
    {
        private const double PhiCompression = 0.65;
        private const double PhiShear = 0.85;
        /// <summary>ACI 318 β1 factor for fc ≤ 28 MPa.</summary>
        private const double Beta1 = 0.85;
        /// <summary>Steel strain at balanced failure (εs = εy, εcu = 0.003 → 600 MPa ratio in SI).</summary>
        private const double BalancedStrainRatioMPa = 600.0;

        public ColumnDesignReportRow Calculate(ColumnDesignData data, double rhoProvided = 0.02)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));

            double b = data.WidthMm;
            double h = data.DepthMm;
            double cover = data.CoverMm > 0 ? data.CoverMm : 40.0;
            double d = h - cover - 8.0;
            double Ag = b * h;              // mm²
            double fc = data.Fc;
            double fy = data.Fy;

            double rhoMin = 0.01;
            double rhoMax = 0.08;
            double rho = Math.Max(rhoMin, Math.Min(rhoMax, rhoProvided));
            double Ast = rho * Ag;          // mm²

            double Po = 0.85 * fc * (Ag - Ast) + fy * Ast;
            double Pn = 0.80 * Po;         // tied column (ACI 318 §22.4.2.1)
            double phiPn = PhiCompression * Pn;

            double mu = Math.Max(Math.Abs(data.Mu2KNm), Math.Abs(data.Mu3KNm));
            double muNm = mu * 1e6;

            double e = (muNm / (data.PuKN * 1000.0));
            double eMin = 0.1 * h;
            e = Math.Max(e, eMin);

            double Mn = Pn * e / 1e6;
            double phiMn = PhiCompression * Mn;

            bool inDiagram = (data.PuKN * 1000.0 <= phiPn) && (muNm <= phiMn * 1e6);

            // Shear
            double Nu = data.PuKN * 1000.0;
            double Vc = 0.17 * (1 + Nu / (14 * Ag)) * Math.Sqrt(fc) * b * d;
            double vuN = data.VuKN * 1000.0;
            double Vs = vuN / PhiShear - Vc;
            if (Vs < 0) Vs = 0;
            double Vn = Vc + Vs;
            double phiVn = PhiShear * Vn;

            bool isAdequate = inDiagram && phiVn >= vuN;

            return new ColumnDesignReportRow
            {
                ElementId = data.ElementId,
                StoryName = data.StoryName,
                Section = data.SectionName,
                bMm = b,
                hMm = h,
                dMm = d,
                PuKN = data.PuKN,
                Mu2KNm = data.Mu2KNm,
                Mu3KNm = data.Mu3KNm,
                PnKN = Math.Round(Pn / 1000.0, 2),
                MnKNm = Math.Round(Mn, 2),
                PhiPnKN = Math.Round(phiPn / 1000.0, 2),
                PhiMnKNm = Math.Round(phiMn, 2),
                RhoMin = rhoMin,
                RhoMax = rhoMax,
                RhoProvided = rho,
                AsProvidedCm2 = Math.Round(Ast / 100.0, 2),
                LongitudinalRebar = SuggestColumnRebar(Ast / 100.0),
                VuKN = data.VuKN,
                VcKN = Math.Round(Vc / 1000.0, 2),
                VsKN = Math.Round(Vs / 1000.0, 2),
                PhiVnKN = Math.Round(phiVn / 1000.0, 2),
                TransverseRebar = SuggestHoops(Vs, b),
                IsInInteractionDiagram = inDiagram,
                IsAdequate = isAdequate,
                Notes = isAdequate ? "OK" : "CHECK REQUIRED"
            };
        }

        public List<(double Pu, double Mu)> BuildInteractionDiagram(ColumnDesignData data, double rho = 0.02)
        {
            var points = new List<(double, double)>();
            double b = data.WidthMm;
            double h = data.DepthMm;
            double Ag = b * h;
            double Ast = rho * Ag;
            double fc = data.Fc;
            double fy = data.Fy;

            double Po = (0.85 * fc * (Ag - Ast) + fy * Ast) / 1000.0;
            double Pb = Beta1 * Beta1 * fc * b * h * BalancedStrainRatioMPa / (BalancedStrainRatioMPa + fy) / 1000.0;
            double Mb = Pb * h * 0.4 / 1000.0;

            points.Add((PhiCompression * 0.80 * Po, 0));
            points.Add((PhiCompression * Pb, PhiCompression * Mb));
            points.Add((0, PhiCompression * 0.9 * 0.85 * fc * Ast * (h / 2.0) / 1e6));
            return points;
        }

        private static string SuggestColumnRebar(double asCm2)
        {
            if (asCm2 <= 10.18) return "4Ø18";
            if (asCm2 <= 20.36) return "8Ø18";
            if (asCm2 <= 30.19) return "6Ø25";
            return $"As={asCm2:F1}cm² (verify)";
        }

        private static string SuggestHoops(double vsN, double bMm)
        {
            if (vsN <= 0) return "Ø10@150mm";
            int spacing = (int)Math.Min(150, Math.Max(75, (0.785 * 2 * 280e6 * bMm * 0.9) / vsN));
            return $"Ø10@{spacing}mm";
        }
    }
}
