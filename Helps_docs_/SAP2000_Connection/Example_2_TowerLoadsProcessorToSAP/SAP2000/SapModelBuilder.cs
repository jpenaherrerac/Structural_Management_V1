using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Globalization;
using Arbol_de_Cargas.Geometry;
using Arbol_de_Cargas.DataProcessing;
using Arbol_de_Cargas.Transformation;
using SAP2000v1;

namespace Arbol_de_Cargas.SAP
{
 /// <summary>
 /// Orquesta la creación completa de modelos SAP2000 a partir de torres y bloques.
 /// Usa TransformOutputCache como fuente única de verdad para valores transformados.
 /// </summary>
 public class SapModelBuilder
 {
 private readonly SapStaHost.StaComRunner _sapRunner;
 private readonly SapProcessor _sapProcessor;
 private readonly Action<string> _logCallback;
 private readonly DataGridView _transformGrid;
 private readonly PointDataManager _pointDataManager;
 private readonly BlockValidator _blockValidator;
 private readonly CoordinateTransformer _coordinateTransformer = new CoordinateTransformer();

 // ==== Reacciones (acumuladas) ====

 public sealed class JointReactionResult
 {
 public string PuntoNombreUi { get; set; } // e.g., "Base1_1"
 public string PuntoNombreSap { get; set; } // nombre real en SAP
 public string Obj { get; set; }
 public string Elm { get; set; }
 public string LoadCase { get; set; }
 public string StepType { get; set; }
 public double StepNum { get; set; }
 public double F1 { get; set; }
 public double F2 { get; set; }
 public double F3 { get; set; }
 public double M1 { get; set; }
 public double M2 { get; set; }
 public double M3 { get; set; }
 }

 public sealed class ModelReactionSet
 {
 public string Archivo { get; set; }
 public int TorreIndex { get; set; } //0-based
 public int BloqueIndexVisible { get; set; } //0-based
 public int BloqueIndexReal { get; set; } //0-based
 public string ModelName { get; set; }
 public string ModelPath { get; set; }
 public List<JointReactionResult> Reactions { get; } = new List<JointReactionResult>();
 
 // NEW: BaseReactWithCentroid results (estructura total)
 public sealed class BaseReactWithCentroidResult
 {
 public string LoadCase { get; set; }
 public string StepType { get; set; }
 public double StepNum { get; set; }
 public double Fx { get; set; }
 public double Fy { get; set; }
 public double Fz { get; set; }
 public double Mx { get; set; }
 public double My { get; set; }
 public double Mz { get; set; }
 public double Gx { get; set; }
 public double Gy { get; set; }
 public double Gz { get; set; }
 public double XCentroidForFx { get; set; }
 public double YCentroidForFx { get; set; }
 public double ZCentroidForFx { get; set; }
 public double XCentroidForFy { get; set; }
 public double YCentroidForFy { get; set; }
 public double ZCentroidForFy { get; set; }
 public double XCentroidForFz { get; set; }
 public double YCentroidForFz { get; set; }
 public double ZCentroidForFz { get; set; }
 }

 public List<BaseReactWithCentroidResult> BaseReactionsWithCentroid { get; } = new List<BaseReactWithCentroidResult>();
 }

 // Key: (archivo, torreIdx) -> modelos por bloque
 private readonly Dictionary<Tuple<string, int>, List<ModelReactionSet>> _reactionsByTower
 = new Dictionary<Tuple<string, int>, List<ModelReactionSet>>();

 public Dictionary<Tuple<string, int>, List<ModelReactionSet>> ReactionsByTower => _reactionsByTower;

 public SapModelBuilder(
 SapStaHost.StaComRunner sapRunner,
 SapProcessor sapProcessor,
 DataGridView transformGrid,
 Action<string> logCallback = null)
 {
 _sapRunner = sapRunner ?? throw new ArgumentNullException(nameof(sapRunner));
 _sapProcessor = sapProcessor ?? throw new ArgumentNullException(nameof(sapProcessor));
 _transformGrid = transformGrid;
 _pointDataManager = new PointDataManager();
 _blockValidator = new BlockValidator();
 _logCallback = logCallback ?? (msg => System.Diagnostics.Debug.WriteLine(msg));
 }

