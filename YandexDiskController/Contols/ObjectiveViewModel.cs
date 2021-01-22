using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using MRS.DocumentManagement.Connection;
using MRS.DocumentManagement.Interface.Dtos;
using MRS.DocumentManagement.Models;
using WPFStorage.Base;
using WPFStorage.Dialogs;

namespace MRS.DocumentManagement.Contols
{
    public class ObjectiveViewModel : BaseViewModel
    {
        #region Data and bending
        // private ProjectModel selectedProject;
        // private ObjectiveModel selectedObjective;
        private ObjectiveModel editObjective = new ObjectiveModel();
        private bool isLocalDB = true;
        private string statusOperation;
        private bool progressVisible;
        private double progressMax;
        private double progressValue;
        private DiskManager disk;

        public ObjectiveViewModel()
        {
            ZeroIdCommand = new HCommand(ZeroId);
            AddObjectiveOfflineCommand = new HCommand(AddObjective);

            // UpdateProjectsCommand = new HCommand(UpdateProjects);
            DeleteObjectiveOfflineCommand = new HCommand(DeleteObjective);
            LoadObjectiveOfflineCommand = new HCommand(UpdateObjectives);
            ChengeStatusOfflineCommand = new HCommand(ChengeStatusOffline);
            UpdateObjectiveOfflineCommand = new HCommand(UpdateObjectiveOffline);
            AddObjectivesCommand = new HCommand<int>(AddObjectives);
            PushCommand = new HCommand(Push);

            Auth.LoadActions.Add(Initialization);
        }

        private void Initialization(string token)
        {
            disk = new DiskManager(token);
        }

        public ObservableCollection<ObjectiveModel> Objectives { get; set; } = ObjectModel.Objectives;

        public ObservableCollection<UserModel> Users { get; set; } = ObjectModel.Users;

        public ObservableCollection<ProjectModel> Projects { get; set; } = ObjectModel.Projects;

        public ProjectModel SelectedProject
        {
            get => ObjectModel.SelectedProject;
            set
            {
                ObjectModel.SelectedProject = value;
                if (Objectives.Count > 0)
                    SelectedObjective = Objectives.First();
                OnPropertyChanged();
            }
        }

        public ObjectiveModel SelectedObjective
        {
            get => ObjectModel.SelectedObjective;
            set
            {
                ObjectModel.SelectedObjective = value;
                if (EditObjective == null) EditObjective = new ObjectiveModel();
                if (ObjectModel.SelectedObjective != null)
                {
                    EditObjective.AuthorID = ObjectModel.SelectedObjective.AuthorID;
                    EditObjective.CreationDate = ObjectModel.SelectedObjective.CreationDate;
                    EditObjective.Description = ObjectModel.SelectedObjective.Description;
                    EditObjective.DueDate = ObjectModel.SelectedObjective.DueDate;
                    EditObjective.ID = ObjectModel.SelectedObjective.ID;
                    EditObjective.ObjectiveTypeID = ObjectModel.SelectedObjective.ObjectiveTypeID;
                    EditObjective.ParentObjectiveID = ObjectModel.SelectedObjective.ParentObjectiveID;
                    EditObjective.Status = ObjectModel.SelectedObjective.Status;
                    EditObjective.Title = ObjectModel.SelectedObjective.Title;
                }
                else
                {
                    EditObjective = new ObjectiveModel();
                }

                OnPropertyChanged();
                OnPropertyChanged("Objectives");
            }
        }

        public ObjectiveModel EditObjective
        {
            get => editObjective; set
            {
                // CheckNullSelectProject();
                editObjective = value;
                OnPropertyChanged();
            }
        }

        public bool IsLocalDB
        {
            get => isLocalDB; set
            {
                isLocalDB = value;
                OnPropertyChanged();
            }
        }

        public bool ProgressVisible
        {
            get => progressVisible; set
            {
                progressVisible = value;
                OnPropertyChanged();
            }
        }

        public double ProgressValue
        {
            get => progressValue; set
            {
                progressValue = value;
                OnPropertyChanged();
            }
        }

        public double ProgressMax
        {
            get => progressMax; set
            {
                progressMax = value;
                OnPropertyChanged();
            }
        }

        public string StatusOperation
        {
            get => statusOperation; set
            {
                statusOperation = value;
                OnPropertyChanged();
            }
        }

        public int NextId
        {
            get => Properties.Settings.Default.ObjectiveNextId;
            set
            {
                Properties.Settings.Default.ObjectiveNextId = value;
                Properties.Settings.Default.Save();
                OnPropertyChanged();
            }
        }

        public HCommand UpdateProjectsCommand { get; }

        public HCommand<bool> LocalDBCommand { get; }

        public HCommand LoadProjectOfflineCommand { get; }

        public HCommand ZeroIdCommand { get; }

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

        public HCommand PushCommand { get; }

