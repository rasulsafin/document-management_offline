using System.Collections.Generic;
using System.Threading.Tasks;
using MRS.DocumentManagement.Database;
using MRS.DocumentManagement.Database.Models;
using MRS.DocumentManagement.Synchronization.Interfaces;
using MRS.DocumentManagement.Synchronization.Models;

namespace MRS.DocumentManagement.Synchronization.Utils.Linkers
{
    internal class DynamicFieldDynamicFieldLinker : ILinker<DynamicField>
    {
        public Task Link(DMContext context, DynamicField field, object parent, EntityType entityType)
        {
            var p = LinkingUtils.CheckAndUpdateLinking<DynamicField>(parent, entityType);
            p.ChildrenDynamicFields ??= new List<DynamicField>();
            p.ChildrenDynamicFields.Add(field);
            return Task.CompletedTask;
        }

        public Task Unlink(DMContext context, DynamicField field, object parent, EntityType entityType)
        {
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
            LinkingUtils.CheckAndUpdateLinking<DynamicField>(parent, entityType);
            if (entityType != EntityType.Remote)
                context.DynamicFields.Update(field);
            return Task.CompletedTask;
        }
    }
}