 /// <summary>
 /// Crea todos los modelos SAP2000 para el workspace completo.
 /// Flujo: Archivo → Torre → Bloque
 /// </summary>
 public void CreateModelsForWorkspace(
 Dictionary<string, List<ExcelToCsv.Bloque>> resultado,
 Dictionary<string, List<TowerModel>> generatedTowers,
 string outputDirectory)
 {
 if (!Directory.Exists(outputDirectory))
 Directory.CreateDirectory(outputDirectory);

 // Sincronizar puntos una sola vez antes de procesar todos los modelos
 try
 {
 _pointDataManager.GuardarPuntosEnBloques(_transformGrid, resultado);
 }
 catch (Exception ex)
 {
 _logCallback($"[SapModelBuilder] Error sincronizando puntos: {ex.Message}");
 }

 int totalModelos =0;
 int errores =0;
 var erroresDetalle = new List<string>();
 var archivosOrdenados = resultado.Keys.OrderBy(k => k, StringComparer.InvariantCulture).ToList();

 foreach (var archivo in archivosOrdenados)
 {
 try
 {
 if (!generatedTowers.TryGetValue(archivo, out var torres) || torres.Count ==0)
 {
 _logCallback($"[SKIP] Archivo '{archivo}': No tiene torres generadas.");
 continue;
 }

 var bloquesAll = resultado[archivo];
 var bloquesValidos = new List<(ExcelToCsv.Bloque bloque, int bloqueIdxReal)>();

 for (int i =0; i < bloquesAll.Count; i++)
 {
 var b = bloquesAll[i];
 if (_blockValidator.TieneHeadersDuplicados(b) || !_blockValidator.TieneEncabezadosNoNumericos(b))
 continue;
 bloquesValidos.Add((b, i));
 }

 _logCallback($"[INFO] Procesando archivo '{archivo}': {torres.Count} torres × {bloquesValidos.Count} bloques = {torres.Count * bloquesValidos.Count} modelos");

 for (int torreIdx =0; torreIdx < torres.Count; torreIdx++)
 {
 var torre = torres[torreIdx];

 for (int bloqueIdxVisible =0; bloqueIdxVisible < bloquesValidos.Count; bloqueIdxVisible++)
 {
 var bloque = bloquesValidos[bloqueIdxVisible].bloque;
 int bloqueIdxReal = bloquesValidos[bloqueIdxVisible].bloqueIdxReal;

 try
 {
 string modelName = $"{SanitizeFileName(archivo)}_Cuerpo{torreIdx +1}_Bloque{bloqueIdxVisible +1}";
 string modelPath = Path.Combine(outputDirectory, $"{modelName}.sdb");

 _logCallback($"[CREANDO] {modelName}...");

 // Mapa puntos por tipo usando identidad estable por archivo (string)
 var tipoToPointIndices = _pointDataManager.BuildTipoToPointIndicesMap(
 _transformGrid, archivo, bloqueIdxReal);

 var reactionSet = new ModelReactionSet
 {
 Archivo = archivo,
 TorreIndex = torreIdx,
 BloqueIndexVisible = bloqueIdxVisible,
 BloqueIndexReal = bloqueIdxReal,
 ModelName = modelName,
 ModelPath = modelPath
 };

 CreateSingleModel(
 torre,
 bloque,
 modelPath,
 modelName,
 tipoToPointIndices,
 archivo,
 bloqueIdxReal,
 reactionSet);

 // Acumular reacciones por torre
 var key = Tuple.Create(archivo, torreIdx);
 if (!_reactionsByTower.TryGetValue(key, out var list))
 {
 list = new List<ModelReactionSet>();
 _reactionsByTower[key] = list;
 }
 list.Add(reactionSet);

 totalModelos++;
 _logCallback($"[OK] {modelName} creado exitosamente");
 }
 catch (Exception ex)
 {
 errores++;
 string errorMsg = $"Archivo '{archivo}', Torre {torreIdx +1}, Bloque {bloqueIdxVisible +1}: {ex.Message}";
 erroresDetalle.Add(errorMsg);
 _logCallback($"[ERROR] {errorMsg}");
 }
 }
 }
 }
 catch (Exception ex)
 {
 errores++;
 string errorMsg = $"Archivo '{archivo}': {ex.Message}";
 erroresDetalle.Add(errorMsg);
 _logCallback($"[ERROR] {errorMsg}");
 }
 }

 // Resumen final
 _logCallback("");
 _logCallback("========================================");
 _logCallback($"RESUMEN DE EXPORTACIÓN A SAP2000");
 _logCallback("========================================");
 _logCallback($"✓ Modelos creados: {totalModelos}");
 if (errores >0)
 {
 _logCallback($"✗ Errores: {errores}");
 foreach (var error in erroresDetalle.Take(10))
 _logCallback($" • {error}");
 if (erroresDetalle.Count >10)
 _logCallback($" ... y {erroresDetalle.Count -10} más");
 }
 _logCallback("========================================");

 // Resumen de reacciones (conteos)
 try
 {
 int torresConReacciones = _reactionsByTower.Count;
 int modelosConReacciones = _reactionsByTower.Values.Sum(v => v != null ? v.Count :0);
 _logCallback($"[INFO] Reacciones acumuladas: torres={torresConReacciones}, modelos={modelosConReacciones}");
 }
 catch { }
 }

