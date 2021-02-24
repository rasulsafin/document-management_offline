using Microsoft.VisualStudio.TestTools.UnitTesting;
using MRS.DocumentManagement.Interface.Dtos;

namespace MRS.DocumentManagement.Connection.Tdms.Tests
{
    [TestClass]
    public class ProjectsTest
    {
        private static ProjectService projectService;

        [ClassInitialize]
        public static void Initialize(TestContext unused)
        {
            projectService = new ProjectService();
        }

        [TestMethod]
        public void GetProject_ExistingProjectID_ReturnsProjectDto()
        {
            var res = projectService.Get("{3033B554-5652-482A-85F1-4915D04250AD}");
            Assert.IsNotNull(res);
            Assert.IsNotNull(res.Items);
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
