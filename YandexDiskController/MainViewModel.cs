using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Threading;
using Microsoft.Win32;
using MRS.DocumentManagement.Connection;
using MRS.DocumentManagement.Connection.YandexDisk;
using MRS.DocumentManagement.Contols;
using WPFStorage.Base;
using WPFStorage.Dialogs;

namespace MRS.DocumentManagement
{
    internal class MainViewModel : BaseViewModel
    {
        public static CoolLogger Logger = new CoolLogger("logApp");
        private static MainViewModel instanse;
        private static YandexDiskController controller;
        private ObservableCollection<DiskElement> folderItems = new ObservableCollection<DiskElement>();
        private Dispatcher dispatcher;
        private string path;
        private bool infoMode;
        private string title;
        private Stack<string> stackPath = new Stack<string>();
        private bool downloadProgress;
        private double currentByte;
        private double totalByte;
        private ProjectViewModel projects = new ProjectViewModel();
        private UserViewModel users = new UserViewModel();
        private ObjectiveViewModel objectives = new ObjectiveViewModel();
        private ItemViewModel items = new ItemViewModel();
        private SynchronizerViewModel synchronizer = new SynchronizerViewModel();
        private int selectedTab;

        public MainViewModel(Dispatcher dispatcher)
        {
            if (!Directory.Exists(PathManager.APP_DIR)) Directory.CreateDirectory(PathManager.APP_DIR);
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

            Auth.LoadActions.Add(Initialize);

            // Auth.LoadActions.Add(InitializeManager);
            Auth.StartAuth();

            SelectedTab = Properties.Settings.Default.SelectedTab;
        }

        #region Binding
        public static string AccessToken { get; private set; }

        public static MainViewModel Instanse { get => instanse; }

        public static YandexDiskController Controller { get => controller; }

        public string NameApp { get; private set; } = "Controller";

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
            get
            {
                return this.folderItems;
            }

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

        public string Title
        {
            get => title; set
            {
                title = value;
                OnPropertyChanged();
            }
        }

        public bool InfoMode
        {
            get => infoMode; set
            {
                infoMode = value;
                OnPropertyChanged();
            }
        }

        public bool DownloadProgress
        {
            get => downloadProgress; set
            {
                downloadProgress = value;
                OnPropertyChanged();
            }
        }

        public double CurrentByte
        {
            get => currentByte; set
            {
                currentByte = value;
                OnPropertyChanged();
            }
        }

        public double TotalByte
        {
            get => totalByte; set
            {
                totalByte = value;
                OnPropertyChanged();
            }
        }

        public DiskElement SelectionElement { get; private set; }

        public ProjectViewModel Projects
        {
            get => projects; set
            {
                projects = value;
                OnPropertyChanged();
            }
        }

        public ObjectiveViewModel Objectives
        {
            get => objectives; set
            {
                objectives = value;
                OnPropertyChanged();
            }
        }

        public UserViewModel Users
        {
            get => users; set
            {
                users = value;
                OnPropertyChanged();
            }
        }

        public ItemViewModel Items
        {
            get => items; set
            {
                items = value;
                OnPropertyChanged();
            }
        }

        public SynchronizerViewModel Synchronizer
        {
            get => synchronizer; set
            {
                synchronizer = value;
                OnPropertyChanged();
            }
        }

        public int SelectedTab
        {
            get => selectedTab; set
            {
                selectedTab = value;
                OnPropertyChanged();
            }
        }

        #endregion
        public void CloseApp()
        {
            // ObjectModel.Synchronizer.Save();
            Properties.Settings.Default.SelectedTab = SelectedTab;
            Properties.Settings.Default.Save();
        }

        internal async void SelectItemAsync(int selectedIndex)
        {
            var item = FolderItems[selectedIndex];
            if (item.IsDirectory)
            {
                stackPath.Push(Path);
                Path = item.Href;
                var items = await controller.GetListAsync(item.Href);
                SetFolderItems(items);
            }
            else
            {
                string tempPath = System.IO.Path.Combine(PathManager.APP_DIR, item.DisplayName);
                if (!System.IO.File.Exists(tempPath))
                {
                    var select = WinBox.SelectorBox(
                        title: "Выбор действия",
                        question: "Выберите одно из двух действий",
                        collect: new[] { "Скачать файл", "Показать информацию" });
                    if (select == "Скачать файл")
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

                        try
                        {
                            Process.Start(tempPath);
                        }
                        catch
                        {
                            Process.Start(new ProcessStartInfo("explorer.exe", " /select, " + tempPath));
                        }
                    }
                    else if (select == "Показать информацию")
                    {
                        var info = await controller.GetInfoAsync(item.Href);
                        if (info != null)
                        {
                            var message = BuildMessage(info);
                            WinBox.ShowMessage(message, "Информация");
                        }
                    }
                }

                // Process.Start("C:\\Windows\\System32\\notepad.exe", tempPath.Trim());
            }
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
                var message = string.Empty;
                foreach (var item in e.AddedItems)
                {
                    if (item is DiskElement element)
                    {
                        message += BuildMessage(element);

                        // message += $"  LastModified={element.}\n";
                    }
                }

