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
            SaveFileCommand = new HCommand(SaveFile);
            LoadFileCommand = new HCommand(LoadFile);
            UnloadCommand = new HCommand(UnloadAsync);
            DownloadCommand = new HCommand(DownloadAsync);

            Initilization();
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

        private void LoadFile()
        {
            if (WinBox.ShowQuestion($"Загрузить список из файла?"))
            {
                if (SelectedProject == null) return;

                string fileName = GetItemFileName(
                    item: ITEM_FILE,
                    root: DIR_NAME,
                    progect: SelectedProject.Title,
                    objective: ToObjective ? SelectedObjective.Title : null);

                string json = File.ReadAllText(fileName);
                var collect = JsonConvert.DeserializeObject<List<ItemModel>>(json);
                Items.Clear();
                foreach (var item in collect)
                {
                    Items.Add(item);
                }

            }
        }
        private string GetItemFileName(string item, string root, string progect, string objective = null)
        {
            string fileName;
            string projDir = Path.Combine(root, progect);
            if (!Directory.Exists(projDir)) Directory.CreateDirectory(projDir);
            if (objective != null)
            {
                fileName = Path.Combine(projDir, $"{objective}_{item}");
            }
            else
            {
                fileName = Path.Combine(projDir, item);
            }

            return fileName;
        }

        private void SaveFile()
        {
            if (WinBox.ShowQuestion($"Сохранить список?"))
            {
                string fileName = GetItemFileName(
                    item: ITEM_FILE,
                    root: DIR_NAME,
                    progect: SelectedProject.Title,
                    objective: ToObjective ? SelectedObjective.Title : null);

                string json = JsonConvert.SerializeObject(Items, Formatting.Indented);
                File.WriteAllText(fileName, json);
            }

        }



        private void DelItems()
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

        private void AddItems()
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Title = "Выберете файл";
            ofd.Multiselect = false;
            if (ofd.ShowDialog() == true)
            {
                ItemModel model = new ItemModel();
                model.ID = Items.Count == 0 ? 1 : Items.Max(x => x.ID) + 1;
                FileInfo file = new FileInfo(ofd.FileName);

                model.Name = file.Name;

                if (ToObjective)
                    model.ExternalItemId = $"/{DIR_NAME}/{SelectedProject.Title}/{SelectedObjective.Title}/{model.Name}";
                else
                    model.ExternalItemId = $"/{DIR_NAME}/{SelectedProject.Title}/{model.Name}";
                model.ItemType = GetItemTypeDto(file.Extension);

                Items.Add(model);
                // Копирование файла и запись данных

            }

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
        private async void Initilization()
        {
            await Task.Run(() =>
            {
                var fileName = Path.Combine(DIR_NAME, PROJECT_FILE);
                if (File.Exists(fileName))
                {
                    try
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
                    catch (Exception ex)
                    {
                        OpenHelper.Geany(fileName);
                        //WinBox.ShowMessage("При загрузки файла призошла ошибка:\n" + ex.Message, "Ошибка", 5000);
                    }
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
                    try
                    {
                        var json = File.ReadAllText(fileName);
                        List<ObjectiveDto> collection = JsonConvert.DeserializeObject<List<ObjectiveDto>>(json);

                        foreach (ObjectiveDto item in collection)
                        {
                            Objectives.Add((ObjectiveModel)item);
                        }
                        SelectedObjective = Objectives.First();

                    }
                    catch (Exception ex)
                    {
                        OpenHelper.LoadExeption(ex, fileName);

                    }
                }
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

    }
}