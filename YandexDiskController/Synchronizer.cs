using MRS.DocumentManagement.Connection.YandexDisk;
using MRS.DocumentManagement.Connection.YandexDisk.Synchronizer;
using MRS.DocumentManagement.Contols;
using MRS.DocumentManagement.Interface.Dtos;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace MRS.DocumentManagement
{
    public class Synchronizer
    {
        private YandexDiskManager yandex;

        public ObservableCollection<TransactionModel> Transactions { get; private set; } = new ObservableCollection<TransactionModel>();
        public ulong Revision { get; private set; }

        public void AddTransaction(TransType type, ID<ProjectDto> id)
        {
            Transactions.Add(new TransactionModel() { Type = type, Table = Table.Project, Id = (int)id, Rev = 0 });
        }
        public void AddTransaction(TransType type, ID<ObjectiveDto> id)
        {
            Transactions.Add(new TransactionModel() { Type = type, Table = Table.Objective, Id = (int)id, Rev = 0 });
        }
        public void AddTransaction(TransType type, ID<ItemDto> id)
        {
            Transactions.Add(new TransactionModel() { Type = type, Table = Table.Item, Id = (int)id, Rev = 0 });
        }
        internal async Task<ulong> GetRevisionServerAsync()
        {
            ChechYandex();
            Revision = await yandex.GetRevisionAsync();
            return Revision;
        }

        private async Task SinchrinizeToServerAsync(Transaction transaction)
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
                //
                // TODO: Найти проект и нужное задание и item по id на сервере
                //
                ProjectDto project = null;
                ObjectiveDto objective = null;
                ItemDto item = null;
                ObjectModel.SaveItem(item, project, objective);
            }
            else if (transaction.Type == TransType.Delete)
            {
                //
                // TODO: Найти проект и нужное задание и item по id но уже на компе
                //
                ProjectDto project = null;
                ObjectiveDto objective = null;
                ItemDto item = null;
                ObjectModel.DeleteItem(item, project, objective);
            }
        }
        private async Task SinchrinizeObjectiveToServerAsync(Transaction transaction)
        {
            var id = new ID<ObjectiveDto>(transaction.IdObject);
            if (transaction.Type == TransType.Update)
            {
                //
                // TODO: Найти проект и нужное задание по id на сервере
                //
                ProjectDto project = null;
                ObjectiveDto objective = null;
                ObjectModel.SaveObjective(project, objective);
            }
            else if (transaction.Type == TransType.Delete)
            {
                //
                // TODO: Найти проект и нужное задание по id но уже на компе
                //
                ProjectDto project = null;
                ObjectiveDto objective = null;
                ObjectModel.DeleteObjective(project, objective);
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

        private async Task SinchrinizeFromServerAsync(Transaction transaction)
        {
            switch (transaction.Table)
            {
                case Table.Project: await SinchrinizeProjectFromServerAsync(transaction); break;
                case Table.Objective: await SinchrinizeObjectiveFromServerAsync(transaction); break;
                case Table.Item: await SinchrinizeItemFromServerAsync(transaction); break;
                default:
                    throw new ArgumentException();
            }
        }

        private Task SinchrinizeItemFromServerAsync(Transaction transaction)
        {
            throw new NotImplementedException();
        }

        private async Task SinchrinizeObjectiveFromServerAsync(Transaction transaction)
        {
            var id = new ID<ObjectiveDto>(transaction.IdObject);
            if (transaction.Type == TransType.Update)
            {
                (ObjectiveDto objective, ProjectDto project) = ObjectiveViewModel.GetObjective(id);
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
                    }
                }
            }
        }

        private async Task SinchrinizeProjectFromServerAsync(Transaction transaction)
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

        internal async Task SynchronizeAsync()
        {
            string dirTrans = PathManager.GetTransactionsDir();
            if (!Directory.Exists(dirTrans)) Directory.CreateDirectory(dirTrans);
            ChechYandex();
            ulong revision = await yandex.GetRevisionAsync();
            List<Transaction> transactions = Transactions.Where(t => t.Rev == 0).Select(t => t.Transaction).ToList();
            if (revision > Revision)
            {
                // Скачиваем изменения с сервера
                List<DateTime> dates = await yandex.GetRevisionsDatesAsync();
                List<Transaction> serverTransactions = new List<Transaction>();
                foreach (var date in dates)
                {
                    var trans = await yandex.GetTransactionsAsync(date);
                    bool stop = false;
                    foreach (var tran in trans)
                    {
                        if (tran.Rev > Revision)
                        {
                            serverTransactions.Add(tran);
                        }
                        else
                        {
                            stop = true;
                            break;
                        }
                    }
                    if (stop) break;
                }
                // Пирменяем изменения сервера
                foreach (var transaction in serverTransactions)
                {
                    await SinchrinizeToServerAsync(transaction);
                }

            }
            if (transactions.Count > 0)
            {
                foreach (var transaction in transactions)
                {
                    await SinchrinizeFromServerAsync(transaction);
                }
                List<Transaction> serverTranNow = await yandex.GetTransactionsAsync(DateTime.Now);
                serverTranNow.AddRange(transactions);
                await yandex.SetTransactionsAsync(DateTime.Now, serverTranNow);
                //revision++;

                Revision = ++revision;
                yandex.SetRevisionAsync(Revision);
                serverTranNow.ForEach(x => x.Rev = revision);
                string filename = PathManager.GetTransactionFile(DateTime.Now);
                string json = JsonConvert.SerializeObject(serverTranNow, Formatting.Indented);
                File.WriteAllText(filename, json);
                foreach (var item in Transactions.ToArray())
                {
                    if (item.Rev != 0)
                        Transactions.Remove(item);
                }
            }
        }

        private void ChechYandex()
        {
            if (yandex == null)
            {
                yandex = new YandexDiskManager(MainViewModel.AccessToken);
                yandex.TempDir = MainViewModel.TEMP_DIR;
                Task.Delay(10);
            }
        }

        public void Load()
        {
            string dirTrans = PathManager.GetTransactionsDir();
            if (!Directory.Exists(dirTrans)) return;
            string filename = PathManager.GetRevisionFile();
            if (File.Exists(filename))
            {
                string text = File.ReadAllText(filename);
                if (ulong.TryParse(text, out ulong rev))
                {
                    Revision = rev;
                }
            }
            filename = PathManager.GetTransactionFile();
            if (File.Exists(filename))
            {
                string jcon = File.ReadAllText(filename);
                List<TransactionModel> list = JsonConvert.DeserializeObject<List<TransactionModel>>(jcon);
                foreach (var item in list)
                {
                    if (!item.Sync)
                    {
                        Transactions.Add(item);
                    }
                }
            }
        }


        public void Save()
        {
            string dirTrans = PathManager.GetTransactionsDir();
            if (!Directory.Exists(dirTrans)) Directory.CreateDirectory(dirTrans);

            string filename = PathManager.GetRevisionFile();
            File.WriteAllText(filename, Revision.ToString());
            filename = PathManager.GetTransactionFile();

            string json = JsonConvert.SerializeObject(Transactions, Formatting.Indented);
            File.WriteAllText(filename, json);
        }
    }
}