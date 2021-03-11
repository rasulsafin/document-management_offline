using System;
using System.Collections.Generic;
using System.Security;
using System.Threading.Tasks;
using MRS.DocumentManagement.Connection.Tdms.Mappers;
using MRS.DocumentManagement.Interface;
using MRS.DocumentManagement.Interface.Dtos;
using TDMS;

namespace MRS.DocumentManagement.Connection.Tdms
{
    public class TdmsProjectsSynchronizer : ISynchronizer<ProjectExternalDto>
    {
        private readonly ProjectMapper mapper = new ProjectMapper();

        public Task<ProjectExternalDto> Add(ProjectExternalDto obj)
            => throw new SecurityException();

        public Task<ProjectExternalDto> Remove(ProjectExternalDto obj)
            => throw new SecurityException();

        public Task<ProjectExternalDto> Update(ProjectExternalDto projectDto)
        {
            TDMSObject project = TdmsConnection.connection.GetObjectByGUID(projectDto.ExternalID);
            mapper.ToModel(projectDto, project);
            return Task.FromResult(mapper.ToDto(project));
        }

        public ICollection<ProjectExternalDto> GetListOfProjects()
        {
            List<ProjectExternalDto> projects = new List<ProjectExternalDto>();
            try
            {
                var queryCom = TdmsConnection.connection.CreateQuery();
                queryCom.AddCondition(TDMSQueryConditionType.tdmQueryConditionObjectDef, ObjectTypeID.OBJECT);

                foreach (TDMSObject obj in queryCom.Objects)
                {
                    projects.Add(mapper.ToDto(obj));
                }

                return projects;
            }
            catch
            {
                return null;
            }
        }

        public ProjectExternalDto Get(string id)
        {
            try
            {
                TDMSObject project = TdmsConnection.connection.GetObjectByGUID(id);
                return mapper.ToDto(project);
            }
            catch
            {
                return null;
            }
        }
    }
}
