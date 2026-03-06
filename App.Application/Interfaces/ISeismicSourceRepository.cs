using System;
using System.Collections.Generic;
using App.Domain.Entities.Sources;

namespace App.Application.Interfaces
{
    public interface ISeismicSourceRepository
    {
        SeismicSource? GetById(Guid id);
        IEnumerable<SeismicSource> GetByProjectId(Guid projectId);
        SeismicSource? GetLatestByProjectId(Guid projectId);
        void Add(SeismicSource source);
        void Update(SeismicSource source);
        void Delete(Guid id);
    }
}
