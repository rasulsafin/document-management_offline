using System;
using System.Collections.Generic;
using System.Security;
using System.Threading.Tasks;
using Brio.Docs.Connections.Tdms.Mappers;
using Brio.Docs.Integration;
using Brio.Docs.Integration.Dtos;
using Brio.Docs.Integration.Interfaces;
using TDMS;

namespace Brio.Docs.Connections.Tdms
{
    public class TdmsProjectsSynchronizer : TdmsSynchronizer, ISynchronizer<ProjectExternalDto>
    {
        private readonly ProjectMapper mapper = new ProjectMapper();

        public TdmsProjectsSynchronizer(TDMSApplication tdms)
            : base(tdms) { }

        public Task<ProjectExternalDto> Add(ProjectExternalDto obj)
            => throw new SecurityException();

        public Task<ProjectExternalDto> Remove(ProjectExternalDto obj)
            => throw new SecurityException();

        public Task<ProjectExternalDto> Update(ProjectExternalDto projectDto)
        {
            TDMSObject project = tdms.GetObjectByGUID(projectDto.ExternalID);
            mapper.ToModel(projectDto, project);
            return Task.FromResult(mapper.ToDto(project));
        }

        public ProjectExternalDto GetById(string id)
        {
            try
            {
                TDMSObject project = tdms.GetObjectByGUID(id);
                return mapper.ToDto(project);
            }
            catch
            {
                return null;
            }
        }

        public Task<IReadOnlyCollection<string>> GetUpdatedIDs(DateTime date) => 
            Task.FromResult<IReadOnlyCollection<string>>(FindByDef(ObjectTypeID.OBJECT, date));

        public Task<IReadOnlyCollection<ProjectExternalDto>> Get(IReadOnlyCollection<string> ids)
        {
            var projects = new List<ProjectExternalDto>();
            foreach (var projectId in ids)
            {
                var project = GetById(projectId);
                if (project != null)
                    projects.Add(project);
            }

            return Task.FromResult<IReadOnlyCollection<ProjectExternalDto>>(projects);
        }
    }
}
