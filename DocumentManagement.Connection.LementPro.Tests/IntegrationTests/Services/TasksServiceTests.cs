using Brio.Docs.Connections.LementPro.Models;
using Brio.Docs.Connections.LementPro.Services;
using Brio.Docs.Client.Dtos;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Brio.Docs.Connections.LementPro.Tests.IntegrationTests.Services
{
    [TestClass]
    public class TasksServiceTests
    {
        private static TasksService service;
        private static ServiceProvider serviceProvider;

        [ClassInitialize]
        public static async Task Init(TestContext unused)
        {
            var services = new ServiceCollection();
            services.AddLementPro();
            services.AddLogging(x => x.SetMinimumLevel(LogLevel.None));
            serviceProvider = services.BuildServiceProvider();
            service = serviceProvider.GetService<TasksService>();
            var connection = serviceProvider.GetService<LementProConnection>();

            var login = "diismagilov";
            var password = "DYZDFMwZ";
            var connectionInfo = new ConnectionInfoExternalDto
            {
                AuthFieldValues = new Dictionary<string, string>
                {
                    { "login", login },
                    { "password", password },
                },
            };

            await connection!.Connect(connectionInfo, CancellationToken.None);
        }

        [ClassCleanup]
        public static void ClassCleanup()
            => serviceProvider.Dispose();

        [TestMethod]
        public async Task RetriveTasksList_RequestDefaultFolderTasksListWithCorrectCredentials_ReturnsTasksList()
        {
            var result = await service.GetAllTasksAsync();

            Assert.IsTrue(result != null);
            Assert.IsTrue(result.Any());
        }

        [TestMethod]
        public async Task GetTaskAsync_ExistingTaskWithCorrectCredentials_ReturnsTask()
        {
            var taskId = 402015;

            var result = await service.GetTaskAsync(taskId);

            Assert.IsNotNull(result);
        }

        [TestMethod]
        public async Task GetTasksTypesAsync_TasksHaveDifferentTypes_ReturnsTaskTypeList()
        {
            var result = await service.GetTasksTypesAsync();

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Any());
        }

        [TestMethod]
        public async Task CreateTaskAsync_NewTaskWithCorrectFields_ReturnsTrue()
        {
            var dateFormat = "yyyy - MM - ddThh: mm:ss.FFFZ";
            var newTaskValue = new ObjectBaseValueToCreate
            {
                Type = 40179,
                Name = $"Задача от {DateTime.Now}",
                Description = $"Описание новой задачи {Guid.NewGuid()}",
                ////I60099 = "44212",
                StartDate = DateTime.Now.ToString(dateFormat),
            };

            var newTask = new ObjectBaseToCreate
            {
                CanAutoEditParents = false,
                Values = newTaskValue,
                FileIds = new List<int>(),
            };

            var result = await service.CreateTaskAsync(newTask);

            Assert.IsTrue(result.IsSuccess.GetValueOrDefault());
        }

        [TestMethod]
        public async Task CreateTaskAsync_NewTaskWithUploadedFile_ReturnsTrue()
        {
            var dateFormat = "yyyy - MM - ddThh: mm:ss.FFFZ";
            var newTaskValue = new ObjectBaseValueToCreate
            {
                Type = 40179,
                Name = $"Задача от {DateTime.Now}",
                Description = $"Описание новой задачи {Guid.NewGuid()}",
                I60099 = "44212",
                StartDate = DateTime.Now.ToString(dateFormat),
            };

            var filePath1 = "C:\\Users\\diismagilov\\Downloads\\red-circle.png";
            var name1 = Path.GetFileName(filePath1);
            var uploaded1 = await service.CommonRequests.AddFileAsync(name1, filePath1);
            var filePath2 = "C:\\Users\\diismagilov\\Downloads\\server.png";
            var name2 = Path.GetFileName(filePath2);
            var uploaded2 = await service.CommonRequests.AddFileAsync(name2, filePath2);
            var newTask = new ObjectBaseToCreate
            {
                CanAutoEditParents = false,
                Values = newTaskValue,
                FileIds = new List<int> { uploaded1.ID.Value, uploaded2.ID.Value },
            };

            var result = await service.CreateTaskAsync(newTask);

            Assert.IsTrue(result.IsSuccess.GetValueOrDefault());
        }

        [TestMethod]
        public async Task DeleteTaskAsync_CreatedNewTask_ReturnsTrue()
        {
            var dateFormat = "yyyy - MM - ddThh: mm:ss.FFFZ";
            var newTaskValue = new ObjectBaseValueToCreate
            {
                Type = 40179,
                IsResolution = false,
                Name = $"Задача от {DateTime.Now}",
                Description = $"Описание новой задачи {Guid.NewGuid()}",
                Project = "402014",
                IsExpired = false,
                I60099 = "44212",
                StartDate = DateTime.Now.ToString(dateFormat),
                BimRef = "402297",
            };
            var newTask = new ObjectBaseToCreate
            {
                CanAutoEditParents = false,
                Values = newTaskValue,
                FileIds = new List<int>(),
            };
            var created = await service.CreateTaskAsync(newTask);

            // Wait for creating (2 sec is enough usually)
            await Task.Delay(3000);

            var result = await service.DeleteTaskAsync(created.ID.Value);

            Assert.IsNotNull(result);
        }

        [TestMethod]
        public async Task GetTaskAsync_DeletedTask_ReturnsTrue()
        {
            var dateFormat = "yyyy - MM - ddThh: mm:ss.FFFZ";
            var newTaskValue = new ObjectBaseValueToCreate
            {
                Type = 40179,
                IsResolution = false,
                Name = $"Задача от {DateTime.Now}",
                Description = $"Описание новой задачи {Guid.NewGuid()}",
                Project = "402014",
                IsExpired = false,
                I60099 = "44212",
                StartDate = DateTime.Now.ToString(dateFormat),
                BimRef = "402297",
            };
            var newTask = new ObjectBaseToCreate
            {
                CanAutoEditParents = false,
                Values = newTaskValue,
                FileIds = new List<int>(),
            };
            var created = await service.CreateTaskAsync(newTask);

            // Wait for creating (2 sec is enough usually)
            await Task.Delay(3000);
            await service.DeleteTaskAsync(created.ID.Value);

            var result = await service.GetTaskAsync(created.ID.Value);

            Assert.IsNotNull(result);
        }

        [TestMethod]
        public async Task UpdateTaskAsync_ExistingTask_ReturnsTrue()
        {
            var id = 402015;
            var userId = 44212;
            var existingTask = await service.GetTaskAsync(id);

            var updatedTaskValue = new ObjectBaseValueToUpdate
            {
                BimRef = existingTask.Values.BimRef.ID,
                Type = existingTask.Values.Type.ID,
                Name = $"Нов22Имя Задача от {DateTime.Now}",
                Project = existingTask.Values.Project.ID,
                I60099 = userId,
                StartDate = "2020-05-10T12:55:37.000Z",
            };
            var taskToUpdate = new ObjectBaseToUpdate
            {
                ID = id,
                Values = updatedTaskValue,
            };

            var result = await service.UpdateTaskAsync(taskToUpdate);

            Assert.IsNotNull(result);
            Assert.AreEqual(id, result.ID);
        }
    }
}
