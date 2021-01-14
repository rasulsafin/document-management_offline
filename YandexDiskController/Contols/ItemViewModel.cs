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
using System.Text;

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
            set
            {
                if (value != null)
                {
                    selectedProject = value;
                    ObjectModel.UpdateObjectives(selectedProject.dto);
                    ToObjective = false;
                    OnPropertyChanged();
                }
            }
        }
        public ObjectiveModel SelectedObjective { 
            get => selectedObjective; 
            set { selectedObjective = value; UpdateItems(); OnPropertyChanged(); } }
        public ItemModel SelectedItem { get => selectedItem; set { selectedItem = value; OnPropertyChanged(); } }
        public bool ToObjective { get => toObjective; set { toObjective = value; UpdateItems(); OnPropertyChanged(); } }

        public int NextId
        {
            get => Properties.Settings.Default.ItemNextId;
            set
            {
                Properties.Settings.Default.ItemNextId = value;
                Properties.Settings.Default.Save();
                OnPropertyChanged();
            }
        }

        public HCommand AddItemsCommand { get; }
        public HCommand DelItemsCommand { get; }
        public HCommand UnloadCommand { get; }
        public HCommand GetIdCommand { get; }
        public HCommand FindItemIdCommand { get; }
        public HCommand ZeroIdCommand { get; }

        //public HCommand RenItemsCommand { get; }
        //public HCommand SaveFileCommand { get; }
        //public HCommand LoadFileCommand { get; }
        //public HCommand DownloadCommand { get; }
        #endregion

        public ItemViewModel()
        {
            AddItemsCommand = new HCommand(AddItem);
            DelItemsCommand = new HCommand(DelItem);
            GetIdCommand = new HCommand(GetIDsAsync);
            FindItemIdCommand = new HCommand(FindItemId);
            ZeroIdCommand = new HCommand(ZeroId);
            //UnloadCommand = new HCommand(UnloadAsync);
            //SaveFileCommand = new HCommand(SaveFile);
            //LoadFileCommand = new HCommand(LoadFile);
            //DownloadCommand = new HCommand(DownloadAsync);

            Initilization();
        }

        private void ZeroId()
        {
            Properties.Settings.Default.ItemNextId = 0;
            Properties.Settings.Default.Save();
        }

        private void AddItem()
        {
            //var dirItems = PathManager.GetItemsDir(SelectedProject.dto);
            //if (!Directory.Exists(dirItems)) Directory.CreateDirectory(dirItems);

            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Title = "Выберете файл";
            ofd.Multiselect = false;
            if (ofd.ShowDialog() == true)
            {
                ItemModel model = new ItemModel();
                model.ID = NextId++;
                
                FileInfo file = new FileInfo(ofd.FileName);
                try
                {
                    string path = PathManager.GetProjectDir(SelectedProject.dto);
                    file = file.CopyTo(Path.Combine(path, file.Name));
                }
                catch { }

                model.Name = file.Name;
                model.ExternalItemId = file.FullName;
                model.IsObjective = ToObjective;
                model.ItemType = GetItemTypeDto(file.Extension);
                Items.Add(model);

                if (ToObjective)
                {
                    ObjectModel.SaveItems(SelectedProject.dto, SelectedObjective.dto);
                    ObjectModel.Synchronizer.Update(model.dto.ID, SelectedObjective.dto.ID, SelectedProject.dto.ID);
                }
                else
                {
                    ObjectModel.SaveItems(SelectedProject.dto);
                    ObjectModel.Synchronizer.Update(model.dto.ID, SelectedProject.dto.ID);
                }
            }
        }
        private void DelItem()
        {
            if (SelectedItem == null) WinBox.ShowMessage("Нет выбранного элемента.");
            else
            {
                if (WinBox.ShowQuestion($"Вы действительно хотите удалить элемент '{SelectedItem.Name}'"))
                {
                    if (ToObjective)
                    {
                        ObjectModel.Synchronizer.Update(SelectedItem.dto.ID, SelectedObjective.dto.ID, SelectedProject.dto.ID);
                        Items.Remove(SelectedItem);
                        ObjectModel.SaveItems(SelectedProject.dto, SelectedObjective.dto);
                    }
                    else
                    {
                        ObjectModel.SaveItems(SelectedProject.dto);
                        Items.Remove(SelectedItem);
                        ObjectModel.Synchronizer.Update(SelectedItem.dto.ID, SelectedProject.dto.ID);
                    }

                    SelectedItem = null;
                    UpdateItems();
                }
            }
        }
        private void FindItemId()
        {
            WinBox.ShowMessage("Эта кнопка более не работает!");
            //if (WinBox.ShowInput(
            //    title: "Открыть елемент",
            //    question: "Введи id",
            //    input: out string input,
            //    okText: "Открыть", cancelText: "Отмена",
            //    defautValue: SelectedObjective == null ? "" : SelectedObjective.ID.ToString()
            //    ))
            //{
            //    if (int.TryParse(input, out int id))
            //    {
            //        (ItemDto item, ObjectiveDto objective, ProjectDto project) = ObjectModel.GetItem((ID<ItemDto>)id);

            //        string message = "ObjectiveDto:\n";
            //        if (item != null)
            //        {
            //            message += $"ID={item.ID}\n";
            //            message += $"Name={item.Name}\n";
            //            message += $"ItemType={item.ItemType}\n";
            //            message += $"ExternalItemId={item.ExternalItemId}\n";
            //            //message += $"ID={item.}\n";
            //        }

            //        message += "ObjectiveDto:\n";
            //        if (objective != null)
            //        {
            //            message += $"ID={objective.ID}\n";
            //            message += $"ProjectID={objective.ProjectID}\n";
            //            message += $"Title={objective.Title}\n";
            //            message += $"Status={objective.Status}\n";
            //            message += $"Description={objective.Description}\n";
            //            message += $"CreationDate={objective.CreationDate}\n";
            //            message += $"DueDate={objective.DueDate}\n";
            //        }

            //        message += $"project:\n";
            //        if (project != null)
            //        {
            //            message += $"ID={project.ID}\n";
            //            message += $"Title={project.Title}\n";
            //        }
            //        WinBox.ShowMessage(message);
            //    }
            //}
        }

        private async void GetIDsAsync()
        {
            ChechYandex();
            if (SelectedProject == null)
            {
                WinBox.ShowMessage("Нет выбранного проекта.");
                return;
            }

            List<ID<ItemDto>> ids = ToObjective ? await yandex.GetItemsIdAsync(SelectedProject.dto, SelectedObjective.dto.ID)
                : await yandex.GetItemsIdAsync(SelectedProject.dto);

            StringBuilder message = new StringBuilder();
            for (int i = 0; i < ids.Count; i++)
            {
                if (i != 0) message.Append(',');
                if (i % 10 == 0) message.Append('\n');
                message.Append(ids[i].ToString());
            }
            WinBox.ShowMessage(message.ToString());
        }

        private async void DownloadAsync()
        {
            //ChechYandex();
            //string message = ToObjective ? $"Скачать список item-ов в задание [{SelectedObjective.Title}] c сервера ?"
            //    : $"Скачать список item-ов в проекте [{SelectedProject.Title}] с сервера?";
            //if (WinBox.ShowQuestion(message))
            //{
            //    List<ItemDto> collect = await yandex.GetItemsAsync(SelectedProject.dto, ToObjective ? SelectedObjective.dto : null);
            //    Items.Clear();
            //    foreach (ItemDto item in collect)
            //    {
            //        Items.Add((ItemModel)item);
            //    }
            //}
        }

        private async void UnloadAsync()
        {
            //ChechYandex();
            ////string message = ToObjective ? $"Загрузить файл на сервер в проект [{SelectedProject.Title}] и задание [{SelectedObjective.Title}]?"
            ////    : $"Загрузить файл на сервер в проект [{SelectedProject.Title}]?";
            ////if (WinBox.ShowQuestion(message))
            ////{
            //OpenFileDialog ofd = new OpenFileDialog();
            //ofd.Title = "Выберете файл";
            //ofd.Multiselect = false;
            //if (ofd.ShowDialog() == true)
            //{
            //    FileInfo file = new FileInfo(ofd.FileName);
            //    ItemDto item = new ItemDto();
            //    item.Name = ofd.SafeFileName;
            //    item.ID = (ID<ItemDto>)(Items.Count + 1);
            //    item.ItemType = GetItemTypeDto(file.Extension);

            //    if (!ToObjective)
            //        await yandex.UnloadItemAsync(item, ofd.FileName, SelectedProject.dto);
            //    else
            //        await yandex.UnloadItemAsync(item, ofd.FileName, SelectedProject.dto, SelectedObjective.dto);
            //}
            ////}
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
            try
            {
                SelectedProject = Projects.First();
                SelectedObjective = Objectives.First();

                ObjectModel.UpdateObjectives(SelectedProject.dto);
                //UpdateItems();
            }
            catch { }
        }

        private void UpdateItems()
        {
            if (SelectedProject != null)
            {
                if (ToObjective)
                {
                    if (SelectedObjective != null)
                        ObjectModel.UpdateItems(SelectedProject.dto, SelectedObjective.dto);
                }
                else
                    ObjectModel.UpdateItems(SelectedProject.dto);
            }
        }

        private void ChechYandex()
        {
            if (yandex == null)
            {
                //yandex = new YandexDiskManager(MainViewModel.AccessToken);
                yandex.TempDir = TEMP_DIR;
            }
        }
        #endregion



    }
}