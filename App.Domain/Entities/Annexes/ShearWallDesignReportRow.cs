namespace App.Domain.Entities.Annexes
{
    public class ShearWallDesignReportRow
    {
        public string ElementId { get; set; }
        public string StoryName { get; set; }
        public double LengthMeters { get; set; }
        public double ThicknessMm { get; set; }
        public double HeightMeters { get; set; }
        public double PuKN { get; set; }
        public double MuKNm { get; set; }
        public double VuKN { get; set; }
        public double MnKNm { get; set; }
        public double PhiMnKNm { get; set; }
        public double VcKN { get; set; }
        public double VsKN { get; set; }
        public double VnKN { get; set; }
        public double PhiVnKN { get; set; }
        public double RhoHorizontal { get; set; }
        public double RhoVertical { get; set; }
        public string HorizontalRebar { get; set; }
        public string VerticalRebar { get; set; }
        public bool RequiresBoundaryElements { get; set; }
        public string BoundaryElementDetails { get; set; }
        public bool IsAdequate { get; set; }
        public string Notes { get; set; }

        public ShearWallDesignReportRow() { }
    }
}
