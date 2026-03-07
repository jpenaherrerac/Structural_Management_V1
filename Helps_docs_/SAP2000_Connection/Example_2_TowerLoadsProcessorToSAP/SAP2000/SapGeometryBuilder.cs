using System;
using System.Collections.Generic;
using System.Linq;
using Arbol_de_Cargas.Geometry;
using SAP2000v1;

namespace Arbol_de_Cargas.SAP
{
    /// <summary>
    /// Construye la geometría de torres en SAP2000: puntos y frames.
    /// </summary>
    public class SapGeometryBuilder
    {
        private cSapModel _sapModel;
        private Action<string> _logCallback;

  public SapGeometryBuilder(cSapModel sapModel, Action<string> logCallback = null)
     {
    _sapModel = sapModel ?? throw new ArgumentNullException(nameof(sapModel));
     _logCallback = logCallback ?? (msg => System.Diagnostics.Debug.WriteLine(msg));
}

        /// <summary>
        /// Crea todos los puntos de la torre en SAP2000.
        /// Retorna un diccionario que mapea identificadores de nodo a nombres asignados por SAP2000.
     /// NO ABORTA por errores - documenta y continúa.
        /// </summary>
  public Dictionary<string, string> CreatePoints(TowerModel tower)
        {
    var puntosSAP = new Dictionary<string, string>();
          int errores = 0;
            var erroresDetalle = new List<string>();

          Log($"[SapGeometryBuilder] Creando {tower.NodeSequence.Count} puntos...");

        // Usar el orden de NodeSequence para mantener la secuencia de inserción
            foreach (var nodeName in tower.NodeSequence)
          {
    try
     {
           if (!tower.Nodes.TryGetValue(nodeName, out var nodePos))
     {
      errores++;
               erroresDetalle.Add($"Nodo '{nodeName}': No encontrado en Nodes dictionary");
             continue;
      }

            string assignedName = "";

           int ret = _sapModel.PointObj.AddCartesian(
        nodePos.X,
 nodePos.Y,
            nodePos.Z,
         ref assignedName,
               nodeName,      // UserName = nombre real del nodo
      "Global",
     false,   // MergeOff = false (permitir merge de puntos coincidentes)
 0  // MergeNumber = 0
     );

       if (ret != 0)
  {
        errores++;
       erroresDetalle.Add($"Nodo '{nodeName}' ({nodePos.X:F2},{nodePos.Y:F2},{nodePos.Z:F2}): Error código {ret}");
     Log($"  ❌ Error creando punto '{nodeName}': código {ret}");
    continue;
              }

      // ✅ Punto creado exitosamente
   puntosSAP[nodeName] = assignedName;
        Log($"  ✓ Punto '{nodeName}' → SAP '{assignedName}' ({nodePos.X:F2}, {nodePos.Y:F2}, {nodePos.Z:F2})");
         }
          catch (Exception ex)
      {
    errores++;
  erroresDetalle.Add($"Nodo '{nodeName}': Excepción {ex.Message}");
      Log($"  ❌ Excepción creando punto '{nodeName}': {ex.Message}");
        }
        }

            // Logging de resumen
            Log($"[SapGeometryBuilder] Puntos procesados: Total={tower.NodeSequence.Count}, Éxito={puntosSAP.Count}, Errores={errores}");
  
        if (errores > 0)
          {
        Log($"[SapGeometryBuilder] Detalles de errores en puntos:");
 foreach (var error in erroresDetalle.Take(20))
   {
         Log($"  ❌ {error}");
        }
   if (erroresDetalle.Count > 20)
       {
      Log($"  ... y {erroresDetalle.Count - 20} errores más");
       }
 }

      return puntosSAP;
  }

