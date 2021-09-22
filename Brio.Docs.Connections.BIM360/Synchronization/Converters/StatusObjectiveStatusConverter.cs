using System.Threading.Tasks;
using Brio.Docs.Common;
using Brio.Docs.Connections.Bim360.Forge.Models.Bim360;
using Brio.Docs.Integration;

namespace Brio.Docs.Connections.Bim360.Synchronization.Converters
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
