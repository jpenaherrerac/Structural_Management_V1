#if SAP2000_AVAILABLE
using System;
using System.Collections.Generic;
using App.Domain.MeshDomain;
using App.Domain.MeshDomain.LocalMesh;
using SAP2000v1;

namespace App.Infrastructure.Sap2000.Motores
{
    /// <summary>
    /// Crea puntos especiales en SAP2000 (centroides de plates, anchors, etc.).
    /// 
    /// NOTA: Los puntos de las esquinas de celdas se crean en ShellAreasCreatorEngine.CreateFromEffectiveCells
    /// usando coordenadas (P_{hash}), no NodeId. El método CreateFromBranchNodes fue eliminado porque
    /// creaba puntos N{nodeId} que incluían nodos fuera del polígono de la losa.
    /// </summary>
    public sealed class SpecialPointsCreatorEngine
    {
        private readonly SapModelFacade _facade;

        /// <summary>
        /// Nombre del grupo para puntos centroide elevados a Z=0.10.
        /// </summary>
        public const string CentroidesElevadosGroupName = "centroides_elevados";

        public SpecialPointsCreatorEngine(SapModelFacade facade)
        {
            _facade = facade ?? throw new ArgumentNullException(nameof(facade));
        }

        /// <summary>
        /// Crea puntos especiales en los centroides de los bounds reconstruidos de Plates.
        /// El punto centroide se crea con Z=0.10.
        /// El UserName se basa en ZhLayer y ZhNumber: "{ZhLayer}_{ZhNumber}_Centr" (p.ej. "ZH6_5_Centr").
        /// Todos los puntos se asignan al grupo "centroides_elevados".
        /// Retorna un mapa PlateId (EntityId) -> nombre asignado por SAP2000.
        /// </summary>
        public Dictionary<Guid, string> CreatePlateCentroidsFromReconstructedEntities(
            IReadOnlyList<ReconstructedEntitySnapshot> reconstructedEntities)
        {
            var map = new Dictionary<Guid, string>();
            if (reconstructedEntities == null) return map;

            // Crear el grupo para centroides elevados (si no existe)
            try { _facade.GroupDef_SetGroup(CentroidesElevadosGroupName); } catch { }

            foreach (var snap in reconstructedEntities)
            {
                if (snap == null) continue;
                if (snap.EntityId == Guid.Empty) continue;
                if (snap.EntityType != ReconstructedEntityType.Plate) continue;

                if (!snap.ReconstructedBounds.HasValue)
                    continue; // cannot locate centroid reliably

                // Naming: {ZhLayer}_{ZhNumber}_Centr
                var zhLayer = (snap.ZhLayer ?? string.Empty).Trim();
                var zhNumber = snap.ZhNumber;
                if (string.IsNullOrWhiteSpace(zhLayer) || !zhNumber.HasValue)
                    continue; // cannot build deterministic name

                var userName = zhLayer + "_" + zhNumber.Value + "_Centr";

                var b = snap.ReconstructedBounds.Value;
                var cx = (b.Xmin + b.Xmax) / 2.0;
                var cy = (b.Ymin + b.Ymax) / 2.0;

                // IMPORTANT: centroid at elevated Z
                const double centroidZ = 0.10;

                var sapName = _facade.PointObj_AddCartesian(cx, cy, centroidZ, userName);
                _facade.PointObj_SetSpecialPoint(sapName, true);

                // Asignar el punto al grupo "centroides_elevados"
                // remove=false significa agregar al grupo, itemType=0 es Object
                try { _facade.PointObj_SetGroupAssign(sapName, CentroidesElevadosGroupName, false, (eItemType)0); } catch { }

                map[snap.EntityId] = sapName;
            }

            return map;
        }
    }
}
#else
using System;
using System.Collections.Generic;
using App.Domain.MeshDomain;

namespace App.Infrastructure.Sap2000.Motores
{
    public sealed class SpecialPointsCreatorEngine
    {
        public const string CentroidesElevadosGroupName = "centroides_elevados";

        public SpecialPointsCreatorEngine(SapModelFacade facade) { }

        public Dictionary<Guid, string> CreatePlateCentroidsFromReconstructedEntities(
            IReadOnlyList<ReconstructedEntitySnapshot> reconstructedEntities)
            => throw new NotSupportedException("SAP2000 not available.");
    }
}
#endif
