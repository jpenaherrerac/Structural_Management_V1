using System.IO;
using App.Domain.Entities.Annexes;

namespace App.Application.Export
{
    public interface IAnnexExporter
    {
        void ExportBeamAnnex(BeamDesignAnnex annex, Stream outputStream);
        void ExportColumnAnnex(ColumnDesignAnnex annex, Stream outputStream);
        void ExportShearWallAnnex(ShearWallDesignAnnex annex, Stream outputStream);
    }
}
