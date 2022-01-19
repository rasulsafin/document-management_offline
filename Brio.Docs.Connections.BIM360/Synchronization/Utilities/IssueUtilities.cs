using System;
using System.Collections.Generic;
using System.Linq;
using Brio.Docs.Connections.Bim360.Forge.Extensions;
using Brio.Docs.Connections.Bim360.Forge.Models;
using Brio.Docs.Connections.Bim360.Forge.Models.Bim360;
using Brio.Docs.Connections.Bim360.Forge.Utils;

namespace Brio.Docs.Connections.Bim360.Synchronization.Utilities
{
    internal static class IssueUtilities
    {
        private static readonly Status[] READ_ONLY_STATUSES = { Status.Void, Status.Closed };
        private static readonly Status REMOVED_STATUS = Status.Void;

        internal static IEnumerable<Status> ReadOnlyStatuses => READ_ONLY_STATUSES;

        internal static Filter GetFilterForUnremoved()
        {
            var statusKey = DataMemberUtilities.GetPath<Issue.IssueAttributes>(x => x.Status);
            return new Filter(statusKey, GetStatusesExceptRemoved().Select(x => x.GetEnumMemberValue()).ToArray());
        }

        internal static bool IsRemoved(Issue issue)
            => issue.Attributes.Status == REMOVED_STATUS;

        private static IEnumerable<Status> GetStatusesExceptRemoved()
            => Enum.GetValues(typeof(Status)).Cast<Status>().Where(x => x != REMOVED_STATUS && x != Status.Undefined);
    }
}
