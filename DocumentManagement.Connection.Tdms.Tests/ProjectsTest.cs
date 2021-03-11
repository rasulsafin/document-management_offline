using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MRS.DocumentManagement.Interface.Dtos;

namespace MRS.DocumentManagement.Connection.Tdms.Tests
{
    [TestClass]
    public class ProjectsTest
    {
        private static readonly string TEST_FILE_PATH = System.IO.Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "Resources", "IntegrationTestFile.txt");
        // TODO: Find project and use its guid to test?
        private static readonly string PROJECT_GUID = "{3033B554-5652-482A-85F1-4915D04250AD}";

        private static TdmsProjectsSynchronizer projectService;
        private static ItemExternalDto item;

        [ClassInitialize]
        public static void Initialize(TestContext unused)
        {
            projectService = new TdmsProjectsSynchronizer();

            item = new ItemExternalDto()
            {
                FileName = System.IO.Path.GetFileName(TEST_FILE_PATH),
                FullPath = TEST_FILE_PATH,
                ItemType = ItemType.File,
            };
        }

        [TestMethod]
        public void GetProject_ExistingProjectID_ReturnsProjectExternalDto()
        {
            var res = projectService.Get(PROJECT_GUID);
            Assert.IsNotNull(res);
            Assert.IsTrue(res.Items.Count > 0);
        }

        [TestMethod]
        public async Task UpdateProject_ExistingProjectID_ReturnsProjectExternalDto()
        {
            var project = projectService.Get(PROJECT_GUID);
            var oldCont = project.Items?.Count;

            project.Items.Add(item);

            var updatedProject = await projectService.Update(project);
            var newCount = updatedProject.Items?.Count;

            Assert.IsNotNull(updatedProject);
            Assert.IsTrue(newCount > oldCont);
            Assert.AreEqual(oldCont + 1, newCount);

            // Remove added item
            var itemToRemove = updatedProject.Items.FirstOrDefault(x => x.FileName == item.FileName);
            updatedProject.Items.Remove(itemToRemove);
            project = await projectService.Update(updatedProject);

            Assert.AreEqual(oldCont, project.Items.Count);
        }

        [TestMethod]
        public void GetListOfProject_ReturnsListOfProjectDto()
        {
            var list = projectService.GetListOfProjects();
            Assert.IsNotNull(list);
            Assert.IsTrue(list.Count > 0);
        }
    }
}
