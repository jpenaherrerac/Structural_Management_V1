#if SAP2000_AVAILABLE
using System;
using System.Collections.Generic;
using System.Linq;
using App.Domain.MeshDomain;
using App.Domain.MeshDomain.LocalMesh;
using SAP2000v1;

namespace App.Infrastructure.Sap2000.Motores
{
    /// <summary>
    /// Creates load patterns and assigns point loads to plate centroid special points.
    /// </summary>
    public sealed class PlateCentroidLoadsEngine
    {
        private readonly SapModelFacade _facade;

        public PlateCentroidLoadsEngine(SapModelFacade facade)
        {
            _facade = facade ?? throw new ArgumentNullException(nameof(facade));
        }

        /// <summary>
        /// Simplified load data for SAP export (avoids cross-layer dependency on App.Application).
        /// </summary>
        public sealed class LoadExportData
        {
            public List<LoadPatternInfo> LoadPatterns { get; set; } = new List<LoadPatternInfo>();
            public List<ZhInfo> Zhs { get; set; } = new List<ZhInfo>();
            public List<LoadAssignmentInfo> LoadAssignments { get; set; } = new List<LoadAssignmentInfo>();
        }

        public sealed class LoadPatternInfo
        {
            public Guid LoadPatternId { get; set; }
            public string Name { get; set; }
            public bool IsActive { get; set; }
        }

        public sealed class ZhInfo
        {
            public Guid ZhId { get; set; }
            public string Name { get; set; }
        }

        public sealed class LoadAssignmentInfo
        {
            public Guid LoadPatternId { get; set; }
            public Guid ZhId { get; set; }
            public string Component { get; set; } // "Fx", "Fy", "Fz", "Mx", "My", "Mz"
            public double Value { get; set; }
        }

        /// <summary>
        /// Creates load patterns in SAP2000 from project LoadPatterns.
        /// Uses type 8 (Other) for all patterns.
        /// </summary>
        public Dictionary<Guid, string> CreateLoadPatterns(LoadExportData loadData)
        {
            var map = new Dictionary<Guid, string>();
            if (loadData?.LoadPatterns == null) return map;

            foreach (var lp in loadData.LoadPatterns)
            {
                if (lp == null) continue;
                if (lp.LoadPatternId == Guid.Empty) continue;
                if (string.IsNullOrWhiteSpace(lp.Name)) continue;
                if (!lp.IsActive) continue;

                try
                {
                    // Type 8 = eLoadPatternType.Other
                    _facade.LoadPatterns_Add(lp.Name, (eLoadPatternType)8, 0.0, true);
                    map[lp.LoadPatternId] = lp.Name;
                }
                catch
                {
                    // Pattern may already exist; continue
                }
            }

            return map;
        }