        #endregion

        public static void DeleteObjective(ProjectDto project, ObjectiveDto objective)
        {
            throw new NotImplementedException();
        }

        private void ZeroId()
        {
            NextId = 1;
        }

        private async void Push()
        {
            if (SelectedObjective == null)
            {
                if (WinBox.ShowInput(
                question: "Нет выбранного задания для отправки.\nВведите ID задания:",
                input: out string text,
                title: "Выбор задания",
                okText: "Отправить",
                cancelText: "Отменить",
                defautValue: "-1") && int.TryParse(text, out int id))
                {
                    SelectedObjective = Objectives.FirstOrDefault(p => p.ID == id);
                }
                else
                {
                    return;
                }
            }

            if (!await disk.Push<ObjectiveDto>(SelectedObjective.dto, SelectedObjective.ID.ToString()))
            {
                WinBox.ShowMessage("Отправить не удалось!");
            }
        }

        private void AddObjective()
        {
            // CheckNullSelectProject();
            var title = EditObjective.Title;
            var description = EditObjective.Description;
            var projectID = SelectedProject.ID;
            ObjectiveModel model = CreateModel(title, description, projectID);
            Objectives.Add(model);
            ObjectModel.SaveObjectives(SelectedProject.dto);
            ObjectModel.Synchronizer.Update(model.dto.ID, SelectedProject.dto.ID);
        }

        private ObjectiveModel CreateModel(string title, string description, int projectID)
        {
            // CheckNullSelectProject();
            ObjectiveModel model = new ObjectiveModel();
            model.ID = NextId++;
            model.ProjectID = projectID;
            model.CreationDate = DateTime.Now;
            model.DueDate = DateTime.Now;
            model.Title = title;
            model.Description = description;
            model.Status = ObjectiveStatus.Undefined;

            if (EditObjective.Author != null)
                model.Author = EditObjective.Author;
            model.ParentObjectiveID = EditObjective.ParentObjectiveID;
            return model;
        }

        private void UpdateObjectiveOffline()
        {
            // CheckNullSelectProject();
            SelectedObjective.Title = EditObjective.Title;
            SelectedObjective.Description = EditObjective.Description;
            SelectedObjective.Status = EditObjective.Status;
            SelectedObjective.DueDate = DateTime.Now;
            SelectedObjective.ParentObjectiveID = EditObjective.ParentObjectiveID;
            SelectedObjective.AuthorID = EditObjective.AuthorID;
            SelectedObjective.ObjectiveTypeID = EditObjective.ObjectiveTypeID;
            // SelectedObjective.ParentObjectiveID = EditObjective.ParentObjectiveID;

            ObjectModel.SaveObjectives(SelectedProject.dto);
            ObjectModel.Synchronizer.Update(SelectedObjective.dto.ID, SelectedProject.dto.ID);
            UpdateObjectives();
        }

        // private void CheckNullSelectProject()
        // {
        //    if (SelectedProject == null) WinBox.ShowMessage("Внимание! Пе выбран проект");
        // }
        private void ChengeStatusOffline()
        {
            if (EditObjective == null)
                return;

            string[] collect = Enum.GetNames<ObjectiveStatus>();
            var newVal = WinBox.SelectorBox(collect);

            var status = Enum.Parse<ObjectiveStatus>(newVal);

            EditObjective.Status = status;
            SelectedObjective.Status = status;
            ObjectModel.SaveObjectives(SelectedProject.dto);
            ObjectModel.Synchronizer.Update(SelectedObjective.dto.ID, SelectedProject.dto.ID);
        }

        private void UpdateObjectives()
        {
            if (SelectedProject != null)
            {
                ObjectModel.UpdateObjectives(SelectedProject.dto);
            }
        }

        private void DeleteObjective()
        {
            if (SelectedObjective != null)
            {
                if (WinBox.ShowQuestion($"Удалить задание {SelectedObjective.Title}?", "Удаление"))
                {
                    ObjectModel.Synchronizer.Delete(SelectedObjective.dto.ID, SelectedProject.dto.ID);

                    SelectedObjective.ID = -1;
                    ObjectModel.SaveObjectives(SelectedProject.dto);
                    UpdateObjectives();
                }
            }
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
                int index = random.Next(0, names.Length);
                string title = names[index];
                index = random.Next(0, discriptions.Length);
                string description = discriptions[index];
                ObjectiveModel model = CreateModel(title, description, SelectedProject.ID);
                model.Status = (ObjectiveStatus)random.Next(0, 4);
                if (SelectedObjective != null)
                    model.ParentObjectiveID = SelectedObjective.ID;
                Objectives.Add(model);

                ObjectModel.Synchronizer.Update(model.dto.ID, SelectedProject.dto.ID);
            }

            ObjectModel.SaveObjectives(SelectedProject.dto);
            UpdateObjectives();
        }
    }
}
