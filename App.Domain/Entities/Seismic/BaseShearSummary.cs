using System;

namespace App.Domain.Entities.Seismic
{
    public class BaseShearSummary
    {
        public string LoadCase { get; set; }
        public double Fx { get; set; }
        public double Fy { get; set; }
        public double Fz { get; set; }
        public double Mx { get; set; }
        public double My { get; set; }
        public double Mz { get; set; }
        public string Units { get; set; }

        public BaseShearSummary() { }

        public BaseShearSummary(string loadCase, double fx, double fy)
        {
            LoadCase = loadCase ?? throw new ArgumentNullException(nameof(loadCase));
            Fx = fx;
            Fy = fy;
            Units = "kN";
        }

        public double TotalHorizontalResultant => Math.Sqrt(Fx * Fx + Fy * Fy);
    }
}
