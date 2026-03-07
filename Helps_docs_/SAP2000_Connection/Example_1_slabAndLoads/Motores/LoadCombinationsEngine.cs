#if SAP2000_AVAILABLE
using System;
using System.Collections.Generic;
using SAP2000v1;

namespace App.Infrastructure.Sap2000.Motores
{
    /// <summary>
    /// Creates load combinations in SAP2000 according to the project specification.
    /// All combinations are hardcoded for the electrical substation slab project.
    /// 
    /// LoadCase mapping:
    /// DEAD = peso propio (siempre acompaña a LC1 con el mismo factor)
    /// LC1 = LC1 (carga muerta adicional - ahora es patrón independiente)
    /// LC2 = LC2 (expansión térmica)
    /// LC3 = LC3 (tensión estática de conductor)
    /// LC4X = LC4x (viento en X)
    /// LC4Y = LC4y (viento en Y)
    /// LC5 = LC5 (tensiones de cortocircuito)
    /// LC6 = LC6 (switching forces)
    /// LC7X = LC7x (sismo en X)
    /// LC7Y = LC7y (sismo en Y)
    /// LC7Z = LC7z (sismo en Z)
    /// LC8 = LIVE (carga viva)
    /// LC9 = LC9 (carga de transporte)
    /// </summary>
    public sealed class LoadCombinationsEngine
    {
        private readonly SapModelFacade _facade;

        // LoadCase names in SAP2000
        private const string DEAD = "DEAD";
        private const string LC1 = "LC1";
        private const string LC2 = "LC2";
        private const string LC3 = "LC3";
        private const string LC4X = "LC4x";
        private const string LC4Y = "LC4y";
        private const string LC5 = "LC5";
        private const string LC6 = "LC6";
        private const string LC7X = "LC7x";
        private const string LC7Y = "LC7y";
        private const string LC7Z = "LC7z";
        private const string LC8 = "LIVE";
        private const string LC9 = "LC9";

        public LoadCombinationsEngine(SapModelFacade facade)
        {
            _facade = facade ?? throw new ArgumentNullException(nameof(facade));
        }

        /// <summary>
        /// Creates all service and ultimate load combinations.
        /// </summary>
        public void CreateAllCombinations(List<string> warnings)
        {
            warnings = warnings ?? new List<string>();

            try
            {
                CreateServiceCombinations(warnings);
                CreateUltimateCombinations(warnings);
                CreateEnvelopes(warnings);
            }
            catch (Exception ex)
            {
                warnings.Add($"LoadCombinationsEngine failed: {ex.Message}");
            }
        }

