using MRS.DocumentManagement.Connection;
using MRS.DocumentManagement.Interface.Dtos;
using MRS.DocumentManagement.Models;
using System;
using System.Collections.ObjectModel;
using System.IO;
using WPFStorage.Base;
using WPFStorage.Dialogs;

namespace MRS.DocumentManagement.Contols
{
    public class ObjectiveViewModel : BaseViewModel
    {
        #region Data and bending
        
        DiskManager yandex;
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
                UpdateObjectives();
                OnPropertyChanged();
            }
        }
        public ObjectiveModel SelectedObjective
        {
            get => selectedObjective;
            set
            {
                selectedObjective = value;
                if (EditObjective == null) EditObjective = new ObjectiveModel();
                EditObjective.AuthorID = selectedObjective.AuthorID;
                EditObjective.CreationDate = selectedObjective.CreationDate;
                EditObjective.Description = selectedObjective.Description;
                EditObjective.DueDate = selectedObjective.DueDate;
                EditObjective.ID = selectedObjective.ID;
                EditObjective.Items = selectedObjective.ID;
                UpdateObjectives();
                OnPropertyChanged();
            }
        }
        public ObjectiveModel EditObjective { get => editObjective; set { editObjective = value; OnPropertyChanged(); } }
        public bool IsLocalDB { get => isLocalDB; set { isLocalDB = value; OnPropertyChanged(); } }
        public bool ProgressVisible { get => progressVisible; set { progressVisible = value; OnPropertyChanged(); } }
        public double ProgressValue { get => progressValue; set { progressValue = value; OnPropertyChanged(); } }
        public double ProgressMax { get => progressMax; set { progressMax = value; OnPropertyChanged(); } }
        public string StatusOperation { get => statusOperation; set { statusOperation = value; OnPropertyChanged(); } }

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
        #endregion

        public ObjectiveViewModel()
        {
            ZeroIdCommand = new HCommand(ZeroId);
            AddObjectiveOfflineCommand = new HCommand(CreateObjective);

            UpdateProjectsCommand = new HCommand(UpdateProjects);
            DeleteObjectiveOfflineCommand = new HCommand(DeleteObjective);
            LoadObjectiveOfflineCommand = new HCommand(UpdateObjectives);
            //SaveObjectiveOfflineCommand = new HCommand(SaveObjectiveOffline);
            ChengeStatusOfflineCommand = new HCommand(ChengeStatusOffline);
            UpdateObjectiveOfflineCommand = new HCommand(UpdateObjectiveOffline);

            GetObjectiveForIdCommand = new HCommand(GetObjectiveForID);

            UploadObjectiveCommand = new HCommand(UploadToServerAsync);
            DownloadObjectiveCommand = new HCommand(DownloadFromServerAsync);
            GetIDCommand = new HCommand(GetIDAsync);
            AddObjectivesCommand = new HCommand<int>(AddObjectives);

            UpdateProjects();            
        }

        private void ZeroId()
        {
            NextId = 1;
        }

        private void CreateObjective()
        {
            var title = EditObjective.Title;
            var description = EditObjective.Description;
            ObjectiveModel model = new ObjectiveModel();
            model.ID = NextId++;
            model.ProjectID = SelectedProject.ID;
            model.CreationDate = DateTime.Now;
            model.DueDate = DateTime.Now;
            model.Title = title;
            model.Description = EditObjective.Description;
            model.Status = ObjectiveStatus.Undefined;                    

            Objectives.Add(model);

            ObjectModel.SaveObjectives(SelectedProject.dto);
            ObjectModel.Synchronizer.Update(model.dto.ID, SelectedProject.dto.ID);
        }
        private void UpdateObjectiveOffline()
        {
            SelectedObjective.Title = EditObjective.Title;
            SelectedObjective.Description = EditObjective.Description;
            SelectedObjective.Status = EditObjective.Status;
            SelectedObjective.DueDate = DateTime.Now;
            SelectedObjective.ParentObjectiveID = EditObjective.ParentObjectiveID;

            ObjectModel.SaveObjectives(SelectedProject.dto);
            ObjectModel.Synchronizer.Update(SelectedObjective.dto.ID, SelectedProject.dto.ID);
        }
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
                    ObjectModel.Synchronizer.Update(SelectedObjective.dto.ID, SelectedProject.dto.ID);
                    Objectives.Remove(SelectedObjective);
                    ObjectModel.SaveObjectives(SelectedProject.dto);
                }
            }
        }
        private void UpdateProjects()
        {
            ObjectModel.UpdateProjects();
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

                model.ID = NextId++;

                model.ProjectID = SelectedProject.ID;
                model.CreationDate = DateTime.Now;
                model.DueDate = DateTime.Now;

                int index = random.Next(0, names.Length);

                model.Title = names[index];

                index = random.Next(0, discriptions.Length);
                model.Description = discriptions[index];

                model.Status = (ObjectiveStatus)random.Next(0, 4);
                Objectives.Add(model);
                ObjectModel.Synchronizer.Update(model.dto.ID, SelectedProject.dto.ID);
            }
            ObjectModel.SaveObjectives(SelectedProject.dto);
            
        }
        private void GetObjectiveForID()
        {
            WinBox.ShowMessage("Эта кнопка больше ничего не делает");            
        }
        private async void GetIDAsync()
        {            
            WinBox.ShowMessage("Эта кнопка больше ничего не делает!");
        }

        private async void UploadToServerAsync()
        {
            WinBox.ShowMessage("Эта кнопка больше ничего не делает!");
            
        }
        private async void DownloadFromServerAsync()
        {
            WinBox.ShowMessage("Эта кнопка больше ничего не делает!");            
        }

        public static void DeleteObjective(ProjectDto project, ObjectiveDto objective)
        {
            throw new NotImplementedException();
        }       

    }
}