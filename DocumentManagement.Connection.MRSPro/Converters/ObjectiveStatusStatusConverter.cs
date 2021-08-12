using System.Threading.Tasks;
using MRS.DocumentManagement.Interface;
using static MRS.DocumentManagement.Connection.MrsPro.Constants;

namespace MRS.DocumentManagement.Connection.MrsPro.Converters
{
    internal class ObjectiveStatusStatusConverter : IConverter<ObjectiveStatus, string>
    {
        public Task<string> Convert(ObjectiveStatus status)
         => Task.FromResult(status switch
         {
             ObjectiveStatus.Open => STATE_OPENED,
             ObjectiveStatus.Undefined => STATE_OPENED,
             ObjectiveStatus.InProgress => STATE_COMPLETED,
             ObjectiveStatus.Ready => STATE_VERIFIED,
             ObjectiveStatus.Late => STATE_OPENED,
             _ => STATE_OPENED,
         });
    }
}
