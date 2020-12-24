#define TEST

using MRS.DocumentManagement;
using MRS.DocumentManagement.Interface.Dtos;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace MRS.DocumentManagement.Connection.YandexDisk
{
    public class YandexDiskManager
    {
        public static CoolLogger logger = new CoolLogger("YandexDisk");

        private static readonly string PROGECTS_FILE = "Projects.json";
        private static readonly string ITEMS_FILE = "Items.json";
        private static readonly string OBJECTIVE_FILE = "Objective.json";
        private static readonly string APP_DIR = "BRIO MRS";

        private string accessToken;
        private YandexDiskController controller;
        public string TempDir { get; set; }

        public YandexDiskManager(string accessToken)
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


        public async Task<ItemDto> UnloadItemAsync(ItemDto dto, string fileName, ProjectDto project, ObjectiveDto objective = null)
        {
            string app = GetDirApp();
            string projDir = await CheckDirProject(project, app);
            string hrefItems = GetItemFileName(ITEMS_FILE, projDir, objective?.Title);
            FileInfo file = new FileInfo(fileName);
            // TODO:  Рассовывать по папочкам здесь!!!
            string hrefFile = GetItemFileName(file.Name, projDir);

            // Получаем информацию о файлах в папке проекта
            var list = await controller.GetListAsync(projDir);            
            List<ItemDto> items = new List<ItemDto>();
            if (list.Any(x => x.Href == hrefItems))// Это не первый item в этой категории
            {
                string json = await controller.GetContetnAsync(hrefItems);
                items = JsonConvert.DeserializeObject<List<ItemDto>>(json);
            }
            // Непосредственная загрузка файла
            await controller.LoadFileAsync(projDir, file.FullName);

            // Информация о файле
            dto.ExternalItemId = hrefFile;
            items.Add(dto);
            string jsn = JsonConvert.SerializeObject(items);
            await controller.SetContetnAsync(hrefItems, jsn);

            return dto;
        }
        public async Task<List<ItemDto>> GetItemsAsync(ProjectDto project, ObjectiveDto objective = null)
        {
            string app = GetDirApp();
            string projDir = await CheckDirProject(project, app);
            string hrefItems = GetItemFileName(ITEMS_FILE, projDir, objective?.Title);
            // Получаем информацию о файлах в папке проекта
            var list = await controller.GetListAsync(projDir);
            List<ItemDto> items = new List<ItemDto>();
            if (list.Any(x => x.Href == hrefItems))// Это не первый item в этой категории
            {
                string json = await controller.GetContetnAsync(hrefItems);
                items = JsonConvert.DeserializeObject<List<ItemDto>>(json);
            }
            return items;
        }

        private string GetItemFileName(string item, string progectDir, string objective = null)
        {
            string fileName = (objective == null)? item : $"{objective}_{item}";

            return $"{progectDir}{fileName}";

        }


        public async Task SetObjectivesAsync(ObjectiveDto[] objectiveDtos, ProjectDto project)
        {
            string app = GetDirApp();
            string projDir = await CheckDirProject(project, app);

            var json = JsonConvert.SerializeObject(objectiveDtos);
            await controller.SetContetnAsync(YandexHelper.FileName(projDir, OBJECTIVE_FILE), json);

            //if (!Directory.Exists(TempDir)) Directory.CreateDirectory(TempDir);
            //string fileName = Path.Combine(TempDir, OBJECTIVE_FILE);
            //var json = JsonConvert.SerializeObject(objectiveDtos);
            //File.WriteAllText(fileName, json);

            //await controller.LoadFileAsync(projDir, fileName);
        }


        private async Task<string> CheckDirProject(ProjectDto project, string app)
        {
            var list = await controller.GetListAsync(app);
            string projDir = $"/{APP_DIR}/{project.Title}/";
            if (!list.Any(x => x.Href == projDir && x.IsDirectory))
            {
                await controller.CreateDirAsync(app, project.Title);
            }

            return projDir;
        }

        public async Task<ObjectiveDto[]> GetObjectivesAsync(ProjectDto project)
        {
            string app = GetDirApp();            
            string projDir = await CheckDirProject(project, app);  
            var list = await controller.GetListAsync(projDir);

            string objFile = $"{projDir}{OBJECTIVE_FILE}";
            if (!list.Any(x => x.Href == objFile && !x.IsDirectory))
            {
                return null;
            }

            var json = await controller.GetContetnAsync(objFile);            
            List<ObjectiveDto> collect = JsonConvert.DeserializeObject<List<ObjectiveDto>>(json);
            return collect.ToArray();

            //if (!Directory.Exists(TempDir)) Directory.CreateDirectory(TempDir);
            //string fileName = Path.Combine(TempDir, OBJECTIVE_FILE);

            //bool res = await controller.DownloadFileAsync(objFile, fileName);
            //if (res)
            //{
            //    var json = File.ReadAllText(fileName);
            //    List<ObjectiveDto> collect = JsonConvert.DeserializeObject<List<ObjectiveDto>>(json);
            //    return collect.ToArray();
            //}
            //return null;
        }

        private static string GetDirApp()
        {
            return $"/{APP_DIR}/";
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
            string app = GetDirApp();

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
            string app = GetDirApp();
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
                string app = GetDirApp();
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
                string app = GetDirApp();
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
                string app = GetDirApp();
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