        /// <summary>
        /// Creates service combinations (S1 - S11.2).
        /// DEAD always accompanies LC1 with the same factor.
        /// </summary>
        private void CreateServiceCombinations(List<string> warnings)
        {
            // S1: 1.0 DEAD + 1.0 LC1
            CreateCombo("S1", new[] { (DEAD, 1.0), (LC1, 1.0) }, warnings);

            // S2.1 - S2.4: DEAD + LC1 ± LC2 + LC3 ± LC4X
            CreateCombo("S2.1", new[] { (DEAD, 1.0), (LC1, 1.0), (LC2, 1.0), (LC3, 1.0), (LC4X, 1.0) }, warnings);
            CreateCombo("S2.2", new[] { (DEAD, 1.0), (LC1, 1.0), (LC2, 1.0), (LC3, 1.0), (LC4X, -1.0) }, warnings);
            CreateCombo("S2.3", new[] { (DEAD, 1.0), (LC1, 1.0), (LC2, -1.0), (LC3, 1.0), (LC4X, 1.0) }, warnings);
            CreateCombo("S2.4", new[] { (DEAD, 1.0), (LC1, 1.0), (LC2, -1.0), (LC3, 1.0), (LC4X, -1.0) }, warnings);

            // S3.1 - S3.4: DEAD + LC1 ± LC2 + LC3 ± LC4Y
            CreateCombo("S3.1", new[] { (DEAD, 1.0), (LC1, 1.0), (LC2, 1.0), (LC3, 1.0), (LC4Y, 1.0) }, warnings);
            CreateCombo("S3.2", new[] { (DEAD, 1.0), (LC1, 1.0), (LC2, 1.0), (LC3, 1.0), (LC4Y, -1.0) }, warnings);
            CreateCombo("S3.3", new[] { (DEAD, 1.0), (LC1, 1.0), (LC2, -1.0), (LC3, 1.0), (LC4Y, 1.0) }, warnings);
            CreateCombo("S3.4", new[] { (DEAD, 1.0), (LC1, 1.0), (LC2, -1.0), (LC3, 1.0), (LC4Y, -1.0) }, warnings);

            // S4.1 - S4.2: DEAD + LC1 ± LC2 + LC5
            CreateCombo("S4.1", new[] { (DEAD, 1.0), (LC1, 1.0), (LC2, 1.0), (LC5, 1.0) }, warnings);
            CreateCombo("S4.2", new[] { (DEAD, 1.0), (LC1, 1.0), (LC2, -1.0), (LC5, 1.0) }, warnings);

            // S5.1 - S5.4: DEAD + LC1 ± LC2 + LC3 ± LC6
            CreateCombo("S5.1", new[] { (DEAD, 1.0), (LC1, 1.0), (LC2, 1.0), (LC3, 1.0), (LC6, 1.0) }, warnings);
            CreateCombo("S5.2", new[] { (DEAD, 1.0), (LC1, 1.0), (LC2, 1.0), (LC3, 1.0), (LC6, -1.0) }, warnings);
            CreateCombo("S5.3", new[] { (DEAD, 1.0), (LC1, 1.0), (LC2, -1.0), (LC3, 1.0), (LC6, 1.0) }, warnings);
            CreateCombo("S5.4", new[] { (DEAD, 1.0), (LC1, 1.0), (LC2, -1.0), (LC3, 1.0), (LC6, -1.0) }, warnings);

            // S6.1 - S6.8: DEAD + LC1 ± LC2 + LC3 ± 0.8*LC7X ± 0.32*LC7Z
            CreateCombo("S6.1", new[] { (DEAD, 1.0), (LC1, 1.0), (LC2, 1.0), (LC3, 1.0), (LC7X, 0.8), (LC7Z, 0.32) }, warnings);
            CreateCombo("S6.2", new[] { (DEAD, 1.0), (LC1, 1.0), (LC2, 1.0), (LC3, 1.0), (LC7X, 0.8), (LC7Z, -0.32) }, warnings);
            CreateCombo("S6.3", new[] { (DEAD, 1.0), (LC1, 1.0), (LC2, 1.0), (LC3, 1.0), (LC7X, -0.8), (LC7Z, 0.32) }, warnings);
            CreateCombo("S6.4", new[] { (DEAD, 1.0), (LC1, 1.0), (LC2, 1.0), (LC3, 1.0), (LC7X, -0.8), (LC7Z, -0.32) }, warnings);
            CreateCombo("S6.5", new[] { (DEAD, 1.0), (LC1, 1.0), (LC2, -1.0), (LC3, 1.0), (LC7X, 0.8), (LC7Z, 0.32) }, warnings);
            CreateCombo("S6.6", new[] { (DEAD, 1.0), (LC1, 1.0), (LC2, -1.0), (LC3, 1.0), (LC7X, 0.8), (LC7Z, -0.32) }, warnings);
            CreateCombo("S6.7", new[] { (DEAD, 1.0), (LC1, 1.0), (LC2, -1.0), (LC3, 1.0), (LC7X, -0.8), (LC7Z, 0.32) }, warnings);
            CreateCombo("S6.8", new[] { (DEAD, 1.0), (LC1, 1.0), (LC2, -1.0), (LC3, 1.0), (LC7X, -0.8), (LC7Z, -0.32) }, warnings);

            // S7.1 - S7.8: DEAD + LC1 ± LC2 + LC3 ± 0.32*LC7X ± 0.8*LC7Z
            CreateCombo("S7.1", new[] { (DEAD, 1.0), (LC1, 1.0), (LC2, 1.0), (LC3, 1.0), (LC7X, 0.32), (LC7Z, 0.8) }, warnings);
            CreateCombo("S7.2", new[] { (DEAD, 1.0), (LC1, 1.0), (LC2, 1.0), (LC3, 1.0), (LC7X, 0.32), (LC7Z, -0.8) }, warnings);
            CreateCombo("S7.3", new[] { (DEAD, 1.0), (LC1, 1.0), (LC2, -1.0), (LC3, 1.0), (LC7X, 0.32), (LC7Z, 0.8) }, warnings);
            CreateCombo("S7.4", new[] { (DEAD, 1.0), (LC1, 1.0), (LC2, -1.0), (LC3, 1.0), (LC7X, 0.32), (LC7Z, -0.8) }, warnings);
            CreateCombo("S7.5", new[] { (DEAD, 1.0), (LC1, 1.0), (LC2, -1.0), (LC3, 1.0), (LC7X, -0.32), (LC7Z, 0.8) }, warnings);
            CreateCombo("S7.6", new[] { (DEAD, 1.0), (LC1, 1.0), (LC2, -1.0), (LC3, 1.0), (LC7X, -0.32), (LC7Z, -0.8) }, warnings);
            CreateCombo("S7.7", new[] { (DEAD, 1.0), (LC1, 1.0), (LC2, -1.0), (LC3, 1.0), (LC7X, 0.32), (LC7Z, 0.8) }, warnings);
            CreateCombo("S7.8", new[] { (DEAD, 1.0), (LC1, 1.0), (LC2, -1.0), (LC3, 1.0), (LC7X, 0.32), (LC7Z, -0.8) }, warnings);

            // S8.1 - S8.8: DEAD + LC1 ± LC2 + LC3 ± 0.8*LC7Y ± 0.32*LC7Z
            CreateCombo("S8.1", new[] { (DEAD, 1.0), (LC1, 1.0), (LC2, 1.0), (LC3, 1.0), (LC7Y, 0.8), (LC7Z, 0.32) }, warnings);
            CreateCombo("S8.2", new[] { (DEAD, 1.0), (LC1, 1.0), (LC2, 1.0), (LC3, 1.0), (LC7Y, 0.8), (LC7Z, -0.32) }, warnings);
            CreateCombo("S8.3", new[] { (DEAD, 1.0), (LC1, 1.0), (LC2, 1.0), (LC3, 1.0), (LC7Y, -0.8), (LC7Z, 0.32) }, warnings);
            CreateCombo("S8.4", new[] { (DEAD, 1.0), (LC1, 1.0), (LC2, 1.0), (LC3, 1.0), (LC7Y, -0.8), (LC7Z, -0.32) }, warnings);
            CreateCombo("S8.5", new[] { (DEAD, 1.0), (LC1, 1.0), (LC2, -1.0), (LC3, 1.0), (LC7Y, 0.8), (LC7Z, 0.32) }, warnings);
            CreateCombo("S8.6", new[] { (DEAD, 1.0), (LC1, 1.0), (LC2, -1.0), (LC3, 1.0), (LC7Y, 0.8), (LC7Z, -0.32) }, warnings);
            CreateCombo("S8.7", new[] { (DEAD, 1.0), (LC1, 1.0), (LC2, -1.0), (LC3, 1.0), (LC7Y, -0.8), (LC7Z, 0.32) }, warnings);
            CreateCombo("S8.8", new[] { (DEAD, 1.0), (LC1, 1.0), (LC2, -1.0), (LC3, 1.0), (LC7Y, -0.8), (LC7Z, -0.32) }, warnings);

            // S9.1 - S9.8: DEAD + LC1 ± LC2 + LC3 ± 0.32*LC7Y ± 0.8*LC7Z
            CreateCombo("S9.1", new[] { (DEAD, 1.0), (LC1, 1.0), (LC2, 1.0), (LC3, 1.0), (LC7Y, 0.32), (LC7Z, 0.8) }, warnings);
            CreateCombo("S9.2", new[] { (DEAD, 1.0), (LC1, 1.0), (LC2, 1.0), (LC3, 1.0), (LC7Y, 0.32), (LC7Z, -0.8) }, warnings);
            CreateCombo("S9.3", new[] { (DEAD, 1.0), (LC1, 1.0), (LC2, 1.0), (LC3, 1.0), (LC7Y, -0.32), (LC7Z, 0.8) }, warnings);
            CreateCombo("S9.4", new[] { (DEAD, 1.0), (LC1, 1.0), (LC2, 1.0), (LC3, 1.0), (LC7Y, -0.32), (LC7Z, -0.8) }, warnings);
            CreateCombo("S9.5", new[] { (DEAD, 1.0), (LC1, 1.0), (LC2, -1.0), (LC3, 1.0), (LC7Y, 0.32), (LC7Z, 0.8) }, warnings);
            CreateCombo("S9.6", new[] { (DEAD, 1.0), (LC1, 1.0), (LC2, -1.0), (LC3, 1.0), (LC7Y, 0.32), (LC7Z, -0.8) }, warnings);
            CreateCombo("S9.7", new[] { (DEAD, 1.0), (LC1, 1.0), (LC2, -1.0), (LC3, 1.0), (LC7Y, -0.32), (LC7Z, 0.8) }, warnings);
            CreateCombo("S9.8", new[] { (DEAD, 1.0), (LC1, 1.0), (LC2, -1.0), (LC3, 1.0), (LC7Y, -0.32), (LC7Z, -0.8) }, warnings);

            // S10: 1.0 DEAD + 1.0 LC1 + 1.0 LC8
            CreateCombo("S10", new[] { (DEAD, 1.0), (LC1, 1.0), (LC8, 1.0) }, warnings);

            // S11.1 - S11.2: DEAD + LC1 ± LC2 + LC3 + LC9
            CreateCombo("S11.1", new[] { (DEAD, 1.0), (LC1, 1.0), (LC2, 1.0), (LC3, 1.0), (LC9, 1.0) }, warnings);
            CreateCombo("S11.2", new[] { (DEAD, 1.0), (LC1, 1.0), (LC2, -1.0), (LC3, 1.0), (LC9, 1.0) }, warnings);

            warnings.Add($"[INFO] Created 49 service combinations (S1 - S11.2).");
        }

