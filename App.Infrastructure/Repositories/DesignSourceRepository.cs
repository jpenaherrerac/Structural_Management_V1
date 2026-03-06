using System;
using System.Collections.Generic;
using System.Linq;
using App.Application.Interfaces;
using App.Domain.Entities.Sources;

namespace App.Infrastructure.Repositories
{
    public class DesignSourceRepository : IDesignSourceRepository
    {
        private readonly Dictionary<Guid, DesignSource> _store = new Dictionary<Guid, DesignSource>();

        public DesignSource? GetById(Guid id)
        {
            _store.TryGetValue(id, out var source);
            return source;
        }

        public IEnumerable<DesignSource> GetByProjectId(Guid projectId) =>
            _store.Values.Where(s => s.ProjectId == projectId).ToList();

        public DesignSource? GetLatestByProjectId(Guid projectId) =>
            _store.Values
                .Where(s => s.ProjectId == projectId)
                .OrderByDescending(s => s.HydratedAt)
                .FirstOrDefault();

        public void Add(DesignSource source)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            _store[source.Id] = source;
        }

        public void Update(DesignSource source)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (!_store.ContainsKey(source.Id))
                throw new InvalidOperationException($"DesignSource {source.Id} not found.");
            _store[source.Id] = source;
        }

        public void Delete(Guid id) => _store.Remove(id);
    }
}
