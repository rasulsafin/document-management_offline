using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using UnityEngine;
using LiteDB;
using MRS.Components.ProgressBar;

namespace Forge.Legacy
{
    public class BIM360Online : AOnlineConnection
    {
        private const string projectFilesFolderName = "Project Files";
        private Bim360.Auth auth = Bim360.Auth.Instance;
        private List<Bim360.Project> projects;
        private ProgressHandler progress = new ProgressHandler();
        public List<Bim360.File.Format> defaultFormats = new List<Bim360.File.Format> {Bim360.File.Format.ifc, Bim360.File.Format.pdf};

        public BIM360Online()
        {
            DbPath = @"BIM360Data.db";
        }

        public override async Task<bool> Connect()
        {
            try
            {
                await auth.SignInAsync();
                return true;
            }
            catch (Exception ex)
            {
                ApiError(ex);
                return false;
            }
        }

        public override void Disconnect()
        {
            auth.ClearUserInfo();
        }

        public override async Task<bool> CreateDB()
        {
            if (System.IO.File.Exists(DbPath))
                System.IO.File.Delete(DbPath);

            try
            {
                progress.AddLayer(2);
                var projects = await GetProjects();
                progress++;
                using (var db = new LiteDatabase(DbPath))
                {
                    var col = db.GetCollection<Project>("Projects");
                    progress.AddLayer(projects.Count);
                    foreach (var project in projects)
                    {
                        var found = col.FindById(project.ID);
                        if (found != null)
                            col.Update(project);
                        else
                            col.Insert(project);
                        progress++;
                    }
                    //test db commands
                    //var col2 = db.GetCollection<mm.Command>("Actions");
                    //var iss = projects.Find(x => x.Name == "Sample Project").Issues.Last();
                    //iss.Description = "test db features [1]";
                    //col2.Insert(new mm.Command() {Parameters = new object[] {iss}, Name = nameof(ChangeIssue)});
                }
                progress++;
                return true;
            }
            catch (Exception ex)
            {
                ApiError(ex);
                return false;
            }
        }

        public override async Task<bool> SynchronizeDB()
        {
            progress.AddLayer(2);
            await UpdateProjectsInfo();
            progress++;
            using (var db = new LiteDatabase(DbPath))
            {
                var col = db.GetCollection<DBIssueItem>("IssueActions");
                progress.AddLayer(col.Count());
                foreach (var item in col.FindAll())
                {
                    try
                    {
                        MethodInfo m = this.GetType().GetMethod(item.Name);
                        object[] issues = { item.Issue };
                        m.Invoke(this, issues);
                        col.Delete(item.ID);
                    }

                    catch (Exception ex)
                    {
                        ApiError(ex, false);
                        //return false;
                    }
                    progress++;
                }
            }
            progress++;
            return true;
        }

        public override async Task<List<Project>> GetProjects()
        {
            try
            {
                progress.AddLayer(2);
                if (projects == null)
                    await UpdateProjectsInfo();
                progress++;
                var result = new List<Project>();
                progress.AddLayer(projects.Count);
                foreach (var project in projects)
                {
                    result.Add(await GetProjectInfo(project));
                    progress++;
                }
                progress++;
                return result;
            }
            catch (Exception ex)
            {
                ApiError(ex);
                return new List<Project>();
            }
        }

        public override async Task<Project> UpdateProject(Project project)
        {
            try
            {
                progress.AddLayer(2);
                if (projects == null)
                    await UpdateProjectsInfo();
                progress++;
                var info = await GetProjectInfo(GetProject(project.ID));
                progress++;
                return info;
            }
            catch (Exception ex)
            {
                ApiError(ex);
                return project;
            }
        }

