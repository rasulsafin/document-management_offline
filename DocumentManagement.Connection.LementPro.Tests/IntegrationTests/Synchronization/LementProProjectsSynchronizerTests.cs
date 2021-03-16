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
        public async Task Add_ProjectWithEmptyId_AddedSuccessfully()
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

        [TestMethod]
        public async Task Update_JustAddedProject_UpdatedSuccessfully()
        {
            var creationDateTime = DateTime.Now;
            var title = $"CreatedBySyncTest {creationDateTime.ToShortTimeString()}";

            // Add
            var project = new ProjectExternalDto
            {
                Title = title,
                UpdatedAt = creationDateTime,
            };
            var added = await synchronizer.Add(project);
            if (added?.ExternalID == null)
                Assert.Fail("Objective adding failed. There is nothing to update.");

            // Update
            await Task.Delay(3000);
            var newTitle = added.Title = $"UPDATED: {title}";
            var result = await synchronizer.Update(added);

            Assert.IsNotNull(result?.Title);
            Assert.AreEqual(newTitle, result.Title);
        }
    }
}
