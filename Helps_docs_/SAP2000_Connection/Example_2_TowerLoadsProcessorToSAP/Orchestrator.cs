using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using PROYECTOS_CSI.Infrastructure.SAP2000;
using PROYECTOS_CSI.Core.Domain.Motores;
using PROYECTOS_CSI.Core.Geometry;
using PROYECTOS_CSI.Core.Geometry.Orders;
using SAP2000v1;

namespace PROYECTOS_CSI
{
 public sealed class Orchestrator
 {
 private readonly SapModelFacade _facade;
 private string _cassetteDir;
 private string _cassetteName;
 public Orchestrator(SapModelFacade facade)
 { _facade = facade ?? throw new ArgumentNullException(nameof(facade)); }

 public void Run(IEnumerable<string> steps, OrchestrationOptions opts)
 {
 // Cassette textual explícito (hardcoded según requerimiento)
 _cassetteName = "Tanque_subterraneo";
 var cassetteRoot = opts?.CassetteRoot ?? Environment.GetEnvironmentVariable("CASSETTE_ROOT");
 _cassetteDir = ResolveCassetteDir(cassetteRoot, _cassetteName);
 Program.Log("CassetteName (hardcoded): {0}", _cassetteName);
 Program.Log("CassetteDir: {0}", _cassetteDir);
 if (!Directory.Exists(_cassetteDir)) { Program.Log("CassetteDir no existe: {0}. Nada que ejecutar.", _cassetteDir); return; }

 // Los steps DEBEN ser leídos del cassette; si no hay, no se ejecuta nada
 var cassetteSteps = LoadCassetteStepsOrdered();
 if (cassetteSteps == null || cassetteSteps.Count ==0)
 { Program.Log("steps.json no encontrado o vacío en cassette '{0}'. Flujo no ejecutado.", _cassetteName); return; }

 foreach (var step in cassetteSteps)
 {
 switch (step)
 {
 case "materials": { var spec = LoadMaterialsSpecFromCassette(); if (spec == null || spec.Materials == null || spec.Materials.Count ==0) { Program.Log("materials: sin datos"); break; } new MaterialEngine(_facade).Apply(spec); break; }
 case "areaProperties": { var spec = LoadAreaPropertiesSpecFromCassette(); if (spec == null || spec.Items == null || spec.Items.Length ==0) { Program.Log("areaProperties: sin datos"); break; } new AreaPropertyEngine(_facade).Apply(spec); break; }
 case "geometry": { var order = LoadGeometryOrderFromCassette(); if (order == null || (order.Contours?.Count ??0) ==0) { Program.Log("geometry: sin datos"); break; } var geomOrch = new PROYECTOS_CSI.Infrastructure.SAP2000.GeometryOrchestrator(_facade.Raw, (ret, where) => _facade.Check(ret, where)); geomOrch.Apply(order); break; }
 case "groups": { var spec = LoadGroupsSpecFromCassette(); if (spec == null || spec.Items == null || spec.Items.Length ==0) { Program.Log("groups: sin datos"); break; } new GroupsCreationAndAssignEngine(_facade).Apply(spec); break; }
 case "pointPatterns": { var spec = LoadPointPatternsSpecFromCassette(); if (spec == null || spec.Items == null || spec.Items.Length ==0) { Program.Log("pointPatterns: sin datos"); break; } new PointsPatternsEngine(_facade).Apply(spec); break; }
 case "patterns": { var spec = LoadPatternsSpecFromCassette(); if (spec == null || spec.Items == null || spec.Items.Length ==0) { Program.Log("patterns: sin datos"); break; } Program.Log("Creando LoadPatterns: {0} patrón(es)", spec.Items.Length); new LoadPatternEngine(_facade).Apply(spec); break; }
 case "cases": { var spec = LoadCasesSpecFromCassette(); if (spec == null || spec.Items == null || spec.Items.Length ==0) { Program.Log("cases: sin datos"); break; } new LoadCaseEngine(_facade).Apply(spec); break; }
 case "combos": { var spec = LoadCombosSpecFromCassette(); if (spec == null || spec.Items == null || spec.Items.Length ==0) { Program.Log("combos: sin datos"); break; } new LoadCombinationEngine(_facade).Apply(spec); break; }
 case "assignLoads": { Program.Log("assignLoads: paso no implementado"); break; }
 case "analyze": { Program.Log("analyze: paso no implementado"); break; }
 default: Program.Log("Paso desconocido: {0}", step); break;
 }
 }
 }