        /// <summary>
        /// Assigns point loads to plate centroid points based on LoadAssignments.
        /// </summary>
        public int AssignLoadsToPlatecentroids(
            LoadExportData loadData,
            BranchMeshSet branch,
            Dictionary<Guid, string> loadPatternMap,
            Dictionary<Guid, string> plateCentroidPointNames,
            List<string> warnings)
        {
            if (loadData == null) return 0;
            if (branch == null) return 0;
            if (loadPatternMap == null || loadPatternMap.Count == 0) return 0;
            if (plateCentroidPointNames == null || plateCentroidPointNames.Count == 0) return 0;
            warnings = warnings ?? new List<string>();

            // Build ZhId -> ZhName lookup
            var zhNameById = new Dictionary<Guid, string>();
            if (loadData.Zhs != null)
            {
                foreach (var zh in loadData.Zhs)
                {
                    if (zh == null || zh.ZhId == Guid.Empty) continue;
                    if (string.IsNullOrWhiteSpace(zh.Name)) continue;
                    zhNameById[zh.ZhId] = zh.Name.Trim();
                }
            }

            // Build PlateId -> (ZhLayer, ZhNumber) lookup from ReconstructedEntities
            var plateInfo = new Dictionary<Guid, (string ZhLayer, int ZhNumber)>();
            if (branch.ReconstructedEntities != null)
            {
                foreach (var snap in branch.ReconstructedEntities)
                {
                    if (snap == null) continue;
                    if (snap.EntityId == Guid.Empty) continue;
                    if (snap.EntityType != ReconstructedEntityType.Plate) continue;

                    var zhLayer = (snap.ZhLayer ?? string.Empty).Trim();
                    var zhNumber = snap.ZhNumber;
                    if (string.IsNullOrWhiteSpace(zhLayer) || !zhNumber.HasValue) continue;

                    plateInfo[snap.EntityId] = (zhLayer, zhNumber.Value);
                }
            }

            // Build ZhLayer -> List of (PlateId, ZhNumber) for matching
            var platesByZhLayer = new Dictionary<string, List<(Guid PlateId, int ZhNumber)>>(StringComparer.OrdinalIgnoreCase);
            foreach (var kv in plateInfo)
            {
                var layer = kv.Value.ZhLayer;
                if (!platesByZhLayer.TryGetValue(layer, out var list))
                {
                    list = new List<(Guid, int)>();
                    platesByZhLayer[layer] = list;
                }
                list.Add((kv.Key, kv.Value.ZhNumber));
            }

            int assignedCount = 0;

            // Group assignments by (LoadPatternId, ZhId) to aggregate components
            var assignmentsByKey = new Dictionary<(Guid LoadPatternId, Guid ZhId), List<LoadAssignmentInfo>>();
            if (loadData.LoadAssignments != null)
            {
                foreach (var a in loadData.LoadAssignments)
                {
                    if (a == null) continue;
                    var key = (a.LoadPatternId, a.ZhId);
                    if (!assignmentsByKey.TryGetValue(key, out var list))
                    {
                        list = new List<LoadAssignmentInfo>();
                        assignmentsByKey[key] = list;
                    }
                    list.Add(a);
                }
            }

            foreach (var kv in assignmentsByKey)
            {
                var loadPatternId = kv.Key.LoadPatternId;
                var zhId = kv.Key.ZhId;
                var components = kv.Value;

                // Get SAP pattern name
                if (!loadPatternMap.TryGetValue(loadPatternId, out var sapPatternName))
                    continue;

                // Get ZH name
                if (!zhNameById.TryGetValue(zhId, out var zhName))
                {
                    warnings.Add($"LoadAssignment references unknown ZhId {zhId}.");
                    continue;
                }

                // Find plates matching this ZhLayer
                if (!platesByZhLayer.TryGetValue(zhName, out var matchingPlates) || matchingPlates.Count == 0)
                {
                    warnings.Add($"No plates found for ZhLayer '{zhName}'.");
                    continue;
                }

                // Build load values array [F1,F2,F3,M1,M2,M3] = [Fx,Fy,Fz,Mx,My,Mz]
                // NOTE: Input values are in kN and kN·m, SAP2000 model is in N and N·m
                // Multiply by 1000 to convert kN -> N and kN·m -> N·m
                var values = new double[6];
                foreach (var comp in components)
                {
                    int idx = ComponentToIndex(comp.Component);
                    if (idx >= 0 && idx < 6)
                        values[idx] = comp.Value * 1000.0;  // Convert kN to N, kN·m to N·m
                }

                // Check if all zeros -> skip
                bool hasLoad = false;
                for (int i = 0; i < 6; i++)
                    if (Math.Abs(values[i]) > 1e-12) { hasLoad = true; break; }
                if (!hasLoad) continue;

                // Apply to all plates in this ZhLayer
                foreach (var (plateId, zhNumber) in matchingPlates)
                {
                    if (!plateCentroidPointNames.TryGetValue(plateId, out var sapPointName))
                    {
                        warnings.Add($"Plate {plateId:N} has no centroid point in SAP2000.");
                        continue;
                    }

                    try
                    {
                        var ret = _facade.Raw.PointObj.SetLoadForce(
                            sapPointName,
                            sapPatternName,
                            ref values,
                            false,
                            "Global",
                            (eItemType)0);  // eItemType.Object = 0

                        if (ret == 0)
                            assignedCount++;
                        else
                            warnings.Add($"SetLoadForce failed for point '{sapPointName}' pattern '{sapPatternName}': ret={ret}");
                    }
                    catch (Exception ex)
                    {
                        warnings.Add($"SetLoadForce exception for '{sapPointName}': {ex.Message}");
                    }
                }
            }

            return assignedCount;
        }

        private static int ComponentToIndex(string component)
        {
            if (string.IsNullOrWhiteSpace(component)) return -1;
            switch (component.Trim().ToUpperInvariant())
            {
                case "FX": return 0;
                case "FY": return 1;
                case "FZ": return 2;
                case "MX": return 3;
                case "MY": return 4;
                case "MZ": return 5;
                default: return -1;
            }
        }
    }
}
#else
using System;
using System.Collections.Generic;

namespace App.Infrastructure.Sap2000.Motores
{
    /// <summary>
    /// Stub for PlateCentroidLoadsEngine when SAP2000 is not available.
    /// </summary>
    public sealed class PlateCentroidLoadsEngine
    {
        public PlateCentroidLoadsEngine(SapModelFacade facade) { }

      public sealed class LoadExportData
        {
    public List<LoadPatternInfo> LoadPatterns { get; set; } = new List<LoadPatternInfo>();
  public List<ZhInfo> Zhs { get; set; } = new List<ZhInfo>();
     public List<LoadAssignmentInfo> LoadAssignments { get; set; } = new List<LoadAssignmentInfo>();
        }

        public sealed class LoadPatternInfo
        {
            public Guid LoadPatternId { get; set; }
     public string Name { get; set; }
   public bool IsActive { get; set; }
        }

        public sealed class ZhInfo
        {
      public Guid ZhId { get; set; }
public string Name { get; set; }
  }

        public sealed class LoadAssignmentInfo
        {
 public Guid LoadPatternId { get; set; }
            public Guid ZhId { get; set; }
          public string Component { get; set; }
     public double Value { get; set; }
}

      public Dictionary<Guid, string> CreateLoadPatterns(LoadExportData loadData)
  => new Dictionary<Guid, string>();

        public int AssignLoadsToPlatecentroids(
      LoadExportData loadData,
      App.Domain.MeshDomain.BranchMeshSet branch,
     Dictionary<Guid, string> loadPatternMap,
            Dictionary<Guid, string> plateCentroidPointNames,
      List<string> warnings)
=> 0;
    }
}
#endif
