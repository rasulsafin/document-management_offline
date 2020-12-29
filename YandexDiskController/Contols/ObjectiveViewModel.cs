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
using System.Text;
using MRS.DocumentManagement.Connection.YandexDisk.Synchronizer;

namespace MRS.DocumentManagement.Contols
{
    public class ObjectiveViewModel : BaseViewModel
    {
        #region Data and bending
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

        public ObservableCollection<ObjectiveModel> Objectives { get; set; } = ObjectModel.Objectives;
        public ObservableCollection<ProjectModel> Projects { get; set; } = ObjectModel.Projects;
        public ProjectModel SelectedProject
        {
            get => selectedProject;
            set
            {
                selectedProject = value;
                UpdateObjective();
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

        public HCommand UpdateProjectsCommand { get; }
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
        public HCommand GetIDCommand { get; }
        public HCommand UpdateObjectiveOfflineCommand { get; }
        public HCommand GetObjectiveForIdCommand { get; }
        public HCommand<int> AddObjectivesCommand { get; } 
        #endregion

        public ObjectiveViewModel()
        {
            UpdateProjectsCommand = new HCommand(UpdateProjects);
            AddObjectiveOfflineCommand = new HCommand(CreateObjective);
            DeleteObjectiveOfflineCommand = new HCommand(DeleteObjective);
            LoadObjectiveOfflineCommand = new HCommand(UpdateObjective);
            SaveObjectiveOfflineCommand = new HCommand(SaveObjectiveOffline);
            ChengeStatusOfflineCommand = new HCommand(ChengeStatusOffline);
            UpdateObjectiveOfflineCommand = new HCommand(UpdateObjectiveOffline);

            GetObjectiveForIdCommand = new HCommand(GetObjectiveForID);

            UploadObjectiveCommand = new HCommand(UploadToServerAsync);
            DownloadObjectiveCommand = new HCommand(DownloadFromServerAsync);
            GetIDCommand = new HCommand(GetIDAsync);
            AddObjectivesCommand = new HCommand<int>(AddObjectives);


            UpdateProjects();
        }

        private void GetObjectiveForID()
        {
            if (WinBox.ShowInput(
                title: "Открыть задачу",
                question: "Введи id",
                input: out string input,
                okText: "Открыть", cancelText: "Отмена",
                defautValue: SelectedObjective == null ? "" : SelectedObjective.ID.ToString()
                ))
            {
                if (int.TryParse(input, out int id))
                {
                    (ObjectiveDto objective, ProjectDto project) = GetObjective((ID<ObjectiveDto>)id);
                    string message = "ObjectiveDto:\n";
                    if (objective != null)
                    {
                        message += $"ID={objective.ID}\n";
                        message += $"ProjectID={objective.ProjectID}\n";
                        message += $"Title={objective.Title}\n";
                        message += $"Status={objective.Status}\n";
                        message += $"Description={objective.Description}\n";
                        message += $"CreationDate={objective.CreationDate}\n";
                        message += $"DueDate={objective.DueDate}\n";
                    }
                    message += $"project:\n";
                    if (project != null)
                    {
                        message += $"ID={project.ID}\n";
                        message += $"Title={project.Title}\n";
                    }
                    WinBox.ShowMessage(message);
                }
            }    
        }

        

        private async void GetIDAsync()
        {
            ChechYandex();
            try
            {
                List<ID<ObjectiveDto>> list = await yandex.GetObjectivesIdAsync(SelectedProject.dto);
                StringBuilder message = new StringBuilder();
                for (int i = 0; i < list.Count; i++)
                {
                    if (i != 0) message.Append(',');
                    if (i % 10 == 0) message.Append('\n');
                    message.Append(list[i].ToString());
                }
                WinBox.ShowMessage(message.ToString());
            }
            catch (FileNotFoundException fileNot)
            {
                WinBox.ShowMessage("Папка проекта отсутвует на диске!");
            }
            catch (Exception ex)
            {
                WinBox.ShowMessage(ex.Message);
            }
        }

        private async void UploadToServerAsync()
        {
            ChechYandex();
            if (WinBox.ShowQuestion($"Загрузить Objective '{SelectedObjective.Title}' на диск?"))
            {
                //var objs = Objectives.Select(x => x.dto).ToArray();
                //    ProgressMax = objs.Length;
                //ProgressVisible = true;
                //ProgressValue =0;

                //foreach (var item in objs)
                //{
                await yandex.UploadObjectiveAsync(SelectedObjective.dto, SelectedProject.dto);
                //ProgressValue++;
                //}
                //ProgressVisible = false;
            }
        }

        public static void DeleteObjective(ProjectDto project, ObjectiveDto objective)
        {
            throw new NotImplementedException();
        }

        public static (ObjectiveDto objective, ProjectDto project) GetObjective(ID<ObjectiveDto> id)
        {
            var projects = ObjectModel.GetProjects();
            foreach (var project in projects)
            {
                var dir = PathManager.GetObjectivesDir(project);
                DirectoryInfo dirInfoObj = new DirectoryInfo(dir);
                if (dirInfoObj.Exists)
                {
                    foreach (var item in dirInfoObj.GetFiles())
                    {
                        if (PathManager.TryParseObjectiveId(item.Name, out ID<ObjectiveDto> _id) && id == _id)
                        {
                            var json = File.ReadAllText(item.FullName);
                            ObjectiveDto objective = JsonConvert.DeserializeObject<ObjectiveDto>(json);
                            return (objective, project);
                        }
                    }
                }
            }
            return (null, null);
        }

        private async void DownloadFromServerAsync()
        {
            ChechYandex();
            if (WinBox.ShowInput(
                title: "Скачивание...",
                question: "Введите id оbjective которую надо скачать с диска?",
                input: out string text,
                okText: "Скачать",
                cancelText: "Отмена"))
            {
                if (int.TryParse(text, out int num))
                {
                    ID<ObjectiveDto> id = (ID<ObjectiveDto>)num;
                    ObjectiveDto objective = await yandex.GetObjectiveAsync(SelectedProject.dto, id);
                    Objectives.Add((ObjectiveModel)objective);
                }

                //ProgressVisible = true;
                //ObjectiveDto[] collect = await yandex.GetObjectiveAsync(SelectedProject.dto);
                //ProgressVisible = false;
                //if (collect == null)
                //    WinBox.ShowMessage("Скачивание завершилось провалом!");
                //else
                //{
                //    Objectives.Clear();
                //    foreach (ObjectiveDto item in collect)
                //    {
                //        Objectives.Add(new ObjectiveModel(item));
                //    }
                //}
            }
        }

        private void UpdateObjectiveOffline()
        {
            SelectedObjective.Title = EditObjective.Title;
            SelectedObjective.Description = EditObjective.Description;
            SelectedObjective.Status = EditObjective.Status;
            SelectedObjective.DueDate = DateTime.Now;
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

        private void UpdateObjective()
        {
            if (SelectedProject != null)
            {
                string objDir = PathManager.GetObjectivesDir(SelectedProject.dto);
                var list = GetObjectives(SelectedProject.dto);
                list.Sort((x, y) => ((int)x.ID).CompareTo((int)y.ID));

                Objectives.Clear();
                foreach (ObjectiveDto item in list)
                {
                    Objectives.Add(new ObjectiveModel(item));
                }
            }
        }

        private void DeleteObjective()
        {
            if (SelectedObjective != null)
            {
                if (WinBox.ShowQuestion($"Удалить задание {SelectedObjective.Title}?", "Удаление"))
                {
                    string filename = PathManager.GetObjectiveFile(SelectedProject.dto, SelectedObjective.dto);
                    File.Delete(filename);
                    Objectives.Remove(SelectedObjective);
                }
            }
        }

        private void CreateObjective()
        {
            ObjectiveModel model = new ObjectiveModel();
            model.ID = ++Properties.Settings.Default.ObjectiveNextId;
            Properties.Settings.Default.Save();
            model.ProjectID = SelectedProject.ID;
            model.CreationDate = DateTime.Now;
            model.DueDate = DateTime.Now;
            model.Title = EditObjective.Title;
            model.Description = EditObjective.Description;
            model.Status = ObjectiveStatus.Undefined;
            //obj.TaskType = new ObjectiveTypeDto();            

            Objectives.Add(model);
            SaveObjective(SelectedProject.dto, model.dto);
        }

        private void SaveObjectiveOffline()
        {
            var collect = Objectives.Select(x => x.dto).ToList();
            SetObjectives(collect, SelectedProject.dto);
        }

        private void UpdateProjects()
        {
            ObjectModel.UpdateProject();
            //List<ProjectDto> collection = ObjectModel.GetProjects();

            //Projects.Clear();
            //foreach (ProjectDto item in collection)
            //{
            //    Projects.Add(new ProjectModel(item));
            //}
            //if (Projects.Count != 0)
            //    SelectedProject = Projects.First();
        }

        public static void SetObjectives(List<ObjectiveDto> objectives, ProjectDto project)
        {
            foreach (var objective in objectives)
            {
                SaveObjective(project, objective);
            }

        }

        public static void SaveObjective(ProjectDto project, ObjectiveDto objective)
        {
            string dirProj = PathManager.GetProjectDir(project);
            if (!Directory.Exists(dirProj)) Directory.CreateDirectory(dirProj);

            string dirObj = PathManager.GetObjectivesDir(project);
            if (!Directory.Exists(dirObj)) Directory.CreateDirectory(dirObj);

            string filename = PathManager.GetObjectiveFile(project, objective);
            var json = JsonConvert.SerializeObject(objective, Formatting.Indented);
            File.WriteAllText(filename, json);
        }

        public static List<ObjectiveDto> GetObjectives(ProjectDto project)
        {
            var result = new List<ObjectiveDto>();

            string dirProj = PathManager.GetProjectDir(project);
            if (!Directory.Exists(dirProj)) return result;

            string dirObj = PathManager.GetObjectivesDir(project);
            if (!Directory.Exists(dirObj)) return result;

            DirectoryInfo dirInfoObj = new DirectoryInfo(dirObj);

            foreach (var item in dirInfoObj.GetFiles())
            {
                if (item.Name.StartsWith("objective"))
                {
                    var json = File.ReadAllText(item.FullName);
                    ObjectiveDto objective = JsonConvert.DeserializeObject<ObjectiveDto>(json);
                    result.Add(objective);
                }
            }
            return result;
        }

        private void UpdateProgress(ulong current, ulong total)
        {
            ProgressMax = total;
            progressValue = current;
        }



        private void AddObjectives(int obj)
        {
            string[] names;
            string[] discriptions;

            string namesFile = "Names.txt";
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

            string discriptionsFile = "Discriptions.txt";
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
                
                model.ID = Properties.Settings.Default.ObjectiveNextId++;

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
                Properties.Settings.Default.Save();
            SaveObjectiveOffline();
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