 /// <summary>
 /// Crea un modelo SAP2000 completo para una torre y bloque.
 /// Usa TransformOutputCache para obtener valores ya transformados (fuente única).
 /// Al final: guarda, ejecuta análisis y extrae reacciones de base.
 /// </summary>
 private void CreateSingleModel(
 TowerModel torre,
 ExcelToCsv.Bloque bloque,
 string modelPath,
 string modelName,
 Dictionary<string, List<int>> tipoToPointIndices,
 string archivo,
 int bloqueIdx,
 ModelReactionSet reactionSet)
 {
 _sapRunner.Invoke(() =>
 {
 var sapModel = _sapProcessor.SapModel;
 Dictionary<string, string> puntosSAP = null;
 List<string> framesCreados = null;

 //1. Inicializar modelo
 try
 {
 _logCallback($" [1/8] Inicializando modelo...");
 sapModel.InitializeNewModel(eUnits.N_m_C);
 sapModel.File.NewBlank();
 }
 catch (Exception ex)
 {
 _logCallback($" ❌ Error en inicialización: {ex.Message}");
 }

 //2. Crear material y sección
 try
 {
 _logCallback($" [2/8] Creando material y sección...");
 var geoBuilder = new SapGeometryBuilder(sapModel, _logCallback);
 geoBuilder.CreateDefaultMaterial();
 geoBuilder.CreateDefaultAngleSection();
 }
 catch (Exception ex)
 {
 _logCallback($" ❌ Error creando material/sección: {ex.Message}");
 }

 //3. Crear puntos
 try
 {
 _logCallback($" [3/8] Creando {torre.Nodes.Count} puntos...");
 var geoBuilder = new SapGeometryBuilder(sapModel, _logCallback);
 puntosSAP = geoBuilder.CreatePoints(torre);
 _logCallback($" ✓ Puntos creados: {puntosSAP.Count}");
 }
 catch (Exception ex)
 {
 _logCallback($" ❌ Error creando puntos: {ex.Message}");
 puntosSAP = new Dictionary<string, string>();
 }

 //3.5. Asignar restricciones de base
 try
 {
 _logCallback($" [3.5/8] Asignando restricciones de base...");
 var geoBuilder = new SapGeometryBuilder(sapModel, _logCallback);
 geoBuilder.AssignBaseRestraints(puntosSAP);
 }
 catch (Exception ex)
 {
 _logCallback($" ❌ Error asignando restricciones: {ex.Message}");
 }

 //4. Crear frames
 try
 {
 _logCallback($" [4/8] Creando {torre.Frames.Count} frames...");
 var geoBuilder = new SapGeometryBuilder(sapModel, _logCallback);
 framesCreados = geoBuilder.CreateFrames(torre, puntosSAP);
 _logCallback($" ✓ Frames creados: {framesCreados.Count}");
 }
 catch (Exception ex)
 {
 _logCallback($" ❌ Error creando frames: {ex.Message}");
 framesCreados = new List<string>();
 }

 //5. Obtener DataTable transformado desde caché (FUENTE ÚNICA DE VERDAD)
 System.Data.DataTable dtOutput = null;
 try
 {
 _logCallback($" [5/8] Obteniendo datos transformados desde caché...");

 Func<string, (int src, int sign)[]> transformConfig = tipo => GetMappingPara(archivo, bloqueIdx, tipo);
 Func<string, int, string> construirTipoConSufijo = (baseTipo, tripIndex) => ConstruirNombreTipoConSufijo(baseTipo, tripIndex);

 dtOutput = TransformOutputCache.GetOrAdd(archivo, bloqueIdx, () =>
 {
 var builder = new OutputTableBuilder();
 return builder.ConstruirDataTableTransformado(bloque, transformConfig, construirTipoConSufijo);
 });

 _logCallback($" ✓ DataTable obtenido: {dtOutput.Columns.Count} columnas, {dtOutput.Rows.Count} filas");
 }
 catch (Exception ex)
 {
 _logCallback($" ❌ Error obteniendo datos transformados: {ex.Message}");
 }

 //6. Crear LoadPatterns y asignar cargas (desde dtOutput)
 if (dtOutput != null && dtOutput.Rows.Count >2)
 {
 try
 {
 _logCallback($" [6/8] Creando LoadPatterns y asignando cargas...");

 var loadPatterns = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase);
 for (int r =2; r < dtOutput.Rows.Count; r++)
 {
 string lp = Convert.ToString(dtOutput.Rows[r][1])?.Trim();
 if (!string.IsNullOrWhiteSpace(lp))
 loadPatterns.Add(lp);
 }

 int createdCount =0;
 foreach (var lp in loadPatterns)
 {
 try
 {
 int ret = sapModel.LoadPatterns.Add(lp, eLoadPatternType.Other,0, true);
 if (ret ==0)
 {
 createdCount++;
 _logCallback($" ✓ LoadPattern '{lp}' creado");
 }
 else
 {
 _logCallback($" ⚠️ LoadPattern '{lp}' no creado (código {ret})");
 }
 }
 catch (Exception ex)
 {
 _logCallback($" ❌ Excepción creando LoadPattern '{lp}': {ex.Message}");
 }
 }
 _logCallback($" ✓ LoadPatterns creados: {createdCount}");

 // NEW: crear patrones de viento (siempre) inmediatamente después de crear LoadPatterns
 try
 {
 CreateAutoWindLoadPatterns(sapModel);
 }
 catch (Exception ex)
 {
 _logCallback($" ⚠️ Error creando patrones de viento: {ex.Message}");
 }

 var tiposColumnas = BuildTipoColumnMap(dtOutput);

 int cargasAsignadas =0;
 int intentosAsignacion =0;
 int skipsPorCero =0;
 int skipsSinPuntos =0;
 int skipsSinPuntoSap =0;

 for (int r =2; r < dtOutput.Rows.Count; r++)
 {
 var row = dtOutput.Rows[r];
 string loadPattern = Convert.ToString(row[1])?.Trim();
 if (string.IsNullOrWhiteSpace(loadPattern))
 continue;

 foreach (var kv in tiposColumnas)
 {
 string tipo = kv.Key;
 int colX = kv.Value.colX;
 int colY = kv.Value.colY;
 int colZ = kv.Value.colZ;

 if (colX <0 || colY <0 || colZ <0)
 continue;

 double fx = ParseDouble(row[colX]);
 double fy = ParseDouble(row[colY]);
 double fz = ParseDouble(row[colZ]);

 if (Math.Abs(fx) <1e-10 && Math.Abs(fy) <1e-10 && Math.Abs(fz) <1e-10)
 {
 skipsPorCero++;
 continue;
 }

 if (!tipoToPointIndices.TryGetValue(tipo, out var pointIndices) || pointIndices == null || pointIndices.Count ==0)
 {
 skipsSinPuntos++;
 continue;
 }

 double[] loadValues = { fx, fy, fz,0,0,0 };

 foreach (var idx in pointIndices)
 {
 string puntoUserName = $"P{idx +1}";
 if (!puntosSAP.TryGetValue(puntoUserName, out string puntoSAPName))
 {
 skipsSinPuntoSap++;
 continue;
 }

 intentosAsignacion++;
 try
 {
 int ret = sapModel.PointObj.SetLoadForce(puntoSAPName, loadPattern, ref loadValues, false, "Global");
 if (ret ==0)
 {
 cargasAsignadas++;
 }
 }
 catch
 {
 // log ya cubierto por contador; mantener silencioso para no saturar
 }
 }
 }
 }

 _logCallback($" ✓ Cargas asignadas: {cargasAsignadas} (intentos={intentosAsignacion}, skipCero={skipsPorCero}, skipSinPuntos={skipsSinPuntos}, skipSinPuntoSAP={skipsSinPuntoSap})");
 }
 catch (Exception ex)
 {
 _logCallback($" ❌ Error en LoadPatterns/cargas: {ex.Message}");
 }
 }
 else
 {
 // NEW: aunque no existan cargas en dtOutput, igual crear patrones de viento.
 try
 {
 CreateAutoWindLoadPatterns(sapModel);
 }
 catch (Exception ex)
 {
 _logCallback($" ⚠️ Error creando patrones de viento (sin dtOutput): {ex.Message}");
 }
 }

 //7. Guardar modelo
 bool savedOk = false;
 try
 {
 _logCallback($" [7/8] Guardando modelo...");
 sapModel.View.RefreshView(0, false);
 int ret = sapModel.File.Save(modelPath);
 if (ret ==0)
 {
 savedOk = true;
 _logCallback($" ✓ Guardado: {Path.GetFileName(modelPath)}");
 }
 else
 {
 _logCallback($" ⚠️ Error al guardar (código {ret})");
 }
 }
 catch (Exception ex)
 {
 _logCallback($" ❌ Error guardando: {ex.Message}");
 }

 //8. Ejecutar análisis y obtener reacciones
 if (savedOk)
 {
 try
 {
 _logCallback($" [8/8] Ejecutando análisis...");
 int retCreate = sapModel.Analyze.CreateAnalysisModel();
 if (retCreate !=0)
 _logCallback($" ⚠️ CreateAnalysisModel retornó {retCreate}");

 // Desactivar por defecto el caso MODAL (puede habilitarse luego desde UI)
 try
 {
 if (!SapWindLoadSettings.Current.RunModalCase)
 {
 int retModal = sapModel.Analyze.SetRunCaseFlag("MODAL", false);
 if (retModal !=0)
 _logCallback($" ⚠️ SetRunCaseFlag('MODAL', false) retornó {retModal}");
 }
 }
 catch (Exception ex)
 {
 _logCallback($" ⚠️ No se pudo desactivar MODAL: {ex.Message}");
 }

 int retRun = sapModel.Analyze.RunAnalysis();
 if (retRun !=0)
 _logCallback($" ⚠️ RunAnalysis retornó {retRun}");

 // Seleccionar casos/resultados para output
 try
 {
 sapModel.Results.Setup.DeselectAllCasesAndCombosForOutput();
 }
 catch { }

 // Seleccionar TODOS los casos y combinaciones disponibles para output.
 // Objetivo: que JointReact devuelva DEAD y cualquier otro caso/combination que SAP tenga.
 try
 {
 // Intentar combos
 try
 {
 int nCombos =0;
 string[] combos = null;
 int retCombos = sapModel.RespCombo.GetNameList(ref nCombos, ref combos);
 if (retCombos ==0 && combos != null)
 {
 for (int i =0; i < combos.Length; i++)
 {
 var c = combos[i];
 if (!string.IsNullOrWhiteSpace(c))
 {
 try { sapModel.Results.Setup.SetComboSelectedForOutput(c); } catch { }
 }
 }
 }
 }
 catch { }

 // Intentar casos
 try
 {
 int nCases =0;
 string[] cases = null;
 int retCases = sapModel.LoadCases.GetNameList(ref nCases, ref cases);
 if (retCases ==0 && cases != null)
 {
 for (int i =0; i < cases.Length; i++)
 {
 var c = cases[i];
 if (!string.IsNullOrWhiteSpace(c))
 {
 try { sapModel.Results.Setup.SetCaseSelectedForOutput(c); } catch { }
 }
 }
 }
 }
 catch { }
 }
 catch { }

 // Reacciones para puntos base
 CaptureBaseReactions(sapModel, puntosSAP, reactionSet);
 
 // NEW: BaseReactWithCentroid (estructura total)
 CaptureBaseReactWithCentroid(sapModel, reactionSet);
 }
 catch (Exception ex)
 {
 _logCallback($" ❌ Error en análisis/reacciones: {ex.Message}");
 }
 }

 _logCallback($" Resumen: Puntos={puntosSAP?.Count ??0}, Frames={framesCreados?.Count ??0}");
 });
 }