        /// <summary>
        /// Creates ultimate combinations (U1 - U11.2).
        /// DEAD always accompanies LC1 with the same factor.
        /// </summary>
        private void CreateUltimateCombinations(List<string> warnings)
        {
            // U1: 1.1 DEAD + 1.1 LC1
            CreateCombo("U1", new[] { (DEAD, 1.1), (LC1, 1.1) }, warnings);

            // U2.1 - U2.4: 1.1*DEAD + 1.1*LC1 ± 1.1*LC2 + 1.1*LC3 ± 1.3*LC4X
            CreateCombo("U2.1", new[] { (DEAD, 1.1), (LC1, 1.1), (LC2, 1.1), (LC3, 1.1), (LC4X, 1.3) }, warnings);
            CreateCombo("U2.2", new[] { (DEAD, 1.1), (LC1, 1.1), (LC2, 1.1), (LC3, 1.1), (LC4X, -1.3) }, warnings);
            CreateCombo("U2.3", new[] { (DEAD, 1.1), (LC1, 1.1), (LC2, -1.1), (LC3, 1.1), (LC4X, 1.3) }, warnings);
            CreateCombo("U2.4", new[] { (DEAD, 1.1), (LC1, 1.1), (LC2, -1.1), (LC3, 1.1), (LC4X, -1.3) }, warnings);

            // U3.1 - U3.4: 1.1*DEAD + 1.1*LC1 ± 1.1*LC2 + 1.1*LC3 ± 1.3*LC4Y
            CreateCombo("U3.1", new[] { (DEAD, 1.1), (LC1, 1.1), (LC2, 1.1), (LC3, 1.1), (LC4Y, 1.3) }, warnings);
            CreateCombo("U3.2", new[] { (DEAD, 1.1), (LC1, 1.1), (LC2, 1.1), (LC3, 1.1), (LC4Y, -1.3) }, warnings);
            CreateCombo("U3.3", new[] { (DEAD, 1.1), (LC1, 1.1), (LC2, -1.1), (LC3, 1.1), (LC4Y, 1.3) }, warnings);
            CreateCombo("U3.4", new[] { (DEAD, 1.1), (LC1, 1.1), (LC2, -1.1), (LC3, 1.1), (LC4Y, -1.3) }, warnings);

            // U4.1 - U4.2: 1.0*DEAD + 1.0*LC1 ± 1.0*LC2 + 1.0*LC5
            CreateCombo("U4.1", new[] { (DEAD, 1.0), (LC1, 1.0), (LC2, 1.0), (LC5, 1.0) }, warnings);
            CreateCombo("U4.2", new[] { (DEAD, 1.0), (LC1, 1.0), (LC2, -1.0), (LC5, 1.0) }, warnings);

            // U5.1 - U5.4: 1.1*DEAD + 1.1*LC1 ± 1.1*LC2 + 1.1*LC3 ± 1.1*LC6
            CreateCombo("U5.1", new[] { (DEAD, 1.1), (LC1, 1.1), (LC2, 1.1), (LC3, 1.1), (LC6, 1.1) }, warnings);
            CreateCombo("U5.2", new[] { (DEAD, 1.1), (LC1, 1.1), (LC2, 1.1), (LC3, 1.1), (LC6, -1.1) }, warnings);
            CreateCombo("U5.3", new[] { (DEAD, 1.1), (LC1, 1.1), (LC2, -1.1), (LC3, 1.1), (LC6, 1.1) }, warnings);
            CreateCombo("U5.4", new[] { (DEAD, 1.1), (LC1, 1.1), (LC2, -1.1), (LC3, 1.1), (LC6, -1.1) }, warnings);

            // U6.1 - U6.8: 1.1*DEAD + 1.1*LC1 ± 1.1*LC2 + 1.1*LC3 ± 1.0*LC7X ± 0.40*LC7Z
            CreateCombo("U6.1", new[] { (DEAD, 1.1), (LC1, 1.1), (LC2, 1.1), (LC3, 1.1), (LC7X, 1.0), (LC7Z, 0.40) }, warnings);
            CreateCombo("U6.2", new[] { (DEAD, 1.1), (LC1, 1.1), (LC2, 1.1), (LC3, 1.1), (LC7X, 1.0), (LC7Z, -0.40) }, warnings);
            CreateCombo("U6.3", new[] { (DEAD, 1.1), (LC1, 1.1), (LC2, 1.1), (LC3, 1.1), (LC7X, -1.0), (LC7Z, 0.40) }, warnings);
            CreateCombo("U6.4", new[] { (DEAD, 1.1), (LC1, 1.1), (LC2, 1.1), (LC3, 1.1), (LC7X, -1.0), (LC7Z, -0.40) }, warnings);
            CreateCombo("U6.5", new[] { (DEAD, 1.1), (LC1, 1.1), (LC2, -1.1), (LC3, 1.1), (LC7X, 1.0), (LC7Z, 0.40) }, warnings);
            CreateCombo("U6.6", new[] { (DEAD, 1.1), (LC1, 1.1), (LC2, -1.1), (LC3, 1.1), (LC7X, 1.0), (LC7Z, -0.40) }, warnings);
            CreateCombo("U6.7", new[] { (DEAD, 1.1), (LC1, 1.1), (LC2, -1.1), (LC3, 1.1), (LC7X, -1.0), (LC7Z, 0.40) }, warnings);
            CreateCombo("U6.8", new[] { (DEAD, 1.1), (LC1, 1.1), (LC2, -1.1), (LC3, 1.1), (LC7X, -1.0), (LC7Z, -0.40) }, warnings);

            // U7.1 - U7.8: 1.1*DEAD + 1.1*LC1 ± 1.1*LC2 + 1.1*LC3 ± 0.40*LC7X ± 1.0*LC7Z
            CreateCombo("U7.1", new[] { (DEAD, 1.1), (LC1, 1.1), (LC2, 1.1), (LC3, 1.1), (LC7X, 0.40), (LC7Z, 1.0) }, warnings);
            CreateCombo("U7.2", new[] { (DEAD, 1.1), (LC1, 1.1), (LC2, 1.1), (LC3, 1.1), (LC7X, 0.40), (LC7Z, -1.0) }, warnings);
            CreateCombo("U7.3", new[] { (DEAD, 1.1), (LC1, 1.1), (LC2, 1.1), (LC3, 1.1), (LC7X, -0.40), (LC7Z, 1.0) }, warnings);
            CreateCombo("U7.4", new[] { (DEAD, 1.1), (LC1, 1.1), (LC2, 1.1), (LC3, 1.1), (LC7X, -0.40), (LC7Z, -1.0) }, warnings);
            CreateCombo("U7.5", new[] { (DEAD, 1.1), (LC1, 1.1), (LC2, -1.1), (LC3, 1.1), (LC7X, 0.40), (LC7Z, 1.0) }, warnings);
            CreateCombo("U7.6", new[] { (DEAD, 1.1), (LC1, 1.1), (LC2, -1.1), (LC3, 1.1), (LC7X, 0.40), (LC7Z, -1.0) }, warnings);
            CreateCombo("U7.7", new[] { (DEAD, 1.1), (LC1, 1.1), (LC2, -1.1), (LC3, 1.1), (LC7X, -0.40), (LC7Z, 1.0) }, warnings);
            CreateCombo("U7.8", new[] { (DEAD, 1.1), (LC1, 1.1), (LC2, -1.1), (LC3, 1.1), (LC7X, -0.40), (LC7Z, -1.0) }, warnings);

            // U8.1 - U8.8: 1.1*DEAD + 1.1*LC1 ± 1.1*LC2 + 1.1*LC3 ± 1.0*LC7Y ± 0.40*LC7Z
            CreateCombo("U8.1", new[] { (DEAD, 1.1), (LC1, 1.1), (LC2, 1.1), (LC3, 1.1), (LC7Y, 1.0), (LC7Z, 0.40) }, warnings);
            CreateCombo("U8.2", new[] { (DEAD, 1.1), (LC1, 1.1), (LC2, 1.1), (LC3, 1.1), (LC7Y, 1.0), (LC7Z, -0.40) }, warnings);
            CreateCombo("U8.3", new[] { (DEAD, 1.1), (LC1, 1.1), (LC2, 1.1), (LC3, 1.1), (LC7Y, -1.0), (LC7Z, 0.40) }, warnings);
            CreateCombo("U8.4", new[] { (DEAD, 1.1), (LC1, 1.1), (LC2, 1.1), (LC3, 1.1), (LC7Y, -1.0), (LC7Z, -0.40) }, warnings);
            CreateCombo("U8.5", new[] { (DEAD, 1.1), (LC1, 1.1), (LC2, -1.1), (LC3, 1.1), (LC7Y, 1.0), (LC7Z, 0.40) }, warnings);
            CreateCombo("U8.6", new[] { (DEAD, 1.1), (LC1, 1.1), (LC2, -1.1), (LC3, 1.1), (LC7Y, 1.0), (LC7Z, -0.40) }, warnings);
            CreateCombo("U8.7", new[] { (DEAD, 1.1), (LC1, 1.1), (LC2, -1.1), (LC3, 1.1), (LC7Y, -1.0), (LC7Z, 0.40) }, warnings);
            CreateCombo("U8.8", new[] { (DEAD, 1.1), (LC1, 1.1), (LC2, -1.1), (LC3, 1.1), (LC7Y, -1.0), (LC7Z, -0.40) }, warnings);

            // U9.1 - U9.8: 1.1*DEAD + 1.1*LC1 ± 1.1*LC2 + 1.1*LC3 ± 0.40*LC7Y ± 1.0*LC7Z
            CreateCombo("U9.1", new[] { (DEAD, 1.1), (LC1, 1.1), (LC2, 1.1), (LC3, 1.1), (LC7Y, 0.40), (LC7Z, 1.0) }, warnings);
            CreateCombo("U9.2", new[] { (DEAD, 1.1), (LC1, 1.1), (LC2, 1.1), (LC3, 1.1), (LC7Y, 0.40), (LC7Z, -1.0) }, warnings);
            CreateCombo("U9.3", new[] { (DEAD, 1.1), (LC1, 1.1), (LC2, 1.1), (LC3, 1.1), (LC7Y, -0.40), (LC7Z, 1.0) }, warnings);
            CreateCombo("U9.4", new[] { (DEAD, 1.1), (LC1, 1.1), (LC2, 1.1), (LC3, 1.1), (LC7Y, -0.40), (LC7Z, -1.0) }, warnings);
            CreateCombo("U9.5", new[] { (DEAD, 1.1), (LC1, 1.1), (LC2, -1.1), (LC3, 1.1), (LC7Y, 0.40), (LC7Z, 1.0) }, warnings);
            CreateCombo("U9.6", new[] { (DEAD, 1.1), (LC1, 1.1), (LC2, -1.1), (LC3, 1.1), (LC7Y, 0.40), (LC7Z, -1.0) }, warnings);
            CreateCombo("U9.7", new[] { (DEAD, 1.1), (LC1, 1.1), (LC2, -1.1), (LC3, 1.1), (LC7Y, -0.40), (LC7Z, 1.0) }, warnings);
            CreateCombo("U9.8", new[] { (DEAD, 1.1), (LC1, 1.1), (LC2, -1.1), (LC3, 1.1), (LC7Y, -0.40), (LC7Z, -1.0) }, warnings);

            // U10: 1.1 DEAD + 1.1 LC1 + 1.0 LC8
            CreateCombo("U10", new[] { (DEAD, 1.1), (LC1, 1.1), (LC8, 1.0) }, warnings);

            // U11.1 - U11.2: 1.1*DEAD + 1.1*LC1 ± 1.1*LC2 + 1.1*LC3 + 1.4*LC9
            CreateCombo("U11.1", new[] { (DEAD, 1.1), (LC1, 1.1), (LC2, 1.1), (LC3, 1.1), (LC9, 1.4) }, warnings);
            CreateCombo("U11.2", new[] { (DEAD, 1.1), (LC1, 1.1), (LC2, -1.1), (LC3, 1.1), (LC9, 1.4) }, warnings);

            warnings.Add($"[INFO] Created 53 ultimate combinations (U1 - U11.2).");
        }

