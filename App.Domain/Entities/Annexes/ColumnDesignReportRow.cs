namespace App.Domain.Entities.Annexes
{
    public class ColumnDesignReportRow
    {
        public string ElementId { get; set; }
        public string StoryName { get; set; }
        public string Section { get; set; }
        public double bMm { get; set; }
        public double hMm { get; set; }
        public double dMm { get; set; }
        public double PuKN { get; set; }
        public double Mu2KNm { get; set; }
        public double Mu3KNm { get; set; }
        public double PnKN { get; set; }
        public double MnKNm { get; set; }
        public double PhiPnKN { get; set; }
        public double PhiMnKNm { get; set; }
        public double RhoMin { get; set; }
        public double RhoMax { get; set; }
        public double RhoProvided { get; set; }
        public double AsProvidedCm2 { get; set; }
        public string LongitudinalRebar { get; set; }
        public double VuKN { get; set; }
        public double VcKN { get; set; }
        public double VsKN { get; set; }
        public double PhiVnKN { get; set; }
        public string TransverseRebar { get; set; }
        public bool IsInInteractionDiagram { get; set; }
        public bool IsAdequate { get; set; }
        public string Notes { get; set; }

        public ColumnDesignReportRow() { }
    }
}
