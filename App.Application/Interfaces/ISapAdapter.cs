using System;
using System.Collections.Generic;
using App.Domain.Entities.Seismic;
using App.Domain.Entities.Design;
using App.Domain.Entities.Sap;

namespace App.Application.Interfaces
{
    /// <summary>
    /// Abstraction over SAP2000 API. No SAP2000 API types are referenced here.
    /// </summary>
    public interface ISapAdapter
    {
        // ── Connection ──────────────────────────────────────────────────────────
        bool IsConnected { get; }
        ISapConnectionManager ConnectionManager { get; }
        SapSession Connect(string sapProgramPath, bool attachToExisting = false);
        void Disconnect();
        string GetSapVersion();

        // ── Model I/O ───────────────────────────────────────────────────────────
        bool OpenModel(string modelFilePath);
        bool SaveModel();
        bool RunAnalysis();
        bool RunDesign();
        bool LockModel();
        bool UnlockModel();

        // ── Element queries ─────────────────────────────────────────────────────
        IEnumerable<string> GetStoryNames();
        IEnumerable<string> GetFrameElementIds();
        IEnumerable<string> GetAreaElementIds();

        // ── Group queries ───────────────────────────────────────────────────────
        IEnumerable<string> GetGroupNames();
        IEnumerable<string> GetGroupElements(string groupName);
        IEnumerable<string> GetSelectedFrameIds();
        IEnumerable<string> GetSelectedAreaIds();

        // ── Definitions ─────────────────────────────────────────────────────────
        bool DefineLoadPattern(string name, string patternType, double selfWeightMultiplier);
        bool DefineLoadCase(string name, string caseType, string analysisType);
        bool DefineLoadCombination(string name, string combinationType, IEnumerable<(string caseName, double factor)> cases);
        bool DefineMassSource(string name, bool includeElements, bool includeAdditionalMasses);
        bool DefineResponseSpectrum(string name, double dampingRatio, IEnumerable<(double period, double accel)> points);
        bool AssignDiaphragm(string storyName, string diaphragmName, bool isRigid);
        bool DefineDiaphragmConstraint(string diaphragmName);
        bool AssignPointConstraint(string pointName, string constraintName);

        // ── Results ─────────────────────────────────────────────────────────────
        BaseShearSummary GetBaseShear(string loadCase);
        IEnumerable<StoryResult> GetStoryShears(string loadCase);
        IEnumerable<ModalResult> GetModalResults();
        IEnumerable<DriftResult> GetStoryDrifts(string loadCase);
        IEnumerable<ElementForceRecord> GetFrameForces(string loadCombination);
        BeamDesignData GetBeamDesignData(string elementId);
        ColumnDesignData GetColumnDesignData(string elementId);
        WallDesignData GetWallDesignData(string elementId);
    }
}
