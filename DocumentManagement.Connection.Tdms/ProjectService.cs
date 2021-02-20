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

        public ProjectDto Get(string id)
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

        public ProjectDto Add(ProjectDto projectDto)
        {
            try
            {
                TDMSObject parent = TdmsConnection.tdms.Root;

                TDMSObject project = parent.Objects.Create(ObjectTypeID.MAINOBJECT);
                parent.Update();

                var projectToCreate = mapper.ToModel(projectDto, project);

                project.Update();
                parent.Update();

                return mapper.ToDto(project);
            }
            catch
            {
                return null;
            }
        }

        public ProjectDto Update(ProjectDto projectDto)
        {
            throw new NotImplementedException();
        }

        public bool Remove(ProjectDto projectDto)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<ProjectDto> GetListOfProjects(string id)
        {
            throw new NotImplementedException();

            //List<Project> projects = new List<Project>();
            //try
            //{
            //    var queryCom = tdms.CreateQuery();
            //    queryCom.AddCondition(TDMSQueryConditionType.tdmQueryConditionObjectDef, ObjectTypeID.Object);

            //    ProgressBar.Progress.AddLayer(queryCom.Objects.Count);

            //    foreach (TDMSObject obj in queryCom.Objects)
            //    {
            //        token.ThrowIfCancellationRequested();

            //        projects.Add(new Project()
            //        {
            //            ID = obj.GUID,
            //            Name = obj.Description
            //        });

            //        ProgressBar.Progress++;
            //    }

            //    return projects.ToArray();
            //}
            //catch (Exception e)
            //{
            //    // ProgressBar.Progress.Cancel();
            //    //ProgressBar.Progress.Clear();
            //    Logger.WriteLog(e.Message + " : " + e.StackTrace, LoggingLevel.Error);
            //    return null;
            //}
        }
    }
}
