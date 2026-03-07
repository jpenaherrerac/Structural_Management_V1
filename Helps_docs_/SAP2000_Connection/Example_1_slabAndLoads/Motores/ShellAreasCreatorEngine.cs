#if SAP2000_AVAILABLE
using System;
using System.Collections.Generic;
using App.Domain.MeshDomain;
using App.Domain.MeshDomain.LocalMesh;

namespace App.Infrastructure.Sap2000.Motores
{
    /// <summary>
    /// Creates SAP2000 shell areas from Branch effective cells.
    /// 
    /// Points are named deterministically:
    /// - If corner has AnchorName (from AnchorPointTag) ? use AnchorName (e.g., "A_ZH01_1_0_0")
    /// - If corner has NodeId (from base cell) ? use "N{NodeId}" (e.g., "N123")
    /// - Otherwise ? use "P_{coordHash}" as fallback
    /// </summary>
    public sealed class ShellAreasCreatorEngine
    {
        private readonly SapModelFacade _facade;

        public ShellAreasCreatorEngine(SapModelFacade facade)
        {
            _facade = facade ?? throw new ArgumentNullException(nameof(facade));
        }

        /// <summary>
        /// Result of creating effective cells in SAP2000.
        /// Contains maps for areas and points created.
        /// </summary>
        public sealed class CreateEffectiveCellsResult
        {
            /// <summary>Map EffectiveCellId ? SAP area name.</summary>
            public Dictionary<int, string> AreaMap { get; } = new Dictionary<int, string>();

            /// <summary>Map coord key ? SAP point name (for all points created).</summary>
            public Dictionary<long, string> PointNameByCoordKey { get; } = new Dictionary<long, string>();

            /// <summary>Map AnchorName ? SAP point name (for anchor points only).</summary>
            public Dictionary<string, string> PointNameByAnchorName { get; } = new Dictionary<string, string>();

            /// <summary>Map NodeId ? SAP point name (for base cell corner points only).</summary>
            public Dictionary<int, string> PointNameByNodeId { get; } = new Dictionary<int, string>();

            /// <summary>Map PlateId ? list of anchor point SAP names in that plate.</summary>
            public Dictionary<Guid, List<string>> AnchorPointsByPlateId { get; } = new Dictionary<Guid, List<string>>();

            /// <summary>List of SAP point names that are on Boundary segments (for restraints).</summary>
            public List<string> BoundaryPointNames { get; } = new List<string>();
        }

