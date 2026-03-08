namespace App.Domain.Entities.Seismic
{
    /// <summary>
    /// Parameters for the story-drift calculation routine.
    /// </summary>
    public class StoryDriftCalculationParameters
    {
        /// <summary>Seismic reduction factor R (e.g. 6 for PorticosCA, 7 for Dual).</summary>
        public double ReductionFactorR { get; set; } = 6.0;

        /// <summary>Structural material: Concreto, Acero, Mampostería.</summary>
        public string Material { get; set; } = "Concreto";

        /// <summary>Normative drift limit Δ/h (e.g. 0.007 for concrete per E.030-2018).</summary>
        public double DriftLimit { get; set; } = 0.007;

        /// <summary>Load case name for X-direction seismic analysis (e.g. "Sdx", "RSX").</summary>
        public string LoadCaseX { get; set; } = "Sdx";

        /// <summary>Load case name for Y-direction seismic analysis (e.g. "Sdy", "RSY").</summary>
        public string LoadCaseY { get; set; } = "Sdy";

        /// <summary>Returns the drift limit based on material selection (E.030-2018, Table 11).</summary>
        public static double GetDriftLimitForMaterial(string material)
        {
            return (material ?? "").ToUpperInvariant() switch
            {
                "CONCRETO" => 0.007,
                "ACERO" => 0.010,
                "MAMPOSTERÍA" or "MAMPOSTERIA" => 0.005,
                _ => 0.007
            };
        }
    }
}
