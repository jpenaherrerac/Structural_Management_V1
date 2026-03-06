using System;
using System.Collections.Generic;
using App.Domain.Entities.Sources;

namespace App.Application.Interfaces
{
    public interface IDesignSourceRepository
    {
        DesignSource? GetById(Guid id);
        IEnumerable<DesignSource> GetByProjectId(Guid projectId);
        DesignSource? GetLatestByProjectId(Guid projectId);
        void Add(DesignSource source);
        void Update(DesignSource source);
        void Delete(Guid id);
    }
}
