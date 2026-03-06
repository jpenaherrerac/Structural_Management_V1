using System;
using System.Collections.Generic;

namespace App.WinForms.UserControls.E030
{
    public enum ZonaSismica { Z1, Z2, Z3, Z4 }
    public enum PerfilSuelo { S0, S1, S2, S3 }
    public enum CategoriaEdificacion { A1, A2, B, C }

    public enum SistemaEstructural
    {
        SMF, IMF, OMF,
        SCBF, OCBF, EBF,
        PorticosCA, Dual, Muros,
        MurosDuctilidadLimitada,
        Albanileria, Madera
    }

    public struct IrregularItem
    {
        public string Nombre;
        public double Factor;
        public string Descripcion;
    }

    /// <summary>
    /// Static lookup tables for E.030 seismic norm (Peru).
    /// Tables N°3, N°4, N°7, N°8 and N°9.
    /// </summary>
    internal static class E030Tables
    {
        // ── TABLA N°3: Factor de Suelo S ─────────────────────────────────────
        private static readonly Dictionary<(ZonaSismica, PerfilSuelo), double> _factorS =
            new Dictionary<(ZonaSismica, PerfilSuelo), double>
            {
                { (ZonaSismica.Z4, PerfilSuelo.S0), 0.80 }, { (ZonaSismica.Z4, PerfilSuelo.S1), 1.00 },
                { (ZonaSismica.Z4, PerfilSuelo.S2), 1.05 }, { (ZonaSismica.Z4, PerfilSuelo.S3), 1.10 },
                { (ZonaSismica.Z3, PerfilSuelo.S0), 0.80 }, { (ZonaSismica.Z3, PerfilSuelo.S1), 1.00 },
                { (ZonaSismica.Z3, PerfilSuelo.S2), 1.15 }, { (ZonaSismica.Z3, PerfilSuelo.S3), 1.20 },
                { (ZonaSismica.Z2, PerfilSuelo.S0), 0.80 }, { (ZonaSismica.Z2, PerfilSuelo.S1), 1.00 },
                { (ZonaSismica.Z2, PerfilSuelo.S2), 1.20 }, { (ZonaSismica.Z2, PerfilSuelo.S3), 1.40 },
                { (ZonaSismica.Z1, PerfilSuelo.S0), 0.80 }, { (ZonaSismica.Z1, PerfilSuelo.S1), 1.00 },
                { (ZonaSismica.Z1, PerfilSuelo.S2), 1.60 }, { (ZonaSismica.Z1, PerfilSuelo.S3), 2.00 },
            };

        // ── TABLA N°4: Períodos TP y TL ──────────────────────────────────────
        private static readonly Dictionary<PerfilSuelo, (double TP, double TL)> _periodos =
            new Dictionary<PerfilSuelo, (double TP, double TL)>
            {
                { PerfilSuelo.S0, (0.30, 3.00) },
                { PerfilSuelo.S1, (0.40, 2.50) },
                { PerfilSuelo.S2, (0.60, 2.00) },
                { PerfilSuelo.S3, (1.00, 1.60) },
            };

        // ── TABLA N°7: Sistemas Estructurales R₀ ────────────────────────────
        private static readonly Dictionary<SistemaEstructural, double> _r0 =
            new Dictionary<SistemaEstructural, double>
            {
                { SistemaEstructural.SMF, 8 }, { SistemaEstructural.IMF, 5 },
                { SistemaEstructural.OMF, 4 }, { SistemaEstructural.SCBF, 7 },
                { SistemaEstructural.OCBF, 4 }, { SistemaEstructural.EBF, 8 },
                { SistemaEstructural.PorticosCA, 8 }, { SistemaEstructural.Dual, 7 },
                { SistemaEstructural.Muros, 6 }, { SistemaEstructural.MurosDuctilidadLimitada, 4 },
                { SistemaEstructural.Albanileria, 3 }, { SistemaEstructural.Madera, 7 },
            };

