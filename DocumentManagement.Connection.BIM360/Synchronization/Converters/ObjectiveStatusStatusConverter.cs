using Brio.Docs.Connections.Bim360.Forge.Models.Bim360;
using Brio.Docs.Interface;
using System.Threading.Tasks;

namespace Brio.Docs.Connections.Bim360.Synchronization.Converters
{
    internal class ObjectiveStatusStatusConverter : IConverter<ObjectiveStatus, Status>
    {
        public Task<Status> Convert(ObjectiveStatus status)
            => Task.FromResult(
                status switch
                {
                    ObjectiveStatus.InProgress => Status.Open,
                    ObjectiveStatus.Ready => Status.Closed,
                    _ => Status.Open
                });
    }
}
