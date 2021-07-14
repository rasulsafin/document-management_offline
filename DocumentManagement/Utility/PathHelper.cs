using System;
using System.IO;
using System.Linq;
using MRS.DocumentManagement.Database.Models;
using MRS.DocumentManagement.Interface.Dtos;

namespace MRS.DocumentManagement.Utility
{
    public class PathHelper
    {
        private static readonly string APPLICATION_DIRECTORY_NAME = "Brio MRS";
        private static readonly string DATABASE_DIRECTORY_NAME = "Database";
        private static readonly string MEDIA_DIRECTORY_NAME = "Media";
        private static readonly string MY_DOCUMENTS = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

        private static readonly char[] INVALID_PATH_CHARS = Path.GetInvalidFileNameChars();

        public static string ApplicationFolder => Combine(MY_DOCUMENTS, APPLICATION_DIRECTORY_NAME);

        public static string Database => Combine(ApplicationFolder, DATABASE_DIRECTORY_NAME);

        public static string GetFileName(string path)
            => Path.GetFileName(path);

        public static string GetFullPath(string projectName, string fileName)
            => Combine(Database, GetValidDirectoryName(projectName), fileName.TrimStart('/', '\\'));

        public static string GetFullPath(Project project, string fileName)
        => Combine(Database, GetValidDirectoryName(project), fileName.TrimStart('/', '\\'));

        public static string GetValidDirectoryName(Project project)
        {
            var db = new DirectoryInfo(Database);
            var dir = db.GetDirectories().FirstOrDefault((info) =>
            {
                (string title, int id) = GetProjectTitleAndId(info.Name);
                if (id == (int)project.ID)
                    return true;
                return false;
            });

            if (dir != null)
                return dir.Name;

            // TODO : You can trim the project title, for example, by taking the first 50 characters.
            return GetValidDirectoryName($"{project.Title}-{project.ID}");
        }

        public static string GetRelativePath(string fileName, ItemType type)
        {
            var relativePath = type switch
            {
                ItemType.Media => Path.Combine(MEDIA_DIRECTORY_NAME, fileName),
                _ => Path.Combine(string.Empty, fileName),
            };
            return '\\' + relativePath;
        }

        private static (string title, int id) GetProjectTitleAndId(string folderName)
        {
            int index = folderName.LastIndexOf('-');
            if (index != -1)
            {
                string idText = folderName.Substring(index + 1);
                if (int.TryParse(idText, out int id))
                {
                    return (folderName.Substring(0, index - 1), id);
                }
            }

            return (folderName, -1);
        }

        private static string GetValidDirectoryName(string name)
        {
            foreach (char c in INVALID_PATH_CHARS)
                name = name.Replace(c, '~');
            return name;
        }

        private static string Combine(params string[] strings)
            => Path.GetFullPath(Path.Combine(strings));
    }
}
