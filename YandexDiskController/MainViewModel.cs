using DocumentManagement.Base;
using DocumentManagement.Connection.YandexDisk;
using DocumentManagement.Contols;
using DocumentManagement.Dialogs;
using Microsoft.Win32;
using MRS.DocumentManagement.Interface.Dtos;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Threading;

namespace DocumentManagement
{
    internal partial class MainViewModel : BaseViewModel
    {
        public static CoolLogger logger = new CoolLogger("logApp");

        #region Binding
        public string NameApp { get; private set; } = "Controller";

        private const string TEMP_DIR = "Temp";
        static MainViewModel instanse;
        static YandexDiskController controller;
        private ObservableCollection<DiskElement> folderItems = new ObservableCollection<DiskElement>();
        private Dispatcher dispatcher;
        private string path;
        private bool infoMode;
        private string title;
        private Stack<string> StackPath = new Stack<string>();
        private bool downloadProgress;
        private double currentByte;
        private double totalByte;

        public static MainViewModel Instanse { get => instanse; }
        public static YandexDiskController Controller { get => controller; }
        public HCommand CreateDirCommand { get; }
        public HCommand RootDirCommand { get; }
        public HCommand BackDirCommand { get; }
        public HCommand DebugCommand { get; }
        public HCommand LoadFileCommand { get; }
        public HCommand DeleteCommand { get; }
        public HCommand RefreshCommand { get; }
        public HCommand MoveCommand { get; }

        public ObservableCollection<DiskElement> FolderItems
        {
            get { return this.folderItems; }
            set
            {
                this.folderItems = value;
                this.OnPropertyChanged("FolderItems");
            }
        }

        public string Path
        {
            get => path;
            set
            {
                path = value;
                OnPropertyChanged();
                Title = $"{NameApp}:{path}";
            }
        }

        public string Title { get => title; set { title = value; OnPropertyChanged(); } }

        public bool InfoMode { get => infoMode; set { infoMode = value; OnPropertyChanged(); } }
        public bool DownloadProgress { get => downloadProgress; set { downloadProgress = value; OnPropertyChanged(); } }
        public double CurrentByte { get => currentByte; set { currentByte = value; OnPropertyChanged(); } }
        public double TotalByte { get => totalByte; set { totalByte = value; OnPropertyChanged(); } }
        public DiskElement SelectionElement { get; private set; }

        ProjectViewModel projects = new ProjectViewModel();
        UserViewModel users = new UserViewModel();
        ObjectiveViewModel objectives = new ObjectiveViewModel();
        ItemViewModel items = new ItemViewModel();
        

        public ProjectViewModel Projects { get => projects; set { projects = value; OnPropertyChanged(); } }

        public ObjectiveViewModel Objectives { get => objectives; set { objectives = value; OnPropertyChanged(); } }

        public UserViewModel Users { get => users; set { users = value; OnPropertyChanged(); } }
        public ItemViewModel Items { get => items; set { items = value; OnPropertyChanged(); } }
        #endregion

        public MainViewModel(Dispatcher dispatcher)
        {
            this.dispatcher = dispatcher;
            instanse = this;
            CreateDirCommand = new HCommand(CreateDir);
            RootDirCommand = new HCommand(RootDir);
            BackDirCommand = new HCommand(BackDir);
            DebugCommand = new HCommand(DebugMethod);
            LoadFileCommand = new HCommand(LoadFile);
            DeleteCommand = new HCommand(DeleteMethod);
            RefreshCommand = new HCommand(Refresh);
            MoveCommand = new HCommand(Move);

            Auth.StartAuth();
            if (!Directory.Exists(TEMP_DIR)) Directory.CreateDirectory(TEMP_DIR);
        }


        private void Refresh(object obj)
        {
            RefreshFolder();
        }

        private async void DeleteMethod(object obj)
        {
            logger.Message("Начинаю удалять!");
            if (SelectionElement == null)
            {
                WinBox.ShowMessage("Не возможно выполнить действие, объект не выбран!", timeout: 1500);
                return;
            }

            var question = "";
            if (SelectionElement.IsDirectory)
                question = $"Удалить директорию {SelectionElement.DisplayName}?";
            else
                question = $"Удалить файл {SelectionElement.DisplayName}?";

            if (WinBox.ShowQuestion(question))
            {
                
                logger.Message("Запуск таймера!");
                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();
                bool res = await controller.DeleteAsync(SelectionElement.Href);
                stopwatch.Stop();
                logger.Message($"Удаление выполнено t={stopwatch.ElapsedMilliseconds} мс") ;
                //if (res)
                Task.Run(()=>RefreshFolder());

                //WinBox.ShowMessage($"Время выполнения: {stopwatch.ElapsedMilliseconds} мс");
            }
        }

        private async void LoadFile(object obj)
        {
            OpenFileDialog OPF = new OpenFileDialog();
            if (OPF.ShowDialog() == true)
            {
                DownloadProgress = true;
                CurrentByte = 0;
                bool res = false;
                try
                {
                    res = await controller.LoadFileAsync(Path, OPF.FileName, ProgressChenge);
                    if (res)
                    {
                        WinBox.ShowMessage("Файл загружен!");
                        await RefreshFolder();
                    }
                    else
                    {
                        WinBox.ShowMessage("Ошибка загрузки файла!");
                    }
                }
                catch (Connection.YandexDisk.TimeoutException)
                {
                    WinBox.ShowMessage("Время ожидания сервера вышло!");
                }
                catch (Exception ex)
                {
                    WinBox.ShowMessage(ex.Message);
                }
                finally
                {
                    DownloadProgress = false;
                }
            }
        }

