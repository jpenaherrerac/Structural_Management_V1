namespace App.Domain.Entities.Seismic
{
    public class ModalResult
    {
        public int ModeNumber { get; set; }
        public double Period { get; set; }
        public double Frequency { get; set; }
        public double CircularFrequency { get; set; }
        public double ModalMassRatioX { get; set; }
        public double ModalMassRatioY { get; set; }
        public double ModalMassRatioZ { get; set; }
        public double CumulativeModalMassX { get; set; }
        public double CumulativeModalMassY { get; set; }
        public double DominantDirection { get; set; }

        public ModalResult() { }

        public ModalResult(int modeNumber, double period)
        {
            ModeNumber = modeNumber;
            Period = period;
            Frequency = period > 0 ? 1.0 / period : 0;
            CircularFrequency = Frequency * 2 * System.Math.PI;
        }
    }
}
