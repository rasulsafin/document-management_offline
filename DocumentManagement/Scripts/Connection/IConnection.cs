﻿using System.Collections.Generic;
using System.Threading.Tasks;

namespace MRS.Bim.DocumentManagement
{
    public interface IConnection
    {
        ConnectionInfo Info { get; set; }

        void PickProject(Project project);

        Task<bool> Add(DMAction[] actions);
        Task<bool> Add(DMFile file);
        Task<bool> Add(DMFile[] files);
        Task<Issue> Add(Issue issue);
        Task<bool> Add(Issue[] issues);
        Task<bool> Add(Job job);
        Task<bool> Add(Job[] jobs);

        /// <summary>
        /// Cancellation of the last action
        /// </summary>
        void Cancel();

        Task<bool> Change(DMFile file);
        Task<bool> Change(DMFile[] files);
        Task<bool> Change(Issue issue);
        Task<bool> Change(Issue[] issues);
        Task<bool> Change(Job job);
        Task<bool> Change(Job[] jobs);

        Task<(bool, string)> Connect(dynamic param);
        void Disconnect();

        Task<bool> Upload(string id,DMFile file);
        Task<bool> Upload(string id,DMFile[] files);
        /// <summary>
        /// Upload new data from DB.
        /// </summary>
        /// <returns></returns>
        Task<bool> Upload();


        Task<bool> Download(string id, DMFile file);
        Task<bool> Download(string id, DMFile[] files);
        /// <summary>
        /// Download data to DB.
        /// </summary>
        /// <returns></returns>
        Task<bool> Download();


        Task<(bool, DMFile)> GetDMFile(string id, string fileId); //?
        /// <summary>
        /// Get files from any object
        /// </summary>
        /// <param name="id">Id of selected object (job/issue/project/etc)</param>
        /// <param name="files"></param>
        /// <returns></returns>
        Task<(bool, DMFile[])> GetDMFiles(string id, DMFileType type);
        /// <summary>
        /// Get issue by id
        /// </summary>
        /// <param name="id">Needed issue id</param>
        /// <param name="issue"></param>
        /// <returns></returns>
        Task<(bool, Issue)> GetIssue(string id);
        /// <summary>
        /// Get list of issues from project
        /// </summary>
        /// <param name="id">Needed project id</param>
        /// <param name="issues"></param>
        /// <returns></returns>
        Task<(bool, Issue[])> GetIssues(string id);
        /// <summary>
        /// Get job by id
        /// </summary>
        /// <param name="id">Job id</param>
        /// <param name="job"></param>
        /// <returns></returns>
        Task<(bool, Job)> GetJob(string id);
        /// <summary>
        /// Get list of jobs from project
        /// </summary>
        /// <param name="id">Project's id</param>
        /// <param name="jobs"></param>
        /// <returns></returns>
        Task<(bool, Job[])> GetJobs(string id);
        /// <summary>
        /// Get project by its id
        /// </summary>
        /// <param name="id">Project's id</param>
        /// <param name="project"></param>
        /// <returns></returns>
        Task<(bool, Project)> GetProject(string id);
        /// <summary>
        /// Get list of all projects from the system
        /// </summary>
        /// <param name="id"></param>
        /// <param name="projects"></param>
        /// <returns></returns>
        Task<(bool, Project[])> GetProjects(string id);
        Task<(bool, DMAccount)> GetAccountInfo();
        Task<(bool, Dictionary<string, DMItem[]>)> GetEnums();

        Task<bool> Remove(DMAction lastAction);
        Task<bool> Remove(DMAction[] actions);
        Task<bool> Remove(DMFile file);
        Task<bool> Remove(DMFile[] files);
        Task<bool> Remove(Issue issue);
        Task<bool> Remove(Issue[] issues);
        Task<bool> Remove(Job job);
        Task<bool> Remove(Job[] jobs);
        Task<bool> Remove(Project project);
    }
}