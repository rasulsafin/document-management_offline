using MRS.DocumentManagement.Connection.YandexDisk;
using MRS.DocumentManagement.Connection.YandexDisk.Synchronizer;
using MRS.DocumentManagement.Contols;
using MRS.DocumentManagement.Interface.Dtos;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace MRS.DocumentManagement
{
    /// <summary>
    /// Код от сюда перекачует в SyncManager 
    /// </summary>
    public class Synchronizer
    {
        public delegate void AddTransactionDelegate();
        public event AddTransactionDelegate TransactionsChange;
        public delegate void ProgressChangeDelegate(int current, int total);
        public event ProgressChangeDelegate ProgressChange;

        private YandexDiskManager yandex;
        private YandexDiskController controller;
        private int total;
        private int current;
        public Revisions Revisions { get; private set; } = new Revisions();


        public async void Initialize(string accessToken)
        {
            if (yandex == null)
            {
                yandex = new YandexDiskManager(accessToken);
                controller = new YandexDiskController(accessToken);
                yandex.TempDir = PathManager.TEMP_DIR;
                //await Task.Delay(5000);
                await LoadRevisions();
            }
        }

        private async Task LoadRevisions()
        {
            string fileName = PathManager.GetRevisionFile();
            try
            {
                string json = await File.ReadAllTextAsync(fileName);
                Revisions = JsonConvert.DeserializeObject<Revisions>(json);
            }
            catch
            {
                Revisions = new Revisions();
                SaveRevisions();
            }
        }

        private void SaveRevisions()
        {
            string dirName = PathManager.GetRevisionsDir();
            if (!Directory.Exists(dirName)) Directory.CreateDirectory(dirName);
            string fileName = PathManager.GetRevisionFile();
            string str = JsonConvert.SerializeObject(Revisions, Formatting.Indented);
            File.WriteAllText(fileName, str);
        }

        #region Update Table
        public void Update(ID<ProjectDto> id)
        {
            if (Syncing) return;
            Revisions.UpdateProject((int)id);
            SaveRevisions();
        }
        public void Update(ID<UserDto> id)
        {
            if (Syncing) return;
            Revisions.UpdateUser((int)id);
            SaveRevisions();
        }
        public void Update(ID<ObjectiveDto> id, ID<ProjectDto> idProj)
        {
            if (Syncing) return;
            Revisions.UpdateObjective((int)idProj, (int)id);
            SaveRevisions();
        }
        public void Update(ID<ItemDto> id, ID<ProjectDto> idProj)
        {
            if (Syncing) return;
            Revisions.UpdateItem((int)idProj, (int)id);
            SaveRevisions();
        }
        public void Update(ID<ItemDto> id, ID<ObjectiveDto> idObj, ID<ProjectDto> idProj)
        {
            if (Syncing) return;
            Revisions.UpdateItem((int)idProj, (int)idObj, (int)id);
            SaveRevisions();
        }
        #endregion



        internal async Task SyncTableAsync(ProgressChangeDelegate progressChange)
        {
            int total = 0;
            int current = 0;
            Revisions revisions = await yandex.GetRevisionsAsync();
            total = GetCount(Revisions);
            Progress<int> progress = new Progress<int>();
            progress.ProgressChanged += (s, p) =>
            {
                current += p;
                progressChange?.Invoke(current, total);
            };

            await Synchronize(progress, new ProjectSynchronizer(yandex),  revisions);
            await Synchronize(progress, new UserSynchronizer(yandex),  revisions);
            //current = await SyncProject(progressChange, total, current, revisions);
            //current = await SyncObjectives(progressChange, total, current, revisions);
            //current = await SyncItems(progressChange, total, current, revisions);

            await yandex.SetRevisionsAsync(Revisions);
            SaveRevisions();
        }
        private async Task Synchronize(IProgress<int> progress, ISynchronizer synchro, Revisions remoreRevisions)
        {
            List<int> download = new List<int>();
            List<int> unload = new List<int>();
            List<Revision> local = synchro.GetRevision(Revisions);
            List<Revision> remote = synchro.GetRevision(remoreRevisions);
            CompareRevision(download, unload, local, remote, progress);                        
            synchro.LoadLocalCollect();

            if (download.Count > 0)
            {
                //надо скачать
                foreach (int num in download)
                {
                    if (await synchro.RemoteExistAsync(num))
                    {
                        await synchro.DownloadAndUpdateAsync(num);
                    }
                    else 
                    {
                        synchro.DeleteLocal(num);
                    }

                    var subSynchronizes = synchro.GetSubSynchronizes(num);
                    if (subSynchronizes != null)
                    {
                        foreach (var subSynchronize in subSynchronizes)
                        {
                            await Synchronize(progress, subSynchronize, remoreRevisions);
                        }
                    }
                    progress.Report(1);
                }
            }
            if (unload.Count > 0)
            {
                //надо загрузить
                foreach (int num in unload)
                {
                    if ( synchro.LocalExist(num))
                    {
                        await synchro.UpdateRemoteAsync(num);
                    }
                    else
                    {
                        await synchro.DeleteRemoteAsync(num);
                    }

                    var subSynchronizes = synchro.GetSubSynchronizes(num);
                    if (subSynchronizes != null)
                    {
                        foreach (var subSynchronize in subSynchronizes)
                        {
                            await Synchronize(progress, subSynchronize, remoreRevisions);
                        }
                    }
                    progress.Report(1);
                }
            }
            synchro.SaveLocalCollect();            
        }
        private static void CompareRevision(List<int> download, List<int> unload,
            List<Revision> local, List<Revision> remote, IProgress<int> progress)
        {
            foreach (var localRev in local)
            {
                // Находим совподения
                var remoteRev = remote.Find(r => r.ID == localRev.ID);
                if (remoteRev == null)
                {
                    // Загружаем на сервер
                    unload.Add(localRev.ID);
                }
                else if (localRev < remoteRev)
                {
                    // Скачиваем с сервера
                    download.Add(localRev.ID);
                    remote.Remove(remoteRev);
                    localRev.Rev = remoteRev.Rev;
                }
                else if (localRev > remoteRev)
                {
                    // Загружаем на сервер
                    unload.Add(localRev.ID);
                    remote.Remove(remoteRev);
                }
                else if (localRev.Equals(remoteRev))
                {
                    // Пропускаем 
                    progress.Report(1);
                }
            }
            foreach (var remoteRev in remote)
            {
                // Скачиваем с сервера
                download.Add(remoteRev.ID);
                local.Add(remoteRev);
            }
        }


        private static int CompareRevision(ProgressChangeDelegate progressChange, int total, int current, Dictionary<int, ulong> remote, Dictionary<int, ulong> local, List<int> download, List<int> unload)
        {
            foreach (var localObj in local)
            {
                var localKey = localObj.Key;
                var localRev = localObj.Value;
                if (remote.ContainsKey(localKey))
                {
                    var servRev = remote[localKey];
                    if (servRev < localRev)
                    {
                        remote.Remove(localKey);
                    }
                    else if (servRev > localRev)
                    {
                        // Скачиваем с сервера
                        download.Add(localKey);
                        local[localKey] = servRev;
                        continue;
                    }
                    else if (servRev == localRev)
                    {
                        remote.Remove(localKey);
                        progressChange?.Invoke(++current, total);
                        continue;
                    }
                }
                // Загружаем на сервер
                unload.Add(localKey);
            }
            foreach (var item in remote)
            {
                var servKey = item.Key;
                // Скачиваем с сервера
                download.Add(servKey);
            }

            return current;
        }

        private async Task<int> SyncItems(ProgressChangeDelegate progressChange, int total, int current, Revisions revisions)
        {
            #region Project => Item
            //foreach (var remoteProject in revisions.ItemsProject)
            //{
            //    if (!Revisions.ItemsProject.ContainsKey(remoteProject.Key))
            //    {
            //        // TODO : Сделать хитрее, если на компе этой таблици нет, то тупое копирование
            //        Revisions.ItemsProject.Add(remoteProject.Key, new Dictionary<int, ulong>());
            //    }
            //}
            //foreach (var localProject in Revisions.ItemsProject)
            //{
            //    ID<ProjectDto> idProj = new ID<ProjectDto>(localProject.Key);
            //    var project = ObjectModel.Projects.First(x => x.dto.ID == idProj).dto;
            //    if (!revisions.ItemsProject.ContainsKey(localProject.Key))
            //    {
            //        // TODO : Сделать хитрее, если на сервере этой таблици нет, то тупое копирование
            //        revisions.ItemsProject.Add(localProject.Key, new Dictionary<int, ulong>());
            //    }
            //    Dictionary<int, ulong> remoteItems = revisions.ItemsProject[localProject.Key];
            //    Dictionary<int, ulong> localItems = localProject.Value;

            //    current = await SyncItemsProject(progressChange, total, current, project, remoteItems, localItems);
            //} 
            #endregion
            //foreach (var remoteProject in revisions.ItemsObjective)
            //{
            //    if (!Revisions.ItemsObjective.ContainsKey(remoteProject.Key))
            //    {
            //        // TODO : Сделать хитрее, если на компе этой таблици нет, то тупое копирование
            //        Revisions.ItemsObjective.Add(remoteProject.Key, new Dictionary<int, Dictionary<int, ulong>>());
            //    }
            //}
            //foreach (var localProject in Revisions.ItemsObjective)
            //{
            //    ID<ProjectDto> idProj = new ID<ProjectDto>(localProject.Key);
            //    var project = ObjectModel.Projects.First(x => x.dto.ID == idProj).dto;

            //    if (!revisions.ItemsObjective.ContainsKey(localProject.Key))
            //    {
            //        // TODO : Сделать хитрее, если на компе этой таблици нет, то тупое копирование
            //        Revisions.ItemsObjective.Add(localProject.Key, new Dictionary<int, Dictionary<int, ulong>>());
            //    }


            //}

            return current;
            //
            // TODO: Я остановился здесь
            //
        }

        private async Task<int> SyncItemsProject(ProgressChangeDelegate progressChange, int total, int current,
            ProjectDto project, Dictionary<int, ulong> remoteItems, Dictionary<int, ulong> localItems)
        {
            List<int> download = new List<int>();
            List<int> unload = new List<int>();
            current = CompareRevision(progressChange, total, current, remoteItems, localItems, download, unload);

            List<ItemDto> items = ObjectModel.GetItems(project);
            if (download.Count > 0)
            {
                //надо скачать
                foreach (int num in download)
                {
                    var id = (ID<ItemDto>)num;
                    ItemDto dto = await yandex.GetItemAsync(project, id);
                    if (dto == null)
                    {
                        //
                        // TODO: Удаление item и файла?
                        //
                        items.RemoveAll(x => x.ID == id);
                    }
                    else
                    {
                        // TODO: Сортировка по папочкам
                        string path = Path.Combine(PathManager.GetProjectDir(project), dto.Name);
                        await yandex.DownloadItem(dto, path);
                        dto.ExternalItemId = path;

                        int index = items.FindIndex(x => x.ID == id);
                        if (index < 0)
                        {
                            items.Add(dto);
                        }
                        else
                        {
                            // TODO: Удалить старый файл?                            
                            items[index] = dto;
                        }
                    }
                    progressChange?.Invoke(++current, total);
                }
            }
            if (unload.Count > 0)
            {
                //надо загрузить
                foreach (int num in unload)
                {
                    var id = (ID<ItemDto>)num;
                    ItemDto dto = items.Find(x => x.ID == id);
                    if (dto == null)
                    {
                        // TODO: Удалять файлы? Сначало понять ссылаются ли другие item на него 
                        await yandex.DeleteItem(project, id);
                    }
                    else
                    {
                        // TODO: Тотже вопрос, если есть старый файл куда его?
                        await yandex.UploadItemAsync(project, dto);
                    }
                    progressChange?.Invoke(++current, total);
                }
            }
            ObjectModel.SaveItems(project, items);
            return current;
        }


        private async Task<int> SyncObjectives(ProgressChangeDelegate progressChange, int total, int current, Revisions revisions)
        {
            //foreach (var remoteProject in revisions.Objectives)
            //{
            //    if (!Revisions.Objectives.ContainsKey(remoteProject.Key))
            //    {
            //        // TODO : Сделать хитрее, если на компе этой таблици нет, то тупое копирование
            //        Revisions.Objectives.Add(remoteProject.Key, new Dictionary<int, ulong>());
            //    }
            //}

            //foreach (var localProject in Revisions.Objectives)
            //{
            //    ID<ProjectDto> idProj = new ID<ProjectDto>(localProject.Key);
            //    var project = ObjectModel.Projects.First(x => x.dto.ID == idProj).dto;
            //    if (!revisions.Objectives.ContainsKey(localProject.Key))
            //    {
            //        // TODO : Сделать хитрее, если на сервере этой таблици нет, то тупое копирование
            //        revisions.Objectives.Add(localProject.Key, new Dictionary<int, ulong>());
            //    }
            //    Dictionary<int, ulong> remoteObjectives = revisions.Objectives[localProject.Key];
            //    Dictionary<int, ulong> localObjectives = localProject.Value;

            //    current = await SyncObjective(progressChange, total, current, project, remoteObjectives, localObjectives);
            //}

            return current;
        }

        private async Task<int> SyncObjective(ProgressChangeDelegate progressChange, int total, int current, ProjectDto project, Dictionary<int, ulong> remoteObjectives, Dictionary<int, ulong> localObjectives)
        {
            List<int> download = new List<int>();
            List<int> unload = new List<int>();
            current = CompareRevision(progressChange, total, current, remoteObjectives, localObjectives, download, unload);
            List<ObjectiveDto> objectives = ObjectModel.GetObjectives(project);
            if (download.Count > 0)
            {
                //надо скачать
                foreach (int num in download)
                {
                    var id = (ID<ObjectiveDto>)num;
                    ObjectiveDto dto = await yandex.GetObjectiveAsync(project, id);
                    if (dto == null)
                    {
                        objectives.RemoveAll(x => x.ID == id);
                    }
                    else
                    {
                        int index = objectives.FindIndex(x => x.ID == id);
                        if (index < 0)
                            objectives.Add(dto);
                        else
                            objectives[index] = dto;
                    }
                    progressChange?.Invoke(++current, total);
                }
            }
            if (unload.Count > 0)
            {
                //надо загрузить
                foreach (int num in unload)
                {
                    var id = (ID<ObjectiveDto>)num;
                    ObjectiveDto dto = objectives.Find(x => x.ID == id);
                    if (dto == null)
                        await yandex.DeleteObjective(project, id);
                    else
                        await yandex.UploadObjectiveAsync(project, dto);
                    progressChange?.Invoke(++current, total);
                }
            }
            ObjectModel.SaveObjectives(project, objectives);
            return current;
        }

        private async Task<int> SyncProject(ProgressChangeDelegate progressChange, int total, int current, Revisions revisions)
        {
            //List<int> download = new List<int>();
            //List<int> unload = new List<int>();
            //current = CompareRevision(progressChange, total, current, revisions.Projects, Revisions.Projects, download, unload);
            //List<ProjectDto> projects = ObjectModel.GetProjects();
            //if (download.Count > 0)
            //{
            //    //надо скачать
            //    foreach (int num in download)
            //    {
            //        var id = (ID<ProjectDto>)num;
            //        ProjectDto dto = await yandex.GetProjectAsync(id);
            //        if (dto == null)
            //        {
            //            projects.RemoveAll(x => x.ID == id);
            //        }
            //        else
            //        {
            //            int index = projects.FindIndex(x => x.ID == id);
            //            if (index < 0)
            //                projects.Add(dto);
            //            else
            //                projects[index] = dto;
            //        }
            //        progressChange?.Invoke(++current, total);
            //    }
            //}
            //if (unload.Count > 0)
            //{
            //    //надо загрузить
            //    foreach (int num in unload)
            //    {
            //        var id = (ID<ProjectDto>)num;
            //        ProjectDto dto = projects.Find(x => x.ID == id);
            //        if (dto == null)
            //            await yandex.DeleteProject(id);
            //        else
            //            await yandex.UnloadProject(dto);
            //        progressChange?.Invoke(++current, total);
            //    }
            //}
            //ObjectModel.SaveProjects(projects);
            return current;
        }




        private int GetCount(Revisions revisions)
        {
            int result = revisions.Projects.Count + revisions.Users.Count;
            result += (int)revisions.Projects.Sum(x => x.Objectives?.Count);
            result += (int)revisions.Projects.Sum(x => x.Items?.Count);
            result += (int)revisions.Projects.Sum(x => x.Objectives?.Sum(q => q.Items?.Count));
            return result;
        }


        #region old
        public ulong Revision { get; private set; }
        public List<Transaction> Transactions { get; private set; } = new List<Transaction>();
        public YandexDiskManager Yandex { get => yandex; set => yandex = value; }
        public bool Syncing { get; private set; }
        public void Load()
        {
            //var list = GetLocalTransaction();
            //Transactions.Clear();
            //Transactions.AddRange(list);
            //Revision = GetLocalRevision();
        }

        //public void AddTransaction(TransType type, ID<ProjectDto> id)
        //{
        //    if (Synchronize) return;
        //    Transactions.Add(new Transaction() { Type = type, Table = Table.Project, IdObject = (int)id, Rev = 0 });
        //    TransactionsChange();
        //    Save();
        //}
        //public void AddTransaction(TransType type, ID<ObjectiveDto> id)
        //{
        //    if (Synchronize) return;
        //    Transactions.Add(new Transaction() { Type = type, Table = Table.Objective, IdObject = (int)id, Rev = 0 });
        //    TransactionsChange();
        //    Save();
        //}
        //public void AddTransaction(TransType type, ID<ItemDto> id)
        //{
        //    if (Synchronize) return;
        //    Transactions.Add(new Transaction() { Type = type, Table = Table.Item, IdObject = (int)id, Rev = 0 });
        //    TransactionsChange();
        //    Save();
        //}
        public async Task<ulong> GetRevisionServerAsync()
        {
            //return await yandex.GetRevisionAsync();
            return 0;
        }


        /// <summary>
        /// Производит синхронизвцию 
        /// </summary>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">При попытки синхронизации до завершения инициализации</exception>
        //public async Task SynchronizeAsync(ProgressChangeDelegate progressChange)
        //{
        //    // 1 Определить список операций Входящих с сервера
        //    // 2 Примерить список опрераций входящих с сервера
        //    // 3 Определить список операции исходящих на сервер
        //    // 4 Применить список операции исходящих на сервер
        //    // 5 Добавиь на сервер новые операции
        //    // 6 Присвоить ревизии новый номер
        //    Synchronize = true;
        //    ProgressChange = progressChange;
        //    if (yandex == null)
        //        throw new ArgumentNullException("Токен не получен, инициализация не завершена!");

        //    ulong revision = await yandex.GetRevisionAsync();
        //    List<Transaction> serverTran = await GetTransactionsServerAsync(revision);
        //    List<Transaction> localTran = GetTransactionsLocal(revision);
        //    total = serverTran.Count + localTran.Count;
        //    current = 0;
        //    if (serverTran.Count != 0)
        //    {
        //        await ApplyTransactionsServerAsync(serverTran);
        //        Revision = revision;
        //    }
        //    if (localTran.Count != 0)
        //        await ApplyTransactionsLocalAsync(localTran);
        //    Save();
        //    Synchronize = false;
        //}

        private async Task SynchronizeToServerAsync(Transaction transaction)
        {
            switch (transaction.Table)
            {
                case Table.Project: await SinchrinizeProjectToServerAsync(transaction); break;
                case Table.Objective: await SinchrinizeObjectiveToServerAsync(transaction); break;
                case Table.Item: await SinchrinizeItemToServerAsync(transaction); break;
                default:
                    throw new ArgumentException();
            }
        }

        private async Task SinchrinizeItemToServerAsync(Transaction transaction)
        {
            //var id = new ID<ItemDto>(transaction.IdObject);
            //if (transaction.Type == TransType.Update)
            //{
            //    (ItemDto item, ObjectiveDto objective, ProjectDto project) = await yandex.GetItemAsync(id);
            //    string path = PathManager.GetProjectDir(project);
            //    await yandex.DownloadItems(item, path);
            //    ObjectModel.SaveItem(item, project, objective);
            //}
            //else if (transaction.Type == TransType.Delete)
            //{
            //    (ItemDto item, ObjectiveDto objective, ProjectDto project) = ObjectModel.GetItem(id);
            //    ObjectModel.DeleteItem(item, project, objective);
            //}
        }

        private async Task SinchrinizeObjectiveToServerAsync(Transaction transaction)
        {
            //var id = new ID<ObjectiveDto>(transaction.IdObject);
            //if (transaction.Type == TransType.Update)
            //{
            //    (ObjectiveDto objective, ProjectDto project) = ObjectModel.GetObjective(id);
            //    if (project == null)
            //    {// Значит идем сложным путём
            //        var ids = await yandex.GetProjectsIdAsync();
            //        foreach (var idProj in ids)
            //        {
            //            var proj = await yandex.GetProjectAsync(idProj);
            //            var objec = await yandex.GetObjectiveAsync(proj, id);
            //            if (objec != null)
            //            {
            //                project = proj;
            //                objective = objec;
            //            }
            //        }
            //    }
            //    ObjectModel.SaveObjective(objective, project);
            //}
            //else if (transaction.Type == TransType.Delete)
            //{
            //    (ObjectiveDto objective, ProjectDto project) = ObjectModel.GetObjective(id);
            //    if (project != null)
            //        ObjectModel.DeleteObjective(objective, project);
            //}
        }

        private async Task SinchrinizeProjectToServerAsync(Transaction transaction)
        {
            //var id = new ID<ProjectDto>(transaction.IdObject);
            //if (transaction.Type == TransType.Update)
            //{
            //    ProjectDto project = await yandex.GetProjectAsync(id);
            //    ObjectModel.SaveProject(project);
            //}
            //else if (transaction.Type == TransType.Delete)
            //{
            //    ObjectModel.DeleteProject(id);
            //}
        }

        /// <summary>
        /// Применяет одно изменение на закачивая его на сервер 
        /// </summary>
        /// <param name="transaction"></param>
        /// <returns></returns>
        private async Task SynchrinizeFromServerAsync(Transaction transaction)
        {
            switch (transaction.Table)
            {
                case Table.Project: await SynchrinizeProjectFromServerAsync(transaction); break;
                case Table.Objective: await SynchrinizeObjectiveFromServerAsync(transaction); break;
                case Table.Item: await SynchrinizeItemFromServerAsync(transaction); break;
                default:
                    throw new ArgumentException();
            }
        }
        private async Task SynchrinizeItemFromServerAsync(Transaction transaction)
        {
            //var id = new ID<ItemDto>(transaction.IdObject);
            //if (transaction.Type == TransType.Update)
            //{
            //    (ItemDto items, ObjectiveDto objective, ProjectDto project) = ObjectModel.GetItem(id);
            //    if (project != null)
            //    {
            //        await yandex.UploadItemAsync(items, project, objective);
            //        transaction.Sync = true;
            //    }
            //}
            //else if (transaction.Type == TransType.Delete)
            //{
            //    (ItemDto item, ObjectiveDto objective, ProjectDto project) = await yandex.GetItemAsync(id);

            //    if (project != null)
            //        await yandex.DeleteItem(id, project, objective);
            //}
        }
        private async Task SynchrinizeObjectiveFromServerAsync(Transaction transaction)
        {
            //var id = new ID<ObjectiveDto>(transaction.IdObject);
            //if (transaction.Type == TransType.Update)
            //{
            //    (ObjectiveDto objective, ProjectDto project) = ObjectModel.GetObjective(id);
            //    if (project != null)
            //        await yandex.UploadObjectiveAsync(objective, project);
            //}
            //else if (transaction.Type == TransType.Delete)
            //{
            //    var projects = ObjectModel.GetProjects();
            //    foreach (var project in projects)
            //    {
            //        var idObjectives = await yandex.GetObjectivesIdAsync(project);
            //        if (idObjectives.Any(x => x == id))
            //        {
            //            await yandex.DeleteObjective(project, id);
            //            //
            //            // TODO : Надо удалить все Items
            //            //

            //            break;
            //        }
            //    }
            //}
        }
        private async Task SynchrinizeProjectFromServerAsync(Transaction transaction)
        {
            //var id = new ID<ProjectDto>(transaction.IdObject);
            //ProjectDto project = await yandex.GetProjectAsync(id);
            //if (transaction.Type == TransType.Update)
            //{
            //    ProjectDto projectNew = ObjectModel.GetProject(id);
            //    if (project == null)
            //    {// Создаем новый проект
            //        await yandex.CreateProjectDir(projectNew);
            //    }
            //    else
            //    { // Изменяем старый
            //        await yandex.RenameProjectDir(project, projectNew);
            //    }
            //    await yandex.UnloadProject(projectNew);
            //}
            //else if (transaction.Type == TransType.Delete)
            //{
            //    if (project != null)
            //    {
            //        await yandex.DeleteProject(project);
            //        await yandex.DeleteProjectDir(project);
            //    }
            //}
        }

        /// <summary>
        /// Примерить список опрераций входящих с сервера
        /// </summary>
        /// <param name="serverTran"></param>
        private async Task ApplyTransactionsServerAsync(List<Transaction> transactions)
        {
            transactions.Sort((x, y) => x.Table.CompareTo(y.Table));
            foreach (var transaction in transactions)
            {
                await SynchronizeToServerAsync(transaction);
                current++;
                ProgressChange?.Invoke(current, total);
            }
        }

        /// <summary>
        /// Определить список операций Входящих с сервера
        /// </summary>
        /// <returns></returns>
        private async Task<List<Transaction>> GetTransactionsServerAsync(ulong revision)
        {
            List<Transaction> transactions = new List<Transaction>();
            //if (revision > Revision)
            //{
            //    // Скачиваем изменения с сервера
            //    List<DateTime> dates = await yandex.GetRevisionsDatesAsync();
            //    foreach (var date in dates)
            //    {
            //        var trans = await yandex.GetTransactionsAsync(date);
            //        bool stop = false;
            //        foreach (var tran in trans)
            //        {
            //            if (tran.Rev > Revision)
            //            {
            //                tran.Server = true;
            //                transactions.Add(tran);
            //            }
            //            else
            //            {
            //                stop = true;
            //                break;
            //            }
            //        }
            //        SaveTrans(trans, date);
            //        if (stop) break;
            //    }
            //}
            ////Revision = revision;
            return transactions;
        }

        /// <summary>
        /// Применить список операции исходящих на сервер
        /// Добавиь на сервер новые операции
        /// Присвоить ревизии новый номер
        /// </summary>
        /// <param name="serverTran"></param>
        private async Task ApplyTransactionsLocalAsync(List<Transaction> transactions)
        {
            //Revision = await yandex.GetRevisionAsync() + 1;
            //// Применить список операции исходящих на сервер
            //foreach (var transaction in transactions)
            //{
            //    transaction.Rev = Revision;
            //    await SynchrinizeFromServerAsync(transaction);
            //    transaction.Sync = true;
            //    transaction.Server = true;
            //    current++;
            //    ProgressChange?.Invoke(current, total);
            //}
            ////  Добавиь на сервер новые операции
            //List<Transaction> serverTranNow = await yandex.GetTransactionsAsync(DateTime.Now);
            //serverTranNow.AddRange(transactions);
            //await yandex.SetTransactionsAsync(DateTime.Now, serverTranNow);
            //SaveTrans(serverTranNow, DateTime.Now);
            //// Присвоить ревизии новый номер
            //yandex.SetRevisionAsync(Revision);
        }

        /// <summary>
        /// Определить список операции исходящих на сервер
        /// </summary>
        /// <returns></returns>
        private List<Transaction> GetTransactionsLocal(ulong revision)
        {
            List<Transaction> result = new List<Transaction>();
            var transDir = new DirectoryInfo(PathManager.GetRevisionsDir());
            foreach (var file in transDir.GetFiles())
            {
                List<Transaction> transaction = GetTransactions(file.FullName);
                foreach (var trans in transaction)
                {
                    if (trans.Rev > revision)
                    {
                        result.Add(trans);
                    }
                }
            }
            return Transactions.Where(x => x.Rev == 0 && !x.Server).ToList();
        }
        public async Task<List<Transaction>> GetAllTransactionAsync()
        {
            List<Transaction> transactions = Transactions.ToList();

            //ulong revision = await yandex.GetRevisionAsync();
            //// Скачиваем изменения с сервера
            //List<DateTime> dates = await yandex.GetRevisionsDatesAsync();
            //foreach (var date in dates)
            //{
            //    var trans = await yandex.GetTransactionsAsync(date);
            //    foreach (var tran in trans)
            //    {
            //        tran.Server = true;
            //        transactions.Add(tran);
            //    }
            //    SaveTrans(trans, date);
            //}
            return transactions;
        }
        public async Task<List<Transaction>> GetNonSyncTransactionAsync()
        {
            //ulong revision = await yandex.GetRevisionAsync();
            //List<Transaction> transactions = GetTransactionsLocal(revision);
            //List<Transaction> transactionsServer = await GetTransactionsServerAsync(revision);
            //transactions.AddRange(transactionsServer);
            //return transactions;
            return null;
        }

        //private void SaveTrans(List<Transaction> trans, DateTime date)
        //{
        //    //string dirTrans = PathManager.GetRevisionsDir();
        //    //if (!Directory.Exists(dirTrans)) Directory.CreateDirectory(dirTrans);

        //    //string filename = PathManager.GetRevisionsDir(date);

        //    //string json = JsonConvert.SerializeObject(trans, Formatting.Indented);
        //    //File.WriteAllText(filename, json);
        //}
        public void Save()
        {
            //var list = Transactions.Where(x => x.Rev == 0 && !x.Server).ToList();

            //string dirTrans = PathManager.GetTransactionsDir();
            //if (!Directory.Exists(dirTrans)) Directory.CreateDirectory(dirTrans);

            //string filename = PathManager.GetRevisionFile();
            //File.WriteAllText(filename, Revision.ToString());
            //filename = PathManager.GetTransactionFile();

            //string json = JsonConvert.SerializeObject(list, Formatting.Indented);
            //File.WriteAllText(filename, json);
        }
        //public List<Transaction> GetLocalTransaction()
        //{
        //    string dirTrans = PathManager.GetRevisionsDir();
        //    if (Directory.Exists(dirTrans))
        //    {
        //        string filename = PathManager.GetTransactionFile();
        //        if (File.Exists(filename))
        //        {
        //            string jcon = File.ReadAllText(filename);
        //            List<Transaction> list = JsonConvert.DeserializeObject<List<Transaction>>(jcon);
        //            return list;
        //        }
        //    }
        //    return new List<Transaction>();
        //}
        //public List<Transaction> GetTransactions(DateTime date)
        //{
        //    string dirTrans = PathManager.GetRevisionsDir();
        //    if (Directory.Exists(dirTrans))
        //    {
        //        string filename = PathManager.GetTransactionFile(date);
        //        return GetTransactions(filename);
        //    }
        //    return new List<Transaction>();
        //}
        public List<Transaction> GetTransactions(string filename)
        {
            if (File.Exists(filename))
            {
                try
                {

                    string jcon = File.ReadAllText(filename);
                    List<Transaction> list = JsonConvert.DeserializeObject<List<Transaction>>(jcon);
                    return list;
                }
                catch { }

            }
            return new List<Transaction>();
        }
        private ulong GetLocalRevision()
        {
            string dirTrans = PathManager.GetRevisionsDir();
            if (Directory.Exists(dirTrans))
            {
                string filename = PathManager.GetRevisionFile();
                if (File.Exists(filename))
                {
                    string text = File.ReadAllText(filename);
                    if (ulong.TryParse(text, out ulong rev))
                    {
                        return rev;
                    }
                }
            }
            return 0;
        }
        #endregion
    }


}