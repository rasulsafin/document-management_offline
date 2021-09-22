using Brio.Docs.Database;
using Brio.Docs.Database.Models;
using Brio.Docs.Synchronization.Interfaces;
using Brio.Docs.Synchronization.Models;
using System;
using System.Threading.Tasks;

namespace Brio.Docs.Synchronization.Utils.Linkers
{
    internal abstract class AItemLinker : ILinker<Item>
    {
        public abstract Task Link(DMContext context, Item item, object parent, EntityType entityType);

        public abstract Task Unlink(DMContext context, Item item, object parent, EntityType entityType);

        public Task Update(DMContext context, Item item, object parent, EntityType entityType)
            => throw new NotSupportedException();
    }
}
