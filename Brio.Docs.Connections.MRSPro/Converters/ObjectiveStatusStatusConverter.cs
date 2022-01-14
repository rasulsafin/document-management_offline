using System.Threading.Tasks;
using Brio.Docs.Common;
using Brio.Docs.Integration.Interfaces;
using static Brio.Docs.Connections.MrsPro.Constants;

namespace Brio.Docs.Connections.MrsPro.Converters
{
    internal class ObjectiveStatusStatusConverter : IConverter<ObjectiveStatus, string>
    {
        public Task<string> Convert(ObjectiveStatus status)
         => Task.FromResult(status switch
         {
             ObjectiveStatus.Open => STATE_OPENED,
             ObjectiveStatus.Undefined => STATE_OPENED,
             ObjectiveStatus.InProgress => STATE_OPENED,
             ObjectiveStatus.Ready => STATE_COMPLETED,
             ObjectiveStatus.Done => STATE_VERIFIED,
             ObjectiveStatus.Late => STATE_OPENED,
             _ => STATE_OPENED,
         });
    }
}