        /// <summary>
        /// Creates envelope combinations for service and ultimate.
        /// </summary>
        private void CreateEnvelopes(List<string> warnings)
        {
            // EnvS: Envelope of all service combinations
            var serviceCombos = new[]
            {
                "S1",
                "S2.1", "S2.2", "S2.3", "S2.4",
                "S3.1", "S3.2", "S3.3", "S3.4",
                "S4.1", "S4.2",
                "S5.1", "S5.2", "S5.3", "S5.4",
                "S6.1", "S6.2", "S6.3", "S6.4", "S6.5", "S6.6", "S6.7", "S6.8",
                "S7.1", "S7.2", "S7.3", "S7.4", "S7.5", "S7.6", "S7.7", "S7.8",
                "S8.1", "S8.2", "S8.3", "S8.4", "S8.5", "S8.6", "S8.7", "S8.8",
                "S9.1", "S9.2", "S9.3", "S9.4", "S9.5", "S9.6", "S9.7", "S9.8",
                "S10",
                "S11.1", "S11.2"
            };
            CreateEnvelopeCombo("EnvS", serviceCombos, warnings);

            // EnvU: Envelope of all ultimate combinations
            var ultimateCombos = new[]
            {
                "U1",
                "U2.1", "U2.2", "U2.3", "U2.4",
                "U3.1", "U3.2", "U3.3", "U3.4",
                "U4.1", "U4.2",
                "U5.1", "U5.2", "U5.3", "U5.4",
                "U6.1", "U6.2", "U6.3", "U6.4", "U6.5", "U6.6", "U6.7", "U6.8",
                "U7.1", "U7.2", "U7.3", "U7.4", "U7.5", "U7.6", "U7.7", "U7.8",
                "U8.1", "U8.2", "U8.3", "U8.4", "U8.5", "U8.6", "U8.7", "U8.8",
                "U9.1", "U9.2", "U9.3", "U9.4", "U9.5", "U9.6", "U9.7", "U9.8",
                "U10",
                "U11.1", "U11.2"
            };
            CreateEnvelopeCombo("EnvU", ultimateCombos, warnings);

            warnings.Add($"[INFO] Created 2 envelope combinations (EnvS, EnvU).");
        }

