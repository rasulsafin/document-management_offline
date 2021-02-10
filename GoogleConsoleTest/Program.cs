using MRS.DocumentManagement.Connection.GoogleDrive;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace GoogleConsoleTest
{
    class Program
    {
        private static GoogleDriveController controller;

        static async Task Main(string[] args)
        {
            controller = new GoogleDriveController();
            await controller.InitializationAsync();
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

                    case "q": exit = true; break;

                    default:
                        break;
                }
            }

        }

        private static async Task DeleteAsync(string[] comList)
        {
            if (comList.Length > 1)
            {
                string id = comList[1];
                var res = await controller.DeleteAsync(id);

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

                if (res)
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

                if (res)
                    Console.WriteLine($"Успех!");
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
