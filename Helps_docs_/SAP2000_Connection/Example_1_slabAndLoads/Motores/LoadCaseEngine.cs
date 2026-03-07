
#if SAP2000_LEGACY
using System;
using System.Collections.Generic;
using PROYECTOS_CSI.Infrastructure.SAP2000;

namespace PROYECTOS_CSI.Core.Domain.Motores
{
 // Define casos de carga (sanitizado, cassette-driven) – Static Linear minimal support
 public sealed class LoadCaseEngine
 {
 private readonly SapModelFacade _facade;
 public LoadCaseEngine(SapModelFacade facade)
 { _facade = facade ?? throw new ArgumentNullException(nameof(facade)); }

 public sealed class CaseLoadSpec { public string PatternName { get; set; } public double Scale { get; set; } }
 public sealed class CaseSpec { public string Name { get; set; } public string Type { get; set; } public CaseLoadSpec[] Loads { get; set; } = Array.Empty<CaseLoadSpec>(); }
 public sealed class CasesSpec { public CaseSpec[] Items { get; set; } = Array.Empty<CaseSpec>(); }

 public void Apply(CasesSpec spec)
 {
 if (spec == null || spec.Items == null || spec.Items.Length ==0) return;
 var m = _facade.Raw;
 foreach (var c in spec.Items)
 {
 if (c == null || string.IsNullOrWhiteSpace(c.Name)) continue;
 // Create Static Linear case
 int rCreate = m.LoadCases.StaticLinear.SetCase(c.Name);
 _facade.Check(rCreate, $"LoadCases.StaticLinear.SetCase({c.Name})");
 // Assign pattern loads (SAP2000 expects arrays of names, type strings, and scale factors)
 int nLoads = c.Loads?.Length ??0;
 var names = new string[nLoads]; var typeNames = new string[nLoads]; var scales = new double[nLoads];
 for (int i=0;i<nLoads;i++)
 {
 names[i] = c.Loads[i].PatternName;
 scales[i] = c.Loads[i].Scale;
 typeNames[i] = "LoadPat"; // pattern load assignment
 }
 int rSet = m.LoadCases.StaticLinear.SetLoads(c.Name, nLoads, ref names, ref typeNames, ref scales);
 _facade.Check(rSet, $"LoadCases.StaticLinear.SetLoads({c.Name})");
 PROYECTOS_CSI.Program.Log("Case '{0}' aplicado: type={1}, nLoads={2}", c.Name, c.Type, nLoads);
 }
 }
 }
}
#endif