 private void CaptureBaseReactions(cSapModel sapModel, Dictionary<string, string> puntosSAP, ModelReactionSet reactionSet)
 {
 if (sapModel == null || puntosSAP == null || reactionSet == null)
 return;

 // Se asume que la geometría crea estos4 nodos base.
 var baseNodes = new[] { "Base1_1", "Base1_2", "Base1_3", "Base1_4" };

 // Renombrado SOLO para UI/Export (no afecta lookup en SAP)
 var displayMap = new Dictionary<string, string>(StringComparer.InvariantCulture)
 {
 { "Base1_1", "Pata_1" },
 { "Base1_2", "Pata_2" },
 { "Base1_3", "Pata_3" },
 { "Base1_4", "Pata_4" }
 };

 foreach (var nodeUi in baseNodes)
 {
 if (!puntosSAP.TryGetValue(nodeUi, out var nodeSap) || string.IsNullOrWhiteSpace(nodeSap))
 {
 _logCallback($" ⚠️ [Reacciones] No existe punto SAP para '{nodeUi}'.");
 continue;
 }

 var nodeUiDisplay = displayMap.TryGetValue(nodeUi, out var disp) ? disp : nodeUi;

 int numberResults =0;
 string[] obj = null;
 string[] elm = null;
 string[] loadCase = null;
 string[] stepType = null;
 double[] stepNum = null;
 double[] f1 = null;
 double[] f2 = null;
 double[] f3 = null;
 double[] m1 = null;
 double[] m2 = null;
 double[] m3 = null;

 try
 {
 int ret = sapModel.Results.JointReact(
 nodeSap,
 eItemTypeElm.ObjectElm,
 ref numberResults,
 ref obj,
 ref elm,
 ref loadCase,
 ref stepType,
 ref stepNum,
 ref f1,
 ref f2,
 ref f3,
 ref m1,
 ref m2,
 ref m3);

 if (ret !=0)
 {
 _logCallback($" ⚠️ [Reacciones] JointReact('{nodeUi}') retornó {ret}");
 continue;
 }

 if (numberResults <=0)
 {
 _logCallback($" ⚠️ [Reacciones] Sin resultados para '{nodeUi}'.");
 continue;
 }

 for (int i =0; i < numberResults; i++)
 {
 var rr = new JointReactionResult
 {
 PuntoNombreUi = nodeUiDisplay,
 PuntoNombreSap = nodeSap,
 Obj = SafeGet(obj, i),
 Elm = SafeGet(elm, i),
 LoadCase = SafeGet(loadCase, i),
 StepType = SafeGet(stepType, i),
 StepNum = SafeGet(stepNum, i),
 F1 = SafeGet(f1, i),
 F2 = SafeGet(f2, i),
 F3 = SafeGet(f3, i),
 M1 = SafeGet(m1, i),
 M2 = SafeGet(m2, i),
 M3 = SafeGet(m3, i)
 };
 reactionSet.Reactions.Add(rr);
 }

 _logCallback($" ✓ [Reacciones] {nodeUiDisplay}: {numberResults} resultados");
 }
 catch (Exception ex)
 {
 _logCallback($" ❌ [Reacciones] Error leyendo '{nodeUiDisplay}': {ex.Message}");
 }
 }
 }

