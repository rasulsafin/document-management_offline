using LiteDB;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Linq;
using MRS.Bim.Tools;

#pragma warning disable 1998

namespace MRS.Bim.DocumentManagement
{
    public class OfflineConnection : IConnection
    {
        private readonly string dbPath;
        private IProgressing progress;

        private Project currentProject = new Project();

        private ConnectionInfo info;
        public ConnectionInfo Info { get => info; set => info = value; }

        public OfflineConnection(string dbPath, IProgressing progress)
        {
            this.dbPath = dbPath;
            this.progress = progress;
            BsonMapper.Global.EmptyStringToNull = false;
            BsonMapper.Global.MaxDepth = 100;
            BsonMapper.Global.TrimWhitespace = false;
            BsonMapper.Global.SerializeNullValues = true;
            BsonMapper.Global.IncludeFields = true;
        }

        
        public void PickProject(Project project)
            => currentProject = project;

        public async Task<(bool, string)> Connect(dynamic param)
        {
            var answer = await Task.Run(() => File.Exists(dbPath));

            return (answer, answer ? null : "Database doesn't exist");
        }

        public void Disconnect() { }

        public void Cancel() { }


        #region ADD
        /// <summary>
        /// TODO: Check actions???
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        public async Task<bool> Add(DMAction action)
            => await Add(new[] {action});

        /// <summary>
        /// TODO: Check actions???
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        public async Task<bool> Add(DMAction[] actions)
        {
            await Task.Run(() =>
            {
                using (var db = new LiteDatabase(dbPath))
                {
                    progress.AddLayer(2);
                    var actionsTable = db.GetCollection<DMAction>("Actions");
                    progress.Set();
                    actionsTable.Insert(actions);
                    progress.Set();
                }
            });

            return true;
        }

        /// <summary>
        /// TODO: Add file?
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public async Task<bool> Add(DMFile file)
            => await Add(new[] {file});

        /// <summary>
        /// TODO: Add files??
        /// </summary>
        /// <param name="files"></param>
        /// <returns></returns>
        public async Task<bool> Add(DMFile[] files)
        {
            await Task.Run(() =>
            {
                using (var db = new LiteDatabase(dbPath))
                {
                    var filesTable = db.FileStorage;
                    progress.AddLayer(files.Length);

                    foreach (var file in files)
                    {
                        filesTable.Upload(file.ID, file.Path);
                        progress.Set();
                    }
                }
            });

            return true;
        }

        public async Task<Issue> Add(Issue issue)
            => await Add(new[] {issue}) ? issue : default;

        public async Task<bool> Add(Issue[] issues)
        {
            await Task.Run(() =>
            {
                using (var db = new LiteDatabase(dbPath))
                {
                    progress.AddLayer(4);
                    var projectsTable = db.GetCollection<Project>("Projects");
                    progress.Set();
                    Project project = projectsTable.FindById(currentProject.ID);
                    progress.Set();
                    project.Issues.AddRange(issues);
                    progress.Set();
                    projectsTable.Update(project);
                    progress.Set();
                }
            });

            return true;
        }

        public async Task<bool> Add(Job job)
            => await Add(new[] {job});

        public async Task<bool> Add(Job[] jobs)
        {
            await Task.Run(() =>
            {
                using (var db = new LiteDatabase(dbPath))
                {
                    progress.AddLayer(4);
                    var projectsTable = db.GetCollection<Project>("Projects");
                    progress.Set();
                    Project project = projectsTable.FindById(currentProject.ID);
                    progress.Set();
                    project.Jobs.AddRange(jobs);
                    progress.Set();
                    projectsTable.Update(project);
                    progress.Set();
                }
            });

            return true;
        }
        #endregion

        #region CHANGE
        /// <summary>
        /// TODO: Change file???
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        public async Task<bool> Change(DMFile file)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// TODO: Change files???
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        public async Task<bool> Change(DMFile[] files)
        {
            throw new NotImplementedException();
        }

        public async Task<bool> Change(Issue issue)
            => await Change(new[] {issue});

        public async Task<bool> Change(Issue[] issues)
        {
            await Task.Run(() =>
            {
                using (var db = new LiteDatabase(dbPath))
                {
                    progress.AddLayer(5);
                    var projectsTable = db.GetCollection<Project>("Projects");
                    progress.Set();
                    Project project = projectsTable.FindById(currentProject.ID);
                    progress.Set();
                    progress.AddLayer(issues.Length);

                    foreach (var issue in issues)
                    {
                        project.Issues.Remove(project.Issues.Find(i => i.ID == issue.ID));
                        progress.Set();
                    }

                    progress.Set();
                    project.Issues.AddRange(issues);
                    progress.Set();

                    projectsTable.Update(project);
                    progress.Set();
                }
            });

            return true;
        }

