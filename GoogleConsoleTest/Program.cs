using MRS.DocumentManagement;
using MRS.DocumentManagement.Connection.GoogleDrive;
using MRS.DocumentManagement.Interface.Dtos;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace GoogleConsoleTest
{
    class Program
    {
        private static GoogleDriveController controller;
        private static GoogleDriveManager manager;

        static async Task Main(string[] args)
        {
            controller = new GoogleDriveController();
            await controller.InitializationAsync();

            manager = new GoogleDriveManager(controller);
            Console.WriteLine("Авторизация прошла успешно!");

            string command = string.Empty;
            bool exit = false;
            while (true)
            {
                if (exit) break;
                Console.Write("Введите команду:");
                command = Console.ReadLine();
                var comList = command.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (comList.Length <= 0) continue;

                switch (comList[0].ToLower())
                {
                    case "list":
                        await GetListAsync(comList);
                        break;

                    case "info":
                        await GetInfoAsync(comList);
                        break;

                    case "create":
                        await CreateAsync(comList);
                        break;

                    case "dwnl":
                        await DownloadAsync(comList);
                        break;

                    case "load":
                        await UnloadAsync(comList);
                        break;

                    case "del":
                        await DeleteAsync(comList);
                        break;

                    case "set":
                        await SetContentAsync(comList, command);
                        break;

                    case "get":
                        await GetContentAsync(comList);
                        break;

                    case "push":
                        await PushAsync(comList);
                        break;

                    case "pull":
                        await PullAsync(comList);
                        break;

                    case "q": exit = true; break;

                    default:
                        break;
                }
            }

        }

        private static async Task PullAsync(string[] comList)
        {
            if (comList.Length >= 3)
            {
                string id = comList[2];
                switch (comList[1].ToLower())
                {
                    case "p":
                    case "project":
                        await PullProject(id);
                        break;

                    case "u":
                    case "user":
                        await PullUser(id);
                        break;

                    case "obj":
                    case "objective":
                        await PullObjective(id);
                        break;
                }
            }
        }

        private static async Task PullObjective(string id)
        {
            var obj = await manager.Pull<ObjectiveDto>(id);
            if (obj != null)
            {
                Console.WriteLine($"Успех! id={obj.ID} Title={obj.Title} ProjectID={obj.ProjectID}");
            }
        }

        private static async Task PullUser(string id)
        {
            var user = await manager.Pull<UserDto>(id);
            if (user != null)
            {
                Console.WriteLine($"Успех! id={user.ID} Login={user.Login} Name={user.Name}");
            }
        }

        private static async Task PullProject(string id)
        {
            var progect = await manager.Pull<ProjectDto>(id);
            if (progect != null)
            {
                Console.WriteLine($"Успех! id={progect.ID} Title={progect.Title}");
            }
        }

        private static async Task PushAsync(string[] comList)
        {
            string type = "user";
            if (comList.Length >= 1)
            {
                type = comList[1];

                switch (type.ToLower())
                {
                    case "project":
                        await PushProject(comList);
                        break;

                    case "user":
                        await PushUser(comList);
                        break;

                    case "obj":
                    case "objective":
                        await PushObjective(comList);
                        break;
                }
            }
        }

        private static async Task PushObjective(string[] comList)
        {
            var obj = new ObjectiveDto();
            int id = 123;
            if (comList.Length >= 2 && int.TryParse(comList[2], out int num)) { id = num; }

            obj.ID = new ID<ObjectiveDto>(id);
            obj.ObjectiveTypeID = new ID<ObjectiveTypeDto>(18);
            obj.ProjectID = new ID<ProjectDto>(1);
            obj.Title = "Жесть отваливается";

            await manager.Push(obj, obj.ID.ToString());
        }

        private static async Task PushUser(string[] comList)
        {
            int num = 20;
            string login = "qwert";
            string name = "Агапит";
            if (comList.Length >= 2 && int.TryParse(comList[2], out num))
                if (comList.Length >= 3) login = comList[3];
            if (comList.Length >= 4) name = comList[4];
            var user = new UserDto(new ID<UserDto>(num), login, name);

            await manager.Push(user, user.ID.ToString());
        }

        private static async Task PushProject(string[] comList)
        {
            ProjectDto project = new ProjectDto();
            if (comList.Length >= 2 && int.TryParse(comList[2], out int num))
                project.ID = (ID<ProjectDto>)num;
            if (comList.Length >= 3)
                project.Title = comList[3];

            await manager.Push(project, project.ID.ToString());
        }

        private static async Task GetContentAsync(string[] comList)
        {
            if (comList.Length >= 2)
            {
                string id = comList[1];
                string name = comList[2];
                var res = await controller.GetContentAsync(id, name);

                if (res != null)
                    Console.WriteLine($"Успех! {res}");
                else
                    Console.WriteLine($"Провал!");
            }
        }

        private static async Task SetContentAsync(string[] comList, string command)
        {
            if (comList.Length >= 3)
            {
                string id = comList[1];
                string name = comList[2];
                var res = await controller.SetContentAsync(command, id, name);

                if (res)
                    Console.WriteLine($"Успех!");
                else
                    Console.WriteLine($"Провал!");
            }
        }

        private static async Task DeleteAsync(string[] comList)
        {
            if (comList.Length == 2)
            {
                string id = comList[1];
                var res = await controller.DeleteAsync(id);

                if (res)
                    Console.WriteLine($"Успех!");
                else
                    Console.WriteLine($"Провал!");

            }
            if (comList.Length == 3)
            {
                string type = comList[1];
                string id = comList[2];
                var res = false;
                switch (comList[1].ToLower())
                {
                    case "p":
                    case "project":
                        res = await manager.Delete<ProjectDto>(id);
                        break;

                    case "u":
                    case "user":
                        res = await manager.Delete<UserDto>(id);
                        break;

                    case "obj":
                    case "objective":
                        res = await manager.Delete<ObjectiveDto>(id);
                        break;
                }
                if (res)
                    Console.WriteLine($"Успех!");
                else
                    Console.WriteLine($"Провал!");
            }
        }

        private static async Task UnloadAsync(string[] comList)
        {
            if (comList.Length > 2)
            {
                string id = comList[1];
                string fileName = comList[2];
                var res = await controller.LoadFileAsync(id, fileName);

                if (res == null)
                    Console.WriteLine($"Успех!");
                else
                    Console.WriteLine($"Провал!");

            }
        }

        private static async Task DownloadAsync(string[] comList)
        {
            if (comList.Length > 2)
            {
                string id = comList[1];
                string name = comList[2];
                var res = await controller.DownloadFileAsync(id, name);

                if (res)
                    Console.WriteLine($"Успех!");
                else
                    Console.WriteLine($"Провал!");

            }
        }

        private static async Task CreateAsync(string[] comList)
        {
            if (comList.Length > 2)
            {
                string name = comList[1];
                string parent = comList[2].StartsWith('-') ? string.Empty : comList[2];
                var res = await controller.CreateDirAsync(parent, name);

                if (res == null)
                    Console.WriteLine($"Успех! id = {res.Href}");
                else
                    Console.WriteLine($"Провал!");

            }
        }

        private static async Task GetInfoAsync(string[] comList)
        {
            if (comList.Length > 1)
            {
                var element = await controller.GetInfoAsync(comList[1]);
                if (element != null)
                {
                    Console.WriteLine($"DisplayName - {element.DisplayName}");
                    Console.WriteLine($"ContentType - {element.ContentType}");
                    Console.WriteLine($"ContentLength - {element.ContentLength}");
                    Console.WriteLine($"CreationDate - {element.CreationDate.ToString()}");
                    Console.WriteLine($"Href - {element.Href}");
                }
                else
                {
                    Console.WriteLine($"Файла не существует!");
                }
            }
        }

        private static async Task GetListAsync(string[] comList)
        {
            System.Collections.Generic.IEnumerable<MRS.DocumentManagement.Connection.DiskElement> elements;
            if (comList.Length > 1)
                elements = await controller.GetListAsync(comList[1]);
            else
                elements = await controller.GetListAsync();
            if (elements != null && elements.Count() > 0)
            {
                foreach (var item in elements)
                {
                    if (item.IsDirectory)
                        Console.WriteLine($"[{item.DisplayName}] - {item.Href}");
                    else
                        Console.WriteLine($"{item.DisplayName}({item.ContentLength}) - {item.Href}");
                }
            }
            else
            {
                Console.WriteLine($"Zero file");
            }
        }
    }
}