     /// <summary>
 /// Crea todos los frames (barras) de la torre en SAP2000 usando coordenadas.
    /// Retorna una lista con los nombres de los frames creados.
        /// NO ABORTA por errores - documenta y continúa.
/// </summary>
        public List<string> CreateFrames(TowerModel tower, Dictionary<string, string> puntosSAP, string sectionName = null)
      {
 // NEW: usar sección configurada si no se pasa explícitamente
 if (string.IsNullOrWhiteSpace(sectionName))
 {
 var settings = SapMaterialSettings.Current;
 sectionName = string.IsNullOrWhiteSpace(settings.FrameSectionName) ? "L100x100x6.5" : settings.FrameSectionName.Trim();
 }

 var framesCreados = new List<string>();
     int errores = 0;
   var erroresDetalle = new List<string>();
    
            // Detectar frames duplicados ANTES de intentar crearlos
            var framesPorCoordenadas = new Dictionary<string, List<string>>();

         Log($"{nameof(SapGeometryBuilder)}.{nameof(CreateFrames)} Creando {tower.Frames.Count} frames...");
          Log($"{nameof(SapGeometryBuilder)}.{nameof(CreateFrames)} 🔍 ANÁLISIS PREVIO DE DUPLICADOS:");

            // Primera pasada: detectar duplicados por coordenadas
foreach (var frame in tower.Frames)
      {
      if (!tower.Nodes.TryGetValue(frame.N1, out var coord1) || 
       !tower.Nodes.TryGetValue(frame.N2, out var coord2))
     continue;

      // Crear clave única basada en coordenadas (ordenadas para detectar frames invertidos)
   string key;
    if (coord1.X < coord2.X || (coord1.X == coord2.X && coord1.Y < coord2.Y) || 
         (coord1.X == coord2.X && coord1.Y == coord2.Y && coord1.Z < coord2.Z))
      {
key = $"({coord1.X:F3},{coord1.Y:F3},{coord1.Z:F3})-({coord2.X:F3},{coord2.Y:F3},{coord2.Z:F3})";
        }
             else
    {
         key = $"({coord2.X:F3},{coord2.Y:F3},{coord2.Z:F3})-({coord1.X:F3},{coord1.Y:F3},{coord1.Z:F3})";
        }

        if (!framesPorCoordenadas.ContainsKey(key))
                {
        framesPorCoordenadas[key] = new List<string>();
      }
         framesPorCoordenadas[key].Add(frame.Name);
            }

            // Reportar duplicados detectados
            var duplicados = framesPorCoordenadas.Where(kv => kv.Value.Count > 1).ToList();
   if (duplicados.Count > 0)
            {
       Log($"{nameof(SapGeometryBuilder)}.{nameof(CreateFrames)}   ⚠️ FRAMES DUPLICADOS DETECTADOS: {duplicados.Count} grupos");
              foreach (var dup in duplicados.Take(5))
     {
          Log($"{nameof(SapGeometryBuilder)}.{nameof(CreateFrames)}     • Coordenadas {dup.Key}:");
  Log($"{nameof(SapGeometryBuilder)}.{nameof(CreateFrames)}       Frames: {string.Join(", ", dup.Value)}");
    Log($"{nameof(SapGeometryBuilder)}.{nameof(CreateFrames)}       → Se creará solo el primero: '{dup.Value[0]}'");
      }
       if (duplicados.Count > 5)
      {
  Log($"{nameof(SapGeometryBuilder)}.{nameof(CreateFrames)}     ... y {duplicados.Count - 5} grupos más de duplicados");
       }
    }
          else
  {
     Log($"{nameof(SapGeometryBuilder)}.{nameof(CreateFrames)}   ✓ No se detectaron frames duplicados");
         }
            Log($"");

            // Segunda pasada: crear frames, saltando duplicados
 var coordenadasCreadas = new HashSet<string>();

    foreach (var frame in tower.Frames)
            {
           try
       {
           // Obtener coordenadas de los nodos del frame
          if (!tower.Nodes.TryGetValue(frame.N1, out var coord1))
        {
        errores++;
    erroresDetalle.Add($"Frame '{frame.Name}': Nodo inicial '{frame.N1}' no encontrado");
 Log($"  ❌ Frame '{frame.Name}': Nodo inicial '{frame.N1}' no encontrado");
       continue;
         }

        if (!tower.Nodes.TryGetValue(frame.N2, out var coord2))
      {
           errores++;
      erroresDetalle.Add($"Frame '{frame.Name}': Nodo final '{frame.N2}' no encontrado");
      Log($"  ❌ Frame '{frame.Name}': Nodo final '{frame.N2}' no encontrado");
       continue;
          }

        // Crear clave única para detectar si ya se creó
      string coordKey;
     if (coord1.X < coord2.X || (coord1.X == coord2.X && coord1.Y < coord2.Y) || 
     (coord1.X == coord2.X && coord1.Y == coord2.Y && coord1.Z < coord2.Z))
     {
             coordKey = $"({coord1.X:F3},{coord1.Y:F3},{coord1.Z:F3})-({coord2.X:F3},{coord2.Y:F3},{coord2.Z:F3})";
   }
      else
        {
         coordKey = $"({coord2.X:F3},{coord2.Y:F3},{coord2.Z:F3})-({coord1.X:F3},{coord1.Y:F3},{coord1.Z:F3})";
         }

     // SKIP si ya se creó un frame con estas coordenadas
      if (coordenadasCreadas.Contains(coordKey))
   {
      Log($"  ⏭️  SKIP Frame '{frame.Name}': Ya existe frame con coordenadas {coordKey}");
               continue; // NO incrementar errores, es un duplicado esperado
        }

 string frameName = "";

        // Usar AddByCoord
          int ret = _sapModel.FrameObj.AddByCoord(
        coord1.X, coord1.Y, coord1.Z,  // xi, yi, zi (I-End)
                   coord2.X, coord2.Y, coord2.Z,  // xj, yj, zj (J-End)
               ref frameName,              // Name (asignado por SAP)
     sectionName,         // PropName
   frame.Name,    // UserName (nombre del frame)
            "Global"       // CSys
          );

                    if (ret != 0)
         {
  errores++;
        string errorMsg = $"Frame '{frame.Name}' ({frame.N1}→{frame.N2}): Error código {ret}";
          erroresDetalle.Add(errorMsg);
         
       // Logging detallado del error
              Log($"  ❌ ERROR código {ret} en frame '{frame.Name}'");
     Log($"     │ Nodos: {frame.N1} → {frame.N2}");
       Log($" │ Coord1: ({coord1.X:F3}, {coord1.Y:F3}, {coord1.Z:F3})");
              Log($"     │ Coord2: ({coord2.X:F3}, {coord2.Y:F3}, {coord2.Z:F3})");
           Log($"     │ Longitud: {Math.Sqrt(Math.Pow(coord2.X - coord1.X, 2) + Math.Pow(coord2.Y - coord1.Y, 2) + Math.Pow(coord2.Z - coord1.Z, 2)):F3} m");
          
   // Diagnóstico según código de error
  switch (ret)
        {
       case 1:
          Log($" │ ⚠️ Código 1: Posibles causas:");
  Log($"     │   • Frame duplicado (mismo nombre o coordenadas)");
      Log($"     │   • Sección '{sectionName}' no existe");
  Log($"     │   • Coordenadas inválidas o idénticas");
        break;
   case 2:
         Log($"     │ ⚠️ Código 2: Parámetros inválidos");
   break;
    default:
       Log($"   │ ⚠️ Código desconocido: {ret}");
break;
                }
       continue;
         }

    // ✅ Frame creado exitosamente
         framesCreados.Add(frameName);
        coordenadasCreadas.Add(coordKey);
         Log($"  ✓ Frame '{frame.Name}' → SAP '{frameName}' ({frame.N1} → {frame.N2})");
                }
    catch (Exception ex)
    {
          errores++;
   erroresDetalle.Add($"Frame '{frame.Name}': Excepción {ex.Message}");
       Log($"  ❌ Excepción creando frame '{frame.Name}': {ex.Message}");
                }
   }

            // Logging de resumen
            int duplicadosEsperados = tower.Frames.Count - coordenadasCreadas.Count;
Log($"");
            Log($"[SapGeometryBuilder] Frames procesados:");
       Log($"  • Total en torre: {tower.Frames.Count}");
     Log($"  • Frames únicos creados: {framesCreados.Count}");
    Log($"  • Duplicados omitidos: {duplicadosEsperados}");
            Log($"  • Errores reales: {errores}");

            if (errores > 0)
            {
        Log($"[SapGeometryBuilder] Detalles de errores REALES en frames:");
              foreach (var error in erroresDetalle.Take(20))
 {
          Log($"  ❌ {error}");
             }
           if (erroresDetalle.Count > 20)
          {
   Log($"  ... y {erroresDetalle.Count - 20} errores más");
       }
  }

      return framesCreados;
        }

