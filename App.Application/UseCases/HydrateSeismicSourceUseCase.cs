using System;
using System.Linq;
using App.Application.Interfaces;
using App.Domain.Entities.Sources;
using App.Domain.Entities.Seismic;
using App.Domain.Enums;

namespace App.Application.UseCases
{
    public class HydrateSeismicSourceRequest
    {
        public Guid ProjectId { get; set; }
        public string Label { get; set; }
        public string SapModelPath { get; set; }
        public string SeismicLoadCaseX { get; set; }
        public string SeismicLoadCaseY { get; set; }
        public bool RunAnalysisFirst { get; set; }
    }

    public class HydrateSeismicSourceResponse
    {
        public bool Success { get; set; }
        public Guid SeismicSourceId { get; set; }
        public StructureOutputSnapshot Snapshot { get; set; }
        public string ErrorMessage { get; set; }

        public static HydrateSeismicSourceResponse Ok(Guid id, StructureOutputSnapshot snapshot) =>
            new HydrateSeismicSourceResponse { Success = true, SeismicSourceId = id, Snapshot = snapshot };

        public static HydrateSeismicSourceResponse Fail(string message) =>
            new HydrateSeismicSourceResponse { Success = false, ErrorMessage = message };
    }

    public class HydrateSeismicSourceUseCase
    {
        private readonly ISapAdapter _sapAdapter;
        private readonly ISeismicSourceRepository _seismicRepository;
        private readonly IProjectRepository _projectRepository;

        public HydrateSeismicSourceUseCase(
            ISapAdapter sapAdapter,
            ISeismicSourceRepository seismicRepository,
            IProjectRepository projectRepository)
        {
            _sapAdapter = sapAdapter ?? throw new ArgumentNullException(nameof(sapAdapter));
            _seismicRepository = seismicRepository ?? throw new ArgumentNullException(nameof(seismicRepository));
            _projectRepository = projectRepository ?? throw new ArgumentNullException(nameof(projectRepository));
        }

        public HydrateSeismicSourceResponse Execute(HydrateSeismicSourceRequest request)
        {
            if (request == null)
                return HydrateSeismicSourceResponse.Fail("Request cannot be null.");

            if (!_projectRepository.Exists(request.ProjectId))
                return HydrateSeismicSourceResponse.Fail($"Project {request.ProjectId} not found.");

            if (!_sapAdapter.IsConnected)
                return HydrateSeismicSourceResponse.Fail("SAP2000 is not connected.");

            if (request.RunAnalysisFirst)
            {
                if (!_sapAdapter.RunAnalysis())
                    return HydrateSeismicSourceResponse.Fail("SAP2000 analysis run failed.");
            }

            var source = new SeismicSource(
                request.ProjectId,
                request.Label ?? $"Seismic-{DateTime.Now:yyyyMMdd-HHmm}",
                request.SapModelPath ?? string.Empty,
                HydrationPurpose.Seismic);

            var snapshot = new StructureOutputSnapshot(request.ProjectId, source.Label);

            var storyNames = _sapAdapter.GetStoryNames().ToList();
            foreach (var s in storyNames) source.AddStoryName(s);

            var storyDataSet = new Domain.Entities.Seismic.StoryDataSet();
            var baseShearX = _sapAdapter.GetBaseShear(request.SeismicLoadCaseX ?? "Sx");
            var baseShearY = _sapAdapter.GetBaseShear(request.SeismicLoadCaseY ?? "Sy");

            var storyResultsX = _sapAdapter.GetStoryShears(request.SeismicLoadCaseX ?? "Sx");
            foreach (var r in storyResultsX) storyDataSet.Add(r);
            snapshot.StoryData = storyDataSet;

            var modalData = new Domain.Entities.Seismic.ModalDataSet();
            foreach (var m in _sapAdapter.GetModalResults()) modalData.Add(m);
            snapshot.ModalData = modalData;

            var driftData = new Domain.Entities.Seismic.DriftDataSet();
            foreach (var d in _sapAdapter.GetStoryDrifts(request.SeismicLoadCaseX ?? "Sx")) driftData.Add(d);
            foreach (var d in _sapAdapter.GetStoryDrifts(request.SeismicLoadCaseY ?? "Sy")) driftData.Add(d);
            snapshot.DriftData = driftData;

            var globalSummary = new Domain.Entities.Seismic.GlobalSeismicSummary
            {
                TotalStructuralWeightKN = storyDataSet.GetTotalWeight(),
                FundamentalPeriodX = modalData.GetFundamentalPeriod(),
                FundamentalPeriodY = modalData.GetFundamentalPeriod(),
                ModalMassParticipationX = modalData.GetSumModalMassX(),
                ModalMassParticipationY = modalData.GetSumModalMassY()
            };
            if (baseShearX != null) globalSummary.AddBaseShear(baseShearX);
            if (baseShearY != null) globalSummary.AddBaseShear(baseShearY);
            snapshot.GlobalSummary = globalSummary;

            var metadata = new HydrationMetadata(_sapAdapter.GetSapVersion(), string.Empty)
            {
                NumberOfStories = storyNames.Count,
                TotalWeightKN = storyDataSet.GetTotalWeight(),
                AnalysisConverged = true,
                NumberOfModes = modalData.Results.Count
            };
            source.AttachMetadata(metadata);

            var cmdSet = new ExecutedCommandSet($"SeismicHydration-{DateTime.Now:yyyyMMdd}");
            cmdSet.AddCommand("GetStoryNames");
            cmdSet.AddCommand("GetBaseShear");
            cmdSet.AddCommand("GetStoryShears");
            cmdSet.AddCommand("GetModalResults");
            cmdSet.AddCommand("GetStoryDrifts");
            source.AttachCommandSet(cmdSet);

            _seismicRepository.Add(source);

            return HydrateSeismicSourceResponse.Ok(source.Id, snapshot);
        }
    }
}
