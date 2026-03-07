#if SAP2000_LEGACY
using System;
using PROYECTOS_CSI.Infrastructure.SAP2000;
using SAP2000v1;

namespace PROYECTOS_CSI.Core.Domain.Motores
{
 // Crea combinaciones de respuesta/carga (sanitizado, cassette-driven)
 public sealed class LoadCombinationEngine
 {
 private readonly SapModelFacade _facade;
 public LoadCombinationEngine(SapModelFacade facade)
 { _facade = facade ?? throw new ArgumentNullException(nameof(facade)); }

 public sealed class ComboMemberSpec { public string CaseName { get; set; } public double ScaleFactor { get; set; } }
 public sealed class ComboSpec { public string Name { get; set; } public int Type { get; set; } public ComboMemberSpec[] Members { get; set; } = Array.Empty<ComboMemberSpec>(); }
 public sealed class CombosSpec { public ComboSpec[] Items { get; set; } = Array.Empty<ComboSpec>(); }

 public void Apply(CombosSpec spec)
 {
 if (spec == null || spec.Items == null || spec.Items.Length ==0) return;
 var m = _facade.Raw;
 foreach (var cb in spec.Items)
 {
 if (cb == null || string.IsNullOrWhiteSpace(cb.Name)) continue;
 int rAdd = m.RespCombo.Add(cb.Name, cb.Type);
 _facade.Check(rAdd, $"RespCombo.Add({cb.Name})");
 foreach (var mem in cb.Members ?? Array.Empty<ComboMemberSpec>())
 {
 if (string.IsNullOrWhiteSpace(mem.CaseName)) continue;
 var nameType = eCNameType.LoadCase;
 int rSet = m.RespCombo.SetCaseList(cb.Name, ref nameType, mem.CaseName, mem.ScaleFactor);
 _facade.Check(rSet, $"RespCombo.SetCaseList({cb.Name})");
 }
 PROYECTOS_CSI.Program.Log("Combo '{0}' aplicado: type={1}, nMembers={2}", cb.Name, cb.Type, (cb.Members?.Length ??0));
 }
 }
 }
}
#endif
