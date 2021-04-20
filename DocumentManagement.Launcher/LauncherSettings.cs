using System;
using System.IO;
using System.Windows;
using System.Xml.Serialization;
using DocumentManagement.Launcher.Dialogs;

namespace DocumentManagement.Launcher
{
    [Serializable]
    public class LauncherSettings
    {
        public static readonly string PATH = "Setting.xml";
        private static LauncherSettings instance;

        public static LauncherSettings Instance
        {
            get
            {
                if (instance == null)
                    Reload();

                return instance;
            }
        }

        public static void Reload() => instance = Load(PATH);

        public bool VisibleConsole { get; set; } = false;

        public string SwaggerPath { get; set; } = @"http://localhost:5000/index.html";

        public string DMApiPath { get; set; } = @"W:/temp/DM/DocumentManagement.Api.exe";

        public void Save() => Save(PATH, this);

        private static void Save(string path, LauncherSettings setting)
        {
            XmlSerializer formatter = new XmlSerializer(typeof(LauncherSettings));

            using (FileStream fs = new FileStream(path, FileMode.Truncate))
            {
                formatter.Serialize(fs, setting);
            }
        }

        private static LauncherSettings Load(string path)
        {
            LauncherSettings settings = null;
            if (File.Exists(path))
            {
                try
                {
                    XmlSerializer formatter = new XmlSerializer(typeof(LauncherSettings));
                    using (FileStream fs = new FileStream(path, FileMode.OpenOrCreate))
                    {
                        settings = (LauncherSettings)formatter.Deserialize(fs);
                        return settings;
                    }
                }
                catch (Exception ex)
                {
                    Application.Current.Dispatcher.Invoke(() => WinBox.ShowMessage($"Ошибка чтения файла!\n {ex.Message}"));
                }
            }
            else
            {
                settings = new LauncherSettings();
                Save(path, settings);
            }

            if (settings == null) settings = new LauncherSettings();

            return settings;
        }

    }
}
