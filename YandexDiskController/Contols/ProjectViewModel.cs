using MRS.DocumentManagement.Base;
using MRS.DocumentManagement.Connection.YandexDisk;
using MRS.DocumentManagement.Dialogs;
using MRS.DocumentManagement.Interface.Dtos;
using MRS.DocumentManagement.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace MRS.DocumentManagement.Contols
{

    public class ProjectViewModel : BaseViewModel
    {
        private static readonly string DIR_NAME = "data";
        private static readonly string FILE_NAME = "projects.xml";
        private static readonly string TEMP_DIR = "Temp.Yandex";
        YandexDiskManager yandex;
        ProjectModel selectProject;
        bool openTempFile = false;
        public ObservableCollection<ProjectModel> Projects { get; set; } = new ObservableCollection<ProjectModel>();
        public ProjectModel SelectProject { get => selectProject; set { selectProject = value; OnPropertyChanged(); } }
        public bool OpenTempFile { get => openTempFile; set { openTempFile = value; OnPropertyChanged(); } }


        public HCommand CreateCommand { get; }
        public HCommand OpenFileCommand { get; }
        public HCommand LoadProjectsCommand { get; }
        public HCommand DownloadProjectsCommand { get; }
        public HCommand DeleteFileCommand { get; }
        public HCommand DeleteCommand { get; private set; }
        public HCommand RenameCommand { get; }
        public HCommand CreateLoadFileCommand { get; }
        public HCommand OpenTempFileCommand { get; }
        public HCommand XMLPackCommand { get; }
        public HCommand XMLUnPackCommand { get; }

        public ProjectViewModel()
        {
            CreateCommand = new HCommand(CreateAsync);
            OpenFileCommand = new HCommand(OpenFile);
            LoadProjectsCommand = new HCommand(UnloadProjectsInServer);
            DownloadProjectsCommand = new HCommand(DownloadProjectsAsync);
            DeleteFileCommand = new HCommand(DeleteFile);
            DeleteCommand = new HCommand(DeleteProjectAsync);
            RenameCommand = new HCommand(RenameProjectAsync);
            CreateLoadFileCommand = new HCommand(CreateLoadFile);
            OpenTempFileCommand = new HCommand(OpenTempFileMethod);
            XMLPackCommand = new HCommand(Pack);
            XMLUnPackCommand = new HCommand(Unpack);
            //LoadProjectsInFile();
        }

        private void Unpack(object obj)
        {
            if (!Directory.Exists(DIR_NAME)) Directory.CreateDirectory(DIR_NAME);
            string fileName = Path.Combine(DIR_NAME, FILE_NAME);
            if (File.Exists(fileName))
            {
                var json = File.ReadAllText(fileName);
                List<ProjectDto> collection = JsonConvert.DeserializeObject<List<ProjectDto>>(json);

                Projects.Clear();
                foreach (ProjectDto item in collection)
                {
                    Projects.Add(new ProjectModel(item));
                }                
            }
        }

        private void Pack(object obj)
        {
            if (!Directory.Exists(DIR_NAME)) Directory.CreateDirectory(DIR_NAME);
            string fileName = Path.Combine(DIR_NAME, FILE_NAME);
            List<ProjectDto> collectionDto = Projects.Select(x => x.dto).ToList();
            try
            {
                var json = JsonConvert.SerializeObject(collectionDto, Formatting.Indented);
                File.WriteAllText(fileName, json);

                //XmlSerializer formatter = new XmlSerializer(typeof(List<ProjectDto>));
                //using (FileStream fs = new FileStream(fileName, FileMode.Create))
                //{
                //    formatter.Serialize(fs, collectionDto);
                //}
            }
            catch (Exception ex)
            {
                WinBox.ShowMessage($"Хуйня:{ex.Message}");
            }
            OpenGeany(fileName);
        }

        private void OpenTempFileMethod(object obj)
        {
            string fileName = Path.Combine(TEMP_DIR, FILE_NAME);
            OpenGeany(fileName);
        }

        private void CreateLoadFile(object obj)
        {
            //string TempDir = TEMP_DIR;
            //string PROGECTS_FILE = FILE_NAME;
            //List<ProjectDto> collectionDto  = Projects.Select(x => x.dto).ToList();

            //List<ProjectYandexModel> collectionProject = new List<ProjectYandexModel>();
            //foreach (var item in collectionDto)
            //{
            //    collectionProject.Add(new ProjectYandexModel(item));
            //}
            //if (!Directory.Exists(TempDir)) Directory.CreateDirectory(TempDir);
            //string fileName = Path.Combine(TempDir, PROGECTS_FILE);
            //XmlSerializer formatter = new XmlSerializer(typeof(List<ProjectYandexModel>));
            //using (FileStream fs = new FileStream(fileName, FileMode.Create))
            //{
            //    formatter.Serialize(fs, collectionProject);
            //}

            //OpenGeany(fileName);
        }

        

        

        private void DeleteFile(object obj)
        {
            string fileName = Path.Combine(DIR_NAME, FILE_NAME);
            FileInfo file = new FileInfo(fileName);
            if (file.Exists) file.Delete();
            Projects.Clear();
        }

        private async void DownloadProjectsAsync(object obj)
        {
            ChechYandex();
            List<ProjectDto> list = await yandex.DownloadProjects();

            Projects.Clear();
            foreach (var item in list)
            {
                Projects.Add(new ProjectModel(item));
            }
        }
        private void OpenFile(object obj)
        {
            string fileName = Path.Combine(DIR_NAME, FILE_NAME);
            FileInfo file = new FileInfo(fileName);
            OpenGeany(file.FullName);
        }

        private static void OpenGeany(string file)
        {
            Process.Start(@"c:\Program Files (x86)\Geany\bin\geany.exe", file);
        }

        private async void UnloadProjectsInServer(object obj)
        {
            ChechYandex();
            await yandex.UnloadProjects(Projects.Select(x => x.dto).ToList());
            
        }


        //private void LoadProjectsInFile()
        //{
        //    if (!Directory.Exists(DIR_NAME)) Directory.CreateDirectory(DIR_NAME);
        //    string fileName = Path.Combine(DIR_NAME, FILE_NAME);
        //    if (File.Exists(fileName))
        //    {
        //        XmlSerializer formatter = new XmlSerializer(typeof(ObservableCollection<ProjectModel>));
        //        using (FileStream fs = new FileStream(fileName, FileMode.OpenOrCreate))
        //        {
        //            ObservableCollection<ProjectModel> collection = (ObservableCollection<ProjectModel>)formatter.Deserialize(fs);

        //            Projects.Clear();
        //            foreach (ProjectModel item in collection)
        //            {
        //                Projects.Add(item);
        //            }
        //        }
        //    }
        //}
        //private void SaveProjects()
        //{
        //    if (!Directory.Exists(DIR_NAME)) Directory.CreateDirectory(DIR_NAME);
        //    string fileName = Path.Combine(DIR_NAME, FILE_NAME);

        //    try
        //    {
        //        XmlSerializer formatter = new XmlSerializer(typeof(ObservableCollection<ProjectModel>));
        //        using (FileStream fs = new FileStream(fileName, FileMode.Create))
        //        {
        //            formatter.Serialize(fs, Projects);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        WinBox.ShowMessage($"Хуйня:{ex.Message}");
        //    }
        //}
        private void ChechYandex()
        {
            if (yandex == null)
            {
                yandex = new YandexDiskManager(MainViewModel.AccessToken);
                yandex.TempDir = TEMP_DIR;
            }
        }

        private async void CreateAsync(object obj)
        {
            ChechYandex();
            if (WinBox.ShowInput(
                question: "Введите название проекта:", 
                input: out string name, 
                title: "Создание проекта", 
                okText: "Создать", 
                cancelText: "Отменить", 
                defautValue: (SelectProject==null)? "Новый проект": SelectProject.Title ))
            {
                int newId = (Projects.Count == 0)? 1 : Projects.Max(x => x.ID) + 1;

                //ProjectDto dto = new ProjectDto();
                //dto.ID = (ID<ProjectDto>)newId;
                //dto.Title = name;
                //ProjectModel project = new ProjectModel(dto);

                ProjectModel project = new ProjectModel();
                project.Title = name;
                project.ID = newId;

                bool res = await yandex.AddProject(project.dto);
                if (res)
                    DownloadProjectsAsync(null);

                if (OpenTempFile)
                    OpenTempFileMethod(null);

                //Projects.Add(project);
                //SaveProjects();
                //WinBox.ShowMessage($"Создам '{name}'");
            }
        }

        private async void DeleteProjectAsync(object obj)
        {
            ChechYandex();
            if (SelectProject == null)
                WinBox.ShowMessage($"Не могу выполнить операцию. Нет выбранного проект.");
            else if (WinBox.ShowQuestion($"Удалить проект '{SelectProject.Title}'?"))
            {
                bool res = await yandex.DeleteProject(SelectProject.dto);
                if (res)
                    DownloadProjectsAsync(null);

                if (OpenTempFile)
                    OpenTempFileMethod(null);
            }
        }

        private async void RenameProjectAsync(object obj)
        {
            ChechYandex();
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

                bool res = await yandex.UpdateProject(SelectProject.dto);
                if (res)
                    DownloadProjectsAsync(null);

                if (OpenTempFile)
                    OpenTempFileMethod(null);
            }
        }

    }
}