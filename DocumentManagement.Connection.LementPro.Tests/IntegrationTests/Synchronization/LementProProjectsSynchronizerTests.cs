using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MRS.DocumentManagement.Connection.LementPro.Synchronization;
using MRS.DocumentManagement.Interface.Dtos;

namespace MRS.DocumentManagement.Connection.LementPro.Tests.IntegrationTests.Synchronization
{
    [TestClass]
    public class LementProProjectsSynchronizerTests
    {
        private static LementProProjectsSynchronizer synchronizer;

        [ClassInitialize]
        public static async Task ClassInitialize(TestContext unused)
        {
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

            var context = await LementProConnectionContext.CreateContext(connectionInfo);
            synchronizer = new LementProProjectsSynchronizer(context);
        }

        [TestMethod]
        public async Task GetUpdatedIDs_AtLeastOneProjectExists_RetrivedSuccessful()
        {
            var result = await synchronizer.GetUpdatedIDs(DateTime.Now);

            Assert.IsTrue(result.Any());
        }

        [TestMethod]
        public async Task Add_ObjectiveWithEmptyId_AddedSuccessfully()
        {
            var creationDateTime = DateTime.Now;
            var project = new ProjectExternalDto
            {
                Title = $"CreatedBySyncTest {creationDateTime.ToShortTimeString()}",
                UpdatedAt = creationDateTime,
            };

            var result = await synchronizer.Add(project);

            Assert.IsNotNull(result?.ExternalID);
        }
    }
}
