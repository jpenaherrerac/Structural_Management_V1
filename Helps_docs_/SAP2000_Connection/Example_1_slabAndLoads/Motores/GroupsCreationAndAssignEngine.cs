#if SAP2000_LEGACY
using System;
using PROYECTOS_CSI.Infrastructure.SAP2000;

namespace PROYECTOS_CSI.Core.Domain.Motores
{
 // Crea y asigna grupos en base a especificación del cassette (sin defaults internos)
 public sealed class GroupsCreationAndAssignEngine
 {
 private readonly SapModelFacade _facade;
 public GroupsCreationAndAssignEngine(SapModelFacade facade)
 { _facade = facade ?? throw new ArgumentNullException(nameof(facade)); }

 // DTOs sanitizados: todo proviene del cassette
 public sealed class GroupSpec
 {
 public string Name { get; set; }
 public string[] AssignAreaProperties { get; set; } = Array.Empty<string>();
 }
 public sealed class GroupsSpec
 {
 public GroupSpec[] Items { get; set; } = Array.Empty<GroupSpec>();
 }

 // Aplica: crea grupos y asigna áreas por propiedad según cassette
 public void Apply(GroupsSpec spec)
 {
 if (spec == null || spec.Items == null || spec.Items.Length ==0) return;
 var m = _facade.Raw;
 foreach (var g in spec.Items)
 {
 if (g == null || string.IsNullOrWhiteSpace(g.Name)) continue;
 // Crear grupo
 int r1 = m.GroupDef.SetGroup(g.Name, -1, true, false, false, false, false, false, false, false, false, false, false);
 try { _facade.Check(r1, $"GroupDef.SetGroup({g.Name})"); } catch { }
 // Seleccionar áreas por propiedades indicadas y asignar al grupo
 if (g.AssignAreaProperties != null && g.AssignAreaProperties.Length >0)
 {
 foreach (var prop in g.AssignAreaProperties)
 {
 if (string.IsNullOrWhiteSpace(prop)) continue;
 try { m.SelectObj.PropertyArea(prop, false); } catch { }
 }
 try { m.AreaObj.SetGroupAssign("", g.Name, false, SAP2000v1.eItemType.SelectedObjects); } catch { }
 try { m.SelectObj.ClearSelection(); } catch { }
 }
 PROYECTOS_CSI.Program.Log("Grupo '{0}' aplicado. Props={1}", g.Name, string.Join(",", g.AssignAreaProperties ?? Array.Empty<string>()));
 }
 }
 }
}
#endif
