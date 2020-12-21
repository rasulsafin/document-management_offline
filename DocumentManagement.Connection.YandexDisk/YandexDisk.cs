using MRS.DocumentManagement.Interface.Dtos;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace DocumentManagement.Connection.YandexDisk
{
    public class YandexDisk
    {
        public static CoolLogger logger = new CoolLogger("YandexDisk");

        private static readonly string PROGECTS_FILE = "Projects.xml";
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
                await controller.CreateDirAsync("/",APP_DIR);
            }
        }

        public string TempDir { get; set; }


        public async Task AddProject(ProjectDto project)
        {
            // Получаем список файлов и папок в папке приложения 
            string app = $"/{APP_DIR}/";
            IEnumerable<DiskElement> list = await controller.GetListAsync(app);
            //
            // TODO : Остановился здесь 21.12.2020
            //

        }

        /// <summary>
        /// Вызывается при бездумном копировании (например при переносе данных) 
        /// </summary>
        /// <param name="collectionProject"></param>
        /// <returns></returns>
        public async Task UnloadProjects(List<ProjectDto> collectionProject)
        {
            string app = $"/{APP_DIR}/";

            if (!Directory.Exists(TempDir)) Directory.CreateDirectory(TempDir);
            string fileName = Path.Combine(TempDir, PROGECTS_FILE);
            XmlSerializer formatter = new XmlSerializer(typeof(List<ProjectDto>));
            using (FileStream fs = new FileStream(fileName, FileMode.Create))
            {
                formatter.Serialize(fs, collectionProject);
            }
            //await controller.DeleteAsync(YandexHelper.NewFile(app, PROGECTS_FILE));
            await controller.LoadFileAsync(app, fileName);

            IEnumerable<DiskElement> list = await controller.GetListAsync(app);

            foreach (var project in collectionProject)
            {
                if (!list.Any(x=>x.IsDirectory && x.DisplayName == project.Title))
                    await controller.CreateDirAsync(app, project.Title);
            }
        }
    }
}
