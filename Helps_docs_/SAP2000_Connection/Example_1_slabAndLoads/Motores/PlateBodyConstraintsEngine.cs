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
    /// Creates SAP2000 groups and assigns Body constraints per plate.
    /// Group contents: plate centroid point (Z=0.10) + anchor points in plate (Z=0).
    /// Constraint name: "{ZhLayer}_{ZhNumber}_Body".
    /// Group name: "{ZhLayer}_{ZhNumber}_Body".
    /// </summary>
    public sealed class PlateBodyConstraintsEngine
    {
        private readonly SapModelFacade _facade;

        public PlateBodyConstraintsEngine(SapModelFacade facade)
        {
            _facade = facade ?? throw new ArgumentNullException(nameof(facade));
        }

        /// <summary>
        /// Creates Body constraints per plate using anchor points (from EffectiveCells export).
        /// This is the preferred method that uses AnchorPointsByPlateId directly.
        /// </summary>
        /// <param name="branch">Branch with ReconstructedEntities (for ZhLayer/ZhNumber).</param>
        /// <param name="plateCentroidPointNamesByPlateId">Map PlateId ? centroid point SAP name.</param>
        /// <param name="anchorPointsByPlateId">Map PlateId ? list of anchor point SAP names.</param>
        /// <param name="warnings">List to collect warnings.</param>
        public void ExecuteWithAnchorPoints(
            BranchMeshSet branch,
            Dictionary<Guid, string> plateCentroidPointNamesByPlateId,
            Dictionary<Guid, List<string>> anchorPointsByPlateId,
            List<string> warnings)
        {
            if (branch == null) throw new ArgumentNullException(nameof(branch));
            plateCentroidPointNamesByPlateId = plateCentroidPointNamesByPlateId ?? new Dictionary<Guid, string>();
            anchorPointsByPlateId = anchorPointsByPlateId ?? new Dictionary<Guid, List<string>>();
            warnings = warnings ?? new List<string>();

            var recPlates = (branch.ReconstructedEntities ?? new List<ReconstructedEntitySnapshot>())
                .Where(e => e != null && e.EntityId != Guid.Empty && e.EntityType == ReconstructedEntityType.Plate)
                .ToList();

            if (recPlates.Count == 0)
            {
                warnings.Add("No reconstructed plates found; skipping Body constraints.");
                return;
            }

            foreach (var p in recPlates)
            {
                var zhLayer = (p.ZhLayer ?? string.Empty).Trim();
                if (string.IsNullOrWhiteSpace(zhLayer) || !p.ZhNumber.HasValue)
                {
                    warnings.Add($"Plate {p.EntityId:N} missing ZhLayer/ZhNumber; skipping Body constraint.");
                    continue;
                }

                var plateId = p.EntityId;
                var baseName = zhLayer + "_" + p.ZhNumber.Value + "_Body";
                var groupName = baseName;
                var constraintName = baseName;

                // Get centroid point
                if (!plateCentroidPointNamesByPlateId.TryGetValue(plateId, out var centroidSapName) || string.IsNullOrWhiteSpace(centroidSapName))
                {
                    warnings.Add($"Plate {plateId:N} ({zhLayer}_{p.ZhNumber}) centroid point not found; skipping Body constraint.");
                    continue;
                }

                // Get anchor points for this plate
                List<string> anchorPoints = null;
                anchorPointsByPlateId?.TryGetValue(plateId, out anchorPoints);

                // Collect all points for the Body group
                var sapPointNames = new List<string> { centroidSapName };
                if (anchorPoints != null && anchorPoints.Count > 0)
                {
                    sapPointNames.AddRange(anchorPoints);
                }
                sapPointNames = sapPointNames.Distinct(StringComparer.OrdinalIgnoreCase).ToList();

                if (sapPointNames.Count < 2)
                {
                    warnings.Add($"Plate {plateId:N} ({zhLayer}_{p.ZhNumber}) has only centroid, no anchor points; Body constraint may not be useful.");
                }

                try
                {
                    // Define Body constraint (all DOF constrained together)
                    var dof = new bool[6];
                    for (int i =0; i <6; i++) dof[i] = true;
                    _facade.ConstraintDef_SetBody(constraintName, ref dof, "Global");

                    // Create group
                    _facade.GroupDef_SetGroup(groupName);

                    // Selection is global state in SAP: ensure it is always cleared.
                    _facade.SelectObj_ClearSelection();
                    try
                    {
                        // Select all points
                        foreach (var pt in sapPointNames)
                        {
                            try
                            {
                                _facade.PointObj_SetSelected(pt, true, (eItemType)0);
                            }
                            catch
                            {
                                warnings.Add($"Could not select point '{pt}' for Body group '{groupName}'.");
                            }
                        }

                        // Assign to group
                        _facade.PointObj_SetGroupAssign(string.Empty, groupName, remove: false, itemType: eItemType.SelectedObjects);
                    }
                    finally
                    {
                        // Prevent leaking selection into downstream engines
                        try { _facade.SelectObj_ClearSelection(); } catch { }
                    }

                    // Assign Body constraint to all points in group
                    _facade.PointObj_SetConstraint(groupName, constraintName, eItemType.Group, replace: true);

                    warnings.Add($"[INFO] Body constraint '{constraintName}' created with {sapPointNames.Count} points (1 centroid + {sapPointNames.Count -1} anchors).");
                }
                catch (Exception ex)
                {
                    warnings.Add($"Failed to create Body constraint '{constraintName}' for plate {plateId:N}: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// Legacy method using nodeMap. Kept for backward compatibility.
        /// </summary>
        [Obsolete("Use ExecuteWithAnchorPoints instead")]
        public void Execute(
            BranchMeshSet branch,
            Dictionary<int, string> nodeMap,
            Dictionary<Guid, string> plateCentroidPointNamesByPlateId,
            List<string> warnings)
        {
            // Forward to the new method if no nodeMap provided
            if (nodeMap == null || nodeMap.Count == 0)
            {
                ExecuteWithAnchorPoints(branch, plateCentroidPointNamesByPlateId, null, warnings);
                return;
            }

            // Original legacy implementation...
            if (branch == null) throw new ArgumentNullException(nameof(branch));
            plateCentroidPointNamesByPlateId = plateCentroidPointNamesByPlateId ?? new Dictionary<Guid, string>();
            warnings = warnings ?? new List<string>();

            var recPlates = (branch.ReconstructedEntities ?? new List<ReconstructedEntitySnapshot>())
                .Where(e => e != null && e.EntityId != Guid.Empty && e.EntityType == ReconstructedEntityType.Plate)
                .ToList();

            if (recPlates.Count == 0)
            {
                warnings.Add("No reconstructed plates found; cannot assign Body constraints per plate.");
                return;
            }

            var nodeIdsByPlateId = BuildPlateNodeIdsFromCellCentroids(branch, recPlates, warnings);

            foreach (var p in recPlates)
            {
                var zhLayer = (p.ZhLayer ?? string.Empty).Trim();
                if (string.IsNullOrWhiteSpace(zhLayer) || !p.ZhNumber.HasValue)
                {
                    warnings.Add($"Plate {p.EntityId:N} missing ZhLayer/ZhNumber; skipping Body constraint.");
                    continue;
                }

                var plateId = p.EntityId;
                var baseName = zhLayer + "_" + p.ZhNumber.Value + "_Body";
                var groupName = baseName;
                var constraintName = baseName;

                if (!plateCentroidPointNamesByPlateId.TryGetValue(plateId, out var centroidSapName) || string.IsNullOrWhiteSpace(centroidSapName))
                {
                    warnings.Add($"Plate {plateId:N} centroid SAP point not found; skipping Body constraint.");
                    continue;
                }

                HashSet<int> plateNodeIds = null;
                nodeIdsByPlateId?.TryGetValue(plateId, out plateNodeIds);

                var dof = new bool[6];
                for (int i = 0; i < 6; i++) dof[i] = true;

                try
                {
                    _facade.ConstraintDef_SetBody(constraintName, ref dof, "Global");
                    _facade.PointObj_SetConstraint(centroidSapName, constraintName, (eItemType)0, replace: true);
                }
                catch (Exception ex)
                {
                    warnings.Add($"Failed to assign Body constraint '{constraintName}' to centroid: {ex.Message}");
                }

                try
                {
                    var sapPointNames = new List<string> { centroidSapName };

                    if (plateNodeIds != null)
                    {
                        foreach (var nodeId in plateNodeIds)
                        {
                            if (!nodeMap.TryGetValue(nodeId, out var sapNodeName) || string.IsNullOrWhiteSpace(sapNodeName))
                                continue;
                            sapPointNames.Add(sapNodeName);
                        }
                    }

                    sapPointNames = sapPointNames.Distinct(StringComparer.OrdinalIgnoreCase).ToList();

                    _facade.GroupDef_SetGroup(groupName);
                    _facade.SelectObj_ClearSelection();
                    foreach (var pt in sapPointNames)
                        _facade.PointObj_SetSelected(pt, true, (eItemType)0);

                    _facade.PointObj_SetGroupAssign(string.Empty, groupName, remove: false, itemType: eItemType.SelectedObjects);
                    _facade.PointObj_SetConstraint(groupName, constraintName, eItemType.Group, replace: true);
                    _facade.SelectObj_ClearSelection();
                }
                catch (Exception ex)
                {
                    warnings.Add($"Failed to create/assign Body group '{groupName}': {ex.Message}");
                }
            }
        }

        private static Dictionary<Guid, HashSet<int>> BuildPlateNodeIdsFromCellCentroids(
            BranchMeshSet branch,
            List<ReconstructedEntitySnapshot> plates,
            List<string> warnings)
        {
            var result = new Dictionary<Guid, HashSet<int>>();

            if (branch.Cells == null || branch.Cells.Count == 0)
            {
                warnings?.Add("Branch has no Cells; cannot infer plate node sets.");
                return result;
            }

            const double tol = 1e-6;

            var plateBounds = new List<(Guid PlateId, double Xmin, double Xmax, double Ymin, double Ymax)>();
            foreach (var p in plates)
            {
                if (p == null) continue;
                if (!p.ReconstructedBounds.HasValue)
                {
                    warnings?.Add($"Plate {p.EntityId:N} missing ReconstructedBounds.");
                    continue;
                }

                var b = p.ReconstructedBounds.Value;
                plateBounds.Add((p.EntityId, b.Xmin, b.Xmax, b.Ymin, b.Ymax));
                result[p.EntityId] = new HashSet<int>();
            }

            foreach (var cell in branch.Cells)
            {
                if (cell == null) continue;

                var cx = cell.Centroid.X;
                var cy = cell.Centroid.Y;

                for (int i = 0; i < plateBounds.Count; i++)
                {
                    var pb = plateBounds[i];
                    if (cx >= pb.Xmin - tol && cx <= pb.Xmax + tol &&
                        cy >= pb.Ymin - tol && cy <= pb.Ymax + tol)
                    {
                        var set = result[pb.PlateId];
                        if (cell.N0 > 0) set.Add(cell.N0);
                        if (cell.N1 > 0) set.Add(cell.N1);
                        if (cell.N2 > 0) set.Add(cell.N2);
                        if (cell.N3 > 0) set.Add(cell.N3);
                        break;
                    }
                }
            }

            return result;
        }
    }
}
#else
using System;
using System.Collections.Generic;
using App.Domain.MeshDomain;

namespace App.Infrastructure.Sap2000.Motores
{
    public sealed class PlateBodyConstraintsEngine
    {
        public PlateBodyConstraintsEngine(SapModelFacade facade) { }

        public void ExecuteWithAnchorPoints(
            BranchMeshSet branch,
       Dictionary<Guid, string> plateCentroidPointNamesByPlateId,
      Dictionary<Guid, List<string>> anchorPointsByPlateId,
      List<string> warnings)
      => throw new System.NotSupportedException("SAP2000 not available.");

        [Obsolete("Use ExecuteWithAnchorPoints instead")]
        public void Execute(
            BranchMeshSet branch,
            Dictionary<int, string> nodeMap,
   Dictionary<Guid, string> plateCentroidPointNamesByPlateId,
            List<string> warnings)
            => throw new System.NotSupportedException("SAP2000 not available.");
    }
}
#endif
