#if SAP2000_LEGACY
using System;
using System.Collections.Generic;

namespace App.Infrastructure.Sap2000.Motores
{
    /// <summary>
    /// Define y asigna propiedades de área (shell) usando `SapModelFacade`.
    /// </summary>
    public sealed class AreaPropertyEngine
    {
        private readonly SapModelFacade _facade;

        public AreaPropertyEngine(SapModelFacade facade)
        {
            _facade = facade ?? throw new ArgumentNullException(nameof(facade));
        }

        // DTOs para cassette
        public sealed class AreaPropertySpec
        {
            public string Name { get; set; }
            public string Material { get; set; }
            public double Thickness { get; set; } // m
            public double Angle { get; set; } // deg, optional
        }

        public sealed class AreaPropertiesSpec
        {
            public string DefaultProperty { get; set; }
            public AreaPropertySpec[] Items { get; set; } = Array.Empty<AreaPropertySpec>();
        }

        // Aplica propiedades de área tipo shell según especificación del cassette
        public void Apply(AreaPropertiesSpec spec)
        {
            if (spec == null || spec.Items == null || spec.Items.Length == 0) return;

            foreach (var ap in spec.Items)
            {
                if (ap == null || string.IsNullOrWhiteSpace(ap.Name) || string.IsNullOrWhiteSpace(ap.Material)) continue;
                if (ap.Thickness <= 0) throw new ArgumentOutOfRangeException(nameof(ap.Thickness), $"Espesor inválido para '{ap.Name}': {ap.Thickness}");

                // shellType=1 (thin), membraneThickness = bendingThickness = Thickness, angle = ap.Angle
                _facade.PropArea_SetShell(ap.Name, 1, ap.Material, ap.Angle, ap.Thickness, ap.Thickness);
            }
        }

        // Método legado (mantener compatibilidad): crea shells simples a partir de diccionario
        public void EnsureAreaProperties(Dictionary<string, double> thicknessByName, string material)
        {
            if (string.IsNullOrWhiteSpace(material)) throw new ArgumentException("Debe especificar el nombre de material para las áreas", nameof(material));
            if (thicknessByName == null || thicknessByName.Count == 0) throw new ArgumentException("Debe proporcionar al menos una propiedad de área", nameof(thicknessByName));

            foreach (var kv in thicknessByName)
            {
                var name = kv.Key;
                var t = kv.Value;
                if (string.IsNullOrWhiteSpace(name)) continue;
                if (t <= 0) throw new ArgumentOutOfRangeException(nameof(thicknessByName), $"Espesor inválido para '{name}': {t}");
                _facade.PropArea_SetShell(name, 1, material, 0.0, t, t);
            }
        }
    }
}
#endif
