#define TEST

using System;
using System.IO;
using MRS.DocumentManagement.Connection.YandexDisk;
using MRS.DocumentManagement.Interface.Dtos;

namespace MRS.DocumentManagement.Connection
{
    public static class PathManager
    {
        public static readonly string APP_DIR = "BRIO MRS";
        public static readonly string TABLE_DIR = "Tables";

        private static readonly string REV_DIR = "Revisions";
        private static readonly string PROJ_DIR = "Projects";
        private static readonly string USERS_DIR = "Users";
        private static readonly string OBJ_DIR = "Objectives";

        private static readonly string USER_FILE = "user_{0}.json";
        private static readonly string PROJ_FILE = "project_{0}.json";
        private static readonly string OBJ_FILE = "objective_{0}.json";
        private static readonly string ITMS_FILE = "items.json";
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

        #region Remote
        public static string GetRevisionsFile() => YandexHelper.DirectoryName(GetRevisionsDir(), REVISION_FILE);

        public static string GetRevisionsDir() => YandexHelper.DirectoryName(APP_DIR, REV_DIR);

        public static string GetItemsFile(ProjectDto project)
        {
            string projDir = GetProjectDir(project);
            return YandexHelper.FileName(projDir, ITMS_FILE);
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

        public static string GetProjectDir(ProjectDto project) => YandexHelper.DirectoryName(APP_DIR, project.Title);

        public static string GetProjectFile(ProjectDto project) => GetProjectFile(project.ID);

        public static string GetProjectFile(ID<ProjectDto> id) => YandexHelper.FileName(GetProjectsDir(), string.Format(PROJ_FILE, id));

        public static string GetProjectsDir() => YandexHelper.DirectoryName(APP_DIR, PROJ_DIR);

        public static string GetUsersDir() => YandexHelper.DirectoryName(APP_DIR, USERS_DIR);

        public static string GetUserFile(UserDto user) => GetUserFile(user.ID);

        public static string GetUserFile(ID<UserDto> id) => YandexHelper.FileName(GetUsersDir(), string.Format(USER_FILE, id));

        public static string GetAppDir() => YandexHelper.DirectoryName("/", APP_DIR);
        #endregion

    }
}
