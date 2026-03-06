using System;
using System.Collections.Generic;

namespace App.Domain.Entities.Loads
{
    public class ResponseSpectrumDefinition
    {
        public Guid Id { get; private set; }
        public string Name { get; private set; }
        public double DampingRatio { get; private set; }
        public string ModalCombinationMethod { get; private set; }
        public string DirectionalCombinationMethod { get; private set; }
        public double ScaleFactor { get; private set; }
        private readonly List<SpectrumPoint> _spectrumPoints = new List<SpectrumPoint>();
        public IReadOnlyList<SpectrumPoint> SpectrumPoints => _spectrumPoints.AsReadOnly();

        private ResponseSpectrumDefinition() { }

        public ResponseSpectrumDefinition(string name, double dampingRatio = 0.05)
        {
            Id = Guid.NewGuid();
            Name = name ?? throw new ArgumentNullException(nameof(name));
            DampingRatio = dampingRatio;
            ModalCombinationMethod = "CQC";
            DirectionalCombinationMethod = "SRSS";
            ScaleFactor = 1.0;
        }

        public void AddSpectrumPoint(double period, double acceleration)
        {
            _spectrumPoints.Add(new SpectrumPoint(period, acceleration));
        }

        public void SetCombinationMethods(string modal, string directional)
        {
            ModalCombinationMethod = modal;
            DirectionalCombinationMethod = directional;
        }

        public void SetScaleFactor(double scaleFactor) => ScaleFactor = scaleFactor;

        public class SpectrumPoint
        {
            public double Period { get; }
            public double Acceleration { get; }
            public SpectrumPoint(double period, double acceleration)
            {
                Period = period;
                Acceleration = acceleration;
            }
        }
    }
}
