#define TEST

using MRS.DocumentManagement.Interface.Dtos;
using System.IO;

namespace MRS.DocumentManagement.Connection.YandexDisk
{
    public static class PathManager
    {
        private static readonly string APP_DIR = "BRIO MRS";
        private static readonly string PROJ_DIR = "Projects";
        private static readonly string OBJ_DIR = "Objectives";
        private static readonly string ITM_DIR = "Items";
        private static readonly string PROJ_FILE = "project_{0}.json";
        private static readonly string OBJ_FILE = "objective_{0}.json";
        private static readonly string ITM_FILE = "item_{0}.json";                

        public static string GetItemsFile(ProjectDto project, ItemDto item)
        {
            string itemDir = GetItemsDir(project);
            return Path.Combine(itemDir, string.Format(ITM_FILE, item.ID));
        }

        public static string GetItemsFile(ProjectDto project, ObjectiveDto objective, ItemDto item)
        {
            string itemDir = GetItemsDir(project, objective);
            return Path.Combine(itemDir, string.Format(ITM_FILE, item.ID));
        }

        public static string GetItemsDir(ProjectDto project, ObjectiveDto objective)
        {
            string projDir = GetProjectDir(project);
            return Path.Combine(projDir, $"{objective.ID}_" + ITM_DIR);
        }

        public static string GetItemsDir(ProjectDto project)
        {
            string projDir = GetProjectDir(project);
            return Path.Combine(projDir, ITM_DIR);
        }

        public static string GetObjectivesDir(ProjectDto project)
        {
            string projDir = GetProjectDir(project);
            return Path.Combine(projDir, OBJ_DIR);
        }

        public static string GetObjectiveFile(ProjectDto project, ObjectiveDto objective)
        {
            string objDir = GetObjectivesDir(project);
            return Path.Combine(objDir, string.Format(OBJ_FILE, objective.ID));
        }


        public static string GetProjectDir(ProjectDto project)
        {
            return Path.Combine(APP_DIR, project.Title);
        }

        public static string GetProjectFile(ProjectDto project)
        {
            return Path.Combine(GetProjectsDir(), string.Format(PROJ_FILE, project.ID));
        }

        public static string GetProjectsDir()
        {
            return Path.Combine(APP_DIR, PROJ_DIR);
        }

        public static string GetAppDir()
        {
            return YandexHelper.DirectoryName("/", APP_DIR);
        }
    }
}
