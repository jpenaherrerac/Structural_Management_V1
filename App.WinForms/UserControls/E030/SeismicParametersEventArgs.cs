using System;

namespace App.WinForms.UserControls.E030
{
    public class SeismicParameters
    {
        public string Zone { get; set; }
        public double Z { get; set; }
        public string SoilType { get; set; }
        public double S { get; set; }
        public string UsageCategory { get; set; }
        public double I { get; set; }
        public double R { get; set; }
        public double Ct { get; set; }
        public double Alpha { get; set; }
        public double Tp { get; set; }
        public double Tl { get; set; }
        public double Fa { get; set; }
        public double Fd { get; set; }
        public double Fs { get; set; }
        public double BuildingHeight { get; set; }
        public double ComputedPeriod { get; set; }
    }

    public class SeismicParametersEventArgs : EventArgs
    {
        public SeismicParameters Parameters { get; }
        public SeismicParametersEventArgs(SeismicParameters parameters) => Parameters = parameters;
    }
}