        public async Task<bool> Change(Job job)
            => await Change(new[] {job});

        public async Task<bool> Change(Job[] jobs)
        {
            await Task.Run(() =>
            {
                using (var db = new LiteDatabase(dbPath))
                {
                    progress.AddLayer(5);
                    var projectsTable = db.GetCollection<Project>("Projects");
                    progress.Set();
                    Project project = projectsTable.FindById(currentProject.ID);
                    progress.Set();
                    progress.AddLayer(jobs.Length);

                    foreach (var job in jobs)
                    {
                        project.Jobs.Remove(project.Jobs.Find(j => j.ID == job.ID));
                        progress.Set();
                    }
                    
                    progress.Set();
                    project.Jobs.AddRange(jobs);                       
                    progress.Set();

                    projectsTable.Update(project);
                    progress.Set();
                }
            });

            return true;
        }
        #endregion

        #region DOWNLOAD
        /// <summary>
        /// TODO: Download??
        /// </summary>
        /// <param name="issue"></param>
        /// <returns></returns>
        public async Task<bool> Download(string id, DMFile file)
        {
            throw new NotImplementedException();
        }

        public Task<bool> Download(string id, DMFile[] files)
        {
            throw new NotImplementedException();
        }

        public Task<bool> Download()
        {
            throw new NotImplementedException();
        }
        #endregion

        #region GET
        /// <summary>
        /// TODO: GetDMFile??
        /// </summary>
        /// <param name="issue"></param>
        /// <returns></returns>
        public async Task<(bool, DMFile)> GetDMFile(string id, string fileId)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// TODO: GetDMFiles[]??
        /// </summary>
        /// <param name="issue"></param>
        /// <returns></returns>
        public async Task<(bool, DMFile[])> GetDMFiles(string id, DMFileType type)
        {
            // throw new NotImplementedException();
            return (false, default);
        }

        public async Task<(bool, Issue)> GetIssue(string id)
        {
            Issue issue = new Issue();

            await Task.Run(() =>
            {
                using (var db = new LiteDatabase(dbPath))
                {
                    progress.AddLayer(2);
                    var projectTable = db.GetCollection<Project>("Projects");
                    progress.Set();
                    Project project = projectTable.FindById(currentProject.ID);
                    issue = project.Issues.Find(i => i.ID == id);
                    progress.Set();
                }
            });

            return (issue != null, issue);
        }

        public async Task<(bool, Issue[])> GetIssues(string id)
        {
            Issue[] issues = { };
            await Task.Run(() =>
            {
                using (var db = new LiteDatabase(dbPath))
                {
                    progress.AddLayer(2);
                    var projectTable = db.GetCollection<Project>("Projects");
                    progress.Set();
                    Project project = projectTable.FindById(currentProject.ID);
                    issues = currentProject.ID == id
                            ? project.Issues.ToArray()
                            : project.Issues.Where(i => i.DMParent.ID == id).ToArray();
                    progress.Set();
                }
            });

            return (issues != null, issues);
        }

        public async Task<(bool, Job)> GetJob(string id)
        {
            Job job = new Job();

            await Task.Run(() =>
            {
                using (var db = new LiteDatabase(dbPath))
                {
                    progress.AddLayer(2);
                    var projectTable = db.GetCollection<Project>("Projects");
                    progress.Set();
                    Project pj = projectTable.FindById(currentProject.ID);
                    job = pj.Jobs.Find(j => j.ID == id);
                    progress.Set();
                }
            });

            return (job != null, job);
        }

        public async Task<(bool, Job[])> GetJobs(string id)
        {
            Job[] jobs = { };

            await Task.Run(() =>
            {
                using (var db = new LiteDatabase(dbPath))
                {
                    progress.AddLayer(2);
                    var projectTable = db.GetCollection<Project>("Projects");
                    progress.Set();
                    jobs = projectTable.Include(p => p.Jobs).FindById(id).Jobs.ToArray();
                    progress.Set();
                }
            });

            return (jobs != null, jobs);
        }

        public async Task<(bool, Project)> GetProject(string id)
        {
            await Task.Run(() =>
            {
                using (var db = new LiteDatabase(dbPath))
                {
                    progress.AddLayer(2);
                    var projectTable = db.GetCollection<Project>("Projects");
                    progress.Set();
                    currentProject = projectTable.FindById(id);
                    progress.Set();
                }
            });

            return (currentProject != null, currentProject);
        }

        public async Task<(bool, Project[])> GetProjects(string id)
        {
            Project[] projects = { };
            await Task.Run(() =>
            {
                using (var db = new LiteDatabase(dbPath))
                {
                    var projectTable = db.GetCollection<Project>("Projects");
                    projects = projectTable.FindAll().ToArray();
                }
            });

            return (projects != null, projects);
        }

