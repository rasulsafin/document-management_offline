using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using MRS.Bim.DocumentManagement;
using MRS.Bim.Tools;
using Newtonsoft.Json;
// ReSharper disable StaticMemberInGenericType

namespace MRS.Bim.DocumentManagement.Utilities
{
    public class CloudBimOnline<T> : AOnlineConnection where T : ICloudManager, new()
    {
        private static readonly string APP_PATH = "Brio MRS";
        private static readonly string ISSUE_FILE_NAME = "issues.json";
        private static readonly string[] IFC_EXTENSIONS = {".ifc", ".ifczip"};

        protected string loggerPath = "CloudBimOnline-Log.txt";
        private readonly ICloudManager manager;
        private readonly IProgressing progress;
        private Project currentProject;
        private DateTime lastModified = new DateTime();
        private string lastProject;

        private Lazy<string> issueFilePath =
                new Lazy<string>(() => Path.Combine(PathUtility.Instance.TempDirectory, ISSUE_FILE_NAME));


        protected CloudBimOnline(string dbPath, IProgressing progress) : base(dbPath)
        {
            manager = new T();
            this.progress = progress;
        }

        public override void PickProject(Project project)
            => currentProject = project;

        public override Task<bool> Add(DMFile file)
            => throw new NotImplementedException();

        public override Task<bool> Add(DMFile[] files)
            => throw new NotImplementedException();

        public override async Task<Issue> Add(Issue issue)
            => (await ChangeIssues(new[] {issue}, false, true)).FirstOrDefault();

        public override async Task<bool> Add(Issue[] issues)
            => await ChangeIssues(issues, false, true) != null;

        public override Task<bool> Add(Job job)
            => throw new NotImplementedException();

        public override Task<bool> Add(Job[] jobs)
            => throw new NotImplementedException();

        public override void Cancel()
        {
            manager.Cancel();
            progress.Cancel();
        }

        public override Task<bool> Change(DMFile file)
            => throw new NotImplementedException();

        public override Task<bool> Change(DMFile[] files)
            => throw new NotImplementedException();

        public override async Task<bool> Change(Issue issue)
            => await ChangeIssues(new[] {issue}, true, true) != null;

        public override async Task<bool> Change(Issue[] issues)
            => await ChangeIssues(issues, true, true) != null;

        public override Task<bool> Change(Job job)
            => throw new NotImplementedException();

        public override Task<bool> Change(Job[] jobs)
            => throw new NotImplementedException();

        public override async Task<(bool, string)> Connect(dynamic parameters)
        {
            var progressLayers = progress.LayersCount;
            try
            {
                return (await manager.Connect(), null);
            }
            catch (Exception ex)
            {
                ApiError(ex, progressLayers);
                return (false, null);
            }
        }

        public override void Disconnect()
            => manager.Disconnect();

        public override async Task<bool> Upload(string id, DMFile file)
            => await Upload(id, new[] {file});

        public override async Task<bool> Upload(string id, DMFile[] files)
        {
            var progressLayers = progress.LayersCount;
            var result = true;
            progress.AddLayer(files.Length);

            foreach (var item in files)
            {
                try
                {
                    result &= await manager.Upload(item.Path, id,
                            new CloudItem {ID = item.ID, Name = item.Name});
                }
                catch (Exception ex)
                {
                    ApiError(ex, progressLayers);
                    result = false;
                }

                progress.Set();
            }

            return result;
        }

        public override async Task<bool> Download(string id, DMFile file)
            => await Download(id, new[] {file});

        public override async Task<bool> Download(string id, DMFile[] files)
        {
            var progressLayers = progress.LayersCount;
            var result = true;
            progress.AddLayer(files.Length);

            foreach (var item in files)
            {
                try
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(item.Path) ?? "");
                    result &= await manager.Download(new CloudItem
                    {
                        ID = item.ID,
                        Name = item.Name
                    }, item.Path);
                }
                catch (Exception ex)
                {
                    ApiError(ex, progressLayers);
                    result = false;
                }

                progress.Set();
            }

