using DocumentManagement.Base;
using DocumentManagement.Connection.YandexDisk;
using DocumentManagement.Dialogs;
using DocumentManagement.Models;
using MRS.DocumentManagement.Interface;
using MRS.DocumentManagement.Interface.Dtos;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Xml.Serialization;

namespace DocumentManagement.Contols
{
    public class ProjectViewModel : BaseViewModel
    {
        private static readonly string DIR_NAME = "data";
        private static readonly string FILE_NAME = "projects.xml";
        private static readonly string TEMP_DIR = "Temp.Yandex";
        YandexDisk yandex;
        ProjectModel selectProject;

        public ObservableCollection<ProjectModel> Projects { get; set; } = new ObservableCollection<ProjectModel>();
        public ProjectModel SelectProject { get => selectProject; set { selectProject = value; OnPropertyChanged(); } }


        public HCommand CreateCommand { get; }
        public HCommand OpenFileCommand { get; }
        public HCommand LoadProjectsCommand { get; }
        public HCommand DownloadProjectsCommand { get; }
        public HCommand DeleteFileCommand { get; }
        public HCommand DeleteCommand { get; private set; }
        public HCommand RenameCommand { get; }

        public ProjectViewModel()
        {
            CreateCommand = new HCommand(Create);
            OpenFileCommand = new HCommand(OpenFile);
            LoadProjectsCommand = new HCommand(UnloadProjectsInServer);
            DownloadProjectsCommand = new HCommand(DownloadProjects);
            DeleteFileCommand = new HCommand(DeleteFile);
            DeleteCommand = new HCommand(DeleteProject);
            RenameCommand = new HCommand(RenameProject);
            LoadProjectsInFile();
        }

        private void RenameProject(object obj)
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
                SelectProject.Title = name;                
                SaveProjects();
            }
        }

        private void DeleteProject(object obj)
        {
            if (SelectProject == null)
                WinBox.ShowMessage($"Не могу выполнить операцию. Нет выбранного проект.");
            else if (WinBox.ShowQuestion($"Удалить проект '{SelectProject.Title}'?"))
            {
                Projects.Remove(SelectProject);
                SaveProjects();
            }
        }

        private void DeleteFile(object obj)
        {
            string fileName = Path.Combine(DIR_NAME, FILE_NAME);
            FileInfo file = new FileInfo(fileName);
            if (file.Exists) file.Delete();
            Projects.Clear();
        }

        private void DownloadProjects(object obj)
        {
            if (MainViewModel.Controller == null)
                WinBox.ShowMessage("Контроллер не создан!");
            WinBox.ShowMessage("Ещё не умею!");
        }

        private async void UnloadProjectsInServer(object obj)
        {
            if (yandex == null)
            {
                yandex = new YandexDisk(MainViewModel.AccessToken);
                yandex.TempDir = TEMP_DIR;
                //WinBox.ShowMessage("Контроллер не создан!");
            }
            await yandex.UnloadProjects(Projects.Select(x => x.dto).ToList());
            
        }

        private void OpenFile(object obj)
        {
            string fileName = Path.Combine(DIR_NAME, FILE_NAME);
            FileInfo file = new FileInfo(fileName);
            Process.Start(@"c:\Program Files (x86)\Geany\bin\geany.exe", file.FullName);
        }

        private void LoadProjectsInFile()
        {
            if (!Directory.Exists(DIR_NAME)) Directory.CreateDirectory(DIR_NAME);
            string fileName = Path.Combine(DIR_NAME, FILE_NAME);
            if (File.Exists(fileName))
            {
                XmlSerializer formatter = new XmlSerializer(typeof(ObservableCollection<ProjectModel>));
                using (FileStream fs = new FileStream(fileName, FileMode.OpenOrCreate))
                {
                    ObservableCollection<ProjectModel> collection = (ObservableCollection<ProjectModel>)formatter.Deserialize(fs);

                    Projects.Clear();
                    foreach (ProjectModel item in collection)
                    {
                        Projects.Add(item);
                    }
                }
            }
        }
        private void SaveProjects()
        {
            if (!Directory.Exists(DIR_NAME)) Directory.CreateDirectory(DIR_NAME);
            string fileName = Path.Combine(DIR_NAME, FILE_NAME);

            try
            {
                XmlSerializer formatter = new XmlSerializer(typeof(ObservableCollection<ProjectModel>));
                using (FileStream fs = new FileStream(fileName, FileMode.Create))
                {
                    formatter.Serialize(fs, Projects);
                }
            }
            catch (Exception ex)
            {
                WinBox.ShowMessage($"Хуйня:{ex.Message}");
            }
        }

        private void Create(object obj)
        {
            if (WinBox.ShowInput(
                question: "Введите название проекта:", 
                input: out string name, 
                title: "Создание проекта", 
                okText: "Создать", 
                cancelText: "Отменить", 
                defautValue: "Новый проект" ))
            {
                int newId = Projects.Count + 1;

                //ProjectDto dto = new ProjectDto();
                //dto.ID = (ID<ProjectDto>)newId;
                //dto.Title = name;
                //ProjectModel project = new ProjectModel(dto);

                ProjectModel project = new ProjectModel();
                project.Title = name;
                project.ID = newId;

                Projects.Add(project);
                SaveProjects();
                //WinBox.ShowMessage($"Создам '{name}'");
            }
        }

    }
}