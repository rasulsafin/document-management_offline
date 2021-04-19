using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Xml.Serialization;
using DocumentManagement.Launcher.Base;
using DocumentManagement.Launcher.Dialogs;

namespace DocumentManagement.Launcher
{
    [Serializable]
    public class LauncherSettings
    {
        private static readonly string PATH = "Setting.xml";
        private static LauncherSettings instance;

        public static LauncherSettings Instance
        {
            get
            {
                if (instance == null)
                    instance = Load(PATH);

                return instance;
            }
        }

        public bool VisibleConsole { get; set; } = false;

        public string SwaggerPath { get; set; } = @"http://localhost:5000/index.html";

        public string DMApiPath { get; set; } = @"W:\temp\DM\DocumentManagement.Api.exe";

        public void Save() => Save(PATH, this);

        private static void Save(string path, LauncherSettings setting)
        {
            XmlSerializer formatter = new XmlSerializer(typeof(LauncherSettings));

            using (FileStream fs = new FileStream(path, FileMode.OpenOrCreate))
            {
                formatter.Serialize(fs, setting);
            }
        }

        private static LauncherSettings Load(string path)
        {
            if (!File.Exists(path))
            {
                var newSettings = new LauncherSettings();
                Save(path, newSettings);
                return newSettings;
            }

            XmlSerializer formatter = new XmlSerializer(typeof(LauncherSettings));
            using (FileStream fs = new FileStream(path, FileMode.OpenOrCreate))
            {
                LauncherSettings setting = (LauncherSettings)formatter.Deserialize(fs);
                return setting;
            }
        }
    }
}
