using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MRS.DocumentManagement.Connection.LementPro.Models;
using MRS.DocumentManagement.Connection.LementPro.Services;
using MRS.DocumentManagement.Connection.LementPro.Utilities;
using MRS.DocumentManagement.Connection.Utils;
using MRS.DocumentManagement.Interface.Dtos;

namespace MRS.DocumentManagement.Connection.LementPro.Tests.IntegrationTests.Services
{
    [TestClass]
    public class TasksServiceTests : TasksService
    {
        private static TasksService service;

        [ClassInitialize]
        public static async Task Init(TestContext unused)
        {
            var requestUtility = new HttpRequestUtility(new HttpConnection());
            var authService = new AuthenticationService(requestUtility);
            var commonRequests = new CommonRequestsUtility(requestUtility);
            service = new TasksService(requestUtility, commonRequests);

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
            var dateFormat = "yyyy - MM - ddThh: mm:ss.FFFZ";
            var newTaskValue = new ObjectBaseValueToCreate
            {
                Type = 40179,
                Name = $"Задача от {DateTime.Now}",
                Description = $"Описание новой задачи {Guid.NewGuid()}",
                //I60099 = "44212",
                StartDate = DateTime.Now.ToString(dateFormat),
            };

            var newTask = new ObjectBaseToCreate
            {
                CanAutoEditParents = false,
                Values = newTaskValue,
                FileIds = new List<int>(),
            };

            var result = await service.CreateTask(newTask);

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

            var filePath = "C:\\Users\\diismagilov\\Downloads\\HelloWallIfc4.ifc";
            var name = Path.GetFileName(filePath);
            var uploaded = await service.CommonRequests.AddFileAsync(name, filePath);
            var newTask = new ObjectBaseToCreate
            {
                CanAutoEditParents = false,
                Values = newTaskValue,
                FileIds = new List<int> { uploaded.ID.Value },
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
                FileIds = new List<int>(),
            };
            var created = await service.CreateTask(newTask);

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
            var created = await service.CreateTask(newTask);

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

            var updatedTaskValue = new TaskValueToUpdate
            {
                BimRef = existingTask.Values.BimRef.ID,
                Type = existingTask.Values.Type.ID,
                Name = $"Нов22Имя Задача от {DateTime.Now}",
                Project = existingTask.Values.Project.ID,
                I60099 = userId,
                StartDate = "2020-05-10T12:55:37.000Z",
            };
            var taskToUpdate = new TaskToUpdate
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