        public override async Task<bool> UploadItem(List<Item> items)
        {
            try
            {
                progress.AddLayer(items.Count);
                foreach (var item in items)
                {
                    progress.AddLayer(5);
                    var fileName = item.Path.Split('/').Last();
                    var folder = await GetFolder(GetProject(item.Parent_id), projectFilesFolderName);
                    progress++;
                    var storage = await Bim360.Storage.CreateAsync(folder, fileName);
                    progress++;

                    using (var fileStream = System.IO.File.OpenRead(item.Path))
                    {
                        await storage.UploadAsync(fileStream);
                    }
                    progress++;

                    var file = (await folder.files.GetAllAsync()).FirstOrDefault(x => x.name == fileName);
                    progress++;

                    if (file != null)
                        await file.SetVersionAsync(storage);
                    else
                        await folder.files.CreateFileAsync(fileName, storage);
                    progress++;
                    progress++;
                }
                return true;
            }
            catch (Exception ex)
            {
                ApiError(ex);
                return false;
            }
        }

        public override async Task<bool> DownloadItem(List<Item> items)
        {
            try
            {
                progress.AddLayer(items.Count);
                foreach (var item in items)
                {
                    progress.AddLayer(2);
                    var file = (await GetProject(item.Parent_id).rootFolder.files.SearchAsync(null)).FirstOrDefault(x => x.id == item.ID);
                    progress++;

                    var downloading = await file.GetFileStreamAsync();
                    System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(item.Path));
                    using (var fileStream = System.IO.File.Create(item.Path))
                    {
                        downloading.Seek(0, System.IO.SeekOrigin.Begin);
                        await downloading.CopyToAsync(fileStream);
                    }
                    progress++;
                    progress++;
                }
                return true;
            }
            catch (Exception ex)
            {
                ApiError(ex);
                return false;
            }
        }

        public override async Task<bool> AddIssue(Issue issue)
        {
            try
            {
                // Add
                progress.AddLayer(3);
                var project = GetProject(issue.Parent_id);
                var sendingIssue = (Bim360.Issue)issue;
                var typeDefault = await project.issues.GetDefaultTypeAsync();
                progress++;

                sendingIssue.attributes.ng_issue_type_id = typeDefault.id;
                sendingIssue.attributes.ng_issue_subtype_id = typeDefault.subtypes[0].id;
                await project.issues.AddAsync(sendingIssue);
                progress++;

                await AttachFiles(project, issue);
                progress++;

                return true;
            }
            catch (Exception ex)
            {
                ApiError(ex);
                return false;
            }
        }

        public override async Task<bool> ChangeIssue(Issue issue)
        {
            try
            {
                var project = GetProject(issue.Parent_id);
                var sendingIssue = (Bim360.Issue)issue;
                progress.AddLayer(2);
                await project.issues.ChangeAsync(sendingIssue);
                progress++;
                await AttachFiles(project, issue);
                progress++;
                return true;
            }
            catch (Exception ex)
            {
                ApiError(ex);
                return false;
            }
        }

        public override async Task<bool> DeleteIssue(Issue issue)
        {
            try
            {
                progress.AddLayer(2);
                if (issue.FinalStatus)
                    await ChangeIssue(new Issue
                    {
                        ID = issue.ID,
                        FinalStatus = true,
                        Parent_id = issue.Parent_id
                    });
                progress++;
                issue.FinalStatus = true;
                var changeIssue = await ChangeIssue(issue);
                progress++;
                return changeIssue;
            }
            catch (Exception ex)
            {
                ApiError(ex);
                return false;
            }
        }

        public async Task<List<Issue>> GetIssues(string projectId) => await GetIssues(GetProject(projectId));

        public override void Reset()
        {
        }

        public override void Cancel()
        {
            auth.Cancel();
            progress.Cancel();
        }

        #region private methods

        private async Task UpdateProjectsInfo()
        {
            projects = new List<Bim360.Project>();
            // Hub
            progress.AddLayer(2);
            var hubs = await Bim360.Hub.GetHubsAsync();
            progress++;
            progress.AddLayer(hubs.Count);
            foreach (var hub in hubs)
            {
                //Projects
                projects.AddRange(await hub.GetProjectsAsync());
                progress++;
            }
            progress++;
            if (!hubs.Any())
                throw new Exception("no access for BIM360");
        }

