using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using MRS.DocumentManagement.Database.Models;

namespace MRS.DocumentManagement.Database.Extensions
{
    public static class UpdatingTimeExtensions
    {
        public static void UpdateDateTime(this ChangeTracker changeTracker, DateTime dateTime = default)
        {
            dateTime = dateTime == default ? DateTime.UtcNow : dateTime;
            var hasChanges = true;
            var changed = Enumerable.Empty<EntityEntry>();
            var comparer = new EntityEntryEqualityComparer();

            while (hasChanges)
            {
                hasChanges = false;
                var entries = changeTracker.Entries().ToArray();
                var changes = entries.Except(changed, comparer);
                changed = entries;

                foreach (var entityEntry in changes)
                {
                    if (entityEntry.Entity is ISynchronizableBase synchronizable &&
                        entityEntry.State is EntityState.Added or EntityState.Modified)
                        synchronizable.UpdatedAt = dateTime;

                    hasChanges |= TryUpdateParent(changeTracker.Context, entityEntry, dateTime);
                }
            }
        }

        private static bool TryUpdateParent(DbContext context, EntityEntry entityEntry, DateTime dateTime)
        {
            var hasChanges = false;

            switch (entityEntry.Entity)
            {
                case Item item
                    when entityEntry.State is EntityState.Added or EntityState.Deleted or EntityState.Modified:
                    UpdateParentDateTime(context, item.Project, item.ProjectID, dateTime);
                    hasChanges = true;
                    break;
                case DynamicField dynamicField
                    when entityEntry.State is EntityState.Added or EntityState.Deleted or EntityState.Modified:
                    UpdateParentDateTime(context, dynamicField.Objective, dynamicField.ObjectiveID, dateTime);
                    hasChanges = true;
                    break;
                case DynamicField dynamicField
                    when entityEntry.State is EntityState.Added or EntityState.Deleted or EntityState.Modified:
                    UpdateParentDateTime(context, dynamicField.ParentField, dynamicField.ParentFieldID, dateTime);
                    hasChanges = true;
                    break;
                case ObjectiveItem objectiveItem
                    when entityEntry.State is EntityState.Added or EntityState.Deleted or EntityState.Modified:
                    UpdateParentDateTime(context, objectiveItem.Objective, objectiveItem.ObjectiveID, dateTime);
                    hasChanges = true;
                    break;
                case BimElementObjective bimElementObjective
                    when entityEntry.State is EntityState.Added or EntityState.Deleted or EntityState.Modified:
                    UpdateParentDateTime(
                        context,
                        bimElementObjective.Objective,
                        bimElementObjective.ObjectiveID,
                        dateTime);
                    hasChanges = true;
                    break;
            }

            return hasChanges;
        }

        private static void UpdateParentDateTime<T>(DbContext context, T parent, int? parentID, DateTime dateTime)
            where T : class, ISynchronizableBase
        {
            if (parent == null && !parentID.HasValue)
                return;

            if (parent != null)
                parent.UpdatedAt = dateTime;
            else
                context.Find<T>(parentID.Value).UpdatedAt = dateTime;
        }
    }
}
