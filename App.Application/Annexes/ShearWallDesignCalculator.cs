using System;
using App.Domain.Entities.Design;
using App.Domain.Entities.Annexes;

namespace App.Application.Annexes
{
    /// <summary>
    /// Calculates shear wall flexural and shear design per ACI 318 / NEC.
    /// </summary>
    public class ShearWallDesignCalculator
    {
        private const double PhiFlexure = 0.90;
        private const double PhiShear = 0.75;
        /// <summary>ACI 318 §11.5.4: minimum horizontal reinforcement ratio for walls.</summary>
        private const double MinHorizontalReinforcement = 0.0025;
        /// <summary>ACI 318 §11.5.4: minimum vertical reinforcement ratio for walls.</summary>
        private const double MinVerticalReinforcement = 0.0025;
        /// <summary>ACI 318 Table 11.5.4.3: αc for slender walls (hw/lw ≥ 2).</summary>
        private const double AlphaCSlender = 0.17;
        /// <summary>ACI 318 Table 11.5.4.3: αc for squat walls (hw/lw ≤ 1.5).</summary>
        private const double AlphaCSquat = 0.25;
        private const double SlenderAspectRatio = 2.0;
        private const double SquatAspectRatio = 1.5;

        public ShearWallDesignReportRow Calculate(WallDesignData data, double rhoHorizontal = 0.0025, double rhoVertical = 0.0025)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));

            double lw = data.LengthMeters * 1000.0;
            double tw = data.ThicknessMm;
            double hw = data.HeightMeters * 1000.0;
            double cover = data.CoverMm > 0 ? data.CoverMm : 25.0;
            double fc = data.Fc;
            double fy = data.Fy;
            double Acv = lw * tw;

            double rhoH = Math.Max(rhoHorizontal, MinHorizontalReinforcement);
            double rhoV = Math.Max(rhoVertical, MinVerticalReinforcement);

            double Mu = data.MuKNm * 1e6;
            double Pu = data.PuKN * 1000.0;

            double As = rhoV * lw * tw;
            double a = As * fy / (0.85 * fc * tw);
            double Mn = As * fy * (lw / 2.0 - a / 2.0) + Pu * lw / 2.0;
            double phiMn = PhiFlexure * Mn / 1e6;

            double Vu = data.VuKN * 1000.0;
            double aspectRatio = hw / lw;
            double alphaC;
            if (aspectRatio <= SquatAspectRatio)
                alphaC = AlphaCSquat;
            else if (aspectRatio >= SlenderAspectRatio)
                alphaC = AlphaCSlender;
            else
                alphaC = AlphaCSlender + (SlenderAspectRatio - aspectRatio) / (SlenderAspectRatio - SquatAspectRatio) * (AlphaCSquat - AlphaCSlender);

            double Vc = alphaC * Math.Sqrt(fc) * Acv;
            double Vs = rhoH * Acv * fy;
            double Vn = Math.Min(Vc + Vs, 0.66 * Math.Sqrt(fc) * Acv);
            double phiVn = PhiShear * Vn;

            double c = a / 0.85;
            double ccLimit = lw / (600.0 * (hw / lw + 1));
            bool requiresBoundary = c > ccLimit;

            bool isAdequate = phiMn >= data.MuKNm && phiVn >= data.VuKN * 1000.0;

            return new ShearWallDesignReportRow
            {
                ElementId = data.ElementId,
                StoryName = data.StoryName,
                LengthMeters = data.LengthMeters,
                ThicknessMm = tw,
                HeightMeters = data.HeightMeters,
                PuKN = data.PuKN,
                MuKNm = data.MuKNm,
                VuKN = data.VuKN,
                MnKNm = Math.Round(Mn / 1e6, 2),
                PhiMnKNm = Math.Round(phiMn, 2),
                VcKN = Math.Round(Vc / 1000.0, 2),
                VsKN = Math.Round(Vs / 1000.0, 2),
                VnKN = Math.Round(Vn / 1000.0, 2),
                PhiVnKN = Math.Round(phiVn / 1000.0, 2),
                RhoHorizontal = rhoH,
                RhoVertical = rhoV,
                HorizontalRebar = SuggestWallRebar(rhoH, tw),
                VerticalRebar = SuggestWallRebar(rhoV, tw),
                RequiresBoundaryElements = requiresBoundary,
                BoundaryElementDetails = requiresBoundary ? $"c={c:F0}mm > limit={ccLimit:F0}mm" : "Not required",
                IsAdequate = isAdequate,
                Notes = isAdequate ? "OK" : "CHECK REQUIRED"
            };
        }

        private static string SuggestWallRebar(double rho, double thicknessMm)
        {
            double spacing = Math.Min(300, (0.785 * 2 * 1000) / (rho * thicknessMm));
            int s = (int)Math.Floor(spacing / 25.0) * 25;
            s = Math.Max(75, Math.Min(300, s));
            return $"2xØ10@{s}mm";
        }
    }
}
