#if SAP2000_LEGACY
using System;
using System.Collections.Generic;
using App.Infrastructure.Sap2000;

namespace App.Infrastructure.Sap2000.Motores
{
    public sealed class MeshDefinitionEngine
    {
        private readonly SapModelFacade _facade;
        public MeshDefinitionEngine(SapModelFacade facade)
        {
            _facade = facade ?? throw new ArgumentNullException(nameof(facade));
        }

        // Nota: este motor depende de un tipo de contorno que actualmente vive en PROYECTOS_CSI.Core.Geometry.
        // Para mantener compilación sin esa dependencia, se deja la firma desacoplada.
        // Cuando se integre el módulo de geometría, se reemplaza `object` por el tipo real.
        public void CreateAreasAndMesh(List<object> contours, Dictionary<string, (int n1, int n2)> meshByPrefix, string defaultProperty)
        {
            if (contours == null || contours.Count == 0) return;
            // TODO: Implementar cuando el tipo de contorno real esté disponible en este proyecto.
            throw new NotImplementedException("MeshDefinitionEngine requiere el módulo de geometría (contours) para implementarse.");
        }
    }
}
#endif
