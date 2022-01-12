using System.Collections.Generic;
using System.Threading.Tasks;
using Brio.Docs.Database;
using Brio.Docs.Database.Extensions;
using Brio.Docs.Synchronization.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Brio.Docs.Tests.Synchronization.Helpers
{
    internal class AssertHelper
    {
        private readonly DMContext context;

        public AssertHelper(DMContext context)
            => this.context = context;

        public async ValueTask IsLocalItemsCount(int count)
            => Assert.AreEqual(
                count,
                await context.Items.Unsynchronized().CountAsync(),
                $"The number of local items is not equal to {count}");

        public void IsSynchronizationSuccessful(ICollection<SynchronizingResult> synchronizingResult)
            => Assert.AreEqual(
                0,
                synchronizingResult.Count,
                $"The synchronization failed with {synchronizingResult.Count} exceptions");
    }
}