        /// <summary>
        /// Creates a linear combination (ComboType=0) with LoadCase members.
        /// </summary>
        private void CreateCombo(string name, (string LoadCase, double Factor)[] members, List<string> warnings)
        {
            try
            {
                // Create combo (type 0 = linear add)
                _facade.RespCombo_Add(name, 0);

                // Add members (LoadCases)
                foreach (var (loadCase, factor) in members)
                {
                    var caseType = eCNameType.LoadCase;
                    _facade.RespCombo_SetCaseList(name, ref caseType, loadCase, factor);
                }
            }
            catch (Exception ex)
            {
                warnings.Add($"Failed to create combo '{name}': {ex.Message}");
            }
        }

        /// <summary>
        /// Creates an envelope combination (ComboType=1) with LoadCombo members.
        /// </summary>
        private void CreateEnvelopeCombo(string name, string[] comboMembers, List<string> warnings)
        {
            try
            {
                // Create combo (type 1 = envelope)
                _facade.RespCombo_Add(name, 1);

                // Add members (other combos) - eCNameType for combo is 1 (LoadCombo)
                foreach (var comboName in comboMembers)
                {
                    // Cast to eCNameType.LoadCombo which is value 1
                    var caseType = (eCNameType)1;
                    _facade.RespCombo_SetCaseList(name, ref caseType, comboName, 1.0);
                }
            }
            catch (Exception ex)
            {
                warnings.Add($"Failed to create envelope '{name}': {ex.Message}");
            }
        }
    }
}
#else
using System.Collections.Generic;

namespace App.Infrastructure.Sap2000.Motores
{
    public sealed class LoadCombinationsEngine
    {
        public LoadCombinationsEngine(SapModelFacade facade) { }
        public void CreateAllCombinations(List<string> warnings) => throw new System.NotSupportedException("SAP2000 not available.");
  }
}
#endif
