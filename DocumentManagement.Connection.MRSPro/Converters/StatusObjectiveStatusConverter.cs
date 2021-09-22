using Brio.Docs.Interface;
using System.Threading.Tasks;
using static Brio.Docs.Connection.MrsPro.Constants;

namespace Brio.Docs.Connection.MrsPro.Converters
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