 private List<string> LoadCassetteStepsOrdered()
 {
 try
 {
 var stepsPath = Path.Combine(_cassetteDir, "steps.json");
 if (!File.Exists(stepsPath)) return new List<string>();
 var json = File.ReadAllText(stepsPath);
 var listBlock = Regex.Match(json, "\\[([\\s\\S]*?)\\]", RegexOptions.Singleline);
 if (!listBlock.Success) return new List<string>();
 var items = Regex.Matches(listBlock.Groups[1].Value, "\\\"([\\s\\S]*?)\\\"", RegexOptions.Singleline)
 .Cast<Match>()
 .Select(m => m.Groups[1].Value.Trim())
 .Where(s => !string.IsNullOrWhiteSpace(s))
 .ToList();
 return items;
 }
 catch { return new List<string>(); }
 }

 private string ResolveCassetteDir(string rootOverride, string cassetteName)
 {
 string root = rootOverride;
 if (string.IsNullOrWhiteSpace(root))
 {
 var baseDir = AppDomain.CurrentDomain.BaseDirectory;
 var candidate = Path.GetFullPath(Path.Combine(baseDir, "..", "..", "..", "PROYECTOS_CSI", "Cassettes"));
 if (Directory.Exists(candidate)) root = candidate;
 else
 {
 var outCandidate = Path.Combine(baseDir, "PROYECTOS_CSI", "Cassettes");
 if (Directory.Exists(outCandidate)) root = outCandidate;
 }
 }
 var dir = Path.Combine(root ?? string.Empty, cassetteName);
 return dir;
 }

