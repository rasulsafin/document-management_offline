using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MRS.DocumentManagement.Database;
using MRS.DocumentManagement.Database.Models;
using MRS.DocumentManagement.Synchronization.Interfaces;
using MRS.DocumentManagement.Synchronization.Models;

namespace MRS.DocumentManagement.Synchronization.Utils.Linkers
{
    internal class ObjectiveDynamicFieldLinker : ILinker<DynamicField>
    {
        private readonly ILogger<ObjectiveDynamicFieldLinker> logger;

        public ObjectiveDynamicFieldLinker(ILogger<ObjectiveDynamicFieldLinker> logger)
        {
            this.logger = logger;
            logger.LogTrace("ObjectiveDynamicFieldLinker created");
        }

        public Task Link(DMContext context, DynamicField field, object parent, EntityType entityType)
        {
            logger.LogTrace(
                "Link started with field: {@Field}, parent: {@Parent}, entityType: {@EntityType}",
                field,
                parent,
                entityType);
            var objective = LinkingUtils.CheckAndUpdateLinking<Objective>(parent, entityType);
            objective.DynamicFields ??= new List<DynamicField>();
            objective.DynamicFields.Add(field);
            return Task.CompletedTask;
        }

        public Task Unlink(DMContext context, DynamicField field, object parent, EntityType entityType)
        {
            logger.LogTrace(
                "Unlink started with field: {@Field}, parent: {@Parent}, entityType: {@EntityType}",
                field,
                parent,
                entityType);
            var objective =  LinkingUtils.CheckAndUpdateLinking<Objective>(parent, entityType);
            field.Objective = null;

            if (entityType == EntityType.Remote)
                objective.DynamicFields.Remove(field);
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
            LinkingUtils.CheckAndUpdateLinking<Objective>(parent, entityType);
            if (entityType != EntityType.Remote)
                context.DynamicFields.Update(field);
            return Task.CompletedTask;
        }
    }
}
