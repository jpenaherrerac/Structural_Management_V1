using System;
using System.Collections.Generic;
using App.Domain.Entities.Design;

namespace App.SAP2000.Adapters
{
    /// <summary>
    /// Reads design output (forces, beam/column/wall design data) from SAP2000.
    /// </summary>
    public static class SapDesignDataReader
    {
        public static IEnumerable<ElementForceRecord> ReadFrameForces(SapConnectionService conn, string loadCombo)
        {
            var results = new List<ElementForceRecord>();
            if (conn.SapModel == null)
            {
                // Mock data
                for (int i = 1; i <= 5; i++)
                {
                    results.Add(new ElementForceRecord
                    {
                        ElementId = $"B{i}", LoadCombination = loadCombo,
                        P = -50.0 * i, V2 = 80.0 * i, M3 = 120.0 * i, Location = "Max"
                    });
                }
                return results;
            }

            try
            {
                int num = 0;
                string[] names = null, loadCases = null, stepTypes = null, pointNames = null;
                double[] stepNums = null, p = null, v2 = null, v3 = null, t = null, m2 = null, m3 = null;

                conn.SapModel.Results.FrameForce(
                    loadCombo, 2, ref num, ref names, ref loadCases,
                    ref stepTypes, ref stepNums, ref pointNames,
                    ref p, ref v2, ref v3, ref t, ref m2, ref m3);

                for (int i = 0; i < num; i++)
                {
                    results.Add(new ElementForceRecord
                    {
                        ElementId = names[i],
                        LoadCombination = loadCases[i],
                        P = p[i], V2 = v2[i], V3 = v3[i],
                        T = t[i], M2 = m2[i], M3 = m3[i],
                        Location = pointNames[i]
                    });
                }
            }
            catch { }
            return results;
        }

        public static BeamDesignData ReadBeamDesignData(SapConnectionService conn, string elementId)
        {
            if (conn.SapModel == null)
            {
                // Return mock data; in real use only return data if element is a beam
                return null;
            }

            try
            {
                int num = 0;
                string[] names = null, combos = null;
                double[] locationM = null, mu3Neg = null, mu3Pos = null, vu2 = null;

                int ret = conn.SapModel.DesignConcrete.GetSummaryResultsBeam(
                    elementId, ref num, ref names, ref combos, ref locationM,
                    ref mu3Neg, ref mu3Pos, ref vu2);

                if (ret != 0 || num == 0) return null;

                double maxMuPos = 0, maxMuNeg = 0, maxVu = 0;
                string govCombo = string.Empty;
                for (int i = 0; i < num; i++)
                {
                    if (Math.Abs(mu3Pos[i]) > maxMuPos) { maxMuPos = Math.Abs(mu3Pos[i]); govCombo = combos[i]; }
                    if (Math.Abs(mu3Neg[i]) > maxMuNeg) maxMuNeg = Math.Abs(mu3Neg[i]);
                    if (Math.Abs(vu2[i]) > maxVu) maxVu = Math.Abs(vu2[i]);
                }

                return new BeamDesignData
                {
                    ElementId = elementId,
                    MuPositiveKNm = maxMuPos,
                    MuNegativeStartKNm = maxMuNeg,
                    MuNegativeEndKNm = maxMuNeg,
                    VuKN = maxVu,
                    LoadCombination = govCombo,
                    Fc = 28.0,
                    Fy = 420.0
                };
            }
            catch { return null; }
        }

        public static ColumnDesignData ReadColumnDesignData(SapConnectionService conn, string elementId)
        {
            if (conn.SapModel == null) return null;

            try
            {
                int num = 0;
                string[] names = null, combos = null;
                double[] locationP = null, pu = null, mu2 = null, mu3 = null,
                    vu2 = null, vu3 = null, rebar = null;

                int ret = conn.SapModel.DesignConcrete.GetSummaryResultsColumn(
                    elementId, ref num, ref names, ref combos, ref locationP,
                    ref pu, ref mu2, ref mu3, ref vu2, ref vu3, ref rebar);

                if (ret != 0 || num == 0) return null;

                double maxPu = 0, maxMu2 = 0, maxMu3 = 0, maxVu = 0;
                string govCombo = string.Empty;
                for (int i = 0; i < num; i++)
                {
                    if (Math.Abs(pu[i]) > maxPu) { maxPu = Math.Abs(pu[i]); govCombo = combos[i]; }
                    if (Math.Abs(mu2[i]) > maxMu2) maxMu2 = Math.Abs(mu2[i]);
                    if (Math.Abs(mu3[i]) > maxMu3) maxMu3 = Math.Abs(mu3[i]);
                    if (Math.Abs(vu2[i]) > maxVu) maxVu = Math.Abs(vu2[i]);
                }

                return new ColumnDesignData
                {
                    ElementId = elementId,
                    PuKN = maxPu,
                    Mu2KNm = maxMu2,
                    Mu3KNm = maxMu3,
                    VuKN = maxVu,
                    LoadCombination = govCombo,
                    Fc = 28.0,
                    Fy = 420.0
                };
            }
            catch { return null; }
        }

        public static WallDesignData ReadWallDesignData(SapConnectionService conn, string elementId)
        {
            if (conn.SapModel == null) return null;

            try
            {
                // Wall design data extraction from SAP2000 pier results
                int num = 0;
                string[] names = null, locations = null, combos = null;
                double[] pu = null, mu2 = null, mu3 = null, vu2 = null, vu3 = null;

                int ret = conn.SapModel.DesignConcrete.GetSummaryResultsPier(
                    elementId, ref num, ref names, ref locations, ref combos,
                    ref pu, ref mu2, ref mu3, ref vu2, ref vu3);

                if (ret != 0 || num == 0) return null;

                double maxPu = 0, maxMu = 0, maxVu = 0;
                string govCombo = string.Empty;
                for (int i = 0; i < num; i++)
                {
                    if (Math.Abs(pu[i]) > maxPu) { maxPu = Math.Abs(pu[i]); govCombo = combos[i]; }
                    if (Math.Abs(mu3[i]) > maxMu) maxMu = Math.Abs(mu3[i]);
                    if (Math.Abs(vu2[i]) > maxVu) maxVu = Math.Abs(vu2[i]);
                }

                return new WallDesignData
                {
                    ElementId = elementId,
                    PuKN = maxPu,
                    MuKNm = maxMu,
                    VuKN = maxVu,
                    LoadCombination = govCombo,
                    Fc = 28.0,
                    Fy = 420.0
                };
            }
            catch { return null; }
        }
    }
}
