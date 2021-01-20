using System;
using System.Diagnostics;
using System.IO;
using WPFStorage.Dialogs;

namespace MRS.DocumentManagement.Contols
{
    internal class OpenHelper
    {
        internal static void Geany(string fileName)
        {
            Process.Start(@"c:\Program Files (x86)\Geany\bin\geany.exe", $"\"{fileName}\"");
        }

        internal static void Notepad(string fileName)
        {
            Process.Start("C:\\Windows\\System32\\notepad.exe", $"\"{fileName}\"") ;
        }

        internal static void LoadExeption(Exception ex, string fileName)
        {
            var select = WinBox.SelectorBox(
                    new[]
                    {
                        "Удалить файл",
                        "Посмотреть",
                        "Закрыть приложение"
                    },
                    "При загрузки файла призошла ошибка:\n" + ex.Message, "Ошибка", 5000);

            if (select == "Посмотреть")
                OpenHelper.Geany(fileName);
            else if (select == "Удалить файл")
                File.Delete(fileName);
            else
                Environment.Exit(0);
        }
    }
}
