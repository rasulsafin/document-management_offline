using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MRS.DocumentManagement;
using MRS.DocumentManagement.Connection.Tdms;
using MRS.DocumentManagement.Interface.Dtos;

namespace DocumentManagement.Connection.Tdms.Test
{
    [TestClass]
    public class ProjectsTest
    {
        private static TdmsConnection connection;
        private static ProjectService projectService;

        private static ProjectDto project;

        [ClassInitialize]
        public static void Initialize(TestContext unused)
        {
            connection = new TdmsConnection();
            projectService = new ProjectService();

            var connectionInfo = new ConnectionInfoDto
            {
                ID = new ID<ConnectionInfoDto>(1),
                ConnectionType = connection.GetConnectionType(),
                AuthFieldValues = new Dictionary<string, string>()
                {
                    {Auth.LOGIN, "gureva" },
                    {Auth.PASSWORD, "123"},
                    {Auth.DATABASE, "kosmos" },
                    {Auth.SERVER, @"192.168.100.6\sqlkosmos" },
                },
            };


            project = new ProjectDto()
            {
                ID = new ID<ProjectDto>(1),
                Title = "TestProject",
            };

            // Authorize
            var signInTask = connection.Connect(connectionInfo);
            signInTask.Wait();
            if (signInTask.Result.Status != RemoteConnectionStatusDto.OK)
            {
                Assert.Fail("Authorization failed");
            }
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
            var res = projectService.Add(project);
            Assert.IsNotNull(res);
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