        // ── TABLA N°8: Irregularidades en Altura (Ia) ───────────────────────
        public static readonly IrregularItem[] IrregularidadesAltura = new[]
        {
            new IrregularItem { Nombre = "Irregularidad de Rigidez – Piso Blando", Factor = 0.75,
                Descripcion = "Rigidez piso <70% superior o <80% promedio tres superiores." },
            new IrregularItem { Nombre = "Irregularidades de Resistencia – Piso Débil", Factor = 0.75,
                Descripcion = "Resistencia cortante piso <80% piso superior." },
            new IrregularItem { Nombre = "Irregularidad Extrema de Rigidez", Factor = 0.50,
                Descripcion = "Rigidez piso <60% superior o <70% promedio tres superiores." },
            new IrregularItem { Nombre = "Irregularidad Extrema de Resistencia", Factor = 0.50,
                Descripcion = "Resistencia cortante piso <65% piso superior." },
            new IrregularItem { Nombre = "Irregularidad de Masa o Peso", Factor = 0.90,
                Descripcion = "Peso piso >1.5× peso piso adyacente." },
            new IrregularItem { Nombre = "Irregularidad Geométrica Vertical", Factor = 0.90,
                Descripcion = "Dimensión planta >1.3× dimensión piso adyacente." },
            new IrregularItem { Nombre = "Discontinuidad en los Sistemas Resistentes", Factor = 0.80,
                Descripcion = "Elemento >10% cortante con desalineamiento vertical >25% dim elemento." },
            new IrregularItem { Nombre = "Discontinuidad Extrema de los Sistemas Resistentes", Factor = 0.60,
                Descripcion = "Elementos discontinuos >25% cortante total." },
        };

        // ── TABLA N°9: Irregularidades en Planta (Ip) ───────────────────────
        public static readonly IrregularItem[] IrregularidadesPlanta = new[]
        {
            new IrregularItem { Nombre = "Irregularidad Torsional", Factor = 0.75,
                Descripcion = "Δmax >1.3 Δprom con excentricidad accidental." },
            new IrregularItem { Nombre = "Irregularidad Torsional Extrema", Factor = 0.60,
                Descripcion = "Δmax >1.5 Δprom con excentricidad accidental." },
            new IrregularItem { Nombre = "Esquinas Entrantes", Factor = 0.90,
                Descripcion = "Esquinas entrantes >20% dimensión total." },
            new IrregularItem { Nombre = "Discontinuidad del Diafragma", Factor = 0.85,
                Descripcion = "Aberturas >50% área o sección <25% área neta." },
            new IrregularItem { Nombre = "Sistemas no Paralelos", Factor = 0.90,
                Descripcion = "Elementos no paralelos (ángulo >30° y >10% cortante)." },
        };

        // ── Lookup methods ──────────────────────────────────────────────────

        public static double GetS(ZonaSismica zona, PerfilSuelo perfil)
        {
            return _factorS.TryGetValue((zona, perfil), out var v) ? v : 0.0;
        }

        public static (double TP, double TL) GetPeriodos(PerfilSuelo perfil)
        {
            return _periodos.TryGetValue(perfil, out var v) ? v : (0.0, 0.0);
        }

        public static double GetR0(SistemaEstructural sistema)
        {
            return _r0.TryGetValue(sistema, out var v) ? v : 0.0;
        }

        public static double GetZFactor(ZonaSismica zona)
        {
            return zona switch
            {
                ZonaSismica.Z1 => 0.10,
                ZonaSismica.Z2 => 0.25,
                ZonaSismica.Z3 => 0.35,
                ZonaSismica.Z4 => 0.45,
                _ => 0.0
            };
        }

        public static double GetUsoFactor(CategoriaEdificacion cat)
        {
            return cat switch
            {
                CategoriaEdificacion.A1 => 1.50,
                CategoriaEdificacion.A2 => 1.50,
                CategoriaEdificacion.B => 1.30,
                CategoriaEdificacion.C => 1.00,
                _ => 1.00
            };
        }

        public static string GetSoilDescription(PerfilSuelo suelo)
        {
            return suelo switch
            {
                PerfilSuelo.S0 => "S0: Roca dura",
                PerfilSuelo.S1 => "S1: Roca o suelos muy rígidos",
                PerfilSuelo.S2 => "S2: Suelos intermedios",
                PerfilSuelo.S3 => "S3: Suelos blandos",
                _ => ""
            };
        }

        /// <summary>
        /// Computes C factor per E030 formula:
        ///   T ≤ TP       → C = 2.5
        ///   TP < T ≤ TL  → C = 2.5 × TP / T
        ///   T > TL       → C = 2.5 × TP × TL / T²
        /// </summary>
        public static double CalcularC(double T, double TP, double TL)
        {
            const double Cmax = 2.5;
            if (T <= 0.0) return 0.0;
            if (T <= TP) return Cmax;
            if (T <= TL) return Cmax * TP / T;
            return Cmax * TP * TL / (T * T);
        }
    }
}
