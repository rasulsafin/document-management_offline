#define TEST

using MRS.DocumentManagement;
using MRS.DocumentManagement.Interface.Dtos;
using Newtonsoft.Json;
using System;
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

        private string accessToken;
        private YandexDiskController controller;
        private bool projectsDirCreate;

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
                x => x.IsDirectory && x.DisplayName == PathManager.APP_DIR
                ))
            {
                await controller.CreateDirAsync("/", PathManager.APP_DIR);
            }
        }


        public async Task<ItemDto> UnloadItemAsync(ItemDto dto, string fileName, ProjectDto project, ObjectiveDto objective = null)
        {
            throw new NotImplementedException();
            //string app = GetDirApp();
            //string projDir = await CheckDirProject(project, app);
            //string hrefItems = GetItemFileName(ITEMS_FILE, projDir, objective?.Title);
            //FileInfo file = new FileInfo(fileName);
            //// TODO:  Рассовывать по папочкам здесь!!!
            //string hrefFile = GetItemFileName(file.Name, projDir);

            //// Получаем информацию о файлах в папке проекта
            //var list = await controller.GetListAsync(projDir);            
            //List<ItemDto> items = new List<ItemDto>();
            //if (list.Any(x => x.Href == hrefItems))// Это не первый item в этой категории
            //{
            //    string json = await controller.GetContetnAsync(hrefItems);
            //    items = JsonConvert.DeserializeObject<List<ItemDto>>(json);
            //}
            //// Непосредственная загрузка файла
            //await controller.LoadFileAsync(projDir, file.FullName);

            //// Информация о файле
            //dto.ExternalItemId = hrefFile;
            //items.Add(dto);
            //string jsn = JsonConvert.SerializeObject(items);
            //await controller.SetContetnAsync(hrefItems, jsn);

            //return dto;
        }
        public async Task<List<ItemDto>> GetItemsAsync(ProjectDto project, ObjectiveDto objective = null)
        {
            throw new NotImplementedException();
            //string app = GetDirApp();
            //string projDir = await CheckDirProject(project, app);
            //string hrefItems = GetItemFileName(ITEMS_FILE, projDir, objective?.Title);
            //// Получаем информацию о файлах в папке проекта
            //var list = await controller.GetListAsync(projDir);
            //List<ItemDto> items = new List<ItemDto>();
            //if (list.Any(x => x.Href == hrefItems))// Это не первый item в этой категории
            //{
            //    string json = await controller.GetContetnAsync(hrefItems);
            //    items = JsonConvert.DeserializeObject<List<ItemDto>>(json);
            //}
            //return items;
        }

        private string GetItemFileName(string item, string progectDir, string objective = null)
        {
            string fileName = (objective == null) ? item : $"{objective}_{item}";

            return $"{progectDir}{fileName}";

        }


        public async Task UploadObjectiveAsync(ObjectiveDto objective, ProjectDto project, Action<ulong, ulong> progressChenge = null)
        {
            await CheckDirProject(project);
            await CheckDirObjectives(project);

            string path = PathManager.GetObjectiveFile(project, objective);
            string json = JsonConvert.SerializeObject(objective);
            await controller.SetContetnAsync(path, json, progressChenge);
        }

        private async Task<bool> CheckDirObjectives(ProjectDto project)
        {
            bool result = true;
            IEnumerable<DiskElement> list = await controller.GetListAsync(PathManager.GetProjectDir(project));
            string path = PathManager.GetObjectivesDir(project);
            if (!list.Any(
                x => x.IsDirectory && x.Href == path
                ))
            {
                await controller.CreateDirAsync("/", path);
                result = false;
            }
            return result;
        }

        public async Task<List<ID<ObjectiveDto>>> GetObjectivesIdAsync(ProjectDto project)
        {
            List<ID<ObjectiveDto>> result = new List<ID<ObjectiveDto>>();
            string path = PathManager.GetObjectivesDir(project);
            IEnumerable<DiskElement> list = await controller.GetListAsync(path);
            foreach (var element in list)
            {
                if (PathManager.TryParseObjectiveId(element.DisplayName, out ID<ObjectiveDto> id))
                {
                    result.Add(id);
                }
            }
            return result;
            //throw new NotImplementedException();
        }

        private async Task<bool> CheckDirProject(ProjectDto project)
        {
            bool result = true;
            IEnumerable<DiskElement> list = await controller.GetListAsync(PathManager.GetAppDir());
            string path = PathManager.GetProjectDir(project);
            if (!list.Any(
                x => x.IsDirectory && x.Href == path
                ))
            {
                await controller.CreateDirAsync("/", path);
                result = false;
            }
            return result;
        }

        public async Task<ObjectiveDto> GetObjectiveAsync(ProjectDto project, ID<ObjectiveDto> id)
        {
            //if (await CheckDirProject(project))
            //    if (await CheckDirObjectives(project))
            //    {
            //        IEnumerable<DiskElement> list = await controller.GetListAsync(PathManager.GetObjectivesDir(project));
            //        string path = PathManager.GetObjectiveFile(project, id);
            //        if (list.Any(x.Href == path))
            //        {
            //            string json = await controller.GetContetnAsync(path);
            //            ObjectiveDto objective = JsonConvert.DeserializeObject<ObjectiveDto>(json);
            //            return objective;                        
            //        }
            //    }
            //return null;
            try
            {
                string path = PathManager.GetObjectiveFile(project, id);
                string json = await controller.GetContetnAsync(path);
                ObjectiveDto objective = JsonConvert.DeserializeObject<ObjectiveDto>(json);
                return objective;
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                logger.Open();
                throw;
            }

        }


        //private static string GetDirApp()
        //{
        //    return $"/{APP_DIR}/";
        //}

        #region Projects
        /// <summary>
        /// Загрузить проект на диск
        /// </summary>
        /// <param name="dto">проект</param>
        /// <returns></returns>
        public async Task UnloadProject(ProjectDto project)
        {
            await CheckProjects();
            string path = PathManager.GetProjectFile(project);
            var json = JsonConvert.SerializeObject(project);
            await controller.SetContetnAsync(path, json);
        }


        /// <summary>
        /// Создает папку проектов
        /// Выполнится один раз 
        /// </summary>
        /// <returns></returns>
        private async Task<bool> CheckProjects()
        {
            bool result = true;
            if (!projectsDirCreate)
            {
                IEnumerable<DiskElement> list = await controller.GetListAsync(PathManager.GetAppDir());

                string path = PathManager.GetProjectsDir();
                if (!list.Any(
                    x => x.IsDirectory && x.Href == path
                    ))
                {
                    await controller.CreateDirAsync("/", path);
                    result = false;
                }
                projectsDirCreate = true;
            }
            return result;
        }

        /// <summary>
        /// Загружает все проекты
        /// </summary>
        /// <returns></returns>
        public async Task<List<ProjectDto>> DownloadProjects()
        {
            if (!await CheckProjects())
                return new List<ProjectDto>();

            IEnumerable<DiskElement> list = await controller.GetListAsync(PathManager.GetProjectsDir());
            var projects = new List<ProjectDto>();
            foreach (var item in list)
            {
                if (item.DisplayName.StartsWith("project_"))
                {
                    string json = await controller.GetContetnAsync(item.Href);
                    ProjectDto project = JsonConvert.DeserializeObject<ProjectDto>(json);
                    projects.Add(project);
                }
            }
            return projects;
        }

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
            throw new NotImplementedException();
            //string app = GetDirApp();

            //if (!Directory.Exists(TempDir)) Directory.CreateDirectory(TempDir);
            //string fileName = Path.Combine(TempDir, PROGECTS_FILE);

            //var json = JsonConvert.SerializeObject(collectionDto);
            //File.WriteAllText(fileName, json);

            //await controller.LoadFileAsync(app, fileName);
        }


        /// <summary>
        /// Скачивание проектов
        /// Вызывается при бездумном копировании  
        /// </summary>
        /// <param name="collectionProject"></param>
        /// <returns></returns>
        //#if TEST
        //        public
        //#else
        //        private
        //#endif



        /// <summary>
        /// Добавляет проект в файл данных.
        /// предварительно проверяет наличие id, если такой id есть то перезаписывет запись.
        /// Создает директорию проекта
        /// </summary>
        /// <param name="project"></param>
        /// <returns></returns>
        public async Task<bool> AddProject(ProjectDto project)
        {
            throw new NotImplementedException();
            //try
            //{
            //    string app = GetDirApp();
            //    IEnumerable<DiskElement> list = await controller.GetListAsync(app);
            //    List<ProjectDto> projects = new List<ProjectDto>();

            //    if (list.Any(x => x.DisplayName == PROGECTS_FILE && !x.IsDirectory))
            //    {
            //        projects = await DownloadProjects();
            //        var find = projects.Find(x => x.ID.Equals(project.ID));
            //        if (find != null)
            //        {
            //            projects.Remove(find);
            //        }
            //    }
            //    projects.Add(project);
            //    await UnloadProjects(projects);

            //    if (!list.Any(x => x.IsDirectory && x.DisplayName == project.Title))
            //        await controller.CreateDirAsync(app, project.Title);
            //    return true;
            //}
            //catch (WebException)
            //{ }
            //return false;
        }


        /// <summary>
        /// Скачивает файл данных, удаляет проект найденный по id, закачивает файл обратно.
        /// Удаляет директорию проекта найденную по DisplayName.
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        public async Task<bool> DeleteProject(ProjectDto dto)
        {
            throw new NotImplementedException();
            //try
            //{
            //    string app = GetDirApp();
            //    IEnumerable<DiskElement> list = await controller.GetListAsync(app);
            //    List<ProjectDto> projects = new List<ProjectDto>();

            //    if (list.Any(x => x.DisplayName == PROGECTS_FILE && !x.IsDirectory))
            //    {
            //        //string fileName = Path.Combine(TempDir, PROGECTS_FILE);
            //        projects = await DownloadProjects();
            //        var find = projects.Find(x => x.ID.Equals(dto.ID));
            //        if (find != null)
            //        {
            //            projects.Remove(find);
            //            await UnloadProjects(projects);
            //        }
            //    }

            //    if (list.Any(x => x.IsDirectory && x.DisplayName == dto.Title))
            //        await controller.DeleteAsync(YandexHelper.DirectoryName(app, dto.Title));
            //    return true;
            //}
            //catch (WebException)
            //{ }
            //return false;
        }

        /// <summary>
        /// Обновляет название проекта в папке
        /// Обновляет название директории
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        public async Task<bool> UpdateProject(ProjectDto dto)
        {
            throw new NotImplementedException();
            //try
            //{
            //    string app = GetDirApp();
            //    IEnumerable<DiskElement> list = await controller.GetListAsync(app);
            //    List<ProjectDto> projects = new List<ProjectDto>();

            //    if (list.Any(x => x.DisplayName == PROGECTS_FILE && !x.IsDirectory))
            //    {
            //        //string fileName = Path.Combine(TempDir, PROGECTS_FILE);
            //        projects = await DownloadProjects();
            //        var find = projects.Find(x => x.ID.Equals(dto.ID));
            //        if (find != null)
            //        {

            //            bool res = await controller.MoveAsync(
            //                originPath: YandexHelper.DirectoryName(app, find.Title),
            //                movePath: YandexHelper.DirectoryName(app, dto.Title));

            //            find.Title = dto.Title;
            //            await UnloadProjects(projects);
            //        }
            //    }

            //    return true;
            //}
            //catch (WebException)
            //{ }
            //return false;
        }
        #endregion


    }
}
