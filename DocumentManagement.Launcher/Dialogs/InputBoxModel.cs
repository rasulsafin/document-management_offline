using System;
using System.Windows;
using System.Windows.Threading;
using DocumentManagement.Launcher.Base;

namespace DocumentManagement.Launcher.Dialogs
{
    public class InputBoxModel : BaseViewModel
    {
        #region Field
        private string input;
        private string title = "Окно ввода";
        private string question = "Введите текст";
        private string okText = "Ввод";
        private string cancelText = "Отмена";
        private HorizontalAlignment questionAlignment;
        private HorizontalAlignment buttonsAlignment;
        private Visibility okVisibility;
        private Visibility cancelVisibility;
        private Visibility inputVisibility;
        private DispatcherTimer timer;
        private long timeout;
        #endregion

        #region Constructor
        public InputBoxModel()
        {
            OkCommand = new HCommand(Ok);
            CancelCommand = new HCommand(Cancel);
        }
        #endregion

        #region Binding
        public string Title
        {
            get => title;
            set
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

        public string Input
        {
            get => input; set
            {
                input = value;
                OnPropertyChanged();
            }
        }

        public string OkText
        {
            get => okText; set
            {
                okText = value;
                OnPropertyChanged();
            }
        }

        public string CancelText
        {
            get => cancelText; set
            {
                cancelText = value;
                OnPropertyChanged();
            }
        }

        public HorizontalAlignment QuestionAlignment
        {
            get => questionAlignment; set
            {
                questionAlignment = value;
                OnPropertyChanged();
            }
        }

        public HorizontalAlignment ButtonsAlignment
        {
            get => buttonsAlignment; set
            {
                buttonsAlignment = value;
                OnPropertyChanged();
            }
        }

        public bool PressOk { get; set; }

        public long Timeout
        {
            get => timeout; set
            {
                timeout = value;
                OnPropertyChanged();
            }
        }

        public Visibility OkVisibility
        {
            get => okVisibility; set
            {
                okVisibility = value;
                OnPropertyChanged();
            }
        }

        public Visibility CancelVisibility
        {
            get => cancelVisibility; set
            {
                cancelVisibility = value;
                OnPropertyChanged();
            }
        }

        public Visibility InputVisibility
        {
            get => inputVisibility; set
            {
                inputVisibility = value;
                OnPropertyChanged();
            }
        }

        public HCommand OkCommand { get; private set; }

        public HCommand CancelCommand { get; private set; } 
        #endregion

        public Action Close { get; internal set; }

        public Action Success { get; internal set; }

        public void SetVisibility(bool ok, bool cancel, bool input)
        {
            OkVisibility = ok ? Visibility.Visible : Visibility.Collapsed;
            CancelVisibility = cancel ? Visibility.Visible : Visibility.Collapsed;
            InputVisibility = input ? Visibility.Visible : Visibility.Collapsed;
        }

        internal void Initialize()
        {
            if (Timeout > 0)
            {
                timer = new DispatcherTimer();
                timer.Interval = new TimeSpan(Timeout * TimeSpan.TicksPerMillisecond);
                timer.Tick += TimerTick;
                timer.Start();
            }
        }

        private void TimerTick(object sender, EventArgs e)
        {
            timer.Stop();
            Close?.Invoke();
        }

        private void Ok() => Success?.Invoke();

        private void Cancel() => Close?.Invoke();
    }
}
