using Microsoft.Win32;
using MRS.DocumentManagement.Base;
using MRS.DocumentManagement.Connection.YandexDisk;
using MRS.DocumentManagement.Dialogs;
using MRS.DocumentManagement.Interface.Dtos;
using MRS.DocumentManagement.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;

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

        public ObservableCollection<ObjectiveModel> Objectives { get; set; } = new ObservableCollection<ObjectiveModel>();
        public ObservableCollection<ProjectModel> Projects { get; set; } = new ObservableCollection<ProjectModel>();
        public ProjectModel SelectedProject { get => selectedProject; set { selectedProject = value; OnPropertyChanged(); } }
        public ObjectiveModel SelectedObjective { get => selectedObjective; set { selectedObjective = value; OnPropertyChanged(); } }
        public ObjectiveModel EditObjective { get => editObjective; set { editObjective = value; OnPropertyChanged(); } }
        public bool IsLocalDB { get => isLocalDB; set { isLocalDB = value; OnPropertyChanged(); } }

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

            if (!Directory.Exists(DIR_NAME)) Directory.CreateDirectory(DIR_NAME);
            if (IsLocalDB)
            {
                LoadProjectOffline(null);
                LoadObjectiveOffline(null);
            }
        }

        private async void UploadObjectiveAsync(object obj)
        {
            ChechYandex();
            if (WinBox.ShowQuestion("Загрузить Objective на диск?"))
            {
                var objs = Objectives.Select(x => x.dto).ToArray();
                await yandex.SetObjectivesAsync(objs, SelectedProject.dto);
            }
        }

        private async void DownloadObjectiveAsync(object obj)
        {
            ChechYandex();
            if (WinBox.ShowQuestion("Скачивать Objective с диска?"))
            {
                ObjectiveDto[] collect = await yandex.GetObjectivesAsync(SelectedProject.dto);
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

        private void OpenFileOffline(object obj)
        {
            //string dirName = Path.Combine(DIR_NAME, SelectedProject.Title);
            string fileName = Path.Combine(DIR_NAME, SelectedProject.Title, OBJECTIVE_FILE);
            OpenHelper.Geany(fileName);
        }

        private void DeleteObjectiveOffline(object obj)
        {
            if (SelectedObjective != null)
            {
                Objectives.Remove(SelectedObjective);
                SaveObjectiveOffline(null);
            }
        }

        private void ChengeStatusOffline(object obj)
        {
            if (obj is string str)
            {
                if (SelectedObjective == null)
                    return;

                var status = Enum.Parse<ObjectiveStatus>(str);
                //SelectedObjective.Status = status;
                //SelectedObjective.DueDate = DateTime.Now;

                foreach (var item in Objectives)
                {
                    if (item.ID == SelectedObjective.ID)
                    {
                        item.Status = status;
                        item.DueDate = DateTime.Now;
                        break;
                    }
                }
                OnPropertyChanged("SelectedObjective");
                OnPropertyChanged("Objectives");
            }
        }

        private void SaveObjectiveOffline(object obj)
        {
            string dirName = Path.Combine(DIR_NAME, SelectedProject.Title);
            if (!Directory.Exists(dirName)) Directory.CreateDirectory(dirName);
            string fileName = Path.Combine(dirName, OBJECTIVE_FILE);
            try
            {
                var json = JsonConvert.SerializeObject(Objectives, Formatting.Indented);
                File.WriteAllText(fileName, json);

            }
            catch (Exception ex)
            {
                WinBox.ShowMessage($"Хуйня:{ex.Message}");
            }
            //FileInfo file = new FileInfo(fileName);
            //OpenHelper.Geany(fileName);
        }

        private void LoadObjectiveOffline(object obj)
        {
            string dirName = Path.Combine(DIR_NAME, SelectedProject.Title);
            if (!Directory.Exists(dirName)) Directory.CreateDirectory(dirName);
            string fileName = Path.Combine(dirName, OBJECTIVE_FILE);
            if (File.Exists(fileName))
            {
                var json = File.ReadAllText(fileName);
                List<ObjectiveDto> collection = JsonConvert.DeserializeObject<List<ObjectiveDto>>(json);

                Objectives.Clear();
                foreach (ObjectiveDto item in collection)
                {
                    Objectives.Add(new ObjectiveModel(item));
                }
            }
        }

        private void AddObjectiveOffline(object obj)
        {
            ObjectiveModel model = new ObjectiveModel();
            if (Objectives.Count != 0)
            {
                var max = Objectives.Max(x => x.ID);
                model.ID =max + 1;
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
            SaveObjectiveOffline(null);
        }

        private void LoadProjectOffline(object obj)
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