 private void CaptureBaseReactWithCentroid(cSapModel sapModel, ModelReactionSet reactionSet)
 {
 if (sapModel == null || reactionSet == null) return;

 int numberResults =0;
 string[] loadCase = null;
 string[] stepType = null;
 double[] stepNum = null;
 double[] fx = null;
 double[] fy = null;
 double[] fz = null;
 double[] mx = null;
 double[] my = null;
 double[] mz = null;
 double gx =0d;
 double gy =0d;
 double gz =0d;
 double[] xCentroidForFx = null;
 double[] yCentroidForFx = null;
 double[] zCentroidForFx = null;
 double[] xCentroidForFy = null;
 double[] yCentroidForFy = null;
 double[] zCentroidForFy = null;
 double[] xCentroidForFz = null;
 double[] yCentroidForFz = null;
 double[] zCentroidForFz = null;

 try
 {
 int ret = sapModel.Results.BaseReactWithCentroid(
 ref numberResults,
 ref loadCase,
 ref stepType,
 ref stepNum,
 ref fx,
 ref fy,
 ref fz,
 ref mx,
 ref my,
 ref mz,
 ref gx,
 ref gy,
 ref gz,
 ref xCentroidForFx,
 ref yCentroidForFx,
 ref zCentroidForFx,
 ref xCentroidForFy,
 ref yCentroidForFy,
 ref zCentroidForFy,
 ref xCentroidForFz,
 ref yCentroidForFz,
 ref zCentroidForFz);

 if (ret !=0)
 {
 _logCallback($" ⚠️ [BaseReactWithCentroid] ret={ret}");
 return;
 }

 if (numberResults <=0)
 {
 _logCallback($" ⚠️ [BaseReactWithCentroid] Sin resultados");
 return;
 }

 for (int i =0; i < numberResults; i++)
 {
 reactionSet.BaseReactionsWithCentroid.Add(new ModelReactionSet.BaseReactWithCentroidResult
 {
 LoadCase = SafeGet(loadCase, i),
 StepType = SafeGet(stepType, i),
 StepNum = SafeGet(stepNum, i),
 Fx = SafeGet(fx, i),
 Fy = SafeGet(fy, i),
 Fz = SafeGet(fz, i),
 Mx = SafeGet(mx, i),
 My = SafeGet(my, i),
 Mz = SafeGet(mz, i),
 Gx = gx,
 Gy = gy,
 Gz = gz,
 XCentroidForFx = SafeGet(xCentroidForFx, i),
 YCentroidForFx = SafeGet(yCentroidForFx, i),
 ZCentroidForFx = SafeGet(zCentroidForFx, i),
 XCentroidForFy = SafeGet(xCentroidForFy, i),
 YCentroidForFy = SafeGet(yCentroidForFy, i),
 ZCentroidForFy = SafeGet(zCentroidForFy, i),
 XCentroidForFz = SafeGet(xCentroidForFz, i),
 YCentroidForFz = SafeGet(yCentroidForFz, i),
 ZCentroidForFz = SafeGet(zCentroidForFz, i),
 });
 }

 _logCallback($" ✓ [BaseReactWithCentroid] {numberResults} resultados (gx,gy,gz)=({gx.ToString("F3", CultureInfo.InvariantCulture)},{gy.ToString("F3", CultureInfo.InvariantCulture)},{gz.ToString("F3", CultureInfo.InvariantCulture)})");
 }
 catch (Exception ex)
 {
 _logCallback($" ⚠️ [BaseReactWithCentroid] Excepción: {ex.Message}");
 }
 }

