using System;
using System.Collections.Generic;
using MRS.DocumentManagement.Connection.Tdms.Mappers;
using MRS.DocumentManagement.Interface.Dtos;
using TDMS;

namespace MRS.DocumentManagement.Connection.Tdms
{
    public class ProjectService
    {
        private readonly ProjectMapper mapper = new ProjectMapper();

        public ProjectExternalDto Get(string id)
        {
            try
            {
                TDMSObject project = TdmsConnection.TDMS.GetObjectByGUID(id);
                return mapper.ToDto(project);
            }
            catch
            {
                return null;
            }
        }

        public ProjectExternalDto Update(ProjectExternalDto projectDto)
        {
            try
            {
                TDMSObject project = TdmsConnection.TDMS.GetObjectByGUID(projectDto.ExternalID);
                mapper.ToModel(projectDto, project);
                return mapper.ToDto(project);
            }
            catch
            {
                return null;
            }
        }

        public ICollection<ProjectExternalDto> GetListOfProjects()
        {
            List<ProjectExternalDto> projects = new List<ProjectExternalDto>();
            try
            {
                var queryCom = TdmsConnection.TDMS.CreateQuery();
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
    }
}
