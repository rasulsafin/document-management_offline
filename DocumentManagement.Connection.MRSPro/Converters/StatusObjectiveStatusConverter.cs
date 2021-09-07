using System.Threading.Tasks;
using MRS.DocumentManagement.Interface;
using static MRS.DocumentManagement.Connection.MrsPro.Constants;

namespace MRS.DocumentManagement.Connection.MrsPro.Converters
{
    internal class StatusObjectiveStatusConverter : IConverter<string, ObjectiveStatus>
    {
        public Task<ObjectiveStatus> Convert(string status)
         => Task.FromResult(status switch
         {
             STATE_VERIFIED => ObjectiveStatus.Ready,
             STATE_OPENED => ObjectiveStatus.Open,
             STATE_COMPLETED => ObjectiveStatus.InProgress,
             _ => ObjectiveStatus.Undefined,
         });
    }
}
