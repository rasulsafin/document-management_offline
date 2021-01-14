using MRS.DocumentManagement.Connection.YandexDisk;
using MRS.DocumentManagement.Models;
using System;
using System.Collections.ObjectModel;
using System.IO;
using WPFStorage.Base;
using WPFStorage.Dialogs;

namespace MRS.DocumentManagement.Contols
{
    public class ProjectViewModel : BaseViewModel
    {

        #region Bending and data
        YandexDiskManager yandex;
        ProjectModel selectProject;
        bool openTempFile = false;
        public ObservableCollection<ProjectModel> Projects { get; set; } = ObjectModel.Projects;
        public ProjectModel SelectProject { get => selectProject; set { selectProject = value; OnPropertyChanged(); } }
        public bool OpenTempFile { get => openTempFile; set { openTempFile = value; OnPropertyChanged(); } }

        public int NextId
        {
            get => Properties.Settings.Default.ProjectNextId;
            set
            {
                Properties.Settings.Default.ProjectNextId = value;
                Properties.Settings.Default.Save();
                OnPropertyChanged();
            }
        }
        public HCommand CreateCommand { get; }
        public HCommand DeleteCommand { get; private set; }
        public HCommand RenameCommand { get; }
        public HCommand CreateSampleProjectCommand { get; }
        public HCommand ServerUnloadCommand { get; }
        public HCommand ServerDownloadCommand { get; }
        public HCommand OpenFileCommand { get; }
        public HCommand ISResetCommand { get; }
        public HCommand UpdateCommand { get; }
        #endregion       
        

        public ProjectViewModel()
        {
            CreateCommand = new HCommand(CreateProject);
            UpdateCommand = new HCommand(Update);
            DeleteCommand = new HCommand(DeleteProject);
            RenameCommand = new HCommand(RenameProject);

            CreateSampleProjectCommand = new HCommand(CreateSampleProject);
            //ServerUnloadCommand = new HCommand(ServerUnload);
            //ServerDownloadCommand = new HCommand(ServerDownload);
            OpenFileCommand = new HCommand(OpenFile);
            ISResetCommand = new HCommand(ISReset);
            Update();
        }

        private void CreateSampleProject()
        {
            string[] names;            

            string namesFile = "ProjectsName.txt";
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

                Random random = new Random();
                int index = random.Next(0, names.Length);
                ProjectModel project = new ProjectModel();
                project.Title = names[index];
                project.ID = ++Properties.Settings.Default.ProjectNextId;
                Projects.Add(project);
                ObjectModel.SaveProjects();
                Properties.Settings.Default.Save();
            }

        }

        private void ISReset()
        {
            NextId = 1;
        }

        private void OpenFile()
        {
            if (SelectProject != null)
            {
                string fileName = PathManager.GetProjectFile(SelectProject.dto);
                OpenHelper.Notepad(fileName);
            }
        }

        private void CreateProject()
        {
            // Создать проект локально
            if (WinBox.ShowInput(
                question: "Введите название проекта:",
                input: out string name,
                title: "Создание проекта",
                okText: "Создать",
                cancelText: "Отменить",
                defautValue: (SelectProject == null) ? "Новый проект" : SelectProject.Title))
            {
                ProjectModel project = new ProjectModel();
                project.Title = name;
                project.ID = NextId++;
                Projects.Add(project);
                ObjectModel.SaveProjects();
                ObjectModel.Synchronizer.Update(project.dto.ID);

            }
            Update();
        }



        

        private void Update()
        {
            ObjectModel.UpdateProjects();                     
        }        

        private void DeleteProject()
        {
            if (SelectProject == null)
                WinBox.ShowMessage($"Не могу выполнить операцию. Нет выбранного проект.");
            else if (WinBox.ShowQuestion($"Удалить проект '{SelectProject.Title}'?"))
            {
                ObjectModel.Synchronizer.Update(SelectProject.dto.ID);
                Projects.Remove(SelectProject);
                ObjectModel.SaveProjects();
                SelectProject = null;
            }
        }

        private void RenameProject()
        {
            if (SelectProject == null)
                WinBox.ShowMessage($"Не могу выполнить операцию. Нет выбранного проект.");
            else if (WinBox.ShowInput(
                question: "Введите новое название проекта:",
                input: out string name,
                title: "Переименование проекта",
                okText: "Переименовать",
                cancelText: "Отменить",
                defautValue: SelectProject.Title))
            {

                ObjectModel.Synchronizer.Update(SelectProject.dto.ID);
                ObjectModel.SaveProjects();

                //ObjectModel.RenameProject(SelectProject.dto.ID, name);
                
                
            }
        }

    }
}