using DocumentManagement.Base;
using DocumentManagement.Connection.YandexDisk;
using DocumentManagement.Dialogs;
using DocumentManagement.Models;
using Microsoft.Win32;
using MRS.DocumentManagement.Interface.Dtos;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace DocumentManagement.Contols
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
        YandexDisk yandex;
        bool toObjective;
        private ProjectModel selectedProject;
        private ObjectiveModel selectedObjective;
        private ItemModel selectedItem;

        public ObservableCollection<ProjectModel> Projects { get; set; } = new ObservableCollection<ProjectModel>();
        public ObservableCollection<ObjectiveModel> Objectives { get; set; } = new ObservableCollection<ObjectiveModel>();
        public ObservableCollection<ItemModel> Items { get; set; } = new ObservableCollection<ItemModel>();
        public ProjectModel SelectedProject
        {
            get => selectedProject;
            set { selectedProject = value; UpdateObjecteve(); OnPropertyChanged(); }
        }
        public ObjectiveModel SelectedObjective { get => selectedObjective; set { selectedObjective = value; OnPropertyChanged(); } }
        public ItemModel SelectedItem { get => selectedItem; set { selectedItem = value; OnPropertyChanged(); } }
        public bool ToObjective { get => toObjective; set { toObjective = value; OnPropertyChanged(); } }

        public HCommand AddItemsCommand { get; }
        public HCommand DelItemsCommand { get; }
        public HCommand RenItemsCommand { get; }
        public HCommand SaveFileCommand { get; }
        public HCommand LoadFileCommand { get; }
        public HCommand UnloadCommand { get; }
        public HCommand DownloadCommand { get; }
        #endregion

        public ItemViewModel()
        {
            AddItemsCommand = new HCommand(AddItems);
            DelItemsCommand = new HCommand(DelItems);
            RenItemsCommand = new HCommand(RenItems);
            SaveFileCommand = new HCommand(SaveFile);
            LoadFileCommand = new HCommand(LoadFile);
            UnloadCommand = new HCommand(Unload);
            DownloadCommand = new HCommand(Download);

            Initilization();
        }

        private void Download(object obj)
        {
            WinBox.ShowMessage("Скачивание с сервера не разработано");
        }

        private void Unload(object obj)
        {
            WinBox.ShowMessage("Загрузка на сервер не разработано");
        }

        private void LoadFile(object obj)
        {
            if (SelectedProject == null) return;
            string fileName = "";
            string projDir = Path.Combine(DIR_NAME, SelectedProject.Title);
            if (!Directory.Exists(projDir)) Directory.CreateDirectory(projDir);
            if (ToObjective)
            {
                if (SelectedObjective == null) return;
                string objectDir = Path.Combine(projDir, SelectedObjective.Title);
                if (!Directory.Exists(objectDir)) Directory.CreateDirectory(objectDir);
                fileName = Path.Combine(objectDir, ITEM_FILE);
            }
            else
            {
                fileName = Path.Combine(projDir, ITEM_FILE);
            }
        }

        private void SaveFile(object obj)
        {
            if (WinBox.ShowQuestion($"Сохранить список?"))
            {
                string fileName = "";
                string projDir = Path.Combine(DIR_NAME, SelectedProject.Title);
                if (!Directory.Exists(projDir)) Directory.CreateDirectory(projDir);
                if (ToObjective)
                {
                    string objectDir = Path.Combine(projDir, SelectedObjective.Title);
                    if (!Directory.Exists(objectDir)) Directory.CreateDirectory(objectDir);
                    fileName = Path.Combine(objectDir, ITEM_FILE);
                }
                else
                {
                    fileName = Path.Combine(projDir, ITEM_FILE);
                }

                string json = JsonConvert.SerializeObject(Items, Formatting.Indented);
                File.WriteAllText(fileName, json);
            }
        }

        private void RenItems(object obj)
        {
            
            WinBox.ShowMessage("Переименовка не разработана");
        }

        private void DelItems(object obj)
        {
            if (SelectedItem == null) WinBox.ShowMessage("Нет выбранного элемента.");
            else
            {
                if (WinBox.ShowQuestion($"Вы действительно хотите удалить елемент '{SelectedItem.Name}'"))
                {
                    Items.Remove(SelectedItem);
                    SelectedItem = null;
                }
            }
        }

        private void AddItems(object obj)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Title = "Выберете файл";
            ofd.Multiselect = false;
            if (ofd.ShowDialog() == true)
            {
                ItemModel model = new ItemModel();
                model.ID = Items.Count == 0 ? 1 : Items.Max(x => x.ID)+1;
                FileInfo file = new FileInfo(ofd.FileName);

                model.Name = file.Name;

                if (ToObjective)
                    model.ExternalItemId = $"/{DIR_NAME}/{SelectedProject.Title}/{SelectedObjective.Title}/{model.Name}";
                else
                    model.ExternalItemId = $"/{DIR_NAME}/{SelectedProject.Title}/{model.Name}";

                if (MEDIA_EXTENTION.Contains(file.Extension))
                {
                    model.ItemType = ItemTypeDto.Media;
                }
                else if (file.Extension.Contains(MEDIA_EXTENTION))
                {
                    model.ItemType = ItemTypeDto.Media;
                }
                else if (MODELS_EXTENTION.Contains(file.Extension))
                {
                    model.ItemType = ItemTypeDto.Bim;
                }
                else
                    model.ItemType = ItemTypeDto.File;

                Items.Add(model);
                // Копирование файла и запись данных

            }

        }

        #region Инициализация 
        private async void Initilization()
        {
            await Task.Run(() =>
            {
                var fileName = Path.Combine(DIR_NAME, PROJECT_FILE);
                if (File.Exists(fileName))
                {
                    var json = File.ReadAllText(fileName);
                    List<ProjectDto> collection = JsonConvert.DeserializeObject<List<ProjectDto>>(json);

                    Projects.Clear();
                    foreach (ProjectDto item in collection)
                    {
                        Projects.Add((ProjectModel)item);
                    }
                    SelectedProject = Projects.First();

                }
            });

            await Task.Run(() =>
            {
                UpdateObjecteve();
            });
        }

        private void UpdateObjecteve()
        {
            if (SelectedProject != null)
            {
                string projDir = Path.Combine(DIR_NAME, SelectedProject.Title);
                if (!Directory.Exists(projDir)) Directory.CreateDirectory(projDir);
                string fileName = Path.Combine(projDir, OBJECTIVE_FILE);
                Objectives.Clear();
                if (File.Exists(fileName))
                {
                    var json = File.ReadAllText(fileName);
                    List<ObjectiveDto> collection = JsonConvert.DeserializeObject<List<ObjectiveDto>>(json);

                    foreach (ObjectiveDto item in collection)
                    {
                        Objectives.Add((ObjectiveModel)item);
                    }
                    SelectedObjective = Objectives.First();
                }
            }
        } 
        #endregion

    }
}