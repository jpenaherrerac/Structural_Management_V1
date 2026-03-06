using System;
using App.Application.Interfaces;
using App.Domain.Entities.Project;

namespace App.Application.UseCases
{
    public class CreateProjectRequest
    {
        public string Name { get; set; }
        public string Code { get; set; }
        public string Description { get; set; }
        public string Location { get; set; }
        public string Client { get; set; }
        public string DesignCode { get; set; }
        public string StructuralSystem { get; set; }
        public int NumberOfStoreys { get; set; }
        public double TotalHeightMeters { get; set; }
        public string BuildingUse { get; set; }
    }

    public class CreateProjectResponse
    {
        public bool Success { get; set; }
        public Guid ProjectId { get; set; }
        public string ErrorMessage { get; set; }

        public static CreateProjectResponse Ok(Guid id) =>
            new CreateProjectResponse { Success = true, ProjectId = id };

        public static CreateProjectResponse Fail(string message) =>
            new CreateProjectResponse { Success = false, ErrorMessage = message };
    }

    public class CreateProjectUseCase
    {
        private readonly IProjectRepository _repository;

        public CreateProjectUseCase(IProjectRepository repository)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        }

        public CreateProjectResponse Execute(CreateProjectRequest request)
        {
            if (request == null)
                return CreateProjectResponse.Fail("Request cannot be null.");

            if (string.IsNullOrWhiteSpace(request.Name))
                return CreateProjectResponse.Fail("Project name is required.");

            if (string.IsNullOrWhiteSpace(request.Code))
                return CreateProjectResponse.Fail("Project code is required.");

            if (_repository.CodeExists(request.Code))
                return CreateProjectResponse.Fail($"Project code '{request.Code}' already exists.");

            var metadata = new ProjectMetadata(
                request.Location ?? "Unspecified",
                request.Client ?? "Unspecified",
                request.DesignCode ?? "ACI 318 / NEC")
            {
                StructuralSystem = request.StructuralSystem,
                NumberOfStoreys = request.NumberOfStoreys,
                TotalHeightMeters = request.TotalHeightMeters,
                BuildingUse = request.BuildingUse
            };

            var project = new Project(request.Name, request.Code, request.Description, metadata);
            _repository.Add(project);

            return CreateProjectResponse.Ok(project.Id);
        }
    }
}
