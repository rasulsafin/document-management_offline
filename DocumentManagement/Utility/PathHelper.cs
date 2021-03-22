using System;
using System.IO;
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
            => Path.GetFullPath(Path.Combine(Database, GetValidDirectoryName(projectName), fileName.TrimStart('/', '\\')));

        public static string GetRelativePath(string fileName, ItemType type)
        {
            return type switch
            {
                ItemType.Media => Path.Combine(MEDIA_DIRECTORY_NAME, fileName),
                _ => Path.Combine(string.Empty, fileName),
            };
        }

        public static string GetValidDirectoryName(string name)
        {
            foreach (char c in INVALID_PATH_CHARS)
                name = name.Replace(c, '~');
            return name;
        }

        private static string Combine(params string[] strings)
            => Path.GetFullPath(Path.Combine(strings));
    }
}
