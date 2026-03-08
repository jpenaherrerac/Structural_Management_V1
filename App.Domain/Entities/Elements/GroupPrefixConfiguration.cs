using System.Collections.Generic;

namespace App.Domain.Entities.Elements
{
    /// <summary>
    /// Stores the editable prefix names used to discover SAP2000 groups
    /// for each structural element type (Vigas, Columnas, Muros, Losas).
    /// </summary>
    public class GroupPrefixConfiguration
    {
        public string BeamPrefix { get; set; } = "Vigas";
        public string ColumnPrefix { get; set; } = "Columnas";
        public string ShearWallPrefix { get; set; } = "Muros";
        public string SlabPrefix { get; set; } = "Losas";

        /// <summary>
        /// Returns the group names expected for a given prefix and floor count.
        /// E.g. prefix="Vigas", floors=3 → ["Vigas_P1", "Vigas_P2", "Vigas_P3"]
        /// </summary>
        public static IReadOnlyList<string> BuildGroupNames(string prefix, int floorCount)
        {
            var names = new List<string>(floorCount);
            for (int i = 1; i <= floorCount; i++)
                names.Add($"{prefix}_P{i}");
            return names;
        }
    }
}
