using System;
using System.Collections.Generic;
using System.Linq;
using MRS.DocumentManagement.Connection.Bim360.Forge.Models;
using MRS.DocumentManagement.Connection.Bim360.Forge.Utils.Extensions;

namespace MRS.DocumentManagement.Connection.Bim360.Synchronization.Utilities
{
    internal static class IssueUtilities
    {
        private static readonly Status REMOVED_STATUS = Status.Void;

        internal static bool IsRemoved(Issue issue)
            => issue.Attributes.Status == REMOVED_STATUS;

        internal static Filter GetFilterForUnremoved()
        {
            var statusKey = typeof(Issue.IssueAttributes).GetDataMemberName(nameof(Issue.IssueAttributes.Status));
            return new Filter(statusKey, GetStatusesExceptRemoved().Select(x => x.GetEnumMemberValue()).ToArray());
        }

        private static IEnumerable<Status> GetStatusesExceptRemoved()
            => Enum.GetValues(typeof(Status)).Cast<Status>().Where(x => x != REMOVED_STATUS);
    }
}
