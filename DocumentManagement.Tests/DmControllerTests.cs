using DocumentManagement.Data;
using DocumentManagement.Models;
using DocumentManagement.Models.Database;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using MRS.Bim.DocumentManagement;
using System;
using System.Collections.Generic;

namespace DocumentManagement.Tests
{
    [TestClass]
    public class DmControllerTests
    {
        private static DocumentManagementContext context = new DocumentManagementContext();
        private static DmController controller = new DmController(
            new AuthRepository(context),
            new ProjectRepository(context),
            new TaskRepository(context),
            new TaskTypeRepository(context),
            new ItemRepository(context));

        private static User user = new User()
        {
            Login = "Anton",
            Password = "Password"
        };

        [TestMethod]
        public void A001_Register_NewUser_ReturnsTrue()
        {         
            var result = controller.Register(user).Result;

            Assert.IsTrue(result);
        }

        [TestMethod]
        public void A002_Register_ExistingUser_ReturnsFalse()
        {        
            var result = controller.Register(user).Result;

            Assert.IsFalse(result);
        }

        [TestMethod]
        public void A003_Login_NewUser_ReturnsFalse()
        {
            User user = new User()
            {
                Login = "User2",
                Password = "Password2"
            };

            var result = controller.Login(user).Result;

            Assert.IsFalse(result);
        }

        [TestMethod]
        public void A004_Login_ExistingUser_ReturnsTrue()
        { 
            var result = controller.Login(user).Result;

            Assert.IsTrue(result);
        }

        [TestMethod]
        public void A005_DeleteUser_ExistingUser_ReturnsTrue()
        {
            var result = controller.DeleteUser(user).Result;

            Assert.IsTrue(result);
        }

        [TestMethod]
        public void B001_AddProject_NewProject_ReturnsTrue()
        {
            Project project = new Project()
            { 
                Title = "Project2"
            };

           var result = controller.AddProject(project, user.Login).Result;

            Assert.IsTrue(result);
        }

        [TestMethod]
        public void B002_GetProjects_ExistingUser_ReturnsOne()
        {       
            var result = controller.GetProjects(user.Login).Result;

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Count == 2);
        }

        [TestMethod]
        public void B003_UpdateProject_ExistingProject_ReturnsTrue()
        {
            var project = controller.GetProjects("User").Result[0];

            project.Items = new List<Item>() { new Item() { Type = TypeItemDm.Media, Path = @"c:\path\test.ifc" } };

            var result = controller.UpdateProject(project).Result;

            Assert.IsTrue(result);
        }


        [TestMethod]
        public void B004_DeleteProject_ExistingProject_ReturnsTrue()
        {
            var result = controller.DeleteProject(1).Result;

            Assert.IsTrue(result);
        }

        [TestMethod]
        public void C001_AddTaskDm_NewTask_ReturnsTrue()
        {
            var userTask = controller.GetUser("User").Result;

            TaskDm task = new TaskDm()
            {
                Author = userTask,
                Title = "Task from test",
                Date = DateTime.Now,
                Descriptions = "Text",
                Status = Status.InProgress,
                Type = new TaskType() { Id = 4, Name = "job" }
            };

            var result = controller.AddTask(task, 4).Result;

            Assert.IsTrue(result);
        }
        [TestMethod]
        public void C002_DeleteTaskDm_ExistingTask_ReturnsTrue()
        {
            var result = controller.DeleteTask(9).Result;

            Assert.IsTrue(result);
        }

        [TestMethod]
        public void C003_GetTasks_ByProjectId_ReturnsListOfTasks()
        {
            var result = controller.GetTasks(4).Result;

            Assert.IsTrue(result.Count == 3);
        }

        [TestMethod]
        public void C004_GetTasks_ByProjectIdAndType_ReturnsListOfTasks()
        {
            var type = controller.GetTypes().Result;
            var result = controller.GetTasks(4, type[1]).Result;

            Assert.IsTrue(result.Count == 1);
        }

        [TestMethod]
        public void C005_UpdateTask_AddExistingFileToExistingTask_ReturnsTrue()
        {
            var userTask = controller.GetUser("User").Result;

            TaskDm addtask = new TaskDm()
            {
                Author = userTask,
                Title = "additional task2",
                Date = DateTime.Now,
                Descriptions = "Text",
                Status = Status.Open,
                Type = new TaskType() { Id = 1, Name = "issue" }
            };

            var task = controller.GetTask(7).Result;
            task.Descriptions = "trying to update";
            task.Tasks = new List<TaskDm>() { addtask };
            task.Items = new List<Item>() { new Item() { Type = TypeItemDm.Media, Path = @"c:\path\item.png" } };

            var result = controller.UpdateTask(task).Result;

            Assert.IsTrue(result);
        }

        [TestMethod]
        public void C006_DeleteItemFromTask_ExistingItem_ReturnsTrue()
        {
            var task = controller.GetTask(7).Result;
            var item = task.Items[0];
            var result = controller.DeleteItemFromTask(item, task).Result;

            Assert.IsTrue(result);
        }

        [TestMethod]
        public void D001_AddType_NewType_ReturnsTrue()
        {
            TaskType type = new TaskType()
            {
                Name = "job"
            };
            var result = controller.AddType(type).Result;

            Assert.IsTrue(result);
        }
        [TestMethod]
        public void D002_DeleteItem_ExistingItem_ReturnsTrue()
        {            
            var result = controller.DeleteItem(@"c:\path\item.png").Result;

            Assert.IsTrue(result);
        }


    }
}
