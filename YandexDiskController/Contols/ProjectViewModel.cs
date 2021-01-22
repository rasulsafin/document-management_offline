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
    public class ProjectViewModel : BaseViewModel
    {
        #region Bending and data
        private DiskManager disk;
        private ProjectModel selectProject;
        private bool openTempFile = false;

        public ProjectViewModel()
        {
            CreateCommand = new HCommand(CreateProject);
            UpdateCommand = new HCommand(Update);
            DeleteCommand = new HCommand(DeleteProject);
            RenameCommand = new HCommand(RenameProject);

            CreateSampleProjectCommand = new HCommand(CreateSampleProject);

            OpenFileCommand = new HCommand(OpenFile);
            ISResetCommand = new HCommand(IDReset);
            PushCommand = new HCommand(Push);
            PullCommand = new HCommand(PullAsync);
            Update();

            Auth.LoadActions.Add(Initialization);
        }

        private void Initialization(string token)
        {
            disk = new DiskManager(token);
        }

        public ObservableCollection<ProjectModel> Projects { get; set; } = ObjectModel.Projects;

        public ProjectModel SelectProject
        {
            get => ObjectModel.SelectedProject; set
            {
                ObjectModel.SelectedProject = value;
                OnPropertyChanged();
            }
        }

        public bool OpenTempFile
        {
            get => openTempFile; set
            {
                openTempFile = value;
                OnPropertyChanged();
            }
        }

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

        public HCommand PushCommand { get; }

        public HCommand PullCommand { get; }

        public HCommand UpdateCommand { get; }
        #endregion

        private async void PullAsync()
        {
            if (WinBox.ShowInput(
            question: "Введите ID проекта для скачивания:",
            input: out string text,
            title: "Выбор проекта",
            okText: "Скачать",
            cancelText: "Отменить",
            defautValue: "-1"))
            {
                ProjectDto project = await disk.Pull<ProjectDto>(text);
                if (project == null)
                {
                    WinBox.ShowMessage("Проект не существует на удаленном устройстве!");
                    return;
                }

                var model = Projects.FirstOrDefault(x => x.ID == (int)project.ID);
                Projects.Remove(model);

                Projects.Add(new ProjectModel(project));
                SelectProject = Projects.Last();
            }
        }

        private async void Push()
        {
            if (SelectProject == null)
            {
                if (WinBox.ShowInput(
                question: "Нет выбранного проекта для отправки.\nВведите ID проекта:",
                input: out string text,
                title: "Выпор проекта",
                okText: "Отправить",
                cancelText: "Отменить",
                defautValue: "-1") && int.TryParse(text, out int id))
                {
                    SelectProject = Projects.FirstOrDefault(p => p.ID == id);
                }
                else
                {
                    return;
                }
            }

            if (!await disk.Push<ProjectDto>(SelectProject.dto, SelectProject.ID.ToString()))
            {
                WinBox.ShowMessage("Отправить не удалось!");
            }
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
                project.ID = NextId++;
                Projects.Add(project);
                ObjectModel.SaveProjects();
                ObjectModel.Synchronizer.Update(project.dto.ID);
            }
        }

        private void IDReset()
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
            if (SelectProject == null && Projects.Count > 0)
            {
                SelectProject = Projects.First();
            }
        }

        private void DeleteProject()
        {
            if (SelectProject == null)
            {
                WinBox.ShowMessage($"Не могу выполнить операцию. Нет выбранного проект.");
            }
            else if (WinBox.ShowQuestion($"Удалить проект '{SelectProject.Title}'?"))
            {
                ObjectModel.Synchronizer.Delete(SelectProject.dto.ID);
                Projects.Remove(SelectProject);
                ObjectModel.SaveProjects();
                SelectProject = null;
            }
        }

        private void RenameProject()
        {
            if (SelectProject == null)
            {
                WinBox.ShowMessage($"Не могу выполнить операцию. Нет выбранного проект.");
            }
            else if (WinBox.ShowInput(
                question: "Введите новое название проекта:",
                input: out string name,
                title: "Переименование проекта",
                okText: "Переименовать",
                cancelText: "Отменить",
                defautValue: SelectProject.Title))
            {
                SelectProject.Title = name;
                ObjectModel.Synchronizer.Update(SelectProject.dto.ID);
                ObjectModel.SaveProjects();
                // ObjectModel.RenameProject(SelectProject.dto.ID, name);
            }
        }
    }
}
