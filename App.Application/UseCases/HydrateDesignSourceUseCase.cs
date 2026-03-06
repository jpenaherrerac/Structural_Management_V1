using System;
using System.Linq;
using App.Application.Interfaces;
using App.Domain.Entities.Sources;
using App.Domain.Entities.Design;
using App.Domain.Enums;

namespace App.Application.UseCases
{
    public class HydrateDesignSourceRequest
    {
        public Guid ProjectId { get; set; }
        public string Label { get; set; }
        public string SapModelPath { get; set; }
        public string DesignLoadCombination { get; set; }
        public bool RunDesignFirst { get; set; }
    }

    public class HydrateDesignSourceResponse
    {
        public bool Success { get; set; }
        public Guid DesignSourceId { get; set; }
        public DesignSnapshot Snapshot { get; set; }
        public string ErrorMessage { get; set; }

        public static HydrateDesignSourceResponse Ok(Guid id, DesignSnapshot snapshot) =>
            new HydrateDesignSourceResponse { Success = true, DesignSourceId = id, Snapshot = snapshot };

        public static HydrateDesignSourceResponse Fail(string message) =>
            new HydrateDesignSourceResponse { Success = false, ErrorMessage = message };
    }

    public class HydrateDesignSourceUseCase
    {
        private readonly ISapAdapter _sapAdapter;
        private readonly IDesignSourceRepository _designRepository;
        private readonly IProjectRepository _projectRepository;

        public HydrateDesignSourceUseCase(
            ISapAdapter sapAdapter,
            IDesignSourceRepository designRepository,
            IProjectRepository projectRepository)
        {
            _sapAdapter = sapAdapter ?? throw new ArgumentNullException(nameof(sapAdapter));
            _designRepository = designRepository ?? throw new ArgumentNullException(nameof(designRepository));
            _projectRepository = projectRepository ?? throw new ArgumentNullException(nameof(projectRepository));
        }

        public HydrateDesignSourceResponse Execute(HydrateDesignSourceRequest request)
        {
            if (request == null)
                return HydrateDesignSourceResponse.Fail("Request cannot be null.");

            if (!_projectRepository.Exists(request.ProjectId))
                return HydrateDesignSourceResponse.Fail($"Project {request.ProjectId} not found.");

            if (!_sapAdapter.IsConnected)
                return HydrateDesignSourceResponse.Fail("SAP2000 is not connected.");

            if (request.RunDesignFirst)
            {
                if (!_sapAdapter.RunDesign())
                    return HydrateDesignSourceResponse.Fail("SAP2000 design run failed.");
            }

            var source = new DesignSource(
                request.ProjectId,
                request.Label ?? $"Design-{DateTime.Now:yyyyMMdd-HHmm}",
                request.SapModelPath ?? string.Empty,
                HydrationPurpose.Design);

            var snapshot = new DesignSnapshot(request.ProjectId, source.Label);

            var frameIds = _sapAdapter.GetFrameElementIds().ToList();
            var combo = request.DesignLoadCombination ?? "ENVOLVENTE";

            foreach (var id in frameIds)
            {
                source.AddElementId(id);

                var beamData = _sapAdapter.GetBeamDesignData(id);
                if (beamData != null) snapshot.AddBeam(beamData);

                var colData = _sapAdapter.GetColumnDesignData(id);
                if (colData != null) snapshot.AddColumn(colData);
            }

            var forces = _sapAdapter.GetFrameForces(combo);
            foreach (var f in forces) snapshot.AddForce(f);

            var areaIds = _sapAdapter.GetAreaElementIds().ToList();
            foreach (var id in areaIds)
            {
                var wallData = _sapAdapter.GetWallDesignData(id);
                if (wallData != null) snapshot.AddWall(wallData);
            }

            var metadata = new HydrationMetadata(_sapAdapter.GetSapVersion(), string.Empty)
            {
                NumberOfFrames = frameIds.Count,
                NumberOfAreas = areaIds.Count,
                AnalysisConverged = true
            };
            source.AttachMetadata(metadata);

            var cmdSet = new ExecutedCommandSet($"DesignHydration-{DateTime.Now:yyyyMMdd}");
            cmdSet.AddCommand("GetFrameElementIds");
            cmdSet.AddCommand("GetBeamDesignData");
            cmdSet.AddCommand("GetColumnDesignData");
            cmdSet.AddCommand("GetFrameForces");
            cmdSet.AddCommand("GetWallDesignData");
            source.AttachCommandSet(cmdSet);

            _designRepository.Add(source);

            return HydrateDesignSourceResponse.Ok(source.Id, snapshot);
        }
    }
}
