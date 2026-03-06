using System;
using System.Collections.Generic;
using System.Linq;
using App.Application.Interfaces;
using App.Domain.Entities.Project;

namespace App.Infrastructure.Repositories
{
    public class ProjectRepository : IProjectRepository
    {
        private readonly Dictionary<Guid, Project> _store = new Dictionary<Guid, Project>();

        public Project? GetById(Guid id)
        {
            _store.TryGetValue(id, out var project);
            return project;
        }

        public Project? GetByCode(string code)
        {
            if (string.IsNullOrWhiteSpace(code)) return null;
            return _store.Values.FirstOrDefault(p =>
                string.Equals(p.Code, code, StringComparison.OrdinalIgnoreCase));
        }

        public IEnumerable<Project> GetAll() => _store.Values.ToList();

        public IEnumerable<Project> GetActive() =>
            _store.Values.Where(p => p.IsActive).ToList();

        public void Add(Project project)
        {
            if (project == null) throw new ArgumentNullException(nameof(project));
            if (_store.ContainsKey(project.Id))
                throw new InvalidOperationException($"Project {project.Id} already exists.");
            _store[project.Id] = project;
        }

        public void Update(Project project)
        {
            if (project == null) throw new ArgumentNullException(nameof(project));
            if (!_store.ContainsKey(project.Id))
                throw new InvalidOperationException($"Project {project.Id} not found.");
            _store[project.Id] = project;
        }

        public void Delete(Guid id)
        {
            _store.Remove(id);
        }

        public bool Exists(Guid id) => _store.ContainsKey(id);

        public bool CodeExists(string code)
        {
            if (string.IsNullOrWhiteSpace(code)) return false;
            return _store.Values.Any(p =>
                string.Equals(p.Code, code, StringComparison.OrdinalIgnoreCase));
        }
    }
}