 // ====== Cassette loaders ======
 private MaterialEngine.MaterialsDto LoadMaterialsSpecFromCassette()
 {
 var path = Path.Combine(_cassetteDir, "materials.json"); if (!File.Exists(path)) return null; var json = File.ReadAllText(path);
 var dto = new MaterialEngine.MaterialsDto();
 var stressM = Regex.Match(json, "\\\"Stress\\\"\\s*:\\s*\\\"([\\s\\S]*?)\\\"", RegexOptions.Singleline);
 var tempM = Regex.Match(json, "\\\"Temp\\\"\\s*:\\s*\\\"([\\s\\S]*?)\\\"", RegexOptions.Singleline);
 dto.Units = new MaterialEngine.UnitsDto { Stress = stressM.Success ? stressM.Groups[1].Value.Trim() : null, Temp = tempM.Success ? tempM.Groups[1].Value.Trim() : null };
 var itemsBlock = Regex.Match(json, "\\\"Materials\\\"\\s*:\\s*\\[(.*?)\\]", RegexOptions.Singleline);
 var list = new List<MaterialEngine.MaterialSpec>();
 if (itemsBlock.Success)
 {
 var items = Regex.Matches(itemsBlock.Groups[1].Value, "\\{[\\s\\S]*?\\}", RegexOptions.Singleline);
 foreach (Match it in items)
 {
 var nameM = Regex.Match(it.Value, "\\\"Name\\\"\\s*:\\s*\\\"([\\s\\S]*?)\\\"", RegexOptions.Singleline);
 var typeM = Regex.Match(it.Value, "\\\"Type\\\"\\s*:\\s*(\\d+)");
 var nuM = Regex.Match(it.Value, "\\\"Nu\\\"\\s*:\\s*([-+]?[0-9]*\\.?[0-9]+)");
 var alphaM = Regex.Match(it.Value, "\\\"Alpha\\\"\\s*:\\s*([-+]?[0-9]*\\.?[0-9]+(?:e[-+]?[0-9]+)?)", RegexOptions.Singleline);
 var iso = new MaterialEngine.IsotropicDto { Nu = nuM.Success ? double.Parse(nuM.Groups[1].Value, System.Globalization.CultureInfo.InvariantCulture) :0.2, Alpha = alphaM.Success ? double.Parse(alphaM.Groups[1].Value, System.Globalization.CultureInfo.InvariantCulture) :5.5e-6 };
 var concBlock = Regex.Match(it.Value, "\\\"Concrete\\\"\\s*:\\s*\\{([\\s\\S]*?)\\}", RegexOptions.Singleline);
 MaterialEngine.ConcreteDto conc = null;
 if (concBlock.Success)
 {
 var fcM = Regex.Match(concBlock.Groups[1].Value, "\\\"fc\\\"\\s*:\\s*([-+]?[0-9]*\\.?[0-9]+)");
 var unitsM = Regex.Match(concBlock.Groups[1].Value, "\\\"fc_Units\\\"\\s*:\\s*\\\"([\\s\\S]*?)\\\"", RegexOptions.Singleline);
 var compM = Regex.Match(concBlock.Groups[1].Value, "\\\"ComputeEFromFc\\\"\\s*:\\s*(true|false)");
 var lwM = Regex.Match(concBlock.Groups[1].Value, "\\\"IsLightweight\\\"\\s*:\\s*(true|false)");
 var fcsM = Regex.Match(concBlock.Groups[1].Value, "\\\"fcsfactor\\\"\\s*:\\s*([-+]?[0-9]*\\.?[0-9]+)");
 var ssM = Regex.Match(concBlock.Groups[1].Value, "\\\"SSType\\\"\\s*:\\s*(\\d+)");
 var hysM = Regex.Match(concBlock.Groups[1].Value, "\\\"SSHysType\\\"\\s*:\\s*(\\d+)");
 var safcM = Regex.Match(concBlock.Groups[1].Value, "\\\"StrainAtfc\\\"\\s*:\\s*([-+]?[0-9]*\\.?[0-9]+)");
 var suM = Regex.Match(concBlock.Groups[1].Value, "\\\"StrainUltimate\\\"\\s*:\\s*([-+]?[0-9]*\\.?[0-9]+)");
 var fsM = Regex.Match(concBlock.Groups[1].Value, "\\\"FinalSlope\\\"\\s*:\\s*([-+]?[0-9]*\\.?[0-9]+)");
 conc = new MaterialEngine.ConcreteDto { fc = fcM.Success ? double.Parse(fcM.Groups[1].Value, System.Globalization.CultureInfo.InvariantCulture) :0.0, fc_Units = unitsM.Success ? unitsM.Groups[1].Value.Trim() : "MPa", ComputeEFromFc = compM.Success && bool.Parse(compM.Groups[1].Value), IsLightweight = lwM.Success && bool.Parse(lwM.Groups[1].Value), fcsfactor = fcsM.Success ? double.Parse(fcsM.Groups[1].Value, System.Globalization.CultureInfo.InvariantCulture) :1.0, SSType = ssM.Success ? int.Parse(ssM.Groups[1].Value) :2, SSHysType = hysM.Success ? int.Parse(hysM.Groups[1].Value) :2, StrainAtfc = safcM.Success ? double.Parse(safcM.Groups[1].Value, System.Globalization.CultureInfo.InvariantCulture) :0.00192, StrainUltimate = suM.Success ? double.Parse(suM.Groups[1].Value, System.Globalization.CultureInfo.InvariantCulture) :0.005, FinalSlope = fsM.Success ? double.Parse(fsM.Groups[1].Value, System.Globalization.CultureInfo.InvariantCulture) : -0.10 };
 }
 var ms = new MaterialEngine.MaterialSpec { Name = nameM.Success ? nameM.Groups[1].Value.Trim() : null, Type = typeM.Success ? int.Parse(typeM.Groups[1].Value) : (int)eMatType.Concrete, Isotropic = iso, Concrete = conc };
 list.Add(ms);
 }
 }
 dto.Materials = list;
 return dto;
 }

