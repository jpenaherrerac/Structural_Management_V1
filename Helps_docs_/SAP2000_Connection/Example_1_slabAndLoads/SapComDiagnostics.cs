using System;
using System.Collections.Generic;
using Microsoft.Win32;

namespace App.Infrastructure.Sap2000
{
 /// <summary>
 /// Registry-based diagnostics for SAP2000 COM registration.
 /// Helps determine which ProgIDs are installed on the machine.
 /// </summary>
 public static class SapComDiagnostics
 {
 public sealed class ProgIdInfo
 {
 public string ProgId { get; set; }
 public bool Exists { get; set; }
 public string Clsid { get; set; }
 public string LocalServer32 { get; set; }
 public string InprocServer32 { get; set; }

 public override string ToString()
 {
 return $"{ProgId}: Exists={Exists}, CLSID={Clsid}, LocalServer32={LocalServer32}, InprocServer32={InprocServer32}";
 }
 }

 public static IReadOnlyList<ProgIdInfo> ProbeKnownSapProgIds()
 {
 var progIds = new[]
 {
 "CSI.SAP2000.API.SapObject",
 "CSI.SAP2000.SapObject",
 "Sap2000v1.SapObject",
 "SAP2000v1.SapObject",

 "CSI.SAP2000.API.Helper",
 "Sap2000v1.Helper",
 "SAP2000v1.Helper",

 // Some installs register older aliases
 "CSI.SAP2000.API.cHelper",
 "Sap2000v1.cHelper",
 };

 var list = new List<ProgIdInfo>();
 foreach (var pid in progIds)
 list.Add(ReadProgId(pid));
 return list;
 }

 private static ProgIdInfo ReadProgId(string progId)
 {
 var info = new ProgIdInfo { ProgId = progId };
 try
 {
 using (var k = Registry.ClassesRoot.OpenSubKey(progId + "\\CLSID"))
 {
 var clsid = k?.GetValue(null) as string;
 info.Clsid = clsid;
 info.Exists = !string.IsNullOrWhiteSpace(clsid);
 }

 if (!string.IsNullOrWhiteSpace(info.Clsid))
 {
 using (var k2 = Registry.ClassesRoot.OpenSubKey("CLSID\\" + info.Clsid + "\\LocalServer32"))
 info.LocalServer32 = k2?.GetValue(null) as string;

 using (var k3 = Registry.ClassesRoot.OpenSubKey("CLSID\\" + info.Clsid + "\\InprocServer32"))
 info.InprocServer32 = k3?.GetValue(null) as string;
 }
 }
 catch
 {
 // ignore
 }
 return info;
 }
 }
}
