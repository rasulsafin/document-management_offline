using System.Collections.Generic;
using System.Windows;

namespace DocumentManagement.Launcher.Dialogs
{
    /// <summary>
    /// Простые окошли для взаимодействия с пользователем
    /// </summary>
    public static class WinBox
    {
        /// <summary>
        /// Окно сообщения пользователю, может закрытся через <c>timeout</c> миллисекунд 
        /// </summary>
        /// <param name="message">Текст cообщение</param>
        /// <param name="title">Текст заголовка окна</param>
        /// <param name="timeout">Таймаут закрытия в миллисекундах</param>
        public static void ShowMessage(string message, string title = "Message", long timeout = 0)
        {
            InputBoxModel model = new InputBoxModel();
            model.Question = message;
            model.Timeout = timeout;
            model.OkText = "Ок";
            model.ButtonsAlignment = HorizontalAlignment.Center;
            if (!string.IsNullOrWhiteSpace(title)) model.Title = title;
            model.SetVisibility(true, false, false);
            ShowInputBoxModel(model);
        }

        public static string SelectorBox(
            IEnumerable<string> collect,
            string question = "Выберите один из вариантов:",
            string title = "Окно выбора",
            int timeout = 0)
        {
            SelectorViewModel model = new SelectorViewModel();
            model.Question = question;
            model.Items = new List<string>(collect);
            model.Title = title;
            model.Timeout = timeout;
            ShowSelectorBoxModel(model);
            return model.Select;
        }

        /// <summary>
        /// Окно ввода строки
        /// </summary>
        /// <param name="question">Вопрос пользователю или объяснение что о должен ввести</param>
        /// <param name="input">Выходной параметр, введённые данные</param>
        /// <param name="title">Заголовок окна</param>
        /// <param name="okText">Тест на кнопке подтверждения ввода</param>
        /// <param name="cancelText">Текст на кнопке отмены деймствия</param>
        /// <param name="defautValue">Значение по умолчанию</param>
        /// <returns></returns>
        public static bool ShowInput(string question, out string input, string title = "Input", string okText = null, string cancelText = null, string defautValue = "")
        {
            InputBoxModel model = new InputBoxModel();
            model.Question = question;
            if (!string.IsNullOrWhiteSpace(defautValue)) model.Input = defautValue;
            if (!string.IsNullOrWhiteSpace(title)) model.Title = title;
            if (!string.IsNullOrWhiteSpace(okText)) model.OkText = okText;
            if (!string.IsNullOrWhiteSpace(cancelText)) model.CancelText = cancelText;
            model.SetVisibility(true, true, true);
            ShowInputBoxModel(model);
            input = model.Input;
            return model.PressOk;
        }

        /// <summary>
        /// Окно вопроса с двумя кнопками выбора.
        /// </summary>
        /// <param name="question">Текст вопроса</param>
        /// <param name="title">Заголовок окна</param>
        /// <param name="okText">Тест на кнопке подтверждения</param>
        /// <param name="cancelText">Текст на кнопке отмены</param>
        /// <returns></returns>
        public static bool ShowQuestion(string question, string title = "Question", string okText = "Yes", string cancelText = "No")
        {
            InputBoxModel model = new InputBoxModel();

            model.Question = question;
            if (!string.IsNullOrWhiteSpace(title)) model.Title = title;
            if (!string.IsNullOrWhiteSpace(okText)) model.OkText = okText;
            if (!string.IsNullOrWhiteSpace(cancelText)) model.CancelText = cancelText;
            model.ButtonsAlignment = HorizontalAlignment.Center;
            model.SetVisibility(true, true, false);

            ShowInputBoxModel(model);

            return model.PressOk;
        }

        private static void ShowSelectorBoxModel(SelectorViewModel model)
        {
            SelectorWindow window = new SelectorWindow(model);
            bool? res = window.ShowDialog();
            if (res == true)
            {
                model.IsResult = true;
            }
        }

        private static void ShowInputBoxModel(InputBoxModel model)
        {
            WindowBox window = new WindowBox(model);
            bool? res = window.ShowDialog();
            if (res == true)
            {
                model.PressOk = true;
            }
        }
    }
}
