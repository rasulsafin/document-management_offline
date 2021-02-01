#define TEST

using MRS.DocumentManagement.Connection.YandexDisk;
using MRS.DocumentManagement.Interface.Dtos;

namespace MRS.DocumentManagement.Connection
{
    public static class PathManager
    {
        public static readonly string APP_DIR = "BRIO MRS";
        public static readonly string TABLE_DIR = "Tables";

        private static readonly string REC_FILE = "{0}.json";
        private static readonly string REVISION_FILE = "Revisions.json";

        public static string GetLocalAppDir() => APP_DIR;

        public static string GetTablesDir() => YandexHelper.DirectoryName(APP_DIR, TABLE_DIR);

        public static string GetTableDir(string tableName) => YandexHelper.DirectoryName(GetTablesDir(), tableName);

        public static string GetFile(string dirName, string fileName) => YandexHelper.DirectoryName(GetDir(dirName), fileName);

        public static string GetDir(string dirName) => YandexHelper.DirectoryName(APP_DIR, dirName);

        public static string GetRecordFile(string tableName, string id) => YandexHelper.FileName(GetTableDir(tableName), string.Format(REC_FILE, id));

        public static string GetLocalRevisionFile()
        {
            return REVISION_FILE;
        }

        public static string GetAppDir() => YandexHelper.DirectoryName("/", APP_DIR);
    }
}
