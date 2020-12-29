#define TEST

using MRS.DocumentManagement.Interface.Dtos;
using System;
using System.IO;

namespace MRS.DocumentManagement.Connection.YandexDisk
{
    public static class PathManager
    {
        public static readonly string APP_DIR = "BRIO MRS";
        private static readonly string TYRANS_DIR = "Transactions";
        private static readonly string PROJ_DIR = "Projects";
        private static readonly string OBJ_DIR = "Objectives";
        private static readonly string ITM_DIR = "Items";

        private static readonly string PROJ_FILE = "project_{0}.json";
        private static readonly string OBJ_FILE = "objective_{0}.json";
        private static readonly string ITM_FILE = "item_{0}.json";
        private static readonly string REVISION_FILE = "Revision.json";

        public static string GetItemsFile(ProjectDto project, ItemDto item)
        {
            string itemDir = GetItemsDir(project);
            return YandexHelper.FileName(itemDir, string.Format(ITM_FILE, item.ID));
        }

        public static string GetItemsFile(ProjectDto project, ObjectiveDto objective, ItemDto item)
        {
            string itemDir = GetItemsDir(project, objective);
            return YandexHelper.FileName(itemDir, string.Format(ITM_FILE, item.ID));
        }

        public static string GetItemsDir(ProjectDto project, ObjectiveDto objective)
        {
            string projDir = GetProjectDir(project);
            return YandexHelper.DirectoryName(projDir, $"{objective.ID}_" + ITM_DIR);
        }

        public static string GetRevisionFile()
        {
            return YandexHelper.DirectoryName(GetTransactionsDir(), REVISION_FILE);
        }

        public static string GetItemsDir(ProjectDto project)
        {
            string projDir = GetProjectDir(project);
            return YandexHelper.DirectoryName(projDir, ITM_DIR);
        }

        public static string GetObjectivesDir(ProjectDto project)
        {
            string projDir = GetProjectDir(project);
            return YandexHelper.DirectoryName(projDir, OBJ_DIR);
        }

        internal static string GetTransactionsDir()
        {
            return YandexHelper.DirectoryName(APP_DIR, TYRANS_DIR);
        }

        public static string GetObjectiveFile(ProjectDto project, ObjectiveDto objective)
        {
            return GetObjectiveFile(project, objective.ID);
        }
        public static string GetObjectiveFile(ProjectDto project, ID<ObjectiveDto> id)
        {
            string objDir = GetObjectivesDir(project);
            return YandexHelper.FileName(objDir, string.Format(OBJ_FILE, id));
        }

        public static string GetProjectDir(ProjectDto project)
        {
            return YandexHelper.DirectoryName(APP_DIR, project.Title);
        }

        public static string GetProjectFile(ProjectDto project) => GetProjectFile(project.ID);
        public static string GetProjectFile(ID<ProjectDto> id) => YandexHelper.FileName(GetProjectsDir(), string.Format(PROJ_FILE, id));
        public static string GetProjectsDir() => YandexHelper.DirectoryName(APP_DIR, PROJ_DIR);

        public static string GetAppDir()
        {
            return YandexHelper.DirectoryName("/", APP_DIR);
        }

        public static string GetTransactionsFile(DateTime date)
        {
            return YandexHelper.FileName(GetTransactionsDir(), date.ToString("yyyy-MM-dd") + ".json");
        }

        internal static bool TryParseTransaction(string text, out DateTime date)
        {
            string str = text.Replace(".json", "");
            if (DateTime.TryParse(str, out DateTime dat))
            {
                date = dat;
                return true;
            }
            date = DateTime.MinValue;
            return false;
        }

        public static bool TryParseObjectiveId(string str, out ID<ObjectiveDto> id)
        {
            string text = str.Replace("objective_", "").Replace(".json", "");
            if (int.TryParse(text, out int num))
            {
                id = new ID<ObjectiveDto>(num);
                return true;
            }
            id = ID<ObjectiveDto>.InvalidID;
            return false;
        }

        internal static bool TryParseProjectId(string str, out ID<ProjectDto> id)
        {
            string text = str.Replace("project_", "").Replace(".json", "");
            if (int.TryParse(text, out int num))
            {
                id = new ID<ProjectDto>(num);
                return true;
            }
            id = ID<ProjectDto>.InvalidID;
            return false;
        }
    }
}
