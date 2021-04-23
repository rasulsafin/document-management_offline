using System;
using System.Threading.Tasks;
using MRS.DocumentManagement.Database;
using MRS.DocumentManagement.Database.Models;
using MRS.DocumentManagement.Synchronization.Interfaces;
using MRS.DocumentManagement.Synchronization.Models;

namespace MRS.DocumentManagement.Synchronization.Utils.Linkers
{
    internal abstract class AItemLinker : ILinker<Item>
    {
        public abstract Task Link(DMContext context, Item item, object parent, EntityType entityType);

        public abstract Task Unlink(DMContext context, Item item, object parent, EntityType entityType);

        public Task Update(DMContext context, Item item, object parent, EntityType entityType)
            => throw new NotSupportedException();
    }
}
