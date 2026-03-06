using System;
using System.Collections.Generic;
using System.Linq;
using App.Application.Interfaces;
using App.Domain.Entities.Sources;

namespace App.Infrastructure.Repositories
{
    public class SeismicSourceRepository : ISeismicSourceRepository
    {
        private readonly Dictionary<Guid, SeismicSource> _store = new Dictionary<Guid, SeismicSource>();

        public SeismicSource? GetById(Guid id)
        {
            _store.TryGetValue(id, out var source);
            return source;
        }

        public IEnumerable<SeismicSource> GetByProjectId(Guid projectId) =>
            _store.Values.Where(s => s.ProjectId == projectId).ToList();

        public SeismicSource? GetLatestByProjectId(Guid projectId) =>
            _store.Values
                .Where(s => s.ProjectId == projectId)
                .OrderByDescending(s => s.HydratedAt)
                .FirstOrDefault();

        public void Add(SeismicSource source)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            _store[source.Id] = source;
        }

        public void Update(SeismicSource source)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (!_store.ContainsKey(source.Id))
                throw new InvalidOperationException($"SeismicSource {source.Id} not found.");
            _store[source.Id] = source;
        }

        public void Delete(Guid id) => _store.Remove(id);
    }
}
