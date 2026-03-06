using System;
using System.Windows.Forms;
using App.Application.Interfaces;
using App.Application.UseCases;
using App.Infrastructure.Repositories;
using App.SAP2000.Adapters;

namespace App.WinForms
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Composition root: wire up dependencies
            IProjectRepository projectRepo = new ProjectRepository();
            ISeismicSourceRepository seismicRepo = new SeismicSourceRepository();
            IDesignSourceRepository designRepo = new DesignSourceRepository();
            ISapAdapter sapAdapter = new SapAdapter();

            var createProjectUseCase = new CreateProjectUseCase(projectRepo);
            var hydrateSeismicUseCase = new HydrateSeismicSourceUseCase(sapAdapter, seismicRepo, projectRepo);
            var hydrateDesignUseCase = new HydrateDesignSourceUseCase(sapAdapter, designRepo, projectRepo);

            Application.Run(new MainForm(sapAdapter, createProjectUseCase, hydrateSeismicUseCase, hydrateDesignUseCase));
        }
    }
}
