using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using Microsoft.Win32;
using MRS.DocumentManagement.Connection;
using MRS.DocumentManagement.Interface.Dtos;
using MRS.DocumentManagement.Models;
using WPFStorage.Base;
using WPFStorage.Dialogs;

namespace MRS.DocumentManagement.Contols
{
    public class ItemViewModel : BaseViewModel
    {
        #region Const
        private static readonly string MEDIA_EXTENTION = ".avi .wav .mp4 .mpg .mpeg .jpg .jpeg .png";
        private static readonly string MODELS_EXTENTION = ".ifc .ifczip .bim ";
        #endregion
        #region bending
        private DiskManager yandex;
        private bool toObjective;

        public ItemViewModel()
        {
            AddItemsCommand = new HCommand(AddItem);
            DelItemsCommand = new HCommand(DelItem);
            GetIdCommand = new HCommand(GetIDsAsync);
            FindItemIdCommand = new HCommand(FindItemId);
            ZeroIdCommand = new HCommand(ZeroId);
            AllItemCommand = new HCommand(AllItem);

            // UnloadCommand = new HCommand(UnloadAsync);
            // SaveFileCommand = new HCommand(SaveFile);
            // LoadFileCommand = new HCommand(LoadFile);
            // DownloadCommand = new HCommand(DownloadAsync);
            Initilization();
        }

        public ObservableCollection<ProjectModel> Projects { get; set; } = ObjectModel.Projects;

        public ObservableCollection<ObjectiveModel> Objectives { get; set; } = new ObservableCollection<ObjectiveModel>();

        public ObservableCollection<ItemModel> Items { get; set; } = ObjectModel.Items;

        public ProjectModel SelectedProject
        {
            get => ObjectModel.SelectedProject;
            set
            {
                if (value != null)
                {
                    ObjectModel.SelectedProject = value;
                    Objectives.Clear();
                    foreach (var objective in ObjectModel.GetObjectives(SelectedProject.dto))
                    {
                        Objectives.Add(new ObjectiveModel(objective));
                    }

                    ToObjective = false;
                    OnPropertyChanged();

                    // OnPropertyChanged("SelectedObjective");
                    // OnPropertyChanged("Objectives");
                    OnPropertyChanged("Items");
                }
            }
        }

        public ObjectiveModel SelectedObjective
        {
            get => ObjectModel.SelectedObjective;
            set
            {
                ObjectModel.SelectedObjective = value;
                UpdateItems();
                OnPropertyChanged();
                OnPropertyChanged("Items");
            }
        }

        public ItemModel SelectedItem
        {
            get => ObjectModel.SelectedItem; set
            {
                ObjectModel.SelectedItem = value;
                OnPropertyChanged();
            }
        }

        public bool ToObjective
        {
            get => toObjective; set
            {
                toObjective = value;
                UpdateItems();
                OnPropertyChanged();
            }
        }

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

        public HCommand AllItemCommand { get; }

        // public HCommand RenItemsCommand { get; }
        // public HCommand SaveFileCommand { get; }
        // public HCommand LoadFileCommand { get; }
        // public HCommand DownloadCommand { get; }
        #endregion

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
            {
                return ItemTypeDto.File;
            }
        }

        private void AllItem()
        {
            Items.Clear();
            AddCollect(SelectedProject.dto.Items);
            foreach (var obj in Objectives)
            {
                AddCollect(obj.dto.Items);
            }

            void AddCollect(IEnumerable<ItemDto> items)
            {
                foreach (var item in items)
                {
                    Items.Add(new ItemModel(item));
                }
            }
        }

        private void ZeroId()
        {
            Properties.Settings.Default.ItemNextId = 0;
            Properties.Settings.Default.Save();
        }

        private void AddItem()
        {
            // var dirItems = PathManager.GetItemsDir(SelectedProject.dto);
            // if (!Directory.Exists(dirItems)) Directory.CreateDirectory(dirItems);
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
                    string path = PathManager.GetLocalProjectDir(SelectedProject.dto);
                    file = file.CopyTo(Path.Combine(path, file.Name));
                }
                catch
                {
                }

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
            if (SelectedItem == null)
            {
                WinBox.ShowMessage("Нет выбранного элемента.");
            }
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
        }

        private void GetIDsAsync()
        {
            WinBox.ShowMessage("Эта кнопка более не работает!");
        }

        #region Инициализация
        private void Initilization()
        {
            try
            {
                SelectedProject = Projects.First();
                SelectedObjective = Objectives.First();

                ObjectModel.UpdateObjectives(SelectedProject.dto);

                // UpdateItems();
            }
            catch
            {
            }
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
                {
                    ObjectModel.UpdateItems(SelectedProject.dto);
                }
            }
        }
        #endregion

    }
}