        /// <summary>
      /// Crea el material Steel por defecto.
        /// </summary>
     public void CreateDefaultMaterial()
        {
 var settings = SapMaterialSettings.Current;
 string materialName = string.IsNullOrWhiteSpace(settings.MaterialName) ? "Steel" : settings.MaterialName.Trim();

 Log($"[SapGeometryBuilder] Creando material '{materialName}'...");

 int ret = _sapModel.PropMaterial.SetMaterial(materialName, eMatType.Steel, -1, "", "");

 if (ret !=0)
 {
 Log($" ❌ Error al crear material {materialName}. Código: {ret}");
 throw new Exception($"Error al crear material {materialName}. Código: {ret}");
 }

 Log($" ✓ Material '{materialName}' creado exitosamente");
 }

 /// <summary>
 /// Crea la sección de ángulo por defecto (configurable) y asigna modifiers.
 /// </summary>
 public void CreateDefaultAngleSection()
 {
 var settings = SapMaterialSettings.Current;
 string materialName = string.IsNullOrWhiteSpace(settings.MaterialName) ? "Steel" : settings.MaterialName.Trim();
 string sectionName = string.IsNullOrWhiteSpace(settings.FrameSectionName) ? "L100x100x6.5" : settings.FrameSectionName.Trim();

 Log($"[SapGeometryBuilder] Creando sección '{sectionName}'...");

 // Nota: en la interop SAP2000v1 la firma expuesta suele ser SetAngle_1 (incluye FilletRadius).
 int ret = _sapModel.PropFrame.SetAngle_1(
 sectionName, // Name
 materialName, // MatProp
 settings.T3,
 settings.T2,
 settings.Tf,
 settings.Tw,
 settings.FilletRadius,
 -1, // Color
 "", // Notes
 "" // GUID
 );

 if (ret !=0)
 {
 Log($" ❌ Error al crear sección {sectionName}. Código: {ret}");
 throw new Exception($"Error al crear sección {sectionName}. Código: {ret}");
 }

 // Aplicar modifiers por defecto (todo =1) o lo que indique Settings
 try
 {
 double[] modifiers = settings.GetModifiersCopy();
 int retMod = _sapModel.PropFrame.SetModifiers(sectionName, ref modifiers);
 if (retMod !=0)
 {
 Log($" ❌ Error al asignar modifiers a {sectionName}. Código: {retMod}");
 throw new Exception($"Error al asignar modifiers a {sectionName}. Código: {retMod}");
 }

 Log($" ✓ Modifiers asignados a '{sectionName}'");
 }
 catch (Exception ex)
 {
 Log($" ❌ Excepción asignando modifiers a {sectionName}: {ex.Message}");
 throw;
 }

 Log($" ✓ Sección '{sectionName}' creada exitosamente");
 }

