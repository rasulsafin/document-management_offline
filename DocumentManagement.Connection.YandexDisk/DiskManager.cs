#define TEST

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using MRS.DocumentManagement.Connection.Synchronizator;
using MRS.DocumentManagement.Connection.YandexDisk;
using MRS.DocumentManagement.Interface.Dtos;
using Newtonsoft.Json;

namespace MRS.DocumentManagement.Connection
{
    public class DiskManager
    {
        // public static CoolLogger logger = new CoolLogger(typeof(DiskManager).Name);

        private string accessToken;
        private IDiskController controller;
        private bool projectsDirCreate;
        private bool transactionsDirCreate;
        private bool usersDirCreate;
        private List<int> projects = new List<int>();
        private List<int> objectives = new List<int>();
        private List<int> items = new List<int>();
        private List<string> tables = new List<string>();
        private bool isInit;

        public DiskManager(string accessToken)
        {
            this.accessToken = accessToken;
            controller = new YandexDiskController(accessToken);
        }

        public DiskManager(IDiskController controller)
        {
            this.controller = controller;
        }

        public async Task<bool> Push<T>(T @object, string id)
        {
            try
            {
                if (!isInit) Initialize();
                string tableName = typeof(T).Name;
                await CheckTableDir(tableName);

                string path = PathManager.GetRecordFile(tableName, id);
                string json = JsonConvert.SerializeObject(@object);
                return await controller.SetContentAsync(path, json);
            }
            catch(Exception ex)
            {
            }

            return false;
        }

        public async Task<T> Pull<T>(string id)
        {
            try
            {
                if (!isInit) await Initialize();
                string tableName = typeof(T).Name;
                if (await CheckTableDir(tableName))
                {
                    string path = PathManager.GetRecordFile(tableName, id);
                    string json = await controller.GetContentAsync(path);
                    T @object = JsonConvert.DeserializeObject<T>(json);
                    return @object;
                }
            }
            catch (FileNotFoundException)
            {
            }

            return default;
        }

        private async Task<bool> CheckTableDir(string tableName)
        {
            bool res = tables.Any(x => x == tableName);
            if (res)return true;
            IEnumerable<DiskElement> list = await controller.GetListAsync(PathManager.GetTablesDir());
            foreach (DiskElement element in list)
            {
                if (element.IsDirectory)
                    tables.Add(element.DisplayName);
                if (element.DisplayName == tableName)
                    res = true;
            }

            if (!res)
                await controller.CreateDirAsync(PathManager.GetTablesDir(), tableName);
            return res;
        }

        private async Task Initialize()
        {
            IEnumerable<DiskElement> list = await controller.GetListAsync();
            if (!list.Any(x => x.IsDirectory && x.DisplayName == PathManager.APP_DIR))
            {
                await controller.CreateDirAsync("/", PathManager.APP_DIR);
            }

            list = await controller.GetListAsync(PathManager.GetAppDir());
            if (!list.Any(x => x.IsDirectory && x.DisplayName == PathManager.TABLE_DIR))
            {
                await controller.CreateDirAsync("/", PathManager.GetTablesDir());
            }

            isInit = true;
        }

        // #region Инициализация

        ///// <summary>
        ///// Возвращает есть ли папка проектов
        ///// если нет Создает папку проектов и возвращает false.
        ///// </summary>
        ///// <returns><placeholder>A <see cref="Task"/> representing the asynchronous operation.</placeholder></returns>
        // private async Task<bool> CheckProjectsDir()
        // {
        //    bool result = true;
        //    if (!projectsDirCreate)
        //    {
        //        IEnumerable<DiskElement> list = await controller.GetListAsync(PathManager.GetAppDir());

        // string path = PathManager.GetProjectsDir();
        //        if (!list.Any(
        //            x => x.IsDirectory && x.Href == path
        //            ))
        //        {
        //            await controller.CreateDirAsync("/", path);
        //            result = false;
        //        }

        // projectsDirCreate = true;
        //    }

        // return result;
        // }

        // private async Task<bool> CheckUsersDir()
        // {
        //    bool result = true;
        //    if (!usersDirCreate)
        //    {
        //        IEnumerable<DiskElement> list = await controller.GetListAsync(PathManager.GetAppDir());