            return result;
        }

        public override Task<bool> Remove(DMFile file)
            => throw new NotImplementedException();

        public override Task<bool> Remove(DMFile[] files)
            => throw new NotImplementedException();

        public override async Task<bool> Remove(Issue issue)
            => await ChangeIssues(new[] {issue}, true, false) != null;

        public override async Task<bool> Remove(Issue[] issues)
            => await ChangeIssues(issues, true, false) != null;

        public override Task<bool> Remove(Job job)
            => throw new NotImplementedException();

        public override Task<bool> Remove(Job[] jobs)
            => throw new NotImplementedException();

        public override Task<bool> Remove(Project project)
            => throw new NotImplementedException();

        public override async Task<(bool, DMFile)> GetDMFile(string id, string fileId)
        {
            var items = (await manager.GetItems(id, needFolders: false)).FirstOrDefault(x => x.ID == fileId);
            var dmFile = items == null
                    ? null
                    : new DMFile {ID = items.ID, Name = items.Name, Path = PathUtility.Instance.ProjectDirectory};
            return (dmFile != null, dmFile);
        }

        public override async Task<(bool, DMFile[])> GetDMFiles(string id, DMFileType type)
        {
            var items = (await manager.GetItems(id, needFolders: false)).Select(x => new DMFile
            {
                ID = x.ID,
                Name = x.Name,
                Path = Path.Combine(PathUtility.Instance.ProjectDirectory, x.Name)
            });

            // ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault
            switch (type)
            {
                case DMFileType.Ifc:
                    return (true,
                            items.Where(x => IFC_EXTENSIONS.Contains(Path.GetExtension(x.Name).ToLower())).ToArray());
                default:
                    return (true, items.ToArray());
            }
        }

        public override async Task<(bool, Issue)> GetIssue(string id)
        {
            var issue = (await GetIssues(id, issueFilePath.Value))?.FirstOrDefault(x => x.ID == id);
            return (issue != null, issue);
        }

        public override async Task<(bool, Issue[])> GetIssues(string id)
        {
            var issues = (await GetIssues(id, issueFilePath.Value))?.Where(x => x.DMParent.ID == id);
            return (issues != null, issues?.ToArray());
        }

        public override Task<(bool, Job)> GetJob(string id)
            => Task.FromResult((false, new Job()));

        public override Task<(bool, Job[])> GetJobs(string id)
            => Task.FromResult((false, new Job[] { }));

        public override async Task<(bool, Project)> GetProject(string id)
        {
            var progressLayers = progress.LayersCount;

            try
            {
                progress.AddLayer(3);
                var mainFolder = await GetMainFolderID();
                var projectName = (await manager.GetItems(mainFolder, needFolders: true)).FirstOrDefault(x => x.ID == id)
                        ?.Name;

                if (string.IsNullOrEmpty(projectName))
                {
                    progress.CloseLayer();
                    return (false, null);
                }

                progress.Set();
                var items = await manager.GetItems(id, needFolders: false);
                progress.Set();
                var project = new Project
                {
                    ID = id,
                    Issues = await GetIssues(id, issueFilePath.Value, items),
                    Name = projectName,
                    Actions = new List<DMAction>(),
                    Attachments = new List<DMFile>(),
                    Blueprints = new List<Blueprint>(),
                    Documents = new List<DMFile>(),
                    Ifcs = new List<DMFile>(),
                    Jobs = new List<Job>()
                };
                progress.Set();
                return (true, project);
            }
            catch (Exception e)
            {
                ApiError(e, progressLayers);
                return (false, null);
            }
        }

        public override async Task<(bool, Project[])> GetProjects(string id)
        {
            var progressLayers = progress.LayersCount;

            try
            {
                progress.AddLayer(3);
                var mainFolder = await GetMainFolderID();
                progress.Set();
                var projectsFolders = await manager.GetItems(mainFolder, needFolders: true);
                progress.Set();
                var result = new Project[projectsFolders.Count];
                progress.AddLayer(projectsFolders.Count);

                for (var i = 0; i < projectsFolders.Count; i++)
                {
                    result[i] = new Project
                    {
                        ID = projectsFolders[i].ID,
                        Name = projectsFolders[i].Name
                    };
                    progress.Set();
                }

                progress.Set();
                return (true, result);
            }
            catch (Exception ex)
            {
                ApiError(ex, progressLayers);
                return (false, new Project[0]);
            }
        }

        public override Task<(bool, DMAccount)> GetAccountInfo()
            => throw new NotImplementedException();
        
        public override Task<(bool, Dictionary<string, DMItem[]>)> GetEnums()
        {
            if (Info == null)
                Info = new ConnectionInfo();
            Info.Enums = new Dictionary<string, DMItem[]>();

            return Task.FromResult((false, new Dictionary<string, DMItem[]>()));
        }

        private void ApiError(Exception ex, int progressLayers, [CallerMemberName] string method = null)
        {
            while (progress.LayersCount > progressLayers)
                progress.CloseLayer();
            var message = $"{DateTime.Now.ToLongTimeString()} - {method} failed with a {ex.GetType()}: {ex.Message}";
            File.AppendAllLines(loggerPath, new List<string> {message});
            if (ex is OperationCanceledException)
                throw ex;
        }

        private async Task<Issue[]> ChangeIssues(Issue[] issuesToChange, bool needRemove, bool needAdd)
        {
            var progressLayers = progress.LayersCount;

            try
            {
                progress.AddLayer(3);
                var path = issueFilePath.Value;
                var issues = await GetIssues((string) currentProject.ID, path);
                progress.Set();

                foreach (var issue in issuesToChange)
                {
                    var attached = new List<DMFile>();
                    var found = issues.Find(x => x.ID == issue.ID);

                    if (found != null)
                    {
                        attached = found.Attachments;
                        if (needRemove)
                            issues.Remove(found);
                    }

                    if (needAdd)
                    {
                        await AttachFiles(issue,
                                issue.Attachments
                                        .Where(newFile => attached.All(existing => newFile.Name != existing.Name))
                                        .ToArray());
                        
                        issue.ID = issue.Name + issue.GetHashCode();
                        issues.Add(issue);
                        progress.Set();
                    }
                }

                await PostIssues(path, currentProject.ID, issues);
                progress.Set();
                return issuesToChange;
            }
            catch (Exception e)
            {
                ApiError(e, progressLayers);
                return null;
            }
        }

        private async Task<List<Issue>> GetIssues(string projectId,
                                                  string path,
                                                  IEnumerable<CloudItem> directory = null)
        {
            var progressLayers = progress.LayersCount;

            try
            {
                var issues = new List<Issue>();
                progress.AddLayer(2);
                var file = (directory ?? await manager.GetItems(projectId, ISSUE_FILE_NAME, false))
                        .FirstOrDefault(x => x.Name == ISSUE_FILE_NAME);
                progress.Set();

                if (file != null)
                {
                    progress.AddLayer(2);
                    var modifiedTime = file.ModifiedTime ?? new DateTime();

                    if (!File.Exists(path) || new FileInfo(path).Length != file.Size ||
                        (modifiedTime - lastModified).TotalSeconds > 10 || lastProject != projectId)
                    {
                        lastProject = projectId;
                        lastModified = modifiedTime;
                        await Download(null, new DMFile {ID = file.ID, Name = file.Name, Path = path});
                    }

                    progress.Set();

                    if (File.Exists(path))
                    {
                        var downloaded = JsonConvert.DeserializeObject<List<Issue>>(File.ReadAllText(path));
                        issues.AddRange(downloaded);
                    }

                    progress.Set();
                }

                progress.Set();
                return issues;
            }
            catch (Exception e)
            {
                ApiError(e, progressLayers);
                return null;
            }
        }

        private async Task AttachFiles(Issue issue, DMFile[] newAttachments)
        {
            var unuploaded = newAttachments.Where(x => File.Exists(x.Path)).ToArray();

            // Try to upload
            var done = new Dictionary<DMFile, bool>();
            progress.AddLayer(unuploaded.Length);

            foreach (var x in unuploaded)
            {
                done.Add(x, await Upload(currentProject.ID, x));
                progress.Set();
            }

            issue.Attachments = issue.Attachments
                    .Where(x => !newAttachments.Contains(x) || done.ContainsKey(x) && done[x]).ToList();
        }

        private async Task PostIssues(string path, string projectID, List<Issue> issues)
        {
            File.WriteAllText(path, JsonConvert.SerializeObject(issues));
            lastModified = DateTime.Now;
            await Upload(projectID, new DMFile 
                    {
                        Name = ISSUE_FILE_NAME,
                        Path = path
                    }
            );
        }

        private async Task<string> GetMainFolderID()
        {
            progress.AddLayer(2);
            var mainFolder = (await manager.GetItems(partOfName: APP_PATH, needFolders: true)).FirstOrDefault();
            progress.Set();
            if (mainFolder == null)
                mainFolder = await manager.CreateAppDirectory(APP_PATH);
            progress.Set();
            return mainFolder.ID;
        }
    }
}