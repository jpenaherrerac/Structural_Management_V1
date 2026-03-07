#if SAP2000_LEGACY
using System;
using App.Infrastructure.Sap2000;

namespace App.Infrastructure.Sap2000.Motores
{
    /// <summary>
    /// Define propiedades de frame (placeholder).
    /// </summary>
    public sealed class FramePropertyEngine
    {
        private readonly SapModelFacade _facade;

        public FramePropertyEngine(SapModelFacade facade)
        {
            _facade = facade ?? throw new ArgumentNullException(nameof(facade));
        }

        public void EnsureFrameProperty(string name, string material, double A, double I2, double I3, double J)
        {
            // TODO: Implementar según especificación.
        }
    }
}
#endif
