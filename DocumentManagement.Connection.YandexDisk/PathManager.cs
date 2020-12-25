#define TEST

using MRS.DocumentManagement.Interface.Dtos;
using System.IO;

namespace MRS.DocumentManagement.Connection.YandexDisk
{
    public static class PathManager
    {
        public static readonly string APP_DIR = "BRIO MRS";
        private static readonly string PROJ_DIR = "Projects";
        private static readonly string OBJ_DIR = "Objectives";
        private static readonly string ITM_DIR = "Items";

        private static readonly string PROJ_FILE = "project_{0}.json";
        private static readonly string OBJ_FILE = "objective_{0}.json";
        private static readonly string ITM_FILE = "item_{0}.json";                

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

        public static string GetProjectFile(ProjectDto project)
        {
            return YandexHelper.FileName(GetProjectsDir(), string.Format(PROJ_FILE, project.ID));
        }

        public static string GetProjectsDir()
        {
            return YandexHelper.DirectoryName(APP_DIR, PROJ_DIR);
        }

        public static string GetAppDir()
        {
            return YandexHelper.DirectoryName("/", APP_DIR);
        }
    }
}
