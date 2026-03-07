#if SAP2000_AVAILABLE
using System;
using System.Collections.Generic;
using SAP2000v1;

namespace App.Infrastructure.Sap2000.Motores
{
    /// <summary>
    /// Assigns uniform distributed loads to area objects in SAP2000.
    /// </summary>
    public sealed class UniformAreaLoadsEngine
    {
        private readonly SapModelFacade _facade;

        /// <summary>
        /// Load pattern name for LC8 (LIVE load) - must match LoadCombinationsEngine.LC8
        /// </summary>
        public const string LC8_PatternName = "LIVE";

        public UniformAreaLoadsEngine(SapModelFacade facade)
        {
            _facade = facade ?? throw new ArgumentNullException(nameof(facade));
        }

        /// <summary>
        /// Assigns a uniform load to all areas with a specific shell property.
        /// Uses SelectObj.PropertyArea to select areas, then AreaObj.SetLoadUniform to assign the load.
        /// </summary>
        /// <param name="propertyName">The shell property name (e.g., "LOSA", "PLATE", "TRENCH").</param>
        /// <param name="loadPattern">The load pattern name (e.g., "LIVE").</param>
        /// <param name="loadValue">The uniform load value in model units [F/L²]. Positive values act downward for gravity direction.</param>
        /// <param name="direction">
        /// Load direction:
        /// 6 = Global Z direction,
        /// 10 = Gravity direction (positive value = downward),
        /// 11 = Projected Gravity direction
        /// </param>
        /// <param name="warnings">List to collect warnings.</param>
        /// <returns>True if successful, false otherwise.</returns>
        public bool AssignUniformLoadByProperty(
            string propertyName,
            string loadPattern,
            double loadValue,
            int direction,
            List<string> warnings)
        {
            warnings = warnings ?? new List<string>();

            if (string.IsNullOrWhiteSpace(propertyName))
            {
                warnings.Add("UniformAreaLoadsEngine: propertyName is empty.");
                return false;
            }

            if (string.IsNullOrWhiteSpace(loadPattern))
            {
                warnings.Add("UniformAreaLoadsEngine: loadPattern is empty.");
                return false;
            }

            try
            {
                // Selection is global in SAP2000: always clear before/after.
                _facade.SelectObj_ClearSelection();
                try
                {
                    // Select all areas with the specified property
                    _facade.SelectObj_PropertyArea(propertyName, deselect: false);

                    // Apply uniform load to selected objects
                    // Name is ignored when itemType = SelectedObjects (eItemType =2)
                    _facade.AreaObj_SetLoadUniform(
                        name: string.Empty,
                        loadPat: loadPattern,
                        value: loadValue,
                        dir: direction,
                        replace: true,
                        cSys: "Global",
                        itemType: eItemType.SelectedObjects);
                }
                finally
                {
                    // Critical: prevent leaking selection into subsequent operations
                    try { _facade.SelectObj_ClearSelection(); } catch { }
                }

                return true;
            }
            catch (Exception ex)
            {
                warnings.Add($"UniformAreaLoadsEngine: Failed to assign load to property '{propertyName}': {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Assigns LC8 (LIVE load) as a uniform distributed load to all shell areas.
        /// Default: 2000 N/m² applied in gravity direction to LOSA, PLATE, and TRENCH areas.
        /// 
        /// Note: The load pattern "LIVE" is used (same as LoadCombinationsEngine.LC8).
        /// SAP2000 creates "LIVE" pattern by default, but we ensure it exists.
        /// </summary>
        /// <param name="loadValueNm2">Load value in N/m². Default is 2000 N/m².</param>
        /// <param name="warnings">List to collect warnings.</param>
        /// <returns>Number of property types successfully assigned.</returns>
        public int AssignLC8UniformLoad(double loadValueNm2, List<string> warnings)
        {
            warnings = warnings ?? new List<string>();

            const int gravityDirection = 10; // Gravity direction (positive = downward in -Z)

            // Ensure LIVE pattern exists (SAP2000 usually has it by default, but create if not)
            try
            {
                // Type 3 = eLoadPatternType.Live
                _facade.LoadPatterns_Add(LC8_PatternName, (eLoadPatternType)3, 0.0, true);
            }
            catch
            {
                // Pattern may already exist - that's OK
            }

            // For gravity direction (dir=10), positive values act downward (in -Z direction)
            // So we use positive value directly
            double loadValue = loadValueNm2;

            int successCount = 0;

            // Assign to all three property types
            var properties = new[]
            {
                MaterialAndPropertiesEngine.PropertyLosa,
                MaterialAndPropertiesEngine.PropertyPlate,
                MaterialAndPropertiesEngine.PropertyTrench
            };

            foreach (var prop in properties)
            {
                try
                {
                    if (AssignUniformLoadByProperty(prop, LC8_PatternName, loadValue, gravityDirection, warnings))
                    {
                        successCount++;
                    }
                }
                catch (Exception ex)
                {
                    warnings.Add($"UniformAreaLoadsEngine: Exception assigning to '{prop}': {ex.Message}");
                }
            }

            if (successCount > 0)
            {
                warnings.Add($"[INFO] LC8 ({LC8_PatternName}) uniform load ({loadValueNm2} N/m²) assigned to {successCount} property type(s).");
            }

            return successCount;
        }
    }
}
#else
using System.Collections.Generic;

namespace App.Infrastructure.Sap2000.Motores
{
    public sealed class UniformAreaLoadsEngine
    {
        public const string LC8_PatternName = "LIVE";

     public UniformAreaLoadsEngine(SapModelFacade facade) { }

    public bool AssignUniformLoadByProperty(
 string propertyName,
            string loadPattern,
     double loadValue,
        int direction,
            List<string> warnings)
       => throw new System.NotSupportedException("SAP2000 not available.");

    public int AssignLC8UniformLoad(double loadValueNm2, List<string> warnings)
            => throw new System.NotSupportedException("SAP2000 not available.");
    }
}
#endif