        // string path = PathManager.GetUsersDir();
        //        if (!list.Any(
        //            x => x.IsDirectory && x.Href == path
        //            ))
        //        {
        //            await controller.CreateDirAsync("/", path);
        //            result = false;
        //        }

        // usersDirCreate = true;
        //    }

        // return result;
        // }

        // private async Task<bool> CheckDirProject(ProjectDto project)
        // {
        //    bool result = true;
        //    if (!projects.Any(x => x == (int)project.ID))
        //    {
        //        IEnumerable<DiskElement> list = await controller.GetListAsync(PathManager.GetAppDir());
        //        string path = PathManager.GetProjectDir(project);
        //        if (!list.Any(
        //            x => x.IsDirectory && x.Href == path
        //            ))
        //        {
        //            await controller.CreateDirAsync("/", path);
        //            result = false;
        //        }

        // projects.Add((int)project.ID);
        //    }

        // return result;
        // }

        // private async Task<bool> CheckRevisionsDir()
        // {
        //    bool result = true;
        //    if (!transactionsDirCreate)
        //    {
        //        IEnumerable<DiskElement> list = await controller.GetListAsync(PathManager.GetAppDir());

        // string path = PathManager.GetRevisionsDir();
        //        if (!list.Any(
        //            x => x.IsDirectory && x.Href == path
        //            ))
        //        {
        //            await controller.CreateDirAsync("/", path);
        //            result = false;
        //        }

        // transactionsDirCreate = true;
        //    }

        // return result;
        // }

        // private async Task<bool> CheckDirObjectives(ProjectDto project)
        // {
        //    bool result = true;
        //    if (!objectives.Any(x => x == (int)project.ID))
        //    {
        //        IEnumerable<DiskElement> list = await controller.GetListAsync(PathManager.GetProjectDir(project));
        //        string path = PathManager.GetObjectivesDir(project);
        //        if (!list.Any(
        //            x => x.IsDirectory && x.Href == path
        //            ))
        //        {
        //            await controller.CreateDirAsync("/", path);
        //            result = false;
        //        }

        // objectives.Add((int)project.ID);
        //    }

        // return result;
        // }

        // #endregion
        // #region Revision
        // public async Task<RevisionCollection> GetRevisionsAsync()
        // {
        //    if (await CheckRevisionsDir())
        //    {
        //        try
        //        {
        //            string path = PathManager.GetRevisionsFile();
        //            string json = await controller.GetContentAsync(path);
        //            RevisionCollection revisions = JsonConvert.DeserializeObject<RevisionCollection>(json);
        //            return revisions;
        //        }
        //        catch (FileNotFoundException) { }
        //    }

        // return new RevisionCollection();
        // }

        // public async Task SetRevisionsAsync(RevisionCollection revisions)
        // {
        //    await CheckRevisionsDir();
        //    string path = PathManager.GetRevisionsFile();
        //    string json = JsonConvert.SerializeObject(revisions);
        //    await controller.SetContentAsync(path, json);
        // }

        //// public async Task<List<DateTime>> GetRevisionsDatesAsync()
        //// {
        ////    var result = new List<DateTime>();
        ////    if (await CheckRevisionsDir())
        ////    {
        ////        string path = PathManager.GetRevisionsDir();
        ////        var list = await controller.GetListAsync(path);

        //// foreach (DiskElement element in list)
        ////        {
        ////            if (PathManager.TryParseTransaction(element.DisplayName, out DateTime date))
        ////            {
        ////                result.Add(date);
        ////            }
        ////        }
        ////    }
        ////    return result;
        //// }
        //// public async Task SetTransactionsAsync(DateTime date, List<Transaction> transactions)
        //// {
        ////    await CheckRevisionsDir();
        ////    string path = PathManager.GetTransactionsFile(date);
        ////    string json = JsonConvert.SerializeObject(transactions);
        ////    await controller.SetContentAsync(path, json);
        //// }
        //// public async Task<List<Transaction>> GetTransactionsAsync(DateTime date)
        //// {
        ////    string path = PathManager.GetTransactionsFile(date);
        ////    try
        ////    {
        ////        string json = await controller.GetContentAsync(path);
        ////        List<Transaction> transactions = JsonConvert.DeserializeObject<List<Transaction>>(json);
        ////        return transactions;
        ////    }
        ////    catch (FileNotFoundException)
        ////    { }
        ////    catch (JsonReaderException)
        ////    { }
        ////    return new List<Transaction>();
        //// }

