using System;
using System.Collections.Generic;
using MRS.DocumentManagement.Connection.Tdms.Helpers;
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
                TDMSObject project = TdmsConnection.tdms.GetObjectByGUID(id);
                return mapper.ToDto(project);
            }
            catch
            {
                return null;
            }
        }

        public ProjectExternalDto Add(ProjectExternalDto projectDto)
        {
            throw new InvalidOperationException();
        }

        public ProjectDto Update(ProjectDto projectDto)
        {
            throw new InvalidOperationException();
        }

        public bool Remove(ProjectExternalDto projectDto)
        {
            throw new InvalidOperationException();
        }

        public ICollection<ProjectExternalDto> GetListOfProjects()
        {
            List<ProjectExternalDto> projects = new List<ProjectExternalDto>();
            try
            {
                var queryCom = TdmsConnection.tdms.CreateQuery();
                queryCom.AddCondition(TDMSQueryConditionType.tdmQueryConditionObjectDef, ObjectTypeID.OBJECT);

                foreach (TDMSObject obj in queryCom.Objects)
                {
                    projects.Add(new ProjectExternalDto()
                    {
                        ExternalID = obj.GUID,
                        Title = obj.Description,
                        // Items = 
                    });
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
