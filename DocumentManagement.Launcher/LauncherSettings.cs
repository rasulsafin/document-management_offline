using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Windows;
using System.Xml.Serialization;

namespace MRS.DocumentManagement.Launcher
{
    [Serializable]
    public static class LauncherSettings
    {
        public static readonly string PATH = "Setting.json";

        public static string SwaggerPath { get; set; } = @"http://localhost:5000/index.html";

        public static string DMExecutablePath { get; set; } = @"W:/temp/DM/DocumentManagement.Api.exe";

        public static void Load()
        {
            LauncherSettingsDto data = null;
            if (File.Exists(PATH))
            {
                try
                {
                    string json = File.ReadAllText(PATH);
                    data = JsonSerializer.Deserialize<LauncherSettingsDto>(json);
                }
                catch (Exception ex)
                {
                    Application.Current.Dispatcher.Invoke(() => MessageBox.Show($"Ошибка чтения файла!\n {ex.Message}"));
                }
            }
            else
            {
                data = new LauncherSettingsDto();
                Save();
            }

            SwaggerPath = data.SwaggerPath;
            DMExecutablePath = data.DMExecutablePath;
        }

        public static void Save()
        {
            LauncherSettingsDto data = new LauncherSettingsDto
            {
                SwaggerPath = SwaggerPath,
                DMExecutablePath = DMExecutablePath,
            };

            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
            };

            string json = JsonSerializer.Serialize(data, options);
            File.WriteAllText(PATH, json);
        }

        public class LauncherSettingsDto
        {
            public string SwaggerPath { get; set; }

            public string DMExecutablePath { get; set; }
        }
    }
}
