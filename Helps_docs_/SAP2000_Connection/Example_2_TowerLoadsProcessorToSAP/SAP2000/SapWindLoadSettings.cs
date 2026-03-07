using System;
using System.Collections.Generic;
using System.Linq;

namespace Arbol_de_Cargas.SAP
{
 /// <summary>
 /// Configuración global para creación automática de load patterns de viento (ASCE7-16).
 /// Se aplica a TODO modelo SAP2000 exportado.
 /// </summary>
 public sealed class SapWindLoadSettings
 {
 public static SapWindLoadSettings Current { get; } = CreateDefaults();

 /// <summary>
 /// Lista de ángulos (grados0..360) para crear patrones WIND_{DirAngle}.
 /// </summary>
 public List<double> DirAngles { get; set; } = new List<double> {0d,90d };

 // Parámetros globales (se aplican a todos los DirAngle)
 public double WindSpeed { get; set; } =0d;

 /// <summary>
 /// ExposureType:1=B,2=C,3=D.
 /// Default: C (2)
 /// </summary>
 public int ExposureType { get; set; } =2;

 public double GustFactor { get; set; } =0.85;
 public double Kd { get; set; } =0.85;

 // Constantes acordadas (no UI)
 public int ExposureFrom { get; set; } =4;
 public double Kzt { get; set; } =1d;
 public double SolidGrossRatio { get; set; } =0.2;

 /// <summary>
 /// Si es true, se permite ejecutar el caso MODAL. Si es false, se desactiva por defecto.
 /// </summary>
 public bool RunModalCase { get; set; } = false;

 public List<double> GetDirAnglesNormalized()
 {
 var list = DirAngles ?? new List<double>();
 // Filtrar NaN/Infinity, normalizar a [0,360), quitar duplicados con tolerancia básica
 var clean = new List<double>();
 foreach (var a in list)
 {
 if (double.IsNaN(a) || double.IsInfinity(a)) continue;
 var n = a %360d;
 if (n <0) n +=360d;
 // evitar duplicados exactos
 if (!clean.Any(x => Math.Abs(x - n) <1e-9))
 clean.Add(n);
 }
 if (clean.Count ==0)
 clean.AddRange(new[] {0d,90d });
 clean.Sort();
 return clean;
 }

 private static SapWindLoadSettings CreateDefaults()
 {
 return new SapWindLoadSettings();
 }
 }
}
