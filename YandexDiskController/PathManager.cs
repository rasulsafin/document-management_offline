using MRS.DocumentManagement.Interface.Dtos;
using System;
using System.IO;

namespace MRS.DocumentManagement
{
    public static class PathManager
    {
        public static readonly string APP_DIR;

        private static readonly string PROJ_DIR = "Projects";
        private static readonly string TYRANS_DIR = "Transactions";

        private static readonly string OBJ_DIR = "Objectives";

        private static readonly string ITM_DIR = "Items";

        private static readonly string PROJ_FILE = "project_{0}.json";
        private static readonly string OBJ_FILE = "objective_{0}.json";
        private static readonly string ITM_FILE = "item_{0}.json";
        private static readonly string ITM_OBJ_FILE = "item_{0}_{1}.json";
        private static readonly string REVISION_FILE = "Revision.json";
        private static readonly string TANSLATION_FILE = "Transactions.json";

        public static readonly string TEMP_DIR = "Temp";


        static PathManager()
        {
            DirectoryInfo directory = new DirectoryInfo("BRIO MRS");
            APP_DIR = directory.FullName;
        }

        public static string GetItemFile(ItemDto item, ProjectDto project, ObjectiveDto objective = null) => GetItemFile(item.ID, project, objective);
        public static string GetItemFile(ID<ItemDto> id, ProjectDto project, ObjectiveDto objective = null)
        {
            if (objective == null)
                return Path.Combine(GetItemsDir(project), string.Format(ITM_FILE, id));
            return Path.Combine(GetItemsDir(project), string.Format(ITM_OBJ_FILE, id, objective.ID.ToString()));            
        }

        public static string GetItemsDir(ProjectDto project)
        {
            string projDir = GetProjectDir(project);
            //if (objective == null)
                return Path.Combine(projDir, ITM_DIR);
            //return Path.Combine(projDir, ITM_DIR + $"_{objective.ID}");
        }


        public static string GetRevisionFile() => Path.Combine(GetTransactionsDir(), REVISION_FILE);
        public static string GetTransactionFile(DateTime date) => Path.Combine(GetTransactionsDir(), date.ToString("yyyy-MM-dd") + ".json");
        public static string GetTransactionFile() => Path.Combine(GetTransactionsDir(), TANSLATION_FILE);
        public static string GetTransactionsDir() => Path.Combine(APP_DIR, TYRANS_DIR);


        public static string GetObjectivesDir(ProjectDto project) => Path.Combine(GetProjectDir(project), OBJ_DIR);
        public static string GetObjectiveFile(ObjectiveDto objective, ProjectDto project) => Path.Combine(GetObjectivesDir(project), string.Format(OBJ_FILE, objective.ID));



        public static string GetProjectDir(ProjectDto project) => Path.Combine(APP_DIR, project.Title);
        public static string GetProjectFile(ID<ProjectDto> id) => Path.Combine(GetProjectsDir(), string.Format(PROJ_FILE, id));
        public static string GetProjectFile(ProjectDto project) => GetProjectFile(project.ID);
        public static string GetProjectsDir() => Path.Combine(APP_DIR, PROJ_DIR);

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
                    idObjective = new  ID<ObjectiveDto>(numObjective);
                    return true;
                }
            }

            idItem = ID<ItemDto>.InvalidID;
            idObjective = ID<ObjectiveDto>.InvalidID;
            return false;
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

    }
}