using System;

namespace Arbol_de_Cargas.SAP
{
 /// <summary>
 /// Configuración global de materiales/secciones para exportación a SAP2000.
 /// Por ahora se aplica a TODO el modelo exportado (todas las torres / frames).
 /// </summary>
 public sealed class SapMaterialSettings
 {
 /// <summary>
 /// Singleton simple para mantener estado global en memoria.
 /// (Persistencia a Proyecto Unificado se puede agregar más adelante.)
 /// </summary>
 public static SapMaterialSettings Current { get; } = CreateDefaults();

 public string MaterialName { get; set; } = "Steel";

 /// <summary>
 /// Nombre de la propiedad de sección (frame property) a crear/usarse.
 /// </summary>
 public string FrameSectionName { get; set; } = "L150x150x6.5";

 // Geometría de sección Angle (SAP: PropFrame.SetAngle)
 public double T3 { get; set; } =0.15; // vertical leg depth [m]
 public double T2 { get; set; } =0.15; // horizontal leg width [m]
 public double Tf { get; set; } =0.008; // horizontal leg thickness [m]
 public double Tw { get; set; } =0.008; // vertical leg thickness [m]
 public double FilletRadius { get; set; } =0.0;

 /// <summary>
 /// Modifiers (8). Por defecto SAP=1.
 /// </summary>
 public double[] Modifiers { get; set; } = new double[] {1d,1d,1d,1d,1d,1d,1d,1d };

 private static SapMaterialSettings CreateDefaults()
 {
 return new SapMaterialSettings();
 }

 public double[] GetModifiersCopy()
 {
 var src = Modifiers ?? new double[] {1d,1d,1d,1d,1d,1d,1d,1d };
 var dst = new double[8];
 for (int i =0; i <8; i++)
 {
 dst[i] = (i < src.Length) ? src[i] :1d;
 }
 return dst;
 }
 }
}