        /// <summary>
   /// Renombra puntos en SAP2000 según el diccionario de mapeo.
        /// OPCIONAL: Usar después de crear frames para mantener nombres consistentes.
        /// </summary>
    public void RenamePoints(Dictionary<string, string> puntosSAP)
        {
            Log($"[SapGeometryBuilder] Mapeo de puntos creados: {puntosSAP.Count} puntos");
            
    foreach (var kv in puntosSAP.Take(10))
      {
                Log($"  {kv.Key} → {kv.Value}");
            }
  
            if (puntosSAP.Count > 10)
          {
          Log($"  ... y {puntosSAP.Count - 10} puntos más");
    }
        }

  /// <summary>
        /// Asigna restricciones (restraints) a los puntos de la base de la torre.
        /// Los puntos de la base tienen nombres que empiezan con "Base1_".
  /// Configuración: U1=U2=U3=True (traslaciones restringidas), R1=R2=R3=False (rotaciones libres).
        /// </summary>
        public void AssignBaseRestraints(Dictionary<string, string> puntosSAP)
        {
      Log($"[SapGeometryBuilder] Asignando restricciones a puntos de base...");
          
            // Filtrar puntos que son de la base (empiezan con "Base1_")
var basePuntos = puntosSAP.Where(kv => kv.Key.StartsWith("Base1_")).ToList();
            
        if (basePuntos.Count == 0)
    {
       Log($"  ⚠️ No se encontraron puntos de base (que empiecen con 'Base1_')");
           return;
            }
       
        Log($"  Puntos de base encontrados: {basePuntos.Count}");

            // Crear array de restricciones: U1=U2=U3=True, R1=R2=R3=False
       bool[] restraints = new bool[6];
            restraints[0] = true;  // U1 (traslación en X) = Restringido
         restraints[1] = true;  // U2 (traslación en Y) = Restringido
         restraints[2] = true;  // U3 (traslación en Z) = Restringido
      restraints[3] = false; // R1 (rotación en X) = Libre
            restraints[4] = false; // R2 (rotación en Y) = Libre
         restraints[5] = false; // R3 (rotación en Z) = Libre
            
   int exitosos = 0;
 int errores = 0;
            
  // Asignar restricciones a cada punto de base
  foreach (var punto in basePuntos)
 {
 try
            {
     // Usar el nombre asignado por SAP2000 (punto.Value)
       int ret = _sapModel.PointObj.SetRestraint(
            punto.Value,     // Name (nombre SAP)
ref restraints,  // Value array
        (eItemType)0);   // ItemType = 0 (Object = punto individual)

       if (ret != 0)
    {
               errores++;
      Log($"  ❌ Error asignando restricción a '{punto.Key}' (SAP: '{punto.Value}'): código {ret}");
       }
   else
  {
    exitosos++;
    Log($"  ✓ Restricción asignada a '{punto.Key}' (SAP: '{punto.Value}')");
  }
 }
  catch (Exception ex)
   {
      errores++;
 Log($"  ❌ Excepción asignando restricción a '{punto.Key}': {ex.Message}");
       }
   }

            Log($"[SapGeometryBuilder] Restricciones de base procesadas: Éxito={exitosos}, Errores={errores}");
     }

        private void Log(string message)
        {
    _logCallback?.Invoke(message);
     // NO escribir a Debug aquí porque el callback ya lo hace
      // System.Diagnostics.Debug.WriteLine(message);  ← ELIMINADO para evitar duplicación
        }
    }
}
