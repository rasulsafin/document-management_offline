using System;
using System.Collections.Generic;

namespace MRS.DocumentManagement.Connection.Utils
{
    public static class PathManager
    {
        /// <summary>
        /// "BRIO MRS".
        /// </summary>
        public static readonly string APPLICATION_ROOT_DIRECTORY_NAME = "BRIO MRS";

        /// <summary>
        /// "Tables".
        /// </summary>
        public static readonly string TABLE_DIRECTORY = "Tables";

        /// <summary>
        /// "Files".
        /// </summary>
        public static readonly string FILES_DIRECTORY = "Files";

        /// <summary>
        /// "{0}.json".
        /// </summary>
        public static readonly string RECORDED_FILE_FORMAT = "{0}.json";

        public static string GetLocalAppDir() => APPLICATION_ROOT_DIRECTORY_NAME;

        public static string GetTablesDir() => DirectoryName(APPLICATION_ROOT_DIRECTORY_NAME, TABLE_DIRECTORY);

        public static string GetTableDir(string tableName) => DirectoryName(GetTablesDir(), tableName);

        public static string GetFile(string dirName, string fileName) => DirectoryName(GetNestedDirectory(dirName), fileName);

        public static string GetNestedDirectory(string dirName) => DirectoryName(APPLICATION_ROOT_DIRECTORY_NAME, dirName);

        public static string GetRecordFile(string tableName, string id) => FileName(GetTableDir(tableName), string.Format(RECORDED_FILE_FORMAT, id));

        public static string GetRootDirectory() => DirectoryName("/", APPLICATION_ROOT_DIRECTORY_NAME);

        public static string DirectoryName(string path, string nameDir)
        {
            List<string> items = new List<string>(path.Split('/', StringSplitOptions.RemoveEmptyEntries));
            items.Add(nameDir);
            string result = string.Join('/', items);
            return $"/{result}/";
        }

        public static string FileName(string path, string nameFile)
        {
            List<string> items = new List<string>(path.Split('/', StringSplitOptions.RemoveEmptyEntries));
            items.Add(nameFile);
            string result = string.Join('/', items);
            return $"/{result}";
        }
    }
}
