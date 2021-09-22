using System.Threading.Tasks;
using Brio.Docs.Common;
using Brio.Docs.Integration;
using Brio.Docs.Integration.Interfaces;
using static Brio.Docs.Connections.MrsPro.Constants;

namespace Brio.Docs.Connections.MrsPro.Converters
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