 private LoadPatternEngine.PatternsSpec LoadPatternsSpecFromCassette()
 {
 var path = Path.Combine(_cassetteDir, "patterns.json"); if (!File.Exists(path)) return null; var json = File.ReadAllText(path);
 var spec = new LoadPatternEngine.PatternsSpec(); var list = new List<LoadPatternEngine.PatternSpec>();
 var itemsBlock = Regex.Match(json, "\\\"Patterns\\\"\\s*:\\s*\\[(.*?)\\]", RegexOptions.Singleline);
 if (!itemsBlock.Success) return spec;
 var items = Regex.Matches(itemsBlock.Groups[1].Value, "\\{[\\s\\S]*?\\}", RegexOptions.Singleline);
 foreach (Match it in items)
 {
 var nameM = Regex.Match(it.Value, "\\\"Name\\\"\\s*:\\s*\\\"([\\s\\S]*?)\\\"");
 var typeM = Regex.Match(it.Value, "\\\"Type\\\"\\s*:\\s*(\\d+)");
 var swM = Regex.Match(it.Value, "\\\"SelfWtMult\\\"\\s*:\\s*([-+]?[0-9]*\\.?[0-9]+)");
 var addM = Regex.Match(it.Value, "\\\"AddAnalysisCase\\\"\\s*:\\s*(true|false)");
 if (!nameM.Success || !typeM.Success) continue;
 list.Add(new LoadPatternEngine.PatternSpec { Name = nameM.Groups[1].Value.Trim(), Type = (SAP2000v1.eLoadPatternType)int.Parse(typeM.Groups[1].Value), SelfWtMult = swM.Success ? double.Parse(swM.Groups[1].Value, System.Globalization.CultureInfo.InvariantCulture) :0.0, AddAnalysisCase = addM.Success ? bool.Parse(addM.Groups[1].Value) : false });
 }
 spec.Items = list.ToArray();
 return spec;
 }

 private LoadCaseEngine.CasesSpec LoadCasesSpecFromCassette()
 {
 var path = Path.Combine(_cassetteDir, "cases.json"); if (!File.Exists(path)) return null; var json = File.ReadAllText(path);
 var spec = new LoadCaseEngine.CasesSpec(); var list = new List<LoadCaseEngine.CaseSpec>();
 var itemsBlock = Regex.Match(json, "\\\"Cases\\\"\\s*:\\s*\\[(.*?)\\]", RegexOptions.Singleline);
 if (!itemsBlock.Success) return spec;
 var items = Regex.Matches(itemsBlock.Groups[1].Value, "\\{[\\s\\S]*?\\}", RegexOptions.Singleline);
 foreach (Match it in items)
 {
 var nameM = Regex.Match(it.Value, "\\\"Name\\\"\\s*:\\s*\\\"([\\s\\S]*?)\\\"", RegexOptions.Singleline);
 var typeM = Regex.Match(it.Value, "\\\"Type\\\"\\s*:\\s*\\\"([\\s\\S]*?)\\\"", RegexOptions.Singleline);
 var loadsBlock = Regex.Match(it.Value, "\\\"Loads\\\"\\s*:\\s*\\[(.*?)\\]", RegexOptions.Singleline);
 var cs = new LoadCaseEngine.CaseSpec { Name = nameM.Success ? nameM.Groups[1].Value.Trim() : null, Type = typeM.Success ? typeM.Groups[1].Value.Trim() : "StaticLinear" };
 var loads = new List<LoadCaseEngine.CaseLoadSpec>();
 if (loadsBlock.Success)
 {
 var lItems = Regex.Matches(loadsBlock.Groups[1].Value, "\\{[\\s\\S]*?\\}", RegexOptions.Singleline);
 foreach (Match lm in lItems)
 {
 var patM = Regex.Match(lm.Value, "\\\"Pattern\\\"\\s*:\\s*\\\"([\\s\\S]*?)\\\"", RegexOptions.Singleline);
 var scM = Regex.Match(lm.Value, "\\\"Scale\\\"\\s*:\\s*([-+]?[0-9]*\\.?[0-9]+)");
 if (!patM.Success) continue;
 loads.Add(new LoadCaseEngine.CaseLoadSpec { PatternName = patM.Groups[1].Value.Trim(), Scale = scM.Success ? double.Parse(scM.Groups[1].Value, System.Globalization.CultureInfo.InvariantCulture) :1.0 });
 }
 }
 cs.Loads = loads.ToArray();
 list.Add(cs);
 }
 spec.Items = list.ToArray();
 return spec;
 }

