using System.Threading.Tasks;
using MRS.DocumentManagement.Connection.Bim360.Forge.Models;

namespace MRS.DocumentManagement.Connection.Bim360.Synchronization.Converters
{
    internal class StatusObjectiveStatusConverter : IConverter<Status, ObjectiveStatus>
    {
        public Task<ObjectiveStatus> Convert(Status status)
            => Task.FromResult(
                status switch
                {
                    Status.Open => ObjectiveStatus.Open,
                    Status.Closed => ObjectiveStatus.Ready,
                    _ => ObjectiveStatus.Undefined,
                });
    }
}
