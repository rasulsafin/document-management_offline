using System;
using System.Collections.Generic;

namespace MRS.DocumentManagement.Connection
{
    public static class PathManager
    {
        public static readonly string APP_DIR = "BRIO MRS";
        public static readonly string TABLE_DIR = "Tables";

        private static readonly string REC_FILE = "{0}.json";
        private static readonly string REVISION_FILE = "Revisions.json";

        public static string GetLocalAppDir() => APP_DIR;

        public static string GetTablesDir() => DirectoryName(APP_DIR, TABLE_DIR);

        public static string GetTableDir(string tableName) => DirectoryName(GetTablesDir(), tableName);

        public static string GetFile(string dirName, string fileName) => DirectoryName(GetDir(dirName), fileName);

        public static string GetDir(string dirName) => DirectoryName(APP_DIR, dirName);

        public static string GetRecordFile(string tableName, string id) => FileName(GetTableDir(tableName), string.Format(REC_FILE, id));

        public static string GetLocalRevisionFile()
        {
            return REVISION_FILE;
        }

        public static string GetAppDir() => DirectoryName("/", APP_DIR);

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
