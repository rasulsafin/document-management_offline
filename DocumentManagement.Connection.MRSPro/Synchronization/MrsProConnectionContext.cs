using System;
using MRS.DocumentManagement.Connection.MrsPro.Services;
using MRS.DocumentManagement.Interface;
using MRS.DocumentManagement.Interface.Dtos;

namespace MRS.DocumentManagement.Connection.MrsPro
{
    public class MrsProConnectionContext : AConnectionContext
    {
        private ProjectService projectService;

        public MrsProConnectionContext(ProjectService projectService)
        {
            this.projectService = projectService;
        }

        protected override ISynchronizer<ObjectiveExternalDto> CreateObjectivesSynchronizer()
        {
            return new MrsProObjectivesSynchronizer();
        }

        protected override ISynchronizer<ProjectExternalDto> CreateProjectsSynchronizer()
        {
            return new MrsProProjectsSynchronizer(projectService);
        }
    }
}
