using Brio.Docs.Database;
using Brio.Docs.Database.Models;
using Brio.Docs.Synchronization.Interfaces;
using Brio.Docs.Synchronization.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Brio.Docs.Synchronization.Utils.Linkers
{
    internal class DynamicFieldDynamicFieldLinker : ILinker<DynamicField>
    {
        private readonly ILogger<DynamicFieldDynamicFieldLinker> logger;

        public DynamicFieldDynamicFieldLinker(ILogger<DynamicFieldDynamicFieldLinker> logger)
        {
            this.logger = logger;
            logger.LogTrace("DynamicFieldDynamicFieldLinker created");
        }

        public Task Link(DMContext context, DynamicField field, object parent, EntityType entityType)
        {
            logger.LogTrace(
                "Link started with field: {@Field}, parent: {@Parent}, entityType: {@EntityType}",
                field,
                parent,
                entityType);
            var p = LinkingUtils.CheckAndUpdateLinking<DynamicField>(parent, entityType);
            p.ChildrenDynamicFields ??= new List<DynamicField>();
            p.ChildrenDynamicFields.Add(field);
            return Task.CompletedTask;
        }

        public Task Unlink(DMContext context, DynamicField field, object parent, EntityType entityType)
        {
            logger.LogTrace(
                "Unlink started with field: {@Field}, parent: {@Parent}, entityType: {@EntityType}",
                field,
                parent,
                entityType);
            var p =  LinkingUtils.CheckAndUpdateLinking<DynamicField>(parent, entityType);
            field.ParentField = null;

            if (entityType == EntityType.Remote)
                p.ChildrenDynamicFields.Remove(field);
            else if (field.ParentFieldID == null)
                context.DynamicFields.Remove(field);
            else
                context.DynamicFields.Update(field);
            return Task.CompletedTask;
        }

        public Task Update(DMContext context, DynamicField field, object parent, EntityType entityType)
        {
            logger.LogTrace(
                "Update started with field: {@Field}, parent: {@Parent}, entityType: {@EntityType}",
                field,
                parent,
                entityType);
            LinkingUtils.CheckAndUpdateLinking<DynamicField>(parent, entityType);
            if (entityType != EntityType.Remote)
                context.DynamicFields.Update(field);
            return Task.CompletedTask;
        }
    }
}
