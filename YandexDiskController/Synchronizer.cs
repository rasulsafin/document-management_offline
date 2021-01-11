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
    public class Synchronizer
    {
        public delegate void AddTransactionDelegate();
        public event AddTransactionDelegate TransactionsChange;

        public delegate void ProgressChangeDelegate(int current, int total);
        public event ProgressChangeDelegate ProgressChange;

        private YandexDiskManager yandex;
        private int total;
        private int current;


        public List<Transaction> Transactions { get; private set; } = new List<Transaction>();
        public ulong Revision { get; private set; }
        public YandexDiskManager Yandex { get => yandex; set => yandex = value; }
        public bool Synchronize { get; private set; }

        public void AddTransaction(TransType type, ID<ProjectDto> id)
        {
            if (Synchronize) return;
            Transactions.Add(new Transaction() { Type = type, Table = Table.Project, IdObject = (int)id, Rev = 0 });
            TransactionsChange();
            Save(); 
        }
        public void AddTransaction(TransType type, ID<ObjectiveDto> id)
        {
            if (Synchronize) return;
            Transactions.Add(new Transaction() { Type = type, Table = Table.Objective, IdObject = (int)id, Rev = 0 });
            TransactionsChange();
            Save();
        }
        public void AddTransaction(TransType type, ID<ItemDto> id)
        {
            if (Synchronize) return;
            Transactions.Add(new Transaction() { Type = type, Table = Table.Item, IdObject = (int)id, Rev = 0 });
            TransactionsChange();
            Save();
        }
        public async Task<ulong> GetRevisionServerAsync()
        {
            return await yandex.GetRevisionAsync();
        }

        /// <summary>
        /// Производит синхронизвцию 
        /// </summary>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">При попытки синхронизации до завершения инициализации</exception>
        public async Task SynchronizeAsync(ProgressChangeDelegate progressChange)
        {
            // 1 Определить список операций Входящих с сервера
            // 2 Примерить список опрераций входящих с сервера
            // 3 Определить список операции исходящих на сервер
            // 4 Применить список операции исходящих на сервер
            // 5 Добавиь на сервер новые операции
            // 6 Присвоить ревизии новый номер
            Synchronize = true;
            ProgressChange = progressChange;
            if (yandex == null)
                throw new ArgumentNullException("Токен не получен, инициализация не завершена!");

            ulong revision = await yandex.GetRevisionAsync();
            List<Transaction> serverTran = await GetTransactionsServerAsync(revision);
            List<Transaction> localTran = GetTransactionsLocal(revision);
            total = serverTran.Count + localTran.Count;
            current = 0;
            if (serverTran.Count != 0)
            {
                await ApplyTransactionsServerAsync(serverTran);
                Revision = revision;
            }
            if (localTran.Count != 0)
                await ApplyTransactionsLocalAsync(localTran);
            Save();
            Synchronize = false;
        }

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
            var id = new ID<ItemDto>(transaction.IdObject);
            if (transaction.Type == TransType.Update)
            {                
                (ItemDto item, ObjectiveDto objective, ProjectDto project) = await yandex.GetItemAsync(id);
                string path = PathManager.GetProjectDir(project);
                await yandex.DownloadItems(item, path);
                ObjectModel.SaveItem(item, project, objective);
            }
            else if (transaction.Type == TransType.Delete)
            {
                (ItemDto item, ObjectiveDto objective, ProjectDto project) = ObjectModel.GetItem(id);
                ObjectModel.DeleteItem(item, project, objective);
            }
        }



        private async Task SinchrinizeObjectiveToServerAsync(Transaction transaction)
        {
            var id = new ID<ObjectiveDto>(transaction.IdObject);
            if (transaction.Type == TransType.Update)
            {
                (ObjectiveDto objective, ProjectDto project) = ObjectModel.GetObjective(id);
                if (project == null)
                {// Значит идем сложным путём
                    var ids = await yandex.GetProjectsIdAsync();
                    foreach (var idProj in ids)
                    {
                        var proj = await yandex.GetProjectAsync(idProj);
                        var objec = await yandex.GetObjectiveAsync(proj, id);
                        if (objec != null)
                        {
                            project = proj;
                            objective = objec;
                        }
                    }
                }
                    ObjectModel.SaveObjective(objective, project);
            }
            else if (transaction.Type == TransType.Delete)
            {
                (ObjectiveDto objective, ProjectDto project) = ObjectModel.GetObjective(id);
                if (project != null)
                    ObjectModel.DeleteObjective(objective, project);
            }
        }

        private async Task SinchrinizeProjectToServerAsync(Transaction transaction)
        {
            var id = new ID<ProjectDto>(transaction.IdObject);
            if (transaction.Type == TransType.Update)
            {
                ProjectDto project = await yandex.GetProjectAsync(id);
                ObjectModel.SaveProject(project);
            }
            else if (transaction.Type == TransType.Delete)
            {
                ObjectModel.DeleteProject(id);
            }
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
            var id = new ID<ItemDto>(transaction.IdObject);
            if (transaction.Type == TransType.Update)
            {
                (ItemDto items, ObjectiveDto objective, ProjectDto project) = ObjectModel.GetItem(id);
                if (project != null)
                {
                    await yandex.UploadItemAsync(items, project, objective);
                    transaction.Sync = true;
                }
            }
            else if (transaction.Type == TransType.Delete)
            {
                (ItemDto item, ObjectiveDto objective, ProjectDto project) = await yandex.GetItemAsync(id);
                        
                if (project != null)
                    await yandex.DeleteItem(id, project, objective);
            }
        }

        private async Task SynchrinizeObjectiveFromServerAsync(Transaction transaction)
        {
            var id = new ID<ObjectiveDto>(transaction.IdObject);
            if (transaction.Type == TransType.Update)
            {
                (ObjectiveDto objective, ProjectDto project) = ObjectModel.GetObjective(id);
                if (project != null)
                    await yandex.UploadObjectiveAsync(objective, project);
            }
            else if (transaction.Type == TransType.Delete)
            {
                var projects = ObjectModel.GetProjects();
                foreach (var project in projects)
                {
                    var idObjectives = await yandex.GetObjectivesIdAsync(project);
                    if (idObjectives.Any(x => x == id))
                    {
                        await yandex.DeleteObjective(project, id);
                        //
                        // TODO : Надо удалить все Items
                        //

                        break;
                    }
                }
            }
        }

        private async Task SynchrinizeProjectFromServerAsync(Transaction transaction)
        {
            var id = new ID<ProjectDto>(transaction.IdObject);
            ProjectDto project = await yandex.GetProjectAsync(id);
            if (transaction.Type == TransType.Update)
            {
                ProjectDto projectNew = ObjectModel.GetProject(id);
                if (project == null)
                {// Создаем новый проект
                    await yandex.CreateProjectDir(projectNew);
                }
                else
                { // Изменяем старый
                    await yandex.RenameProjectDir(project, projectNew);
                }
                await yandex.UnloadProject(projectNew);
            }
            else if (transaction.Type == TransType.Delete)
            {
                if (project != null)
                {
                    await yandex.DeleteProject(project);
                    await yandex.DeleteProjectDir(project);
                }
            }
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
            if (revision > Revision)
            {
                // Скачиваем изменения с сервера
                List<DateTime> dates = await yandex.GetRevisionsDatesAsync();
                foreach (var date in dates)
                {
                    var trans = await yandex.GetTransactionsAsync(date);
                    bool stop = false;
                    foreach (var tran in trans)
                    {
                        if (tran.Rev > Revision)
                        {
                            tran.Server = true;
                            transactions.Add(tran);
                        }
                        else
                        {
                            stop = true;
                            break;
                        }
                    }
                    SaveTrans(trans, date);
                    if (stop) break;
                }
            }
            //Revision = revision;
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
            Revision = await yandex.GetRevisionAsync() + 1;
            // Применить список операции исходящих на сервер
            foreach (var transaction in transactions)
            {
                transaction.Rev = Revision;
                await SynchrinizeFromServerAsync(transaction);
                transaction.Sync = true;
                transaction.Server = true;
                current++;
                ProgressChange?.Invoke(current, total);
            }
            //  Добавиь на сервер новые операции
            List<Transaction> serverTranNow = await yandex.GetTransactionsAsync(DateTime.Now);
            serverTranNow.AddRange(transactions);
            await yandex.SetTransactionsAsync(DateTime.Now, serverTranNow);
            SaveTrans(serverTranNow, DateTime.Now);
            // Присвоить ревизии новый номер
            yandex.SetRevisionAsync(Revision);
        }

        /// <summary>
        /// Определить список операции исходящих на сервер
        /// </summary>
        /// <returns></returns>
        private List<Transaction> GetTransactionsLocal(ulong revision)
        {
            List<Transaction> result = new List<Transaction>();
            var transDir = new DirectoryInfo(PathManager.GetTransactionsDir());
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

            ulong revision = await yandex.GetRevisionAsync();
            // Скачиваем изменения с сервера
            List<DateTime> dates = await yandex.GetRevisionsDatesAsync();
            foreach (var date in dates)
            {
                var trans = await yandex.GetTransactionsAsync(date);
                foreach (var tran in trans)
                {
                    tran.Server = true;
                    transactions.Add(tran);
                }
                SaveTrans(trans, date);
            }
            return transactions;
        }



        public async Task<List<Transaction>> GetNonSyncTransactionAsync()
        {
            ulong revision = await yandex.GetRevisionAsync();
            List<Transaction> transactions = GetTransactionsLocal(revision);
            List<Transaction> transactionsServer = await GetTransactionsServerAsync(revision);
            transactions.AddRange(transactionsServer);
            return transactions;
        }



        public void Initialize(string accessToken)
        {
            if (yandex == null)
            {
                yandex = new YandexDiskManager(accessToken);
                yandex.TempDir = PathManager.TEMP_DIR;
                Task.Delay(5000);
            }
        }
        public void Load()
        {
            var list = GetLocalTransaction();
            Transactions.Clear();
            Transactions.AddRange(list);
            Revision = GetLocalRevision();
        }

        private void SaveTrans(List<Transaction> trans, DateTime date)
        {
            string dirTrans = PathManager.GetTransactionsDir();
            if (!Directory.Exists(dirTrans)) Directory.CreateDirectory(dirTrans);

            string filename = PathManager.GetTransactionFile(date);

            string json = JsonConvert.SerializeObject(trans, Formatting.Indented);
            File.WriteAllText(filename, json);
        }

        public void Save()
        {
            var list = Transactions.Where(x => x.Rev == 0 && !x.Server).ToList();

            string dirTrans = PathManager.GetTransactionsDir();
            if (!Directory.Exists(dirTrans)) Directory.CreateDirectory(dirTrans);

            string filename = PathManager.GetRevisionFile();
            File.WriteAllText(filename, Revision.ToString());
            filename = PathManager.GetTransactionFile();

            string json = JsonConvert.SerializeObject(list, Formatting.Indented);
            File.WriteAllText(filename, json);
        }
        public List<Transaction> GetLocalTransaction()
        {
            string dirTrans = PathManager.GetTransactionsDir();
            if (Directory.Exists(dirTrans))
            {
                string filename = PathManager.GetTransactionFile();
                if (File.Exists(filename))
                {
                    string jcon = File.ReadAllText(filename);
                    List<Transaction> list = JsonConvert.DeserializeObject<List<Transaction>>(jcon);
                    return list;
                }
            }
            return new List<Transaction>();
        }

        public List<Transaction> GetTransactions(DateTime date)
        {
            string dirTrans = PathManager.GetTransactionsDir();
            if (Directory.Exists(dirTrans))
            {
                string filename = PathManager.GetTransactionFile(date);
                return GetTransactions(filename);
            }
            return new List<Transaction>();
        }
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
            string dirTrans = PathManager.GetTransactionsDir();
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
    }
}