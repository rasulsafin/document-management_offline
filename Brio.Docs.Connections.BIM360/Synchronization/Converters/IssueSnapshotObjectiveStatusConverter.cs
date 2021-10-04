using System.Linq;
using System.Threading.Tasks;
using MRS.DocumentManagement.Connection.Bim360.Synchronization.Extensions;
using MRS.DocumentManagement.Connection.Bim360.Utilities;
using MRS.DocumentManagement.Connection.Bim360.Utilities.Snapshot;
using MRS.DocumentManagement.Interface;

namespace MRS.DocumentManagement.Connection.Bim360.Synchronization.Converters
{
    internal class IssueSnapshotObjectiveStatusConverter : IConverter<IssueSnapshot, ObjectiveStatus>
    {
        public Task<ObjectiveStatus> Convert(IssueSnapshot issueSnapshot)
        {
            var project = issueSnapshot.ProjectSnapshot;
            var config = project.StatusesRelations ?? IfcConfigUtilities.GetDefaultStatusesConfig();
            var status = issueSnapshot.Entity.GetSuitableStatuses(config)
               .Append(ObjectiveStatus.Undefined)
               .First();
            return Task.FromResult(status);
        }
    }
}
