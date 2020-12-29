using WPFStorage.Base;
using MRS.DocumentManagement.Connection.YandexDisk;
using WPFStorage.Dialogs;
using MRS.DocumentManagement.Models;
using Microsoft.Win32;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using MRS.DocumentManagement.Interface.Dtos;
using System;
using MRS.DocumentManagement.Connection.YandexDisk.Synchronizer;

namespace MRS.DocumentManagement.Contols
{
    public class ItemViewModel : BaseViewModel
    {
        #region Const
        private static readonly string MEDIA_EXTENTION = ".avi .wav .mp4 .mpg .mpeg .jpg .jpeg .png";
        private static readonly string MODELS_EXTENTION = ".ifc .ifczip .bim ";
        private static readonly string DIR_NAME = "data";
        private static readonly string TEMP_DIR = "Temp.Yandex";
        private static readonly string OBJECTIVE_FILE = "objective.json";
        private static readonly string PROJECT_FILE = "projects.json";
        private static readonly string ITEM_FILE = "items.json";
        #endregion
        #region bending
        YandexDiskManager yandex;
        bool toObjective;
        private ProjectModel selectedProject;
        private ObjectiveModel selectedObjective;
        private ItemModel selectedItem;

        public ObservableCollection<ProjectModel> Projects { get; set; } = ObjectModel.Projects;
        public ObservableCollection<ObjectiveModel> Objectives { get; set; } = ObjectModel.Objectives;
        public ObservableCollection<ItemModel> Items { get; set; } = ObjectModel.Items;
        public ProjectModel SelectedProject
        {
            get => selectedProject;
            set { selectedProject = value; UpdateObjecteve(); UpdateItems(); OnPropertyChanged(); }
        }
        public ObjectiveModel SelectedObjective { get => selectedObjective; set { selectedObjective = value; UpdateItems(); OnPropertyChanged(); } }
        public ItemModel SelectedItem { get => selectedItem; set { selectedItem = value; OnPropertyChanged(); } }
        public bool ToObjective { get => toObjective; set { toObjective = value; OnPropertyChanged(); } }

        public HCommand AddItemsCommand { get; }
        public HCommand DelItemsCommand { get; }
        public HCommand UnloadCommand { get; }
        public HCommand GetIdCommand { get; }

        //public HCommand RenItemsCommand { get; }
        //public HCommand SaveFileCommand { get; }
        //public HCommand LoadFileCommand { get; }
        //public HCommand DownloadCommand { get; }
        #endregion

        public ItemViewModel()
        {
            AddItemsCommand = new HCommand(AddItem);
            DelItemsCommand = new HCommand(DelItems);
            UnloadCommand = new HCommand(UnloadAsync);
            GetIdCommand = new HCommand(GetIDs);

            //SaveFileCommand = new HCommand(SaveFile);
            //LoadFileCommand = new HCommand(LoadFile);
            //DownloadCommand = new HCommand(DownloadAsync);

            Initilization();
        }

        private void GetIDs()
        {
            throw new NotImplementedException();
        }

        private async void DownloadAsync()
        {
            ChechYandex();
            string message = ToObjective ? $"Скачать список item-ов в задание [{SelectedObjective.Title}] c сервера ?"
                : $"Скачать список item-ов в проекте [{SelectedProject.Title}] с сервера?";
            if (WinBox.ShowQuestion(message))
            {
                List<ItemDto> collect = await yandex.GetItemsAsync(SelectedProject.dto, ToObjective ? SelectedObjective.dto : null);
                Items.Clear();
                foreach (ItemDto item in collect)
                {
                    Items.Add((ItemModel)item);
                }
            }
        }

        private async void UnloadAsync()
        {
            ChechYandex();
            //string message = ToObjective ? $"Загрузить файл на сервер в проект [{SelectedProject.Title}] и задание [{SelectedObjective.Title}]?"
            //    : $"Загрузить файл на сервер в проект [{SelectedProject.Title}]?";
            //if (WinBox.ShowQuestion(message))
            //{
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Title = "Выберете файл";
            ofd.Multiselect = false;
            if (ofd.ShowDialog() == true)
            {
                FileInfo file = new FileInfo(ofd.FileName);
                ItemDto item = new ItemDto();
                item.Name = ofd.SafeFileName;
                item.ID = (ID<ItemDto>)(Items.Count + 1);
                item.ItemType = GetItemTypeDto(file.Extension);

                if (!ToObjective)
                    await yandex.UnloadItemAsync(item, ofd.FileName, SelectedProject.dto);
                else
                    await yandex.UnloadItemAsync(item, ofd.FileName, SelectedProject.dto, SelectedObjective.dto);
            }
            //}
        }

        private void AddItem()
        {
            var dirItems = PathManager.GetItemsDir(SelectedProject.dto);
            if (!Directory.Exists(dirItems)) Directory.CreateDirectory(dirItems);

            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Title = "Выберете файл";
            ofd.Multiselect = false;
            if (ofd.ShowDialog() == true)
            {
                ItemModel model = new ItemModel();
                model.ID = Properties.Settings.Default.ItemNextId++;
                Properties.Settings.Default.Save();
                FileInfo file = new FileInfo(ofd.FileName);

                model.Name = file.Name;
                model.ExternalItemId = file.FullName;
                model.IsObjective = ToObjective;
                model.ItemType = GetItemTypeDto(file.Extension);
                Items.Add(model);
                if (ToObjective)
                    SaveItem(model.dto, SelectedProject.dto, SelectedObjective.dto);
                else
                    SaveItem(model.dto, SelectedProject.dto);
            }
        }