        //// public async void SetRevisionAsync(ulong revision)
        //// {
        ////    await CheckRevisionsDir();
        ////    string path = PathManager.GetRevisionFile();
        ////    await controller.SetContentAsync(path, revision.ToString());
        //// }
        //// public async Task<ulong> GetRevisionAsync()
        //// {
        ////    if (await CheckRevisionsDir())
        ////    {
        ////        try
        ////        {
        ////            string path = PathManager.GetRevisionFile();
        ////            string text = await controller.GetContentAsync(path);
        ////            if (ulong.TryParse(text, out ulong rev))
        ////            {
        ////                return rev;
        ////            }
        ////        }
        ////        catch (FileNotFoundException)
        ////        { }
        ////    }
        ////    return 0;
        //// }
        // #endregion
        // #region Users
        // public async Task<UserDto> GetUserAsync(ID<UserDto> id)
        // {
        //    if (await CheckUsersDir())
        //    {
        //        try
        //        {
        //            string path = PathManager.GetUserFile(id);
        //            string json = await controller.GetContentAsync(path);
        //            UserDto user = JsonConvert.DeserializeObject<UserDto>(json);
        //            return user;
        //        }
        //        catch (FileNotFoundException) { }
        //    }

        // return null;
        // }

        // public async Task UnloadUser(UserDto user)
        // {
        //    await CheckUsersDir();
        //    string path = PathManager.GetUserFile(user);
        //    var json = JsonConvert.SerializeObject(user);
        //    await controller.SetContentAsync(path, json);
        // }

        // public async Task DeleteUser(ID<UserDto> id)
        // {
        //    if (await CheckUsersDir())
        //    {
        //        try
        //        {
        //            string path = PathManager.GetUserFile(id);
        //            await controller.DeleteAsync(path);
        //        }
        //        catch (FileNotFoundException) { }
        //    }
        // }

        // #endregion
        // #region Projects
        // public async Task<ProjectDto> GetProjectAsync(ID<ProjectDto> id)
        // {
        //    if (await CheckProjectsDir())
        //    {
        //        try
        //        {
        //            string path = PathManager.GetProjectFile(id);
        //            string json = await controller.GetContentAsync(path);
        //            ProjectDto project = JsonConvert.DeserializeObject<ProjectDto>(json);
        //            return project;
        //        }
        //        catch (FileNotFoundException) { }
        //    }

        // return null;
        // }

        ///// <summary>Загрузить проект на диск.</summary>
        // public async Task UnloadProject(ProjectDto project)
        // {
        //    await CheckProjectsDir();
        //    string path = PathManager.GetProjectFile(project);
        //    var json = JsonConvert.SerializeObject(project);
        //    await controller.SetContentAsync(path, json);
        // }

        ///// <summary>Удаляет файт проекта.</summary>
        // public async Task DeleteProject(ID<ProjectDto> id)
        // {
        //    if (await CheckProjectsDir())
        //    {
        //        string path = PathManager.GetProjectFile(id);
        //        try
        //        {
        //            await controller.DeleteAsync(path);
        //        }
        //        catch (FileNotFoundException)// Нахуя его удалять если его и так нет
        //        { }
        //    }
        // }

        // public async Task<List<ID<ProjectDto>>> GetProjectsIdAsync()
        // {
        //    List<ID<ProjectDto>> result = new List<ID<ProjectDto>>();
        //    if (await CheckProjectsDir())
        //    {
        //        string path = PathManager.GetProjectsDir();
        //        IEnumerable<DiskElement> list = await controller.GetListAsync(path);
        //        foreach (var element in list)
        //        {
        //            if (PathManager.TryParseProjectId(element.DisplayName, out ID<ProjectDto> id))
        //            {
        //                result.Add(id);
        //            }
        //        }
        //    }

        // return result;
        // }

        ///// <summary>Создает папку проекта.</summary>
        // public async Task CreateProjectDir(ProjectDto project)
        // {
        //    // await CheckProjectsDir();
        //    string path = PathManager.GetProjectDir(project);
        //    await controller.CreateDirAsync("/", path);
        // }

        ///// <summary>Удаляет папку проекта.</summary>
        // public async Task DeleteProjectDir(ProjectDto project)
        // {
        //    string path = PathManager.GetProjectDir(project);
        //    await controller.DeleteAsync(path);
        // }