        public Task<(bool, DMAccount)> GetAccountInfo()
        {
            throw new NotImplementedException();
        }

        public async Task<(bool, Dictionary<string, DMItem[]>)> GetEnums()
        {
            Dictionary<string, DMItem[]> result = new Dictionary<string, DMItem[]>();
            await Task.Run(() =>
            {
                using (var db = new LiteDatabase(dbPath))
                {
                    var infoTable = db.GetCollection<(string, DMItem[])>("Enums");
                    result = infoTable.FindAll().ToDictionary(item=>item.Item1, item=> item.Item2);                  

                    if (Info == null)
                        Info = new ConnectionInfo();

                    Info.Enums = result;
                }
            });

            return (result != null, result);
        }

        #endregion

        #region REMOVE
        /// <summary>
        /// TODO: Remove last action
        /// </summary>
        /// <param name="issue"></param>
        /// <returns></returns>
        public Task<bool> Remove(DMAction lastAction)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// TODO: Remove actions []
        /// </summary>
        /// <param name="issue"></param>
        /// <returns></returns>
        public Task<bool> Remove(DMAction[] actions)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// TODO: Remove file
        /// </summary>
        /// <param name="issue"></param>
        /// <returns></returns>
        public async Task<bool> Remove(DMFile file)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// TODO: Remove Files[]
        /// </summary>
        /// <param name="issue"></param>
        /// <returns></returns>
        public async Task<bool> Remove(DMFile[] files)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// TODO: RemoveIssue
        /// </summary>
        /// <param name="issue"></param>
        /// <returns></returns>
        public async Task<bool> Remove(Issue issue) 
            => await Remove(new[] { issue });

        /// <summary>
        /// TODO: RemoveJob
        /// </summary>
        /// <param name="issue"></param>
        /// <returns></returns>
        public async Task<bool> Remove(Issue[] issues)
        {
            await Task.Run(() =>
            {
                using (var db = new LiteDatabase(dbPath))
                {
                    progress.AddLayer(3);
                    var projectsTable = db.GetCollection<Project>("Projects");
                    progress.Set();
                    Project project = projectsTable.FindById(currentProject.ID);
                    progress.Set();
                    progress.AddLayer(issues.Length);

                    foreach (var issue in issues)
                    {
                        project.Issues.Remove(project.Issues.Find(i => i.ID == issue.ID));
                        progress.Set();
                    }

                    projectsTable.Update(project);
                    progress.Set();
                }
            });

            return true;
        }

        /// <summary>
        /// TODO: RemoveJob
        /// </summary>
        /// <param name="job"></param>
        /// <returns></returns>
        public async Task<bool> Remove(Job job)
             => await Remove(new[] { job });

        /// <summary>
        /// TODO: RemoveJobs
        /// </summary>
        /// <param name="jobs"></param>
        /// <returns></returns>
        public async Task<bool> Remove(Job[] jobs)
        {
            await Task.Run(() =>
            {
                using (var db = new LiteDatabase(dbPath))
                {
                    progress.AddLayer(3);
                    var projectsTable = db.GetCollection<Project>("Projects");
                    progress.Set();
                    Project project = projectsTable.FindById(currentProject.ID);
                    progress.Set();
                    progress.AddLayer(jobs.Length);

                    foreach (var job in jobs)
                    {
                        project.Jobs.Remove(project.Jobs.Find(j => j.ID == job.ID));
                        progress.Set();
                    }

                    projectsTable.Update(project);
                    progress.Set();
                }
            });

            return true;
        }

        public async Task<bool> Remove(Project project)
        {
            return await Task.Run(() =>
            {
                using (var db = new LiteDatabase(dbPath))
                {
                    progress.AddLayer(2);
                    var projectsTable = db.GetCollection<Project>("Projects");
                    progress.Set();
                    var result = projectsTable.Delete(project.ID);
                    progress.Set();
                    return result;
                }
            });
        }
        #endregion

        #region UPLOAD
        /// <summary>
        /// TODO: Upload file?
        /// </summary>
        /// <param name="id"></param>
        /// <param name="file"></param>
        /// <returns></returns>
        public async Task<bool> Upload(string id, DMFile file) => await Task.FromResult(false);
        /// <summary>
        /// TODO: Upload files?
        /// </summary>
        /// <param name="id"></param>
        /// <param name="file"></param>
        /// <returns></returns>
        public Task<bool> Upload(string id, DMFile[] files)
        {
            throw new NotImplementedException();
        }

        public async Task<bool> Upload() => await Task.FromResult(false);
        #endregion
    }
}