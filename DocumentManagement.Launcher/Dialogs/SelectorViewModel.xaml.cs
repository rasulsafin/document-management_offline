using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Threading;
using DocumentManagement.Launcher.Base;

namespace DocumentManagement.Launcher.Dialogs
{

    public class SelectorViewModel : BaseViewModel
    {
        #region Field
        private string title = "Окно ввода";
        private string question = "Вопрос";
        private List<string> items;
        private DispatcherTimer timer;
        #endregion

        #region Constructor
        public SelectorViewModel()
        {
            SelectItem = new HCommand<string>(SelectItemMethod);
        }
        #endregion

        #region Binding
        public string Title
        {
            get => title; set
            {
                title = value;
                OnPropertyChanged();
            }
        }

        public string Question
        {
            get => question; set
            {
                question = value;
                OnPropertyChanged();
            }
        }

        public List<string> Items
        {
            get => items; set
            {
                items = value;
                OnPropertyChanged();
            }
        }

        public string Select { get; private set; }

        public bool IsResult { get; internal set; }

        public int Timeout { get; internal set; } 

        public HCommand<string> SelectItem { get; }
        #endregion

        public Action Close { get; internal set; }

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

        private void SelectItemMethod(string obj)
        {
            Select = obj;
            Close?.Invoke();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            timer.Stop();
            Close?.Invoke();
        }
    }
}
