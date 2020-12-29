using MRS.DocumentManagement.Connection.YandexDisk;
using MRS.DocumentManagement.Connection.YandexDisk.Synchronizer;
using MRS.DocumentManagement.Interface.Dtos;
using MRS.DocumentManagement.Models;
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

        #region Bending and data
        YandexDiskManager yandex;
        ProjectModel selectProject;
        bool openTempFile = false;
        public ObservableCollection<ProjectModel> Projects { get; set; } = ObjectModel.Projects;
        public ProjectModel SelectProject { get => selectProject; set { selectProject = value; OnPropertyChanged(); } }
        public bool OpenTempFile { get => openTempFile; set { openTempFile = value; OnPropertyChanged(); } }


        public HCommand CreateCommand { get; }
        public HCommand DeleteCommand { get; private set; }
        public HCommand RenameCommand { get; }
        public HCommand ServerUnloadCommand { get; }
        public HCommand ServerDownloadCommand { get; }
        public HCommand OpenFileCommand { get; }
        public HCommand UpdateCommand { get; }
        #endregion       
        

        public ProjectViewModel()
        {
            CreateCommand = new HCommand(CreateProject);
            UpdateCommand = new HCommand(Update);
            DeleteCommand = new HCommand(DeleteProject);
            RenameCommand = new HCommand(RenameProject);
            ServerUnloadCommand = new HCommand(ServerUnload);
            ServerDownloadCommand = new HCommand(ServerDownload);
            OpenFileCommand = new HCommand(OpenFile);

            

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
                ProjectModel project = new ProjectModel();
                project.Title = name;
                project.ID = ++Properties.Settings.Default.ProjectNextId;
                Projects.Add(project);
                ObjectModel.SaveProject(project.dto);
                Properties.Settings.Default.Save();

            }
            Update();
        }



        

        private void Update()
        {
            ObjectModel.UpdateProject();                     
        }        

        private void DeleteProject()
        {
            if (SelectProject == null)
                WinBox.ShowMessage($"Не могу выполнить операцию. Нет выбранного проект.");
            else if (WinBox.ShowQuestion($"Удалить проект '{SelectProject.Title}'?"))
            {
                ObjectModel.DeleteProject(SelectProject.dto.ID);                
                
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

                //SelectProject.Title = name;

                ObjectModel.RenameProject(SelectProject.dto.ID, name);
                //string fileName = PathManager.GetProjectFile(SelectProject.dto);
                //string json = JsonConvert.SerializeObject(SelectProject.dto, Formatting.Indented);
                //File.WriteAllText(fileName, json);


                

            }
        }


        
        private async void ServerDownload()
        {
            ChechYandex();
            List<ProjectDto> list = await yandex.DownloadProjects();

            Projects.Clear();
            foreach (var item in list)
            {
                Projects.Add(new ProjectModel(item));
            }
        }

        private async void ServerUnload()
        {
            ChechYandex();
            if (SelectProject != null)
            {
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