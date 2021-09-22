using Brio.Docs.Database;
using Brio.Docs.Database.Models;
using Brio.Docs.Integration.Dtos;
using Brio.Docs.Integration.Interfaces;
using Brio.Docs.Tests.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Brio.Docs.Tests
{
    internal class SynchronizerTestsHelper
    {
        public enum SynchronizerCall
        {
            Nothing,
            Add,
            Update,
            Remove,
        }

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

        public static async Task<(Project local, Project synchronized, ProjectExternalDto remote)> ArrangeProject(Mock<ISynchronizer<ProjectExternalDto>> projectSynchronizer, SharedDatabaseFixture fixture)
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
            MockGetRemote(projectSynchronizer, new[] { projectRemote }, x => x.ExternalID);
            projectSynchronized.IsSynchronized = true;
            projectLocal.SynchronizationMate = projectSynchronized;
            await fixture.Context.Projects.AddRangeAsync(projectSynchronized, projectLocal);
            await fixture.Context.SaveChangesAsync();
            return (projectLocal, projectSynchronized, projectRemote);
        }

        public static void CheckSynchronizerCalls<T>(
            Mock<ISynchronizer<T>> synchronizer,
            SynchronizerCall call,
            Times times = default)
        {
            if (times == default)
                times = Times.Once();

            synchronizer.Verify(x => x.Add(It.IsAny<T>()), call == SynchronizerCall.Add ? times : Times.Never());
            synchronizer.Verify(x => x.Remove(It.IsAny<T>()), call == SynchronizerCall.Remove ? times : Times.Never());
            synchronizer.Verify(x => x.Update(It.IsAny<T>()), call == SynchronizerCall.Update ? times : Times.Never());
        }

        public static void MockGetRemote<T>(Mock<ISynchronizer<T>> synchronizer, IReadOnlyCollection<T> array, Func<T, string> getIDFunc)
        {
            synchronizer
               .Setup(x => x.GetUpdatedIDs(It.IsAny<DateTime>()))
               .ReturnsAsync(array.Select(getIDFunc).ToArray());
            synchronizer
               .Setup(x => x.Get(It.IsAny<IReadOnlyCollection<string>>()))
               .ReturnsAsync(array);
        }
    }
}