        private async Task RefreshFolder()
        {
            logger.Message("Обновляю");
            var items = await controller.GetListAsync(Path);
            SetFolderItems(items);
            logger.Message("Обновил");
        }

        private async void DebugMethod(object obj)
        {
            DownloadProgress = true;
            TotalByte = 100;
            CurrentByte = 0;
            await Task.Delay(3000);
            for (int i = 0; i < 100; i++)
            {
                await Task.Delay(300);
                CurrentByte = i;
            }
            await Task.Delay(3000);
            DownloadProgress = false;
        }

        private async void BackDir(object obj)
        {
            if (StackPath.Count != 0)
            {
                Path = StackPath.Pop();
                await RefreshFolder();
            }
        }

        private async void RootDir(object obj)
        {
            StackPath.Clear();
            Path = "/";
            await RefreshFolder();
        }

        private async void CreateDir(object obj)
        {
            if (WinBox.ShowInput(
                question: "Введите название папки:",
                input: out string nameDir,
                title: "Создание папки",
                okText: "Создать", cancelText: "Отмена"))
            {// Создать папку 
                bool res = await controller.CreateDirAsync(Path, nameDir);
                if (res)
                    await RefreshFolder();
                else
                    WinBox.ShowMessage("Каталог не создан!");
            }
        }
        private async void Move(object obj)
        {
            if (SelectionElement == null)
                WinBox.ShowMessage("Не могу выполнить операцию, нет выбранного элемента!");
            else if (WinBox.ShowInput(
                question: $"Введите новое название папки '{SelectionElement.DisplayName}':",
                input: out string nameDir,
                title: "Переименование папки",
                defautValue: SelectionElement.DisplayName,
                okText: "Перименовать", cancelText: "Отмена"))
            {// Создать папку 
                bool res = await controller.MoveAsync(SelectionElement.Href, YandexHelper.DirectoryName(Path, nameDir));
                if (res)
                    await RefreshFolder();
                else
                    WinBox.ShowMessage("Каталог не переименован!");
            }

        }


        internal async void SelectItemAsync(int selectedIndex)
        {
            var item = FolderItems[selectedIndex];
            if (item.IsDirectory)
            {
                StackPath.Push(Path);
                Path = item.Href;
                var items = await controller.GetListAsync(item.Href);
                SetFolderItems(items);
            }
            else
            {
                string tempPath = System.IO.Path.Combine(TEMP_DIR, item.DisplayName);
                if (!System.IO.File.Exists(tempPath))
                {
                    if (WinBox.ShowQuestion("Скачать файл во временный каталог и открыть?"))
                    {
                        DownloadProgress = true;
                        CurrentByte = 0;
                        bool res = await controller.DownloadFileAsync(item.Href, tempPath, ProgressChenge);
                        DownloadProgress = false;
                        if (res)
                        {
                            WinBox.ShowMessage("Файл загружен!");
                        }
                        else
                        {
                            WinBox.ShowMessage("Ошибка загрузки файла!");
                        }
                    }
                    else
                        return;
                }
                try
                {
                    Process.Start(tempPath);
                }
                catch
                {
                    Process.Start(new ProcessStartInfo("explorer.exe", " /select, " + tempPath));
                }
                //Process.Start("C:\\Windows\\System32\\notepad.exe", tempPath.Trim());
            }
        }

        private void ProgressChenge(ulong current, ulong total)
        {
            if (TotalByte != total) TotalByte = total;
            CurrentByte = current;
        }

        internal void SelectionChanged(SelectionChangedEventArgs e)
        {
            var count = e.AddedItems.Count;
            if (count != 0)
            {
                var obj = e.AddedItems[count - 1];
                if (obj is DiskElement lastElement)
                    SelectionElement = lastElement;
            }
            if (InfoMode)
            {
                var message = "";
                foreach (var item in e.AddedItems)
                {
                    if (item is DiskElement element)
                    {
                        message += $"DisplayName={element.DisplayName}\n-----\n";
                        message += $"  Href={element.Href}\n";
                        message += $"  ContentLength={element.ContentLength}\n";
                        message += $"  ContentType={element.ContentType}\n";
                        message += $"  Getetag={element.Getetag}\n";
                        message += $"  LastModified={element.Resourcetype}\n";
                        message += $"  Creationdate={element.Creationdate}\n";
                        message += $"  LastModified={element.LastModified}\n";
                        message += $"  LastModified={element.Mulca_digest_url}\n";
                        message += $"  LastModified={element.Mulca_file_url}\n";
                        message += $"  LastModified={element.Status}\n";
                        message += $"  File_url={element.File_url}\n";
                        //message += $"  LastModified={element.}\n"; 
                    }
                }
                WinBox.ShowMessage(message, "Информация");
            }
        }
        private void SetFolderItems(IEnumerable<DiskElement> items)
        {
            FolderItems.Clear();
            foreach (var item in items)
            {
                if (item.Href != Path)
                    FolderItems.Add(item);
            }
        }
    }
}