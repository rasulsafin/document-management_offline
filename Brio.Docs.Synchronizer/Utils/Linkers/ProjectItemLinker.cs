using Brio.Docs.Database;
using Brio.Docs.Database.Models;
using Brio.Docs.Synchronization.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Brio.Docs.Synchronization.Utils.Linkers
{
    internal class ProjectItemLinker : AItemLinker
    {
        private readonly ILogger<ProjectItemLinker> logger;

        public ProjectItemLinker(ILogger<ProjectItemLinker> logger)
        {
            this.logger = logger;
            logger.LogTrace("ProjectItemLinker created");
        }

        public override Task Link(DMContext context, Item item, object parent, EntityType entityType)
        {
            logger.LogTrace(
                "Link started with item: {@Item}, parent: {@Parent}, entityType: {@EntityType}",
                item,
                parent,
                entityType);
            var project = LinkingUtils.CheckAndUpdateLinking<Project>(parent, entityType);
            project.Items ??= new List<Item>();
            project.Items.Add(item);
            return Task.CompletedTask;
        }

        public override Task Unlink(DMContext context, Item item, object parent, EntityType entityType)
        {
            logger.LogTrace(
                "Unlink started with item: {@Item}, parent: {@Parent}, entityType: {@EntityType}",
                item,
                parent,
                entityType);
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
