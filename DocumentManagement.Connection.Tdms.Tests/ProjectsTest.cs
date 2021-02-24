using Microsoft.VisualStudio.TestTools.UnitTesting;
using MRS.DocumentManagement.Interface.Dtos;

namespace MRS.DocumentManagement.Connection.Tdms.Tests
{
    [TestClass]
    public class ProjectsTest
    {
        private static ProjectService projectService;

        private static ProjectDto project;
        private static ProjectDto projectToDelete;

        [ClassInitialize]
        public static void Initialize(TestContext unused)
        {
            projectService = new ProjectService();
            project = new ProjectDto()
            {
                ID = new ID<ProjectDto>(1),
                Title = "TestProject",
            };

            projectToDelete = new ProjectDto()
            {
                ID = new ID<ProjectDto>(2),
                Title = "TestProject2",
            };
        }

        [TestMethod]
        public void GetProject_ExistingProjectID_ReturnsProjectDto()
        {
            var res = projectService.Get("{3033B554-5652-482A-85F1-4915D04250AD}");
            Assert.IsNotNull(res);
            Assert.IsNotNull(res.Items);
        }

        [TestMethod]
        public void AddProject_NonExistingProject_ReturnsProjectDto()
        {
        }

        [TestMethod]
        public void UpdateProject_ExistingProject_ReturnsProjectDto()
        {

        }

        [TestMethod]
        public void RemoveProject_ExistingProject_ReturnsTrue()
        {
        }

        [TestMethod]
        public void GetListOfProject_ReturnsListOfProjectDto()
        {

        }
    }
}
