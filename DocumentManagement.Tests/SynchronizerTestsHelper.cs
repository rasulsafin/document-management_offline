using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using MRS.DocumentManagement.Database;
using MRS.DocumentManagement.Database.Models;
using MRS.DocumentManagement.Interface;
using MRS.DocumentManagement.Interface.Dtos;
using MRS.DocumentManagement.Tests.Utility;

namespace MRS.DocumentManagement.Tests
{
    internal class SynchronizerTestsHelper
    {
        public static void CheckSynchronizedItems(Item local, Item synchronized)
        {
            Assert.AreEqual(local.RelativePath, synchronized.RelativePath);
            Assert.AreEqual(local.Project?.SynchronizationMateID ?? 0, synchronized.Project?.ID ?? 0);
            Assert.AreEqual(local.SynchronizationMateID, synchronized.ID);
            Assert.IsFalse(local.IsSynchronized);
            Assert.IsTrue(synchronized.IsSynchronized);
            Assert.AreEqual(local.ItemType, synchronized.ItemType);
            Assert.AreEqual(local.ExternalID, synchronized.ExternalID);
        }

        public static void CheckIDs(ISynchronizableBase a, ISynchronizableBase b)
        {
            Assert.AreEqual(a.SynchronizationMateID, b.SynchronizationMateID);
            Assert.AreEqual(a.IsSynchronized, b.IsSynchronized);
        }

        public static void CheckSynchronized(ISynchronizableBase local, ISynchronizableBase synchronized)
        {
            Assert.AreEqual(local.SynchronizationMateID, synchronized.ID);
            Assert.IsFalse(local.IsSynchronized);
            Assert.IsTrue(synchronized.IsSynchronized);
        }

        public static async Task<(Project local, Project synchronized, ProjectExternalDto remote)> ArrangeProject(Mock<IConnectionContext> context, SharedDatabaseFixture fixture)
        {
            // Arrange.
            var projectLocal = MockData.DEFAULT_PROJECTS[0];
            var projectSynchronized = MockData.DEFAULT_PROJECTS[0];
            var projectRemote = new ProjectExternalDto
            {
                ExternalID = "external_id",
                Title = projectLocal.Title,
                UpdatedAt = DateTime.Now,
            };
            projectLocal.ExternalID = projectSynchronized.ExternalID = projectRemote.ExternalID;
            context.Setup(x => x.Projects).ReturnsAsync(new[] { projectRemote });
            projectSynchronized.IsSynchronized = true;
            projectLocal.SynchronizationMate = projectSynchronized;
            await fixture.Context.Projects.AddRangeAsync(projectSynchronized, projectLocal);
            await fixture.Context.SaveChangesAsync();
            return (projectLocal, projectSynchronized, projectRemote);
        }
    }
}