        private void DelItems()
        {
            if (SelectedItem == null) WinBox.ShowMessage("Нет выбранного элемента.");
            else
            {
                if (WinBox.ShowQuestion($"Вы действительно хотите удалить элемент '{SelectedItem.Name}'"))
                {
                    Items.Remove(SelectedItem);
                    SelectedItem = null;
                }
            }
        }

        internal static void DeleteItem(ItemDto item, ProjectDto project, ObjectiveDto objective)
        {
            throw new NotImplementedException();
        }

        public static void SaveItem(ItemDto item, ProjectDto project, ObjectiveDto objective = null)
        {
            var dirProj = PathManager.GetProjectDir(project);
            if (!Directory.Exists(dirProj)) Directory.CreateDirectory(dirProj);

            var dirItems = PathManager.GetItemsDir(project);
            if (!Directory.Exists(dirItems)) Directory.CreateDirectory(dirItems);

            string path = objective == null
                    ? PathManager.GetItemFile(item, project, objective)
                    : PathManager.GetItemFile(item, project);

            string json = JsonConvert.SerializeObject(item, Formatting.Indented);
            File.WriteAllText(path, json);
        }

        private static ItemTypeDto GetItemTypeDto(string extension)
        {
            if (MEDIA_EXTENTION.Contains(extension))
            {
                return ItemTypeDto.Media;
            }
            else if (MODELS_EXTENTION.Contains(extension))
            {
                return ItemTypeDto.Bim;
            }
            else
                return ItemTypeDto.File;
        }

        #region Инициализация 
        private void Initilization()
        {

            //await Task.Run(() =>
            //{
            //    //var collect = ObjectModel.GetProjects();
            //    //foreach (ProjectDto item in collect)
            //    //{
            //    //    Projects.Add((ProjectModel)item);
            //    //}
            //    //if (Projects.Count != 0)
            //    //    SelectedProject = Projects.First();
            //});

            //await Task.Run(() =>
            //{
            UpdateObjecteve();
            //});
            //await Task.Run(() =>
            //{
            UpdateItems();
            //});

        }

        private void UpdateItems()
        {
            if (SelectedProject != null)
            {
                ObjectiveDto obj = ToObjective ? SelectedObjective.dto : null;
                List<ItemDto> collection = GetItems(SelectedProject.dto, obj);
                Items.Clear();
                foreach (ItemDto item in collection)
                {
                    Items.Add((ItemModel)item);
                }
            }
        }

        private List<ItemDto> GetItems(ProjectDto project, ObjectiveDto objective = null)
        {
            var dirItem = PathManager.GetItemsDir(project, objective);
            if (!Directory.Exists(dirItem)) return new List<ItemDto>();
            var result = new List<ItemDto>();
            DirectoryInfo dir = new DirectoryInfo(dirItem);
            foreach (var file in dir.GetFiles())
            {
                if (file.Name.StartsWith("item_"))
                {
                    var json = File.ReadAllText(file.FullName);
                    ItemDto item = JsonConvert.DeserializeObject<ItemDto>(json);
                    result.Add(item);
                }
            }
            return result;
        }

        private void UpdateObjecteve()
        {
            if (SelectedProject != null)
            {
                List<ObjectiveDto> collection = ObjectiveViewModel.GetObjectives(SelectedProject.dto);
                Objectives.Clear();
                foreach (ObjectiveDto item in collection)
                {
                    Objectives.Add((ObjectiveModel)item);
                }
                if (Objectives.Count > 0)
                    SelectedObjective = Objectives.First();
            }
        }

        private void ChechYandex()
        {
            if (yandex == null)
            {
                yandex = new YandexDiskManager(MainViewModel.AccessToken);
                yandex.TempDir = TEMP_DIR;
            }
        }
        #endregion

        //private void LoadFile()
        //{
        //    if (WinBox.ShowQuestion($"Загрузить список из файла?"))
        //    {
        //        if (SelectedProject == null) return;

        //        string fileName = GetItemFileName(
        //            item: ITEM_FILE,
        //            root: DIR_NAME,
        //            progect: SelectedProject.Title,
        //            objective: ToObjective ? SelectedObjective.Title : null);

        //        string json = File.ReadAllText(fileName);
        //        var collect = JsonConvert.DeserializeObject<List<ItemModel>>(json);
        //        Items.Clear();
        //        foreach (var item in collect)
        //        {
        //            Items.Add(item);
        //        }

        //    }
        //}
        //private void SaveFile()
        //{
        //    if (WinBox.ShowQuestion($"Сохранить список?"))
        //    {
        //        string fileName = GetItemFileName(
        //            item: ITEM_FILE,
        //            root: DIR_NAME,
        //            progect: SelectedProject.Title,
        //            objective: ToObjective ? SelectedObjective.Title : null);

        //        string json = JsonConvert.SerializeObject(Items, Formatting.Indented);
        //        File.WriteAllText(fileName, json);
        //    }

        //}
        //private string GetItemFileName(string item, string root, string progect, string objective = null)
        //{
        //    string fileName;
        //    string projDir = Path.Combine(root, progect);
        //    if (!Directory.Exists(projDir)) Directory.CreateDirectory(projDir);
        //    if (objective != null)
        //    {
        //        fileName = Path.Combine(projDir, $"{objective}_{item}");
        //    }
        //    else
        //    {
        //        fileName = Path.Combine(projDir, item);
        //    }

        //    return fileName;
        //}

    }
}