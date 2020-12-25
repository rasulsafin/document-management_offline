using MRS.DocumentManagement.Connection.YandexDisk;
using MRS.DocumentManagement.Interface.Dtos;
using MRS.DocumentManagement.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using WPFStorage.Base;
using WPFStorage.Dialogs;

namespace MRS.DocumentManagement.Contols
{
    public class ProjectViewModel : BaseViewModel
    {
        
        YandexDiskManager yandex;
        ProjectModel selectProject;
        bool openTempFile = false;
        public ObservableCollection<ProjectModel> Projects { get; set; } = new ObservableCollection<ProjectModel>();
        public ProjectModel SelectProject { get => selectProject; set { selectProject = value; OnPropertyChanged(); } }
        public bool OpenTempFile { get => openTempFile; set { openTempFile = value; OnPropertyChanged(); } }


        public HCommand CreateCommand { get; }       
        public HCommand DeleteCommand { get; private set; }
        public HCommand RenameCommand { get; }
        public HCommand ServerUnloadCommand { get; }
        public HCommand ServerDownloadCommand { get; }
        public HCommand OpenFileCommand { get; }
        public HCommand UpdateCommand { get; }

        public ProjectViewModel()
        {
            CreateCommand = new HCommand(CreateProject);
            UpdateCommand = new HCommand(Update);
            DeleteCommand = new HCommand(DeleteProject);
            RenameCommand = new HCommand(RenameProject);
            ServerUnloadCommand = new HCommand(ServerUnload);
            ServerDownloadCommand = new HCommand(ServerDownload);
            OpenFileCommand = new HCommand(OpenFile);
            
            if (!Directory.Exists(PathManager.GetProjectsDir())) Directory.CreateDirectory(PathManager.GetProjectsDir());

            Update();
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
                int newId = (Projects.Count == 0) ? 1 : Projects.Max(x => x.ID) + 1;
                ProjectModel project = new ProjectModel();
                project.Title = name;
                project.ID = newId;
                Projects.Add(project);

                string fileName = PathManager.GetProjectFile(project.dto);

                string json = JsonConvert.SerializeObject(project.dto, Formatting.Indented);
                File.WriteAllText(fileName, json);
            }
            Update();
        }

        private void Update()
        {
            DirectoryInfo projDir = new DirectoryInfo(PathManager.GetProjectsDir());
            List<string> files = new List<string>();
            foreach (var project in Projects)
            {
                files.Add(PathManager.GetProjectFile(project.dto));                
            }
            
            foreach (var item in projDir.GetFiles())
            {
                if (!files.Contains(item.FullName))
                {
                    LoadProject(item.FullName);
                }
            }
        }

        private void DeleteProject()
        {            
            if (SelectProject == null)
                WinBox.ShowMessage($"Не могу выполнить операцию. Нет выбранного проект.");
            else if (WinBox.ShowQuestion($"Удалить проект '{SelectProject.Title}'?"))
            {
                string fileName = PathManager.GetProjectFile(SelectProject.dto);

                File.Delete(fileName);

                Projects.Remove(SelectProject);
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
                SelectProject.Title = name;
                string fileName = PathManager.GetProjectFile(SelectProject.dto);
                string json = JsonConvert.SerializeObject(SelectProject.dto, Formatting.Indented);
                File.WriteAllText(fileName, json);
            }
        }

                
        private void LoadProject(string fileName)
        {
            string json = File.ReadAllText(fileName);
            ProjectDto dto = JsonConvert.DeserializeObject<ProjectDto>(json);
            ProjectModel model = new ProjectModel(dto);
            Projects.Add(model);
        }

        private async void ServerDownload( )
        {
            ChechYandex();
            List<ProjectDto> list = await yandex.DownloadProjects();

            Projects.Clear();
            foreach (var item in list)
            {
                Projects.Add(new ProjectModel(item));
            }
        }       

        private async void ServerUnload( )
        {
            ChechYandex();
            if (SelectProject != null)
            {
                //string fileName = PathManager.GetProjectFile(SelectProject.dto);
                await yandex.UnloadProject(SelectProject.dto);
                
            }

            
        }
        
        private void ChechYandex()
        {
            if (yandex == null)
            {
                yandex = new YandexDiskManager(MainViewModel.AccessToken);
                yandex.TempDir = MainViewModel.TEMP_DIR;
            }
        }

    }
}