 private LoadCombinationEngine.CombosSpec LoadCombosSpecFromCassette()
 {
 var path = Path.Combine(_cassetteDir, "combos.json"); if (!File.Exists(path)) return null; var json = File.ReadAllText(path);
 var spec = new LoadCombinationEngine.CombosSpec(); var list = new List<LoadCombinationEngine.ComboSpec>();
 var itemsBlock = Regex.Match(json, "\\\"Combos\\\"\\s*:\\s*\\[(.*?)\\]", RegexOptions.Singleline);
 if (!itemsBlock.Success) return spec;
 var items = Regex.Matches(itemsBlock.Groups[1].Value, "\\{[\\s\\S]*?\\}", RegexOptions.Singleline);
 foreach (Match it in items)
 {
 var nameM = Regex.Match(it.Value, "\\\"Name\\\"\\s*:\\s*\\\"([\\s\\S]*?)\\\"", RegexOptions.Singleline);
 var typeM = Regex.Match(it.Value, "\\\"Type\\\"\\s*:\\s*(\\d+)");
 var memBlock = Regex.Match(it.Value, "\\\"Members\\\"\\s*:\\s*\\[(.*?)\\]", RegexOptions.Singleline);
 var cb = new LoadCombinationEngine.ComboSpec { Name = nameM.Success ? nameM.Groups[1].Value.Trim() : null, Type = typeM.Success ? int.Parse(typeM.Groups[1].Value) :0 };
 var memList = new List<LoadCombinationEngine.ComboMemberSpec>();
 if (memBlock.Success)
 {
 var mItems = Regex.Matches(memBlock.Groups[1].Value, "\\{[\\s\\S]*?\\}", RegexOptions.Singleline);
 foreach (Match mm in mItems)
 {
 var caseM = Regex.Match(mm.Value, "\\\"Case\\\"\\s*:\\s*\\\"([\\s\\S]*?)\\\"", RegexOptions.Singleline);
 var scM = Regex.Match(mm.Value, "\\\"Scale\\\"\\s*:\\s*([-+]?[0-9]*\\.?[0-9]+)");
 if (!caseM.Success) continue;
 memList.Add(new LoadCombinationEngine.ComboMemberSpec { CaseName = caseM.Groups[1].Value.Trim(), ScaleFactor = scM.Success ? double.Parse(scM.Groups[1].Value, System.Globalization.CultureInfo.InvariantCulture) :1.0 });
 }
 }
 cb.Members = memList.ToArray();
 list.Add(cb);
 }
 spec.Items = list.ToArray();
 return spec;
 }

 private AreaPropertyEngine.AreaPropertiesSpec LoadAreaPropertiesSpecFromCassette()
 {
 var apPath = Path.Combine(_cassetteDir, "area-properties.json"); if (!File.Exists(apPath)) return null; var json = File.ReadAllText(apPath);
 var spec = new AreaPropertyEngine.AreaPropertiesSpec();
 var defM = Regex.Match(json, "\\\"DefaultProperty\\\"\\s*:\\s*\\\"([\\s\\S]*?)\\\"", RegexOptions.Singleline);
 if (defM.Success) spec.DefaultProperty = defM.Groups[1].Value.Trim();
 var itemsBlock = Regex.Match(json, "\\\"Properties\\\"\\s*:\\s*\\[(.*?)\\]", RegexOptions.Singleline);
 if (!itemsBlock.Success) return spec;
 var items = Regex.Matches(itemsBlock.Groups[1].Value, "\\{[\\s\\S]*?\\}", RegexOptions.Singleline);
 var list = new List<AreaPropertyEngine.AreaPropertySpec>();
 foreach (Match it in items)
 {
 var nameM = Regex.Match(it.Value, "\\\"Name\\\"\\s*:\\s*\\\"([\\s\\S]*?)\\\"", RegexOptions.Singleline);
 var matM = Regex.Match(it.Value, "\\\"Material\\\"\\s*:\\s*\\\"([\\s\\S]*?)\\\"", RegexOptions.Singleline);
 var thM = Regex.Match(it.Value, "\\\"Thickness\\\"\\s*:\\s*([-+]?[0-9]*\\.?[0-9]+)");
 var angM = Regex.Match(it.Value, "\\\"Angle\\\"\\s*:\\s*([-+]?[0-9]*\\.?[0-9]+)");
 if (!nameM.Success || !matM.Success || !thM.Success) continue;
 list.Add(new AreaPropertyEngine.AreaPropertySpec { Name = nameM.Groups[1].Value.Trim(), Material = matM.Groups[1].Value.Trim(), Thickness = double.Parse(thM.Groups[1].Value, System.Globalization.CultureInfo.InvariantCulture), Angle = angM.Success ? double.Parse(angM.Groups[1].Value, System.Globalization.CultureInfo.InvariantCulture) :0.0 });
 }
 spec.Items = list.ToArray();
 return spec;
 }

