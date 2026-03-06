using System;
using System.Collections.Generic;

namespace App.WinForms.UserControls.E030
{
    /// <summary>
    /// Internal data structure for E030 seismic parameters per direction.
    /// </summary>
    public class EntradaE030
    {
        public ZonaSismica? Zona { get; set; }
        public PerfilSuelo? Suelo { get; set; }
        public double? Ct { get; set; }
        public double? C { get; set; }
        public double? T { get; set; }
        public CategoriaEdificacion? Categoria { get; set; }
        public double? U { get; set; }
        public SistemaEstructural? Sistema { get; set; }
        public double Ia { get; set; } = 1.0;
        public double Ip { get; set; } = 1.0;
        public double R0 { get; set; }
        public double R => Ia * Ip * R0;
    }

    /// <summary>
    /// Event args carrying the current dictionary of seismic values.
    /// </summary>
    public class SeismicParametersEventArgs : EventArgs
    {
        public IReadOnlyDictionary<string, double> Values { get; }
        public SeismicParametersEventArgs(IReadOnlyDictionary<string, double> values) => Values = values;
    }
}