        //// public async Task RenameProjectDir(ProjectDto projectOld, ProjectDto projectNew)
        //// {
        ////    await CheckProjectsDir();
        ////    string pathOld = PathManager.GetProjectDir(projectOld);
        ////    string pathNew = PathManager.GetProjectDir(projectNew);
        ////    if (pathNew == pathOld) return;
        ////    try
        ////    {
        ////        await controller.MoveAsync(pathOld, pathNew);
        ////    }
        ////    catch (FileNotFoundException)
        ////    {
        ////        await controller.CreateDirAsync("/", pathNew);
        ////    }
        //// }

        // #endregion
        // #region Objective
        // public async Task DeleteObjective(ProjectDto project, ID<ObjectiveDto> id)
        // {
        //    if (await CheckDirProject(project)
        //        && await CheckDirObjectives(project))
        //    {
        //        string path = PathManager.GetObjectiveFile(project, id);
        //        try
        //        {
        //            await controller.DeleteAsync(path);
        //        }
        //        catch (FileNotFoundException)// Нахуя его удалять если его и так нет
        //        { }
        //    }
        // }

        // public async Task<List<ID<ObjectiveDto>>> GetObjectivesIdAsync(ProjectDto project)
        // {
        //    List<ID<ObjectiveDto>> result = new List<ID<ObjectiveDto>>();
        //    string path = PathManager.GetObjectivesDir(project);
        //    if (await CheckDirProject(project)
        //        && await CheckDirObjectives(project))
        //    {
        //        IEnumerable<DiskElement> list = await controller.GetListAsync(path);
        //        foreach (var element in list)
        //        {
        //            if (PathManager.TryParseObjectiveId(element.DisplayName, out ID<ObjectiveDto> id))
        //            {
        //                result.Add(id);
        //            }
        //        }
        //    }

        // return result;
        //    // throw new NotImplementedException();
        // }

        // public async Task UploadObjectiveAsync(ProjectDto project, ObjectiveDto objective, Action<ulong, ulong> progressChenge = null)
        // {
        //    await CheckDirProject(project);
        //    await CheckDirObjectives(project);

        // string path = PathManager.GetObjectiveFile(project, objective);
        //    string json = JsonConvert.SerializeObject(objective);
        //    await controller.SetContentAsync(path, json, progressChenge);
        // }

        // public async Task<ObjectiveDto> GetObjectiveAsync(ProjectDto project, ID<ObjectiveDto> id)
        // {
        //    try
        //    {
        //        string path = PathManager.GetObjectiveFile(project, id);
        //        string json = await controller.GetContentAsync(path);
        //        ObjectiveDto objective = JsonConvert.DeserializeObject<ObjectiveDto>(json);
        //        return objective;
        //    }

        // catch (Exception ex)
        //    {
        //        if (ex is FileNotFoundException || ex is DirectoryNotFoundException)
        //        {
        //            return null;
        //        }
        //        else
        //        {
        //            logger.Error(ex);
        //            logger.Open();
        //            throw;
        //        }
        //    }

        // // return null;
        // }
        // #endregion
        // #region items

        // public async Task<(long contentLength, DateTime editDate, string path)> GetInfoFile(ProjectDto project, ItemDto item)
        // {
        //    var path = YandexHelper.FileName(PathManager.GetProjectDir(project), item.Name);
        //    long contentLength = -1;
        //    DateTime editDate = DateTime.MinValue;
        //    try
        //    {
        //        var info = await controller.GetInfoAsync(path);
        //        if (long.TryParse(info.ContentLength, out long length))
        //            contentLength = length;
        //        editDate = info.LastModified;
        //    }
        //    catch (FileNotFoundException)
        //    {
        //    }

        // return (contentLength, editDate, path);
        // }

        // public async Task<List<ItemDto>> GetItemsAsync(ProjectDto project)
        // {
        //    List<ItemDto> result = new List<ItemDto>();
        //    if (await CheckDirProject(project))
        //    {
        //        string fileName = PathManager.GetItemsFile(project);
        //        string json = await controller.GetContentAsync(fileName);
        //        List<ItemDto> items = JsonConvert.DeserializeObject<List<ItemDto>>(json);
        //        return items;
        //    }

        // return result;
        // }

