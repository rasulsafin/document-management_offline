using System.Linq;
using System.Threading.Tasks;
using Brio.Docs.Common;
using Brio.Docs.Connections.Bim360.Synchronization.Extensions;
using Brio.Docs.Connections.Bim360.Utilities;
using Brio.Docs.Connections.Bim360.Utilities.Snapshot;
using Brio.Docs.Connections.Bim360.Utilities.Snapshot.Models;
using Brio.Docs.Integration.Interfaces;

namespace Brio.Docs.Connections.Bim360.Synchronization.Converters
{
    internal class IssueSnapshotObjectiveStatusConverter : IConverter<IssueSnapshot, ObjectiveStatus>
    {
        public Task<ObjectiveStatus> Convert(IssueSnapshot issueSnapshot)
        {
            var project = issueSnapshot.ProjectSnapshot;
            var config = project.StatusesRelations ?? ConfigurationUtilities.GetDefaultStatusesConfig();
            var status = issueSnapshot.Entity.GetSuitableStatuses(config)
               .Append(ObjectiveStatus.Undefined)
               .First();
            return Task.FromResult(status);
        }
    }
}