 private GroupsCreationAndAssignEngine.GroupsSpec LoadGroupsSpecFromCassette()
 {
 var groupsPath = Path.Combine(_cassetteDir, "groups.json"); if (!File.Exists(groupsPath)) return null; var json = File.ReadAllText(groupsPath);
 var spec = new GroupsCreationAndAssignEngine.GroupsSpec();
 var itemsBlock = Regex.Match(json, "\\\"Groups\\\"\\s*:\\s*\\[(.*?)\\]", RegexOptions.Singleline);
 if (!itemsBlock.Success) return spec;
 var items = Regex.Matches(itemsBlock.Groups[1].Value, "\\{[\\s\\S]*?\\}", RegexOptions.Singleline);
 var list = new List<GroupsCreationAndAssignEngine.GroupSpec>();
 foreach (Match it in items)
 {
 var nameM = Regex.Match(it.Value, "\\\"Name\\\"\\s*:\\s*\\\"([\\s\\S]*?)\\\"", RegexOptions.Singleline);
 if (!nameM.Success) continue;
 var g = new GroupsCreationAndAssignEngine.GroupSpec { Name = nameM.Groups[1].Value.Trim() };
 var propsBlock = Regex.Match(it.Value, "\\\"AssignAreaProperties\\\"\\s*:\\s*\\[(.*?)\\]", RegexOptions.Singleline);
 if (propsBlock.Success)
 {
 var props = Regex.Matches(propsBlock.Groups[1].Value, "\\\"([\\s\\S]*?)\\\"", RegexOptions.Singleline).Cast<Match>().Select(m => m.Groups[1].Value.Trim()).Where(s => !string.IsNullOrWhiteSpace(s)).ToArray();
 g.AssignAreaProperties = props;
 }
 list.Add(g);
 }
 spec.Items = list.ToArray();
 return spec;
 }

 private PointsPatternsEngine.PointPatternsSpec LoadPointPatternsSpecFromCassette()
 {
 var patternsPath = Path.Combine(_cassetteDir, "patterns.json"); var calcPath = Path.Combine(_cassetteDir, "calculations.json"); if (!File.Exists(patternsPath)) return null; var json = File.ReadAllText(patternsPath);
 double alt_liquido =0.0, H =0.0, h_SAL =0.0; try { if (File.Exists(calcPath)) { var calc = File.ReadAllText(calcPath); alt_liquido = ParseDouble(calc, "alt_liquido"); H = ParseDouble(calc, "H"); h_SAL = ParseDouble(calc, "h_SAL"); } } catch { }
 var spec = new PointsPatternsEngine.PointPatternsSpec(); var itemsBlock = Regex.Match(json, "\\\"PointPatterns\\\"\\s*:\\s*\\[(.*?)\\]", RegexOptions.Singleline);
 if (!itemsBlock.Success) return spec;
 var items = Regex.Matches(itemsBlock.Groups[1].Value, "\\{[\\s\\S]*?\\}", RegexOptions.Singleline);
 var list = new List<PointsPatternsEngine.PointPatternSpec>();
 foreach (Match it in items)
 {
 var nameM = Regex.Match(it.Value, "\\\"Name\\\"\\s*:\\s*\\\"([\\s\\S]*?)\\\"", RegexOptions.Singleline);
 var restrM = Regex.Match(it.Value, "\\\"Restriction\\\"\\s*:\\s*(\\d+)");
 var magM = Regex.Match(it.Value, "\\\"Magnitude\\\"\\s*:\\s*([-+]?[0-9]*\\.?[0-9]+)");
 if (!nameM.Success || !restrM.Success) continue;
 double mag = magM.Success ? double.Parse(magM.Groups[1].Value, System.Globalization.CultureInfo.InvariantCulture) :0.0;
 list.Add(new PointsPatternsEngine.PointPatternSpec { Name = nameM.Groups[1].Value.Trim(), Restriction = int.Parse(restrM.Groups[1].Value), Magnitude = mag });
 }
 spec.Items = list.ToArray();
 return spec;
 }

