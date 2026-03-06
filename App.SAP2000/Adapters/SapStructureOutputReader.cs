using System;
using System.Collections.Generic;
using App.Domain.Entities.Seismic;

namespace App.SAP2000.Adapters
{
    /// <summary>
    /// Reads structural analysis output (base shear, story shears, modal data, drifts) from SAP2000.
    /// </summary>
    public static class SapStructureOutputReader
    {
        public static BaseShearSummary ReadBaseShear(SapConnectionService conn, string loadCase)
        {
            if (!conn.IsConnected) return new BaseShearSummary(loadCase, 0, 0);

            if (conn.SapModel == null)
            {
                // Mock data for development
                return new BaseShearSummary(loadCase, 850.0, 0)
                {
                    Fy = 820.0,
                    Units = "kN"
                };
            }

            try
            {
                int numberResults = 0;
                string[] loadCases = null, stepTypes = null;
                double[] stepNums = null, f1 = null, f2 = null, f3 = null, m1 = null, m2 = null, m3 = null;

                conn.SapModel.Results.BaseReact(
                    ref numberResults, ref loadCases, ref stepTypes, ref stepNums,
                    ref f1, ref f2, ref f3, ref m1, ref m2, ref m3);

                for (int i = 0; i < numberResults; i++)
                {
                    if (string.Equals(loadCases[i], loadCase, StringComparison.OrdinalIgnoreCase))
                    {
                        return new BaseShearSummary(loadCase, f1[i], f2[i])
                        {
                            Fz = f3[i], Mx = m1[i], My = m2[i], Mz = m3[i], Units = "kN, kN-m"
                        };
                    }
                }
            }
            catch { }
            return new BaseShearSummary(loadCase, 0, 0);
        }

        public static IEnumerable<StoryResult> ReadStoryShears(SapConnectionService conn, string loadCase)
        {
            var results = new List<StoryResult>();

            if (conn.SapModel == null)
            {
                // Mock data
                string[] stories = { "PISO 1", "PISO 2", "PISO 3", "PISO 4", "PISO 5" };
                for (int i = 0; i < stories.Length; i++)
                {
                    results.Add(new StoryResult(stories[i], i + 1, (i + 1) * 3.0)
                    {
                        ShearX = 850.0 - i * 120.0,
                        ShearY = 820.0 - i * 115.0,
                        WeightKN = 2000.0,
                        LoadCase = loadCase
                    });
                }
                return results;
            }

            try
            {
                int num = 0;
                string[] storyNames = null, loadCases = null, stepTypes = null;
                double[] stepNums = null, direction = null, driftX = null, driftY = null,
                    dispX = null, dispY = null, shearX = null, shearY = null;

                conn.SapModel.Results.StoryDrifts(
                    ref num, ref storyNames, ref loadCases, ref stepTypes, ref stepNums,
                    ref direction, ref driftX, ref driftY, ref dispX, ref dispY, ref shearX, ref shearY);

                for (int i = 0; i < num; i++)
                {
                    if (string.Equals(loadCases[i], loadCase, StringComparison.OrdinalIgnoreCase))
                    {
                        results.Add(new StoryResult(storyNames[i], i + 1, 0)
                        {
                            ShearX = shearX[i],
                            ShearY = shearY[i],
                            LoadCase = loadCase
                        });
                    }
                }
            }
            catch { }
            return results;
        }

        public static IEnumerable<ModalResult> ReadModalResults(SapConnectionService conn)
        {
            var results = new List<ModalResult>();

            if (conn.SapModel == null)
            {
                // Mock modal data
                double[] periods = { 0.85, 0.82, 0.41, 0.38, 0.26, 0.24 };
                double[] massX = { 0.72, 0.02, 0.13, 0.01, 0.05, 0.01 };
                double[] massY = { 0.02, 0.73, 0.01, 0.14, 0.01, 0.04 };
                double cumX = 0, cumY = 0;
                for (int i = 0; i < periods.Length; i++)
                {
                    cumX += massX[i]; cumY += massY[i];
                    results.Add(new ModalResult(i + 1, periods[i])
                    {
                        ModalMassRatioX = massX[i],
                        ModalMassRatioY = massY[i],
                        CumulativeModalMassX = cumX,
                        CumulativeModalMassY = cumY
                    });
                }
                return results;
            }

            try
            {
                int num = 0;
                string[] loadCases = null, stepTypes = null;
                double[] stepNums = null, periods = null, freq = null, circFreq = null, eigenVal = null;

                conn.SapModel.Results.ModalPeriod(
                    ref num, ref loadCases, ref stepTypes, ref stepNums,
                    ref periods, ref freq, ref circFreq, ref eigenVal);

                for (int i = 0; i < num; i++)
                {
                    results.Add(new ModalResult(i + 1, periods[i]));
                }
            }
            catch { }
            return results;
        }

        public static IEnumerable<DriftResult> ReadStoryDrifts(SapConnectionService conn, string loadCase)
        {
            var results = new List<DriftResult>();

            if (conn.SapModel == null)
            {
                string[] stories = { "PISO 1", "PISO 2", "PISO 3", "PISO 4", "PISO 5" };
                for (int i = 0; i < stories.Length; i++)
                {
                    results.Add(new DriftResult(stories[i], loadCase, 0.004 + i * 0.0005, 0.0038 + i * 0.0004)
                    {
                        StoryHeightMeters = 3.0
                    });
                }
                return results;
            }

            try
            {
                int num = 0;
                string[] storyNames = null, loadCases = null, stepTypes = null;
                double[] stepNums = null, dir = null, driftX = null, driftY = null,
                    dispX = null, dispY = null, shearX = null, shearY = null;

                conn.SapModel.Results.StoryDrifts(
                    ref num, ref storyNames, ref loadCases, ref stepTypes, ref stepNums,
                    ref dir, ref driftX, ref driftY, ref dispX, ref dispY, ref shearX, ref shearY);

                for (int i = 0; i < num; i++)
                {
                    if (string.Equals(loadCases[i], loadCase, StringComparison.OrdinalIgnoreCase))
                    {
                        results.Add(new DriftResult(storyNames[i], loadCase, driftX[i], driftY[i])
                        {
                            DisplacementX = dispX[i],
                            DisplacementY = dispY[i]
                        });
                    }
                }
            }
            catch { }
            return results;
        }
    }
}
