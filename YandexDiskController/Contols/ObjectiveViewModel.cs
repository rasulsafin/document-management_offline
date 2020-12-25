using Microsoft.Win32;
using WPFStorage.Base;
using MRS.DocumentManagement.Connection.YandexDisk;
using WPFStorage.Dialogs;
using MRS.DocumentManagement.Interface.Dtos;
using MRS.DocumentManagement.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;

namespace MRS.DocumentManagement.Contols
{
    public class ObjectiveViewModel : BaseViewModel
    {
        private static readonly string DIR_NAME = "data";
        private static readonly string OBJECTIVE_FILE = "objective.json";
        private static readonly string PROJECT_FILE = "projects.json";
        private static readonly string TEMP_DIR = "Temp.Yandex";
        YandexDiskManager yandex;
        ProjectModel selectedProject;
        private ObjectiveModel selectedObjective;
        private ObjectiveModel editObjective = new ObjectiveModel();
        bool isLocalDB = true;
        private string statusOperation;
        private bool progressVisible;
        private double progressMax;
        private double progressValue;

        public ObservableCollection<ObjectiveModel> Objectives { get; set; } = new ObservableCollection<ObjectiveModel>();
        public ObservableCollection<ProjectModel> Projects { get; set; } = new ObservableCollection<ProjectModel>();
        public ProjectModel SelectedProject
        {
            get => selectedProject;
            set
            {
                selectedProject = value;
                LoadObjectiveOffline();
                OnPropertyChanged();
            }
        }
        public ObjectiveModel SelectedObjective
        {
            get => selectedObjective;
            set
            {
                selectedObjective = value;
                EditObjective = selectedObjective;
                OnPropertyChanged();
            }
        }
        public ObjectiveModel EditObjective { get => editObjective; set { editObjective = value; OnPropertyChanged(); } }
        public bool IsLocalDB { get => isLocalDB; set { isLocalDB = value; OnPropertyChanged(); } }
        public bool ProgressVisible { get => progressVisible; set { progressVisible = value; OnPropertyChanged(); } }
        public double ProgressValue { get => progressValue; set { progressValue = value; OnPropertyChanged(); } }
        public double ProgressMax { get => progressMax; set { progressMax = value; OnPropertyChanged(); } }
        public string StatusOperation { get => statusOperation; set { statusOperation = value; OnPropertyChanged(); } }

        public HCommand<bool> LocalDBCommand { get; }
        public HCommand LoadProjectOfflineCommand { get; }
        public HCommand AddObjectiveOfflineCommand { get; }
        public HCommand LoadObjectiveOfflineCommand { get; }
        public HCommand SaveObjectiveOfflineCommand { get; }
        public HCommand ChengeStatusOfflineCommand { get; }
        public HCommand DeleteObjectiveOfflineCommand { get; }
        public HCommand OpenFileOfflineCommand { get; }
        public HCommand UploadObjectiveCommand { get; }
        public HCommand DownloadObjectiveCommand { get; }
        public HCommand UpdateObjectiveOfflineCommand { get; }
        public HCommand<int> AddObjectivesCommand { get; }

        public ObjectiveViewModel()
        {
            LocalDBCommand = new HCommand<bool>(LocalBase);
            LoadProjectOfflineCommand = new HCommand(LoadProjectOffline);
            AddObjectiveOfflineCommand = new HCommand(AddObjectiveOffline);
            LoadObjectiveOfflineCommand = new HCommand(LoadObjectiveOffline);
            SaveObjectiveOfflineCommand = new HCommand(SaveObjectiveOffline);
            ChengeStatusOfflineCommand = new HCommand(ChengeStatusOffline);
            DeleteObjectiveOfflineCommand = new HCommand(DeleteObjectiveOffline);
            OpenFileOfflineCommand = new HCommand(OpenFileOffline);
            UploadObjectiveCommand = new HCommand(UploadObjectiveAsync);
            DownloadObjectiveCommand = new HCommand(DownloadObjectiveAsync);
            UpdateObjectiveOfflineCommand = new HCommand(UpdateObjectiveOffline);
            AddObjectivesCommand = new HCommand<int>(AddObjectives);

            if (!Directory.Exists(DIR_NAME)) Directory.CreateDirectory(DIR_NAME);
            if (IsLocalDB)
            {
                LoadProjectOffline();
                LoadObjectiveOffline();
            }
        }




