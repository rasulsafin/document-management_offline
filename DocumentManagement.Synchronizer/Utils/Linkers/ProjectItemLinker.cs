using System.Collections.Generic;
using System.Threading.Tasks;
using MRS.DocumentManagement.Database;
using MRS.DocumentManagement.Database.Models;
using MRS.DocumentManagement.Synchronization.Models;

namespace MRS.DocumentManagement.Synchronization.Utils.Linkers
{
    internal class ProjectItemLinker : AItemLinker
    {
        public override Task Link(DMContext context, Item item, object parent, EntityType entityType)
        {
            var project = LinkingUtils.CheckAndUpdateLinking<Project>(parent, entityType);
            project.Items ??= new List<Item>();
            project.Items.Add(item);
            return Task.CompletedTask;
        }

        public override Task Unlink(DMContext context, Item item, object parent, EntityType entityType)
        {
            var project = LinkingUtils.CheckAndUpdateLinking<Project>(parent, entityType);
            item.ProjectID = null;

            if (entityType == EntityType.Remote)
                project.Items.Remove(item);
            else if (item.Objectives?.Count == 0)
                context.Items.Remove(item);
            else
                context.Items.Update(item);
            return Task.CompletedTask;
        }
    }
}
