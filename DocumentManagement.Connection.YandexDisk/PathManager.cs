#define TEST

using MRS.DocumentManagement.Interface.Dtos;
using System;
using System.IO;

namespace MRS.DocumentManagement.Connection.YandexDisk
{
    public static class PathManager
    {
        public static readonly string APP_DIR = "BRIO MRS";
        private static readonly string REV_DIR = "Revisions";
        private static readonly string PROJ_DIR = "Projects";
        private static readonly string USERS_DIR = "Users";
        private static readonly string OBJ_DIR = "Objectives";
        private static readonly string ITM_DIR = "Items";

        private static readonly string USER_FILE = "user_{0}.json";
        private static readonly string PROJ_FILE = "project_{0}.json";
        private static readonly string OBJ_FILE = "objective_{0}.json";
        private static readonly string ITM_FILE = "item_{0}.json";
        private static readonly string ITM_OBJ_FILE = "item_{0}_{1}.json";
        private static readonly string REVISION_FILE = "Revisions.json";


        
        public static string GetRevisionsFile() => YandexHelper.DirectoryName(GetRevisionsDir(), REVISION_FILE);
        public static string GetRevisionsDir() => YandexHelper.DirectoryName(APP_DIR, REV_DIR);



        public static string GetItemFile(ProjectDto project, ItemDto item) => GetItemFile(project, item.ID);
        public static string GetItemFile(ProjectDto project, ID<ItemDto> id)
        {
            string itemDir = GetItemsDir(project);
            return YandexHelper.FileName(itemDir, string.Format(ITM_FILE, id));
        }
        public static string GetItemFile(ProjectDto project, ObjectiveDto objective, ItemDto item) => GetItemFile(project, objective.ID, item.ID);
        public static string GetItemFile(ProjectDto project, ID<ObjectiveDto> idObjective, ID<ItemDto> id)
        {
            string itemDir = GetItemsDir(project);
            return YandexHelper.FileName(itemDir, string.Format(ITM_OBJ_FILE, id, idObjective));
        }
        public static string GetItemsDir(ProjectDto project)
        {
            string projDir = GetProjectDir(project);
            return YandexHelper.DirectoryName(projDir, ITM_DIR);
        }

        

        //public static string GetItemsDir(ProjectDto project, ObjectiveDto objective) => GetItemsDir(project, objective.ID);

        //public static string GetItemsDir(ProjectDto project)
        //{
        //    string projDir = GetProjectDir(project);
        //    return YandexHelper.DirectoryName(projDir, $"{ITM_DIR}");
        //}



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


        public static string GetTransactionsFile(DateTime date)
        {
            return YandexHelper.FileName(GetRevisionsDir(), date.ToString("yyyy-MM-dd") + ".json");
        }
        public static string GetAppDir()
        {
            return YandexHelper.DirectoryName("/", APP_DIR);
        }


        public static bool TryParseTransaction(string text, out DateTime date)
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
        public static bool TryParseProjectId(string str, out ID<ProjectDto> id)
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
        public static bool TryParseItemId(string str, out ID<ItemDto> idItem, out ID<ObjectiveDto> idObjective)
        {
            string[] texts = str.Replace("item_", "").Replace(".json", "").Split('_');
            if (texts.Length == 1)
            {
                if (int.TryParse(texts[0], out int num))
                {
                    idItem = new ID<ItemDto>(num);
                    idObjective = ID<ObjectiveDto>.InvalidID;
                    return true;
                }
            }

            if (texts.Length == 2)
            {
                if (int.TryParse(texts[0], out int numItem) && int.TryParse(texts[1], out int numObjective))
                {
                    idItem = new ID<ItemDto>(numItem);
                    idObjective = new ID<ObjectiveDto>(numObjective);
                    return true;
                }
            }

            idItem = ID<ItemDto>.InvalidID;
            idObjective = ID<ObjectiveDto>.InvalidID;
            return false;
        }

        
    }
}
