using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Threading;
using WPFStorage.Base;

namespace WPFStorage.Dialogs
{
    /// <summary>
    /// Логика взаимодействия для SelectorWindow.xaml
    /// </summary>
    public partial class SelectorWindow : Window
    {
        public SelectorWindow(SelectorViewModel model)
        {
            InitializeComponent();
            Closing += (e, o) => { DialogResult = model.Select != null; };
            model.Close += () => Close();
            DataContext = model;
            Loaded += (a, b) => { model.Initialize(); };
        }
    }

    public class SelectorViewModel: BaseViewModel
    {
        private string title = "Окно ввода";
        public string Title { get => title; set { title = value; OnPropertyChanged(); } }

        private string question = "Вопрос";
        public string Question { get => question; set { question = value; OnPropertyChanged(); } }

        List<string> items;
        public List<string> Items { get => items; set { items = value; OnPropertyChanged(); } }

        public HCommand<string> SelectItem { get; }
        public string Select { get; private set; }
        public bool IsResult { get; internal set; }
        public int Timeout { get; internal set; }

        internal Action Close;
        private DispatcherTimer timer;

        public SelectorViewModel()
        {
            SelectItem = new HCommand<string>(SelectItemMethod);
           
        }

        private void SelectItemMethod(string obj)
        {
            Select = obj;
            Close?.Invoke();
        }

        internal void Initialize()
        {
            if (Timeout > 0)
            {
                timer = new DispatcherTimer();
                timer.Interval = new TimeSpan(Timeout * TimeSpan.TicksPerMillisecond);
                timer.Tick += Timer_Tick;
                timer.Start();
            }
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            timer.Stop();
            Close?.Invoke();
        }
    }
}
