using MRS.DocumentManagement.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace MRS.DocumentManagement.Dialogs
{
    public static class WinBox
    {
        private static void ShowDialogModel(InputBoxModel model)
        {
            InputBoxWindow window = new InputBoxWindow(model);
            bool? res = window.ShowDialog();
            if (res == true)
            {
                model.PressOk = true;
            }
        }

        //public static string ShowDialog(string question, string title = "Input")
        //{
        //    InputBoxModel model = new InputBoxModel();
        //    model.Question = question;
        //    if (!string.IsNullOrWhiteSpace(title)) model.Title = title;
        //    model.SetVisibility(true, true, true);
        //    ShowDialogModel(model);
        //    return model.Input;
        //}

        public static bool ShowInput(string question, out string input, string title = "Input", string okText = null, string cancelText = null, string defautValue="")
        {
            InputBoxModel model = new InputBoxModel();
            model.Question = question;
            if (!string.IsNullOrWhiteSpace(defautValue)) model.Input = defautValue;
            if (!string.IsNullOrWhiteSpace(title)) model.Title = title;
            if (!string.IsNullOrWhiteSpace(okText)) model.OkText = okText;
            if (!string.IsNullOrWhiteSpace(cancelText)) model.CancelText = cancelText;
            model.SetVisibility(true, true, true);
            ShowDialogModel(model);
            input = model.Input;
            return model.PressOk;
        }

        internal static void ShowMessage(string message, string title = "Message", long timeout = 0)
        {
            InputBoxModel model = new InputBoxModel();
            model.Question = message;
            model.Timeout = timeout;
            model.OkText = "Ок";
            model.ButtonsAlignment = HorizontalAlignment.Center;
            if (!string.IsNullOrWhiteSpace(title)) model.Title = title;
            model.SetVisibility(true, false, false);
            ShowDialogModel(model);
            //return model.Input;
        }

        internal static bool ShowQuestion(string question, string title = "Question", string okText = "Yes", string cancelText = "No")
        {
            InputBoxModel model = new InputBoxModel();

            model.Question = question;
            if (!string.IsNullOrWhiteSpace(title)) model.Title = title;
            if (!string.IsNullOrWhiteSpace(okText)) model.OkText = okText;
            if (!string.IsNullOrWhiteSpace(cancelText)) model.CancelText = cancelText;
            model.ButtonsAlignment = HorizontalAlignment.Center;
            model.SetVisibility(true, true, false);

            ShowDialogModel(model);

            return model.PressOk;
        }
    }


    /// <summary>
    /// Логика взаимодействия для InputBoxWindow.xaml
    /// </summary>
    public partial class InputBoxWindow : Window
    {
        //private InputBoxModel model;
        public InputBoxWindow(InputBoxModel model)
        {
            InitializeComponent();
            //this.model = model;
            model.Success = () => { DialogResult = true; Close(); };
            model.Close = () => { DialogResult = false; Close(); };
            DataContext = model;
            Loaded += (a,b) => { model.Initialize(); };
        }        
    }

    public class InputBoxModel : BaseViewModel
    {
        private string title = "Окно ввода";
        public string Title { get => title; set { title = value; OnPropertyChanged(); } }

        private string question = "Введите текст";
        public string Question { get => question; set { question = value; OnPropertyChanged(); } }

        private string input;
        public string Input { get => input; set { input = value; OnPropertyChanged(); } }

        private string okText = "Ввод";
        public string OkText { get => okText; set { okText = value; OnPropertyChanged(); } }

        private string cancelText = "Отмена";
        public string CancelText { get => cancelText; set { cancelText = value; OnPropertyChanged(); } }

        private string button3Text;
        public string Button3Text { get => button3Text; set { button3Text = value; OnPropertyChanged(); } }

        private HorizontalAlignment questionAlignment;
        public HorizontalAlignment QuestionAlignment { get => questionAlignment; set { questionAlignment = value; OnPropertyChanged(); } }

        private HorizontalAlignment buttonsAlignment;
        public HorizontalAlignment ButtonsAlignment { get => buttonsAlignment; set { buttonsAlignment = value; OnPropertyChanged(); } }
        public bool PressOk { get; set; }
        private long timeout;
        public long Timeout{get => timeout; set{timeout = value;OnPropertyChanged();}}
        
        public HCommand OkCommand { get; private set; }
        public HCommand CancelCommand { get; private set; }
        public HCommand Button3Command { get; private set; }
        public Action Close { get; internal set; }
        public Action Success { get; internal set; }
        public Visibility[] Visibilities { get => visibilities; set { visibilities = value; OnPropertyChanged(); } }


        private Visibility[] visibilities = new Visibility[4];
        DispatcherTimer timer;

        public InputBoxModel()
        {

            OkCommand = new HCommand(Ok);
            CancelCommand = new HCommand(Cancel);
            Button3Command = new HCommand(Button3);            
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            timer.Stop();
            Close?.Invoke();
        }

        private void Ok(object obj) => Success?.Invoke();
        private void Cancel(object obj) => Close?.Invoke();

        private void Button3(object obj)
        {
            //Close?.Invoke();
        }

        public void SetVisibility(bool ok, bool cancel, bool input, bool button3 = false)
        {
            Visibilities[0] = (ok) ? Visibility.Visible : Visibility.Collapsed;
            Visibilities[1] = (cancel) ? Visibility.Visible : Visibility.Collapsed;
            Visibilities[2] = (button3) ? Visibility.Visible : Visibility.Collapsed;
            Visibilities[3] = (input) ? Visibility.Visible : Visibility.Collapsed;
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
    }
}
