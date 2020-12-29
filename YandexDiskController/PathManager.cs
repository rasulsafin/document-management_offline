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
        private static readonly string REVISION_FILE = "Revision.json";
        private static readonly string TANSLATION_FILE = "Transactions.json";

        private static readonly string TEMP_DIR = "Temp.Yandex";


        static PathManager()
        {
            DirectoryInfo directory = new DirectoryInfo("BRIO MRS");
            APP_DIR = directory.FullName;
        }

        public static string GetRevisionFile() => Path.Combine(GetTransactionsDir(), REVISION_FILE);
        public static string GetTransactionFile(DateTime date) => Path.Combine(GetTransactionsDir(), date.ToString("yyyy-MM-dd") + ".json");
        public static string GetTransactionFile() => Path.Combine(GetTransactionsDir(), TANSLATION_FILE);
        public static string GetTransactionsDir() => Path.Combine(APP_DIR, TYRANS_DIR);

        public static string GetItemFile(ItemDto item, ProjectDto project, ObjectiveDto objective = null) => Path.Combine(GetItemsDir(project, objective), string.Format(ITM_FILE, item.ID));

        public static string GetItemsDir(ProjectDto project, ObjectiveDto objective = null)
        {
            string projDir = GetProjectDir(project);
            if (objective == null)
                return Path.Combine(projDir, ITM_DIR);
            return Path.Combine(projDir, ITM_DIR + $"_{objective.ID}");
        }

        public static string GetObjectivesDir(ProjectDto project) => Path.Combine(GetProjectDir(project), OBJ_DIR);

        public static string GetObjectiveFile(ProjectDto project, ObjectiveDto objective) => Path.Combine(GetObjectivesDir(project), string.Format(OBJ_FILE, objective.ID));


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

    }
}