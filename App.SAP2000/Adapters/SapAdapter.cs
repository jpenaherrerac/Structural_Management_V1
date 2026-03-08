using System;
using System.Collections.Generic;
using App.Application.Interfaces;
using App.Domain.Entities.Design;
using App.Domain.Entities.Sap;
using App.Domain.Entities.Seismic;

namespace App.SAP2000.Adapters
{
    /// <summary>
    /// Implements ISapAdapter by delegating to SapConnectionService, SapModelFacade,
    /// and the static readers.  The actual SAP2000 API calls are isolated here so
    /// other projects have no API dependency.
    /// </summary>
    public class SapAdapter : ISapAdapter
    {
        private readonly SapConnectionService _connectionService;
        private readonly SapModelFacade _facade;
        private SapSession? _currentSession;

        public bool IsConnected => _connectionService.IsConnected;
        public ISapConnectionManager ConnectionManager => _connectionService;

        public SapAdapter()
        {
            _connectionService = new SapConnectionService();
            _facade = new SapModelFacade(_connectionService);
        }

        // ── Connection ──────────────────────────────────────────────────────────

        public SapSession Connect(string sapProgramPath, bool attachToExisting = false)
        {
            _currentSession = _connectionService.Connect(sapProgramPath, attachToExisting);
            return _currentSession;
        }

        public void Disconnect()
        {
            _connectionService.Disconnect();
            _currentSession = null;
        }

        public string GetSapVersion() => _connectionService.GetSapVersion();

        // ── Model I/O ───────────────────────────────────────────────────────────

        public bool OpenModel(string modelFilePath) =>
            _connectionService.OpenModel(modelFilePath);

        public bool SaveModel() => _connectionService.SaveModel();

        public bool RunAnalysis() => _connectionService.RunAnalysis();

        public bool RunDesign() => _connectionService.RunDesign();

        public bool LockModel() => _connectionService.SetModelLocked(true);

        public bool UnlockModel() => _connectionService.SetModelLocked(false);

        // ── Element queries ─────────────────────────────────────────────────────

        public IEnumerable<string> GetStoryNames() =>
            _connectionService.GetStoryNames();

        public IEnumerable<string> GetFrameElementIds() =>
            _connectionService.GetFrameElementIds();

        public IEnumerable<string> GetAreaElementIds() =>
            _connectionService.GetAreaElementIds();

        // ── Group queries ───────────────────────────────────────────────────────

        public IEnumerable<string> GetGroupNames() =>
            _connectionService.GetGroupNames();

        public IEnumerable<string> GetGroupElements(string groupName) =>
            _connectionService.GetGroupElements(groupName);

        public IEnumerable<string> GetSelectedFrameIds() =>
            _connectionService.GetSelectedFrameIds();

        public IEnumerable<string> GetSelectedAreaIds() =>
            _connectionService.GetSelectedAreaIds();

        // ── Definitions ─────────────────────────────────────────────────────────

        public bool DefineLoadPattern(string name, string patternType, double selfWeightMultiplier) =>
            _connectionService.DefineLoadPattern(name, patternType, selfWeightMultiplier);

        public bool DefineLoadCase(string name, string caseType, string analysisType) =>
            _connectionService.DefineLoadCase(name, caseType, analysisType);

        public bool DefineLoadCombination(string name, string combinationType,
            IEnumerable<(string caseName, double factor)> cases) =>
            _connectionService.DefineLoadCombination(name, combinationType, cases);

        public bool DefineMassSource(string name, bool includeElements, bool includeAdditionalMasses) =>
            _connectionService.DefineMassSource(name, includeElements, includeAdditionalMasses);

        public bool DefineResponseSpectrum(string name, double dampingRatio,
            IEnumerable<(double period, double accel)> points) =>
            _connectionService.DefineResponseSpectrum(name, dampingRatio, points);

        public bool AssignDiaphragm(string storyName, string diaphragmName, bool isRigid) =>
            _connectionService.AssignDiaphragm(storyName, diaphragmName, isRigid);

        public bool DefineDiaphragmConstraint(string diaphragmName) =>
            _connectionService.DefineDiaphragmConstraint(diaphragmName);

        public bool AssignPointConstraint(string pointName, string constraintName) =>
            _connectionService.AssignPointConstraint(pointName, constraintName);

        // ── Results ─────────────────────────────────────────────────────────────

        public BaseShearSummary GetBaseShear(string loadCase) =>
            SapStructureOutputReader.ReadBaseShear(_connectionService, loadCase);

        public IEnumerable<StoryResult> GetStoryShears(string loadCase) =>
            SapStructureOutputReader.ReadStoryShears(_connectionService, loadCase);

        public IEnumerable<ModalResult> GetModalResults() =>
            SapStructureOutputReader.ReadModalResults(_connectionService);

        public IEnumerable<DriftResult> GetStoryDrifts(string loadCase) =>
            SapStructureOutputReader.ReadStoryDrifts(_connectionService, loadCase);

        public IEnumerable<ElementForceRecord> GetFrameForces(string loadCombination) =>
            SapDesignDataReader.ReadFrameForces(_connectionService, loadCombination);

        public BeamDesignData GetBeamDesignData(string elementId) =>
            SapDesignDataReader.ReadBeamDesignData(_connectionService, elementId);

        public ColumnDesignData GetColumnDesignData(string elementId) =>
            SapDesignDataReader.ReadColumnDesignData(_connectionService, elementId);

        public WallDesignData GetWallDesignData(string elementId) =>
            SapDesignDataReader.ReadWallDesignData(_connectionService, elementId);
    }
}
