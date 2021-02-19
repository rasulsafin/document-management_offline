using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MRS.DocumentManagement.Connection.LementPro.Models;
using MRS.DocumentManagement.Connection.LementPro.Services;
using MRS.DocumentManagement.Connection.LementPro.Utilities;
using MRS.DocumentManagement.Interface.Dtos;

namespace MRS.DocumentManagement.Connection.LementPro.Tests.IntegrationTests.Services
{
    [TestClass]
    public class TasksServiceTests
    {
        private static TasksService service;

        [ClassInitialize]
        public static async Task Init(TestContext unused)
        {
            var requestUtility = new HttpRequestUtility(new NetConnector());
            var authService = new AuthenticationService(requestUtility);
            var commonRequests = new CommonRequestsUtility(requestUtility);
            service = new TasksService(authService, requestUtility, commonRequests);

            var login = "diismagilov";
            var password = "DYZDFMwZ";
            var connectionInfo = new ConnectionInfoDto
            {
                AuthFieldValues = new Dictionary<string, string>
                {
                    { "login", login },
                    { "password", password },
                },
            };

            var (_, _) = await authService.SignInAsync(connectionInfo);
        }

        [ClassCleanup]
        public static void ClassCleanup()
            => service.Dispose();

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
            //var defaultType = (await service.GetTasksTypesAsync()).First();
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
                FileIds = new List<string>(),
            };

            var result = await service.CreateTask(newTask);

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
                FileIds = new List<string>(),
            };
            var created = await service.CreateTask(newTask);

            // Wait for creating (2 sec is enough usually)
            await Task.Delay(3000);

            var result = await service.DeleteTaskAsync(created.ID.Value);

            Assert.IsTrue(result);
        }

        [TestMethod]
        public async Task UpdateTaskAsync_CreatedNewTask_ReturnsTrue()
        {
            var id = 402015;
            var existingTask = await service.GetTaskAsync(id);

            var dateFormat = "yyyy - MM - ddThh: mm:ss.FFFZ";
            var updatedTaskValue = new TaskValueToUpdate
            {
                BimRef = 402297,
                Controllers = string.Empty,
                CreationDate = existingTask.Values.CreationDate,
                Description = $"Описание новой задачи {Guid.NewGuid()}",
                DocumentResolutionFiles = new List<object>(),
                Executors = string.Empty,
                Favorites = string.Empty,
                Type = 40179,
                IsResolution = false,
                Name = $"НовИмя Задача от {DateTime.Now}",
                Project = 402014,
                IsExpired = false,
                I60099 = 44212,
                LastModifiedDate = DateTime.Now.ToString(dateFormat),
            };
            var taskToUpdate = new TaskToUpdate
            {
                CanAutoEditParents = false,
                ID = id,
                Values = updatedTaskValue,
                AddedFileIds = new List<string>(),
                RemovedFileIds = new List<string>(),
            };

            var result = await service.UpdateTaskAsync(taskToUpdate);

            Assert.IsNotNull(result);
            Assert.AreEqual(id, result.ID);
        }
    }
}
