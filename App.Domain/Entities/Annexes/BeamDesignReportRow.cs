namespace App.Domain.Entities.Annexes
{
    public class BeamDesignReportRow
    {
        public string ElementId { get; set; }
        public string StoryName { get; set; }
        public string Section { get; set; }
        public double bMm { get; set; }
        public double hMm { get; set; }
        public double dMm { get; set; }
        public double MuPositiveKNm { get; set; }
        public double MuNegativeKNm { get; set; }
        public double MnKNm { get; set; }
        public double PhiMnKNm { get; set; }
        public double AsRequiredCm2 { get; set; }
        public double AsMinCm2 { get; set; }
        public double AsProvidedCm2 { get; set; }
        public string LongitudinalRebar { get; set; }
        public double VuKN { get; set; }
        public double VcKN { get; set; }
        public double VsKN { get; set; }
        public double VnKN { get; set; }
        public double PhiVnKN { get; set; }
        public string TransverseRebar { get; set; }
        public bool IsAdequate { get; set; }
        public string Notes { get; set; }

        public BeamDesignReportRow() { }
    }
}