        /// <summary>
        /// Creates SAP2000 shell areas from the Branch effective quads (base cells + per-plate fine cells).
        /// 
        /// Points are named:
        /// - AnchorName if available (anchor corners)
        /// - "N{NodeId}" if available (base cell corners)
        /// - "P_{hash}" as fallback
        /// 
        /// Areas are named "E{EffectiveCellId}".
        /// </summary>
        public CreateEffectiveCellsResult CreateFromEffectiveCells(
            BranchMeshSet branch,
            string propertyLosa,
            string propertyPlate,
            string propertyTrench,
            SapBranchExecuteResult result,
            double z = 0.0,
            double coordTol = 1e-6)
        {
            var res = new CreateEffectiveCellsResult();

            if (branch == null)
            {
                result.Warnings.Add("Branch is null. Skipping effective area creation.");
                return res;
            }

            BranchMeshSet.EffectiveCellsSnapshot snap;
            try { snap = branch.GetEffectiveCellsForExport(coordTol); }
            catch (Exception ex)
            {
                result.Warnings.Add("EffectiveCellsForExport failed: " + ex.Message);
                return res;
            }

            var cells = snap?.Cells;
            if (cells == null || cells.Count == 0)
            {
                result.Warnings.Add("Branch has no EffectiveCells. Skipping area creation.");
                return res;
            }

            // Get boundary coordinate keys for detecting boundary points
            HashSet<long> boundaryCoordKeys;
            try { boundaryCoordKeys = branch.GetBoundaryCoordKeys(coordTol); }
            catch { boundaryCoordKeys = new HashSet<long>(); }

            // Groups
            try { _facade.GroupDef_SetGroup("LOSA"); } catch { }
            try { _facade.GroupDef_SetGroup("PLATES"); } catch { }
            try { _facade.GroupDef_SetGroup("TRENCHES"); } catch { }

            // Build PlateId -> ZhLayer lookup and create ZH groups dynamically
            var plateZhLayer = new Dictionary<Guid, string>();
            var zhLayerGroups = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            if (branch.ReconstructedEntities != null)
            {
                foreach (var snapEnt in branch.ReconstructedEntities)
                {
                    if (snapEnt == null) continue;
                    if (snapEnt.EntityType != ReconstructedEntityType.Plate) continue;

                    var layer = snapEnt.ZhLayer?.Trim();
                    if (string.IsNullOrWhiteSpace(layer)) continue;

                    plateZhLayer[snapEnt.EntityId] = layer;
                    zhLayerGroups.Add(layer);
                }
            }

            // Create ZH groups (e.g., "ZH1", "ZH2", etc.)
            foreach (var zhLayer in zhLayerGroups)
            {
                try { _facade.GroupDef_SetGroup(zhLayer); } catch { }
            }

            // Precompute trench bounds (strict: ReconstructedBounds only)
            var trenchBounds = new List<(double Xmin, double Xmax, double Ymin, double Ymax)>();
            if (branch.ReconstructedEntities != null)
            {
                for (int i = 0; i < branch.ReconstructedEntities.Count; i++)
                {
                    var snapEnt = branch.ReconstructedEntities[i];
                    if (snapEnt == null) continue;
                    var ent = snapEnt.ToReconstructedEntity();
                    if (ent == null) continue;
                    if (ent.EntityType != ReconstructedEntityType.Trench) continue;
                    if (!ent.ReconstructedBounds.HasValue) continue;

                    var b = ent.ReconstructedBounds.Value;
                    trenchBounds.Add((b.Xmin, b.Xmax, b.Ymin, b.Ymax));
                }
            }

            bool inAnyTrench(double cx, double cy)
            {
                const double tol = 1e-6;
                for (int i = 0; i < trenchBounds.Count; i++)
                {
                    var b = trenchBounds[i];
                    if (cx >= b.Xmin - tol && cx <= b.Xmax + tol && cy >= b.Ymin - tol && cy <= b.Ymax + tol)
                        return true;
                }
                return false;
            }

            long coordKey(double x, double y)
            {
                long ix = (long)Math.Round(x / coordTol);
                long iy = (long)Math.Round(y / coordTol);
                return ix * 1_000_000_000L + iy;
            }

            // Helper to get or create a point with appropriate naming
            string getOrCreatePoint(double x, double y, int? nodeId, string anchorName, Guid plateId)
            {
                var k = coordKey(x, y);

                // Already created?
                if (res.PointNameByCoordKey.TryGetValue(k, out var existing) && !string.IsNullOrWhiteSpace(existing))
                    return existing;

                // Determine userName for this point
                string userName;
                if (!string.IsNullOrEmpty(anchorName))
                {
                    // Anchor point: use AnchorName directly
                    userName = anchorName;
                }
                else if (nodeId.HasValue && nodeId.Value > 0)
                {
                    // Base cell corner with NodeId
                    userName = "N" + nodeId.Value;
                }
                else
                {
                    // Fallback: coordinate hash
                    userName = "P_" + k.ToString();
                }

                // Create point in SAP
                var sapName = _facade.PointObj_AddCartesian(x, y, z, userName);
                _facade.PointObj_SetSpecialPoint(sapName, true);

                // Store in maps
                res.PointNameByCoordKey[k] = sapName;

                if (!string.IsNullOrEmpty(anchorName))
                {
                    res.PointNameByAnchorName[anchorName] = sapName;

                    // Track anchor points by plate
                    if (plateId != Guid.Empty)
                    {
                        if (!res.AnchorPointsByPlateId.TryGetValue(plateId, out var list))
                        {
                            list = new List<string>();
                            res.AnchorPointsByPlateId[plateId] = list;
                        }
                        if (!list.Contains(sapName))
                            list.Add(sapName);
                    }
                }

                if (nodeId.HasValue && nodeId.Value > 0)
                {
                    res.PointNameByNodeId[nodeId.Value] = sapName;
                }

                // Check if this point is on a Boundary segment
                if (boundaryCoordKeys.Contains(k))
                {
                    if (!res.BoundaryPointNames.Contains(sapName))
                        res.BoundaryPointNames.Add(sapName);
                }

                return sapName;
            }

            foreach (var c in cells)
            {
                if (c == null) continue;

                // Determine property/group
                string propName;
                string groupName;

                if (c.PlateId != Guid.Empty)
                {
                    propName = propertyPlate;
                    groupName = "PLATES";
                }
                else
                {
                    var cx = (c.X0 + c.X1) / 2.0;
                    var cy = (c.Y0 + c.Y1) / 2.0;

                    if (trenchBounds.Count > 0 && inAnyTrench(cx, cy))
                    {
                        propName = propertyTrench;
                        groupName = "TRENCHES";
                    }
                    else
                    {
                        propName = propertyLosa;
                        groupName = "LOSA";
                    }
                }

                // Create/get points for each corner
                var p0 = getOrCreatePoint(c.X0, c.Y0, c.N0, c.AnchorName0, c.PlateId);
                var p1 = getOrCreatePoint(c.X1, c.Y0, c.N1, c.AnchorName1, c.PlateId);
                var p2 = getOrCreatePoint(c.X1, c.Y1, c.N2, c.AnchorName2, c.PlateId);
                var p3 = getOrCreatePoint(c.X0, c.Y1, c.N3, c.AnchorName3, c.PlateId);

                var userName = "E" + c.EffectiveCellId;

                try
                {
                    var points = new[] { p0, p1, p2, p3 };
                    var sapName = _facade.AreaObj_AddByPoint(4, points, propName, userName);
                    res.AreaMap[c.EffectiveCellId] = sapName;
                    try { _facade.AreaObj_SetGroupAssign(sapName, groupName); } catch { }

                    // Assign to ZH-specific group if this is a plate area
                    if (c.PlateId != Guid.Empty && plateZhLayer.TryGetValue(c.PlateId, out var zhLayer))
                    {
                        try { _facade.AreaObj_SetGroupAssign(sapName, zhLayer); } catch { }
                    }
                }
                catch (Exception ex)
                {
                    result.Warnings.Add($"Failed to create effective area E{c.EffectiveCellId}: {ex.Message}");
                }
            }

            result.AreasCreated = res.AreaMap.Count;
            return res;
        }
    }
}
#else
using System;
using System.Collections.Generic;
using App.Domain.MeshDomain;

namespace App.Infrastructure.Sap2000.Motores
{
    public sealed class ShellAreasCreatorEngine
    {
      public ShellAreasCreatorEngine(SapModelFacade facade) { }

    public sealed class CreateEffectiveCellsResult
        {
 public Dictionary<int, string> AreaMap { get; } = new Dictionary<int, string>();
     public Dictionary<long, string> PointNameByCoordKey { get; } = new Dictionary<long, string>();
        public Dictionary<string, string> PointNameByAnchorName { get; } = new Dictionary<string, string>();
     public Dictionary<int, string> PointNameByNodeId { get; } = new Dictionary<int, string>();
  public Dictionary<Guid, List<string>> AnchorPointsByPlateId { get; } = new Dictionary<Guid, List<string>>();
            public List<string> BoundaryPointNames { get; } = new List<string>();
        }

    public CreateEffectiveCellsResult CreateFromEffectiveCells(
    BranchMeshSet branch,
    string propertyLosa,
    string propertyPlate,
            string propertyTrench,
     SapBranchExecuteResult result,
            double z = 0.0,
     double coordTol = 1e-6)
            => throw new System.NotSupportedException("SAP2000 not available.");
  }
}
#endif