        // public async Task SetItemsAsync(ProjectDto project, List<ItemDto> items)
        // {
        //    await CheckDirProject(project);
        //    string fileName = PathManager.GetItemsFile(project);
        //    string json = JsonConvert.SerializeObject(items);
        //    await controller.SetContentAsync(fileName, json);
        // }

        ///// <summary>Скачать.</summary>
        ///// <param name="item">item указывающий на файл.</param>
        ///// <param name="path">Папака в которую записывается файл.</param>
        ///// <param name="progressChenge"> x. </param>
        ///// <returns><placeholder>A <see cref="Task"/> representing the asynchronous operation.</placeholder></returns>
        // public async Task DownloadItemFile(ItemDto item, string path, Action<ulong, ulong> progressChenge = null)
        // {
        //    string fileName = Path.Combine(path, item.Name);
        //    await controller.DownloadFileAsync(item.ExternalItemId, fileName, progressChenge);
        //    item.ExternalItemId = fileName;
        // }

        ///// <summary>
        ///// Загружает  файл на который указывает item.
        ///// </summary>
        ///// <param name="project"> x </param>
        ///// <param name="item"> xf </param>
        ///// <param name="progressChenge"> xf. </param>
        ///// <returns>  xd </returns>
        // public async Task UnloadFileItem(ProjectDto project, ItemDto item, Action<ulong, ulong> progressChenge = null)
        // {
        //    FileInfo fileInfo = new FileInfo(item.ExternalItemId);
        //    if (!fileInfo.Exists) throw new FileNotFoundException();

        // //
        //    // TODO: Сортировка файлов по папочкам будет осуществлятся здесь
        //    //
        //    // await CheckDirItems(project);
        //    string path = PathManager.GetProjectDir(project);
        //    string diskName = YandexHelper.FileName(path, fileInfo.Name);
        //    await controller.LoadFileAsync(path, item.ExternalItemId, progressChenge);
        //    item.ExternalItemId = diskName;
        // }

        // #endregion
    }

    // #region LOGGER
    // public class CoolLogger
    // {
    //    private FileInfo logFile;
    //    // private StreamWriter log;

    // public CoolLogger(string name)
    //    {
    //        logFile = new FileInfo($"{name}.log");
    //        using (var log = logFile.AppendText())
    //        {
    //            log.WriteLine("\n=== Начало логгирования ===");
    //            log.WriteLine("date : " + System.DateTime.Now.ToString("dd.MM.yy HH:mm:ss.FFF"));
    //        }
    //    }

    // public void Message(string message = "",
    //    [System.Runtime.CompilerServices.CallerMemberName] string memberName = "",
    //    [System.Runtime.CompilerServices.CallerFilePath] string sourceFilePath = "",
    //    [System.Runtime.CompilerServices.CallerLineNumber] int sourceLineNumber = 0)
    //    {
    //        using (var log = logFile.AppendText())
    //        {
    //            log.Write($"\n{memberName}[{sourceLineNumber}]");
    //            if (!string.IsNullOrWhiteSpace(message)) log.Write($": " + message);
    //        }
    //    }

    // public void Save()
    //    {
    //        // logFile
    //    }

    // public void Error(System.Exception ex)
    //    {
    //        // Get stack trace for the exception with source file information
    //        var st = new System.Diagnostics.StackTrace(ex, true);
    //        // Get the top stack frame
    //        var frame = st.GetFrame(0);
    //        // Get the line number from the stack frame
    //        var line = frame.GetFileLineNumber();

    // using (var log = logFile.AppendText())
    //        {
    //            log.WriteLine($"\nError<{ex.GetType().Name}>:{ex.Message}");
    //            log.WriteLine($"Source:{ex.Source};"
    //                + $"TargetSite.Name:{ex.TargetSite.Name};"
    //                + $"HResult:{ex.HResult};"
    //                + $"line:{line};"
    //                );
    //            log.WriteLine($"StackTrace:{ex.StackTrace}");
    //        }
    //    }

    // internal void Clear()
    //    {
    //        logFile.Delete();
    //    }

    // public void Open()
    //    {
    //        // System.Diagnostics.Process.Start(logFile.FullName);
    //        Process.Start("C:\\Windows\\System32\\notepad.exe", logFile.FullName.Trim());
    //    }
    // }
    // #endregion
}
