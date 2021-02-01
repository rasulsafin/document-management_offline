using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using LiteDB;

namespace MRS.Bim.DocumentManagement
{
    public abstract class AOnlineConnection : IConnection
    {
        private readonly string dbPath;

        private ConnectionInfo info;
        public ConnectionInfo Info { get => info; set => info = value; }

        protected AOnlineConnection(string dbPath)
            => this.dbPath = dbPath;

        #region DB Methods

        public Task<bool> Add(DMAction[] actions) => Task.FromResult(false);

        public async Task<bool> Download()
        {
            // TODO: Save connection info
            try
            {
                var isSuccess = true;
                var (hasProjects, projectsList) = await GetProjects("");

                if (!hasProjects)
                    return false;

                if (File.Exists(dbPath))
                    File.Delete(dbPath);

                var (hasEnums, enumList) = await GetEnums();

                using (var db = new LiteDatabase(dbPath))
                {
                    db.Mapper.IncludeFields = true;
                    var projects = db.GetCollection<Project>("Projects");
                    foreach (var project in projectsList)
                    {
                        var (hasProject, current) = ((bool, Project))await GetProject(project.ID);
                        if (hasProject)
                            projects.Insert(current);
                        else
                            isSuccess = false;
                    }

                    if (hasEnums)
                    {
                        var enums = db.GetCollection<(string, DMItem[])>("Enums");
                        foreach(var item in enumList)
                        {
                            enums.Insert((item.Key, item.Value));
                        }
                    }
                }
                return isSuccess;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> Upload()
        {
            var isSuccess = false;

            if (File.Exists(dbPath))
            {
                using (var db = new LiteDatabase(dbPath))
                {
                    isSuccess = true;
                    var actions = db.GetCollection<DMAction>("Actions");
                    foreach (var action in actions.FindAll())
                    {
                        try
                        {
                            if (action.ProjectId != ConnectionHandler.Instance.CurrentProject.ID)
                                continue;

                            var m = ConnectionHandler.Instance.GetType()
                                    .GetMethod(action.MethodName, action.Parameters.Select(x => x.GetType()).ToArray());

                            if (m == null)
                            {
                                isSuccess = false;
                                continue;
                            }

                            dynamic awaitable = Task.Run(async () => await (dynamic)m.Invoke(ConnectionHandler.Instance, action.Parameters));
                            await awaitable;
                            var result = awaitable.GetAwaiter().GetResult();
                            isSuccess = result is bool ? isSuccess & result : isSuccess;
                            actions.Delete(action.ID);
                        }
                        catch
                        {
                            isSuccess = false;
                        }
                    }
                }
            }

            return isSuccess;
        }

        public async Task<bool> Remove(DMAction lastAction)
            => await Remove(new[] {lastAction});

        public Task<bool> Remove(DMAction[] actions)
        {
            var isSuccess = true;

            using (var db = new LiteDatabase(dbPath))
            {
                var actionsCollection = db.GetCollection<DMAction>("Actions");
                foreach (var action in actions)
                    isSuccess &= actionsCollection.Delete(action.ID);
            }

            return Task.FromResult(isSuccess);
        }

        #endregion

        #region Abstract Methods


        public abstract void PickProject(Project project);

        public abstract Task<bool> Add(DMFile file);

        public abstract Task<bool> Add(DMFile[] files);

        public abstract Task<Issue> Add(Issue issue);

        public abstract Task<bool> Add(Issue[] issues);

        public abstract Task<bool> Add(Job job);

        public abstract Task<bool> Add(Job[] jobs);

        public abstract void Cancel();

        public abstract Task<bool> Change(DMFile file);

        public abstract Task<bool> Change(DMFile[] files);

        public abstract Task<bool> Change(Issue issue);

        public abstract Task<bool> Change(Issue[] issues);

        public abstract Task<bool> Change(Job job);

        public abstract Task<bool> Change(Job[] jobs);

        public abstract Task<(bool, string)> Connect(dynamic parameters);

        public abstract void Disconnect();

        public abstract Task<bool> Upload(string id, DMFile file);

        public abstract Task<bool> Upload(string id, DMFile[] files);

        public abstract Task<bool> Download(string id, DMFile[] files);

        public abstract Task<(bool, DMFile)> GetDMFile(string id, string fileId);

        public abstract Task<(bool, DMFile[])> GetDMFiles(string id, DMFileType type);

        public abstract Task<(bool, Issue)> GetIssue(string id);

        public abstract Task<(bool, Issue[])> GetIssues(string id);

        public abstract Task<(bool, Job)> GetJob(string id);

        public abstract Task<(bool, Job[])> GetJobs(string id);

        public abstract Task<(bool, Project)> GetProject(string id);

        public abstract Task<(bool, Project[])> GetProjects(string id);
        
        public abstract Task<(bool, DMAccount)> GetAccountInfo();
        
        public abstract Task<(bool, Dictionary<string, DMItem[]>)> GetEnums();

        public abstract Task<bool> Download(string id, DMFile file);


        public abstract Task<bool> Remove(DMFile file);

        public abstract Task<bool> Remove(DMFile[] files);

        public abstract Task<bool> Remove(Issue issue);

        public abstract Task<bool> Remove(Issue[] issues);

        public abstract Task<bool> Remove(Job job);

        public abstract Task<bool> Remove(Job[] jobs);

        public abstract Task<bool> Remove(Project project);

        #endregion
    }
}