        private void AddObjectives(int obj)
        {
            string[] names;
            string[] discriptions;

            string namesFile = Path.Combine(DIR_NAME, "Names.txt");
            if (!File.Exists(namesFile))
            {
                if (WinBox.ShowQuestion("Файл с именами не существует создать его?"))
                {
                    OpenHelper.Geany(namesFile);
                }
                return;
            }
            else
            {
                names = File.ReadAllLines(namesFile);
            }

            string discriptionsFile = Path.Combine(DIR_NAME, "Discriptions.txt");
            if (!File.Exists(discriptionsFile))
            {
                if (WinBox.ShowQuestion("Файл с описаниями не существует создать его?"))
                {
                    OpenHelper.Geany(discriptionsFile);
                }
                return;
            }
            else
            {
                discriptions = File.ReadAllLines(discriptionsFile);
            }
            if (discriptions == null || names == null) return;

            Random random = new Random();

            for (int i = 0; i < obj; i++)
            {
                ObjectiveModel model = new ObjectiveModel();
                if (Objectives.Count != 0)
                {
                    var max = Objectives.Max(x => x.ID);
                    model.ID = max + 1;
                }
                else
                    model.ID = 1;

                model.ProjectID = SelectedProject.ID;
                model.CreationDate = DateTime.Now;
                model.DueDate = DateTime.Now;

                int index = random.Next(0, names.Length);

                model.Title = names[index];

                index = random.Next(0, discriptions.Length);
                model.Description = discriptions[index];

                model.Status = (ObjectiveStatus)random.Next(0, 4);

                Objectives.Add(model);
            }
            SaveObjectiveOffline();
        }

        private async void UploadObjectiveAsync()
        {
            ChechYandex();
            if (WinBox.ShowQuestion("Загрузить Objective на диск?"))
            {
                var objs = Objectives.Select(x => x.dto).ToArray();
                ProgressVisible = true;
                await yandex.SetObjectivesAsync(objs, SelectedProject.dto, UpdateProgress);
                ProgressVisible = false;
            }
        }

        private async void DownloadObjectiveAsync()
        {
            ChechYandex();
            if (WinBox.ShowQuestion("Скачивать Objective с диска?"))
            {
                ProgressVisible = true;
                ObjectiveDto[] collect = await yandex.GetObjectivesAsync(SelectedProject.dto, UpdateProgress);
                ProgressVisible = false;
                if (collect == null)
                    WinBox.ShowMessage("Скачивание завершилось провалом!");
                else
                {
                    Objectives.Clear();
                    foreach (ObjectiveDto item in collect)
                    {
                        Objectives.Add(new ObjectiveModel(item));
                    }
                }
            }
        }

        private void UpdateProgress(ulong current, ulong total)
        {
            ProgressMax = total;
            progressValue = current;
        }

        private void OpenFileOffline()
        {
            //string dirName = Path.Combine(DIR_NAME, SelectedProject.Title);
            string fileName = Path.Combine(DIR_NAME, SelectedProject.Title, OBJECTIVE_FILE);
            OpenHelper.Geany(fileName);
        }

        private void DeleteObjectiveOffline()
        {
            if (SelectedObjective != null)
            {
                Objectives.Remove(SelectedObjective);
                SaveObjectiveOffline();
            }
        }

        private void ChengeStatusOffline()
        {
            if (EditObjective == null)
                return;

            string[] collect = Enum.GetNames<ObjectiveStatus>();
            var newVal = WinBox.SelectorBox(collect);

            var status = Enum.Parse<ObjectiveStatus>(newVal);

            EditObjective.Status = status;
        }

