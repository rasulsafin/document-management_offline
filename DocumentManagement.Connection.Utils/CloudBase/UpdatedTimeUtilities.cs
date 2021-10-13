using System;
using MRS.DocumentManagement.Interface.Dtos;

namespace MRS.DocumentManagement.Connection.Utils
{
    public static class UpdatedTimeUtilities
    {
        public static void UpdateTime(ProjectExternalDto project, DateTime time = default)
        {
            var now = GetCorrectTime(time);
            project.UpdatedAt = now;

            if (project.Items != null)
            {
                foreach (var item in project.Items)
                    UpdateTime(item, time);
            }
        }

        public static void UpdateTime(ObjectiveExternalDto objective, DateTime time = default)
        {
            var now = GetCorrectTime(time);
            objective.UpdatedAt = now;

            if (objective.Items != null)
            {
                foreach (var item in objective.Items)
                    UpdateTime(item, time);
            }

            if (objective.DynamicFields != null)
            {
                foreach (var dynamicField in objective.DynamicFields)
                    UpdateTime(dynamicField, time);
            }

            if (objective.Location?.Item != null)
                UpdateTime(objective.Location.Item, time);
        }

        private static void UpdateTime(ItemExternalDto item, DateTime time)
            => item.UpdatedAt = time;

        private static void UpdateTime(DynamicFieldExternalDto dynamicField, DateTime time)
            => dynamicField.UpdatedAt = time;

        private static DateTime GetCorrectTime(DateTime time)
            => time == default ? DateTime.UtcNow : time;
    }
}
