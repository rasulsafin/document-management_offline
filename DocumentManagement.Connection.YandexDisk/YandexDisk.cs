#define TEST

using MRS.DocumentManagement.Interface.Dtos;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace DocumentManagement.Connection.YandexDisk
{
    public class YandexDisk
    {
        public static CoolLogger logger = new CoolLogger("YandexDisk");

        private static readonly string PROGECTS_FILE = "Projects.xml";
        private static readonly string OBJECTIVE_FILE = "Objective.json";
        private static readonly string APP_DIR = "BRIO MRS";

        private string accessToken;
        private YandexDiskController controller;

        public YandexDisk(string accessToken)
        {
            this.accessToken = accessToken;
            controller = new YandexDiskController(accessToken);
            Initialize();
        }

        private async void Initialize()
        {
            IEnumerable<DiskElement> list = await controller.GetListAsync();
            if (!list.Any(
                x => x.IsDirectory && x.DisplayName == APP_DIR
                ))
            {
                await controller.CreateDirAsync("/", APP_DIR);
            }
        }

        public string TempDir { get; set; }

        public async Task UnloadObjectivesAsync(ObjectiveDto[] objectiveDtos, ProjectDto project)
        {
            string app = $"/{APP_DIR}/";
            var list = await controller.GetListAsync(app);
            string projDir = $"/{APP_DIR}/{project.Title}/";
            if (! list.Any(x => x.Href == projDir && x.IsDirectory))
            {
                await controller.CreateDirAsync(app, project.Title);
            }

            if (!Directory.Exists(TempDir)) Directory.CreateDirectory(TempDir);
            string fileName = Path.Combine(TempDir, OBJECTIVE_FILE);
            var json = JsonConvert.SerializeObject(objectiveDtos);
            File.WriteAllText(fileName, json);

            await controller.LoadFileAsync(projDir, fileName);
        }

        public async Task<ObjectiveDto[]> DownloadObjectivesAsync(ProjectDto project)
        {
            string app = $"/{APP_DIR}/";
            var list = await controller.GetListAsync(app);
            string projDir = $"/{APP_DIR}/{project.Title}/";
            if (!list.Any(x => x.Href == projDir && x.IsDirectory))
            {
                return null;
            }

            list = await controller.GetListAsync(projDir);
            string objFile = $"{projDir}{OBJECTIVE_FILE}";
            if (!list.Any(x => x.Href == objFile && !x.IsDirectory))
            {
                return null;
            }

            if (!Directory.Exists(TempDir)) Directory.CreateDirectory(TempDir);
            string fileName = Path.Combine(TempDir, OBJECTIVE_FILE);

            bool res = await controller.DownloadFileAsync(objFile, fileName);
            if (res)
            {
                var json = File.ReadAllText(fileName);
                ObjectiveDto[] collect = JsonConvert.DeserializeObject<ObjectiveDto[]>(json);
                return collect;
            }
            return null;
        }

        #region Projects

        /// <summary>
        /// Загрузка проектов 
        /// Вызывается при бездумном копировании  
        /// </summary>
        /// <param name="collectionProject"></param>
        /// <returns></returns>
#if TEST
        public
#else
        private 
#endif
            async Task UnloadProjects(List<ProjectDto> collectionDto)
        {
            string app = $"/{APP_DIR}/";

            if (!Directory.Exists(TempDir)) Directory.CreateDirectory(TempDir);
            string fileName = Path.Combine(TempDir, PROGECTS_FILE);

            var json = JsonConvert.SerializeObject(collectionDto);
            File.WriteAllText(fileName, json);

            await controller.LoadFileAsync(app, fileName);
        }

        /// <summary>
        /// Скачивание проектов
        /// Вызывается при бездумном копировании  
        /// </summary>
        /// <param name="collectionProject"></param>
        /// <returns></returns>
#if TEST
        public
#else
        private
#endif
            async Task<List<ProjectDto>> DownloadProjects()
        {
            string app = $"/{APP_DIR}/";
            if (!Directory.Exists(TempDir)) Directory.CreateDirectory(TempDir);
            string fileName = Path.Combine(TempDir, PROGECTS_FILE);

            bool res = await controller.DownloadFileAsync(YandexHelper.FileName(app, PROGECTS_FILE), fileName);
            //if (!res) 
            var json = File.ReadAllText(fileName);
            List<ProjectDto> collection = JsonConvert.DeserializeObject<List<ProjectDto>>(json);
            return collection;
        }


        /// <summary>
        /// Добавляет проект в файл данных.
        /// предварительно проверяет наличие id, если такой id есть то перезаписывет запись.
        /// Создает директорию проекта
        /// </summary>
        /// <param name="project"></param>
        /// <returns></returns>
        public async Task<bool> AddProject(ProjectDto project)
        {
            try
            {
                string app = $"/{APP_DIR}/";
                IEnumerable<DiskElement> list = await controller.GetListAsync(app);
                List<ProjectDto> projects = new List<ProjectDto>();

                if (list.Any(x => x.DisplayName == PROGECTS_FILE && !x.IsDirectory))
                {
                    projects = await DownloadProjects();
                    var find = projects.Find(x => x.ID.Equals(project.ID));
                    if (find != null)
                    {
                        projects.Remove(find);
                    }
                }
                projects.Add(project);
                await UnloadProjects(projects);

                if (!list.Any(x => x.IsDirectory && x.DisplayName == project.Title))
                    await controller.CreateDirAsync(app, project.Title);
                return true;
            }
            catch (WebException)
            { }
            return false;
        }


        /// <summary>
        /// Скачивает файл данных, удаляет проект найденный по id, закачивает файл обратно.
        /// Удаляет директорию проекта найденную по DisplayName.
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        public async Task<bool> DeleteProject(ProjectDto dto)
        {
            try
            {
                string app = $"/{APP_DIR}/";
                IEnumerable<DiskElement> list = await controller.GetListAsync(app);
                List<ProjectDto> projects = new List<ProjectDto>();

                if (list.Any(x => x.DisplayName == PROGECTS_FILE && !x.IsDirectory))
                {
                    //string fileName = Path.Combine(TempDir, PROGECTS_FILE);
                    projects = await DownloadProjects();
                    var find = projects.Find(x => x.ID.Equals(dto.ID));
                    if (find != null)
                    {
                        projects.Remove(find);
                        await UnloadProjects(projects);
                    }
                }

                if (list.Any(x => x.IsDirectory && x.DisplayName == dto.Title))
                    await controller.DeleteAsync(YandexHelper.DirectoryName(app, dto.Title));
                return true;
            }
            catch (WebException)
            { }
            return false;
        }

        /// <summary>
        /// Обновляет название проекта в папке
        /// Обновляет название директории
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        public async Task<bool> UpdateProject(ProjectDto dto)
        {
            try
            {
                string app = $"/{APP_DIR}/";
                IEnumerable<DiskElement> list = await controller.GetListAsync(app);
                List<ProjectDto> projects = new List<ProjectDto>();

                if (list.Any(x => x.DisplayName == PROGECTS_FILE && !x.IsDirectory))
                {
                    //string fileName = Path.Combine(TempDir, PROGECTS_FILE);
                    projects = await DownloadProjects();
                    var find = projects.Find(x => x.ID.Equals(dto.ID));
                    if (find != null)
                    {

                        bool res = await controller.MoveAsync(
                            originPath: YandexHelper.DirectoryName(app, find.Title),
                            movePath: YandexHelper.DirectoryName(app, dto.Title));

                        find.Title = dto.Title;
                        await UnloadProjects(projects);
                    }
                }

                return true;
            }
            catch (WebException)
            { }
            return false;
        } 
        #endregion


    }
}