        private async Task<Project> GetProjectInfo(Bim360.Project project)
        {
            //Files
            progress.AddLayer(5);
            var folderPlans = await GetFolder(project, "Plans");
            progress++;
            var folderProjectFiles = await GetFolder(project, projectFilesFolderName);
            progress++;
            var searchResult = await folderPlans.files.SearchAsync(defaultFormats);
            progress++;
            searchResult.AddRange(await folderProjectFiles.files.SearchAsync(defaultFormats));
            progress++;

            // Result item
            var info = new Project
            {
                Issues = await GetIssues(project),
                ID = project.id,
                Name = project.name,
                Items = searchResult.Select(x => (Item)x).ToList()
            };
            progress++;
            return info;
        }

        private Bim360.Project GetProject(string id) => projects.Find(x => x.id == id);

        private async Task<List<Issue>> GetIssues(Bim360.Project project)
        {
            try
            {
                progress.AddLayer(1);
                var issues = (await project.issues.AllAsync()).Where(x => x.attributes.status != "void")
                    .Select(x => (Issue) x).ToList();
                issues.ForEach(x => x.Parent_id = project.id);
                progress++;
                return issues;
            }
            catch (Exception ex)
            {
                ApiError(ex);
                return new List<Issue>();
            }
        }

        private async Task<Bim360.Folder> GetFolder(Bim360.Project project, params string[] path)
        {
            var folder = project.rootFolder;
            progress.AddLayer(path.Length);
            foreach (var item in path)
            {
                folder = (await folder.GetAllFoldersAsync()).FirstOrDefault(x => x.name == item);
                if (folder == null)
                    return null;
                progress++;
            }
            return folder;
        }

        private const string loggerPath = "BIM360-Log.txt";
        private void ApiError(Exception ex, bool progressReset = true, [CallerMemberName]string method = null)
        {
            if (progressReset)
                progress.Clear();
            var message = $"{DateTime.Now.ToLongTimeString()} - {method} failed with a {ex.GetType()}: {ex.Message}";
            Debug.Log(message);
            System.IO.File.AppendAllLines(loggerPath,
                new List<string>
                    {message});
        }


        private const int waitToAttach = 3000;
        private async Task AttachFiles(Bim360.Project project, Issue issue)
        {
            progress.AddLayer(5);
            // Find issue
            var sendedIssue = (Bim360.Issue) (await GetIssues(project)).Find(x =>
                x.Description == issue.Description && x.Start_date == issue.Start_date &&
                (string.IsNullOrEmpty(issue.ID) || issue.ID == x.ID));
            progress++;

            // New attachments
            var unuploaded = issue.Attachments.Where(x =>
                    string.IsNullOrEmpty(x.ID) &&
                    System.IO.File.Exists(x.Path) &&
                    sendedIssue.attachments.All(existAtt => existAtt.name != x.Name))
                .ToList();
            unuploaded.ForEach(x => x.Parent_id = project.id);

            // Try to upload
            var done = new Dictionary<Item, bool>();
            progress.AddLayer(unuploaded.Count);
            foreach (var x in unuploaded)
            {
                done.Add(x, await UploadItem(new List<Item> {x}));
                progress++;
            }
            progress++;

            await Task.Delay(waitToAttach);
            progress++;

            // Attach uploaded files
            var fileNames = unuploaded.Where(x => done[x]).Select(x => x.Path.Split('/').Last());
            var files =
                (await project.rootFolder.files.SearchAsync(null)).Where(x => fileNames.Contains(x.name));
            progress++;

            var attachments = files.Select(x => new Bim360.Attachment
            {
                issueId = sendedIssue.id, itemId = x.id,
                type = "dm", name = x.name
            });
            progress.AddLayer(attachments.Count());
            foreach (var attachment in attachments)
            {
                await sendedIssue.AttachFile(project, attachment);
                progress++;
            }
            progress++;
        }

        #endregion
    }
}