 private GeometryOrder LoadGeometryOrderFromCassette()
 {
 var geomPath = Path.Combine(_cassetteDir, "geometry.json"); var defPath = Path.Combine(_cassetteDir, "valores_defecto.json"); if (!File.Exists(geomPath)) return null; var jsonGeom = File.ReadAllText(geomPath);
 var order = new GeometryOrder();
 var contoursBlock = Regex.Match(jsonGeom, "\\\"Contours\\\"\\s*:\\s*\\[(.*?)\\]", RegexOptions.Singleline);
 if (contoursBlock.Success)
 {
 var items = Regex.Matches(contoursBlock.Groups[1].Value, "\\{[\\s\\S]*?\\}", RegexOptions.Singleline);
 foreach (Match item in items)
 {
 var labelM = Regex.Match(item.Value, "\\\"Label\\\"\\s*:\\s*\\\"([\\s\\S]*?)\\\"", RegexOptions.Singleline);
 var label = labelM.Success ? labelM.Groups[1].Value.Trim() : null;
 var pointsBlock = Regex.Match(item.Value, "\\\"Points\\\"\\s*:\\s*\\[(.*?)\\]", RegexOptions.Singleline);
 var pts = new List<Point3D>();
 if (pointsBlock.Success)
 {
 var ptItems = Regex.Matches(pointsBlock.Groups[1].Value, "\\{[\\s\\S]*?\\}", RegexOptions.Singleline);
 foreach (Match pm in ptItems)
 {
 double x = ParseDouble(pm.Value, "x"); double y = ParseDouble(pm.Value, "y"); double z = ParseDouble(pm.Value, "z");
 pts.Add(new Point3D(x,y,z));
 }
 }
 if (label != null) order.Contours.Add(new LabeledContour { Label = label, Points = pts });
 }
 }
 if (File.Exists(defPath))
 {
 var jsonDef = File.ReadAllText(defPath);
 TryAddMesh(jsonDef, order.MeshByPrefix);
 }
 return order;
 }

 private static void TryAddMesh(string json, Dictionary<string,(int n1,int n2)> dict)
 {
 var meshBlock = Regex.Match(json, "\\\"Mesh\\\"\\s*:\\s*\\{([\\s\\S]*?)\\}", RegexOptions.Singleline);
 if (!meshBlock.Success) return;
 var items = Regex.Matches(meshBlock.Groups[1].Value, "\\\"([A-Za-z0-9_]+)\\\"\\s*:\\s*\\{([\\s\\S]*?)\\}", RegexOptions.Singleline);
 foreach (Match m in items)
 {
 var key = m.Groups[1].Value.Trim().ToUpperInvariant();
 var body = m.Groups[2].Value;
 var n1m = Regex.Match(body, "\\\"n1\\\"\\s*:\\s*(\\d+)");
 var n2m = Regex.Match(body, "\\\"n2\\\"\\s*:\\s*(\\d+)");
 if (n1m.Success && n2m.Success)
 {
 dict[key] = (int.Parse(n1m.Groups[1].Value), int.Parse(n2m.Groups[1].Value));
 }
 }
 }

 private static double ParseDouble(string json, string key)
 { var m = Regex.Match(json, "\\\""+Regex.Escape(key)+"\\\"\\s*:\\s*([-+]?[0-9]*\\.?[0-9]+)"); return m.Success ? double.Parse(m.Groups[1].Value, System.Globalization.CultureInfo.InvariantCulture) :0.0; }
 }

 public sealed class OrchestrationOptions
 { public string CassetteRoot { get; set; } public string CassetteName { get; set; } public string MaterialName { get; set; } public eMatType MaterialType { get; set; } = eMatType.Concrete; public double? Fc_kgfcm { get; set; } public double E_kN_m2 { get; set; } public double Nu { get; set; } =0.2; public double Alpha { get; set; } =0.0000055; public Dictionary<string,double> AreaThicknessByName { get; set; } }
}
