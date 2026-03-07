#if SAP2000_AVAILABLE
using System;
using System.Collections.Generic;
using App.Domain.MeshDomain.AnchorRefinement;

namespace App.Infrastructure.Sap2000.Motores
{
    /// <summary>
    /// Creates SAP2000 special points for anchor points (tags) generated during anchor refinement.
    /// Uses AnchorPointTag.AName as UserName to preserve deterministic naming.
    /// </summary>
    public sealed class AnchorPointsCreatorEngine
    {
        private readonly SapModelFacade _facade;

        public AnchorPointsCreatorEngine(SapModelFacade facade)
        {
            _facade = facade ?? throw new ArgumentNullException(nameof(facade));
        }

        /// <summary>
        /// Creates SAP2000 points for anchor tags at Z=0.
        /// Returns a map PlateId -> list of SAP point names created.
        /// </summary>
        public Dictionary<Guid, List<string>> CreateFromAnchorPointTags(
            IReadOnlyDictionary<Guid, List<AnchorPointTag>> tagsByPlateId,
            double z = 0.0)
        {
            var map = new Dictionary<Guid, List<string>>();
            if (tagsByPlateId == null) return map;

            foreach (var kv in tagsByPlateId)
            {
                var plateId = kv.Key;
                var tags = kv.Value;
                if (plateId == Guid.Empty || tags == null || tags.Count == 0) continue;

                var list = new List<string>();
                foreach (var t in tags)
                {
                    if (t == null) continue;
                    if (t.GlobalPoint == null) continue;

                    var userName = (t.AName ?? string.Empty).Trim();
                    if (string.IsNullOrWhiteSpace(userName))
                        continue;

                    var sapName = _facade.PointObj_AddCartesian(t.GlobalPoint.X, t.GlobalPoint.Y, z, userName);
                    _facade.PointObj_SetSpecialPoint(sapName, true);
                    list.Add(sapName);
                }

                if (list.Count > 0)
                    map[plateId] = list;
            }

            return map;
        }
    }
}
#endif
