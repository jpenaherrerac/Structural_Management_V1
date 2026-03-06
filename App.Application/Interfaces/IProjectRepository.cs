using System;
using System.Collections.Generic;
using App.Domain.Entities.Project;

namespace App.Application.Interfaces
{
    public interface IProjectRepository
    {
        Project? GetById(Guid id);
        Project? GetByCode(string code);
        IEnumerable<Project> GetAll();
        IEnumerable<Project> GetActive();
        void Add(Project project);
        void Update(Project project);
        void Delete(Guid id);
        bool Exists(Guid id);
        bool CodeExists(string code);
    }
}
