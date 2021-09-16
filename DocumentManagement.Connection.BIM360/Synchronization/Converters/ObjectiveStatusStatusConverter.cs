using System.Threading.Tasks;
using MRS.DocumentManagement.Connection.Bim360.Forge.Models.Bim360;
using MRS.DocumentManagement.Interface;

namespace MRS.DocumentManagement.Connection.Bim360.Synchronization.Converters
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