                WinBox.ShowMessage(message, "Информация");
            }
        }

        private static string BuildMessage(DiskElement element)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append($"DisplayName={element.DisplayName}\n-----\n");
            builder.Append($"  Href={element.Href}\n");
            builder.Append($"  ContentLength={element.ContentLength}\n");
            builder.Append($"  ContentType={element.ContentType}\n");
            builder.Append($"  Getetag={element.Getetag}\n");
            builder.Append($"  LastModified={element.Resourcetype}\n");
            builder.Append($"  Creationdate={element.Creationdate}\n");
            builder.Append($"  LastModified={element.LastModified}\n");
            builder.Append($"  LastModified={element.Mulca_digest_url}\n");
            builder.Append($"  LastModified={element.Mulca_file_url}\n");
            builder.Append($"  LastModified={element.Status}\n");
            builder.Append($"  File_url={element.File_url}\n");
            return builder.ToString();
        }

        private void Initialize(string accessToken)
        {
            AccessToken = accessToken;
            controller = new YandexDiskController(AccessToken);
            RootDir();
        }

        private async void Refresh()
        {
            await RefreshFolder();
        }

        private async void DeleteMethod()
        {
            Logger.Message("Начинаю удалять!");
            if (SelectionElement == null)
            {
                WinBox.ShowMessage("Не возможно выполнить действие, объект не выбран!", timeout: 1500);
                return;
            }

            var question = string.Empty;
            if (SelectionElement.IsDirectory)
                question = $"Удалить директорию {SelectionElement.DisplayName}?";
            else
                question = $"Удалить файл {SelectionElement.DisplayName}?";

            if (WinBox.ShowQuestion(question))
            {
                Logger.Message("Запуск таймера!");
                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();
                bool res = await controller.DeleteAsync(SelectionElement.Href);
                stopwatch.Stop();
                Logger.Message($"Удаление выполнено t={stopwatch.ElapsedMilliseconds} мс");

                await RefreshFolder();
            }
        }

        private async void LoadFile()
        {
            OpenFileDialog opf = new OpenFileDialog();
            if (opf.ShowDialog() == true)
            {
                DownloadProgress = true;
                CurrentByte = 0;
                bool res = false;
                try
                {
                    res = await controller.LoadFileAsync(Path, opf.FileName, ProgressChenge);
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
                catch (Connection.TimeoutException)
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
            Logger.Message("Обновляю");
            var items = await controller.GetListAsync(Path);
            SetFolderItems(items);
            Logger.Message("Обновил");
        }

        private async void DebugMethod()
        {
            // DownloadProgress = true;
            // TotalByte = 100;
            // CurrentByte = 0;
            // await Task.Delay(3000);
            // for (int i = 0; i < 100; i++)
            // {
            //    await Task.Delay(300);
            //    CurrentByte = i;
            // }

            // await Task.Delay(3000);
            // DownloadProgress = false;
            if (WinBox.ShowInput(
                title: "Проверка информации",
                question: "Введите название папки или файла информацию о котором хотите проверить:",
                input: out string nameDir,
                okText: "Ввести",
                cancelText: "Отмена",
                defautValue: SelectionElement.Href))
            {
                try
                {
                    var res = await controller.GetInfoAsync(nameDir);
                    var mes = BuildMessage(res);
                    WinBox.ShowMessage(mes);
                }
                catch (FileNotFoundException)
                {
                    WinBox.ShowMessage("Элемент не существует!");
                }
            }
        }

        private async void BackDir()
        {
            if (stackPath.Count != 0)
            {
                Path = stackPath.Pop();
                await RefreshFolder();
            }
        }

        private async void RootDir()
        {
            stackPath.Clear();
            Path = "/";
            await RefreshFolder();
        }

        private async void CreateDir()
        {
            if (WinBox.ShowInput(
                question: "Введите название папки:",
                input: out string nameDir,
                title: "Создание папки",
                okText: "Создать",
                cancelText: "Отмена"))
            {// Создать папку
                bool res = await controller.CreateDirAsync(Path, nameDir);
                if (res)
                    await RefreshFolder();
                else
                    WinBox.ShowMessage("Каталог не создан!");
            }
        }

        private async void Move()
        {
            if (SelectionElement == null)
            {
                WinBox.ShowMessage("Не могу выполнить операцию, нет выбранного элемента!");
            }
            else if (WinBox.ShowInput(
                question: $"Введите новое название папки '{SelectionElement.DisplayName}':",
                input: out string nameDir,
                title: "Переименование папки",
                defautValue: SelectionElement.DisplayName,
                okText: "Перименовать",
                cancelText: "Отмена"))
            {// Создать папку
                bool res = await controller.MoveAsync(SelectionElement.Href, YandexHelper.DirectoryName(Path, nameDir));
                if (res)
                    await RefreshFolder();
                else
                    WinBox.ShowMessage("Каталог не переименован!");
            }
        }

        private void ProgressChenge(ulong current, ulong total)
        {
            if (TotalByte != total) TotalByte = total;
            CurrentByte = current;
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