 private static string SafeGet(string[] arr, int idx)
 {
 if (arr == null) return string.Empty;
 if (idx <0 || idx >= arr.Length) return string.Empty;
 return arr[idx] ?? string.Empty;
 }

 private static double SafeGet(double[] arr, int idx)
 {
 if (arr == null) return 0d;
 if (idx <0 || idx >= arr.Length) return 0d;
 return arr[idx];
 }

 /// <summary>
 /// Construye mapa tipo -> (colX, colY, colZ) desde las columnas del DataTable.
 /// </summary>
 private Dictionary<string, (int colX, int colY, int colZ)> BuildTipoColumnMap(System.Data.DataTable dt)
 {
 var result = new Dictionary<string, (int colX, int colY, int colZ)>(StringComparer.InvariantCultureIgnoreCase);

 for (int col =3; col < dt.Columns.Count; col++)
 {
 string colName = dt.Columns[col].ColumnName ?? string.Empty;

 if (colName.EndsWith(" X", StringComparison.InvariantCultureIgnoreCase))
 {
 string tipoBase = colName.Substring(0, colName.Length -2);
 if (!result.ContainsKey(tipoBase)) result[tipoBase] = (-1, -1, -1);
 var entry = result[tipoBase];
 result[tipoBase] = (col, entry.colY, entry.colZ);
 }
 else if (colName.EndsWith(" Y", StringComparison.InvariantCultureIgnoreCase))
 {
 string tipoBase = colName.Substring(0, colName.Length -2);
 if (!result.ContainsKey(tipoBase)) result[tipoBase] = (-1, -1, -1);
 var entry = result[tipoBase];
 result[tipoBase] = (entry.colX, col, entry.colZ);
 }
 else if (colName.EndsWith(" Z", StringComparison.InvariantCultureIgnoreCase))
 {
 string tipoBase = colName.Substring(0, colName.Length -2);
 if (!result.ContainsKey(tipoBase)) result[tipoBase] = (-1, -1, -1);
 var entry = result[tipoBase];
 result[tipoBase] = (entry.colX, entry.colY, col);
 }
 }

 return result;
 }

 private double ParseDouble(object value)
 {
 if (value == null || value == DBNull.Value) return 0d;
 double.TryParse(
 Convert.ToString(value, System.Globalization.CultureInfo.InvariantCulture),
 System.Globalization.NumberStyles.Float,
 System.Globalization.CultureInfo.InvariantCulture,
 out double result);
 return result;
 }

 /// <summary>
 /// Mapea el archivo, bloque y tipo a una configuración de transformación (permutación de ejes).
 /// </summary>
 private (int src, int sign)[] GetMappingPara(string archivo, int bloqueIdxReal, string tipo)
 {
 // Default identity mapping
 var identidad = _coordinateTransformer.ComputePermutation("+X", "+Y", "+Z");

 if (_transformGrid == null)
 return identidad;

 foreach (DataGridViewRow r in _transformGrid.Rows)
 {
 if (!(r.Tag is TransformContext ctx))
 continue;

 string rowArchivo = Convert.ToString(r.Cells["Archivo"].Value) ?? string.Empty;
 if (!string.Equals(rowArchivo, archivo ?? string.Empty, StringComparison.InvariantCulture))
 continue;

 // ✅ Use stable key: ctx.BloqueIndex is real0-based index in full block list
 if (ctx.BloqueIndex != bloqueIdxReal)
 continue;

 if (!string.Equals(ctx.Tipo, tipo, StringComparison.InvariantCulture))
 continue;

 string c1 = _coordinateTransformer.NormalizarEje(r.Cells["C1"].Value as string) ?? "+X";
 string c2 = _coordinateTransformer.NormalizarEje(r.Cells["C2"].Value as string) ?? "+Y";
 string c3 = _coordinateTransformer.NormalizarEje(r.Cells["C3"].Value as string) ?? "+Z";
 return _coordinateTransformer.ComputePermutation(c1, c2, c3);
 }

 return identidad;
 }

 private string ConstruirNombreTipoConSufijo(string baseTipo, int tripIndex)
 {
 return _coordinateTransformer.ConstruirNombreTipoConSufijo(baseTipo, tripIndex);
 }

 private string SanitizeFileName(string fileName)
 {
 var invalidos = Path.GetInvalidFileNameChars();
 foreach (var c in invalidos)
 fileName = fileName.Replace(c, '_');
 return fileName;
 }

 // NEW: helper centralizado
 private void CreateAutoWindLoadPatterns(cSapModel sapModel)
 {
 if (sapModel == null) return;

 var s = SapWindLoadSettings.Current;
 var angles = s.GetDirAnglesNormalized();

 foreach (var a in angles)
 {
 string name = $"WIND_{a.ToString(CultureInfo.InvariantCulture)}";

 int retAdd;
 try
 {
 retAdd = sapModel.LoadPatterns.Add(name, eLoadPatternType.Wind,0, true);
 }
 catch
 {
 retAdd = sapModel.LoadPatterns.Add(name, eLoadPatternType.Other,0, true);
 }

 if (retAdd !=0)
 {
 _logCallback($" ⚠️ Wind LoadPattern '{name}' no creado (código {retAdd})");
 continue;
 }

 int retSet = sapModel.LoadPatterns.AutoWind.SetASCE716(
 name,
 s.ExposureFrom,
 a,
0d,
0d,
0,
0d,
0d,
 false,
0d,
0d,
 s.WindSpeed,
 s.ExposureType,
 s.Kzt,
 s.GustFactor,
 s.Kd,
 s.SolidGrossRatio,
 false);

 if (retSet ==0)
 {
 _logCallback($" ✓ AutoWind ASCE7-16 configurado: '{name}' (DirAngle={a.ToString(CultureInfo.InvariantCulture)})");
 }
 else
 {
 _logCallback($" ⚠️ AutoWind.SetASCE716 falló para '{name}' (código {retSet})");
 }
 }
 }
 }

}