        private void SaveObjectiveOffline()
        {
            string dirName = Path.Combine(DIR_NAME, SelectedProject.Title);
            if (!Directory.Exists(dirName)) Directory.CreateDirectory(dirName);
            string fileName = Path.Combine(dirName, OBJECTIVE_FILE);
            try
            {
                var collect = Objectives.Select(x => x.dto).ToArray();


                var json = JsonConvert.SerializeObject(collect);
                //var json = JsonConvert.SerializeObject(collect, Formatting.Indented);
                File.WriteAllText(fileName, json);

            }
            catch (Exception ex)
            {
                WinBox.ShowMessage($"Хуйня:{ex.Message}");
            }
            //FileInfo file = new FileInfo(fileName);
            //OpenHelper.Geany(fileName);
        }

        private void LoadObjectiveOffline()
        {
            StatusOperation = "Загружаю задания";
            string dirName = Path.Combine(DIR_NAME, SelectedProject.Title);
            if (!Directory.Exists(dirName)) Directory.CreateDirectory(dirName);
            string fileName = Path.Combine(dirName, OBJECTIVE_FILE);
            try
            {
                Objectives.Clear();
                if (File.Exists(fileName))
                {
                    StatusOperation = "Читаю файл " + fileName;
                    var json = File.ReadAllText(fileName);
                    StatusOperation = "DeserializeObject ";

                    List<ObjectiveDto> collection = JsonConvert.DeserializeObject<List<ObjectiveDto>>(json);
                    StatusOperation = "Add ";
                    foreach (ObjectiveDto item in collection)
                    {
                        Objectives.Add(new ObjectiveModel(item));
                    }
                }
            }
            catch (Exception ex)
            {
                OpenHelper.LoadExeption(ex, fileName);
            }
            finally
            {
                StatusOperation = " ";

            }
        }

        private void AddObjectiveOffline()
        {
            ObjectiveModel model = new ObjectiveModel();
            if (Objectives.Count != 0)
            {
                var max = Objectives.Max(x => x.ID);
                model.ID = max + 1;
            }
            else
                model.ID = 1;

            model.ProjectID = SelectedProject.ID;
            model.CreationDate = DateTime.Now;
            model.DueDate = DateTime.Now;
            model.Title = EditObjective.Title;
            model.Description = EditObjective.Description;
            model.Status = ObjectiveStatus.Undefined;
            //obj.TaskType = new ObjectiveTypeDto();

            Objectives.Add(model);
            SaveObjectiveOffline();
        }
        private void UpdateObjectiveOffline()
        {
            SelectedObjective.Title = EditObjective.Title;
            SelectedObjective.Description = EditObjective.Description;
            SelectedObjective.Status = EditObjective.Status;
            SelectedObjective.DueDate = DateTime.Now;
        }
        private void LoadProjectOffline()
        {
            var fileName = Path.Combine(DIR_NAME, PROJECT_FILE);
            if (!File.Exists(fileName))
            {
                OpenFileDialog OPF = new OpenFileDialog();
                OPF.Filter = $"PROJECT_FILE|{PROJECT_FILE}|Все файлы|*.*";
                if (OPF.ShowDialog() == true)
                {
                    //WinBox.ShowMessage($"{OPF.FileName}");
                    File.Copy(OPF.FileName, fileName);
                }
            }
            var json = File.ReadAllText(fileName);
            List<ProjectDto> collection = JsonConvert.DeserializeObject<List<ProjectDto>>(json);

            Projects.Clear();
            foreach (ProjectDto item in collection)
            {
                Projects.Add(new ProjectModel(item));
            }
            SelectedProject = Projects.First();
        }

        private void LocalBase(bool obj)
        {
            //WinBox.ShowMessage($"{obj}");
        }

        private void ChechYandex()
        {
            if (yandex == null)
            {
                yandex = new YandexDiskManager(MainViewModel.AccessToken);
                yandex.TempDir = TEMP_DIR;
            }
        }
    }
}