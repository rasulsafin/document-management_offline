using System.Threading.Tasks;
using MRS.DocumentManagement.Connection.Bim360.Synchronization.Helpers.Snapshot;
using MRS.DocumentManagement.Connection.Bim360.Utilities;
using MRS.DocumentManagement.Interface.Dtos;

namespace MRS.DocumentManagement.Connection.Bim360.Synchronization.Converters
{
    internal class IssueTypeSnapshotDynamicFieldConverter : IConverter<IssueTypeSnapshot, DynamicFieldExternalDto>
    {
        public Task<DynamicFieldExternalDto> Convert(IssueTypeSnapshot type)
        {
            var result = TypeDFHelper.GetDefault();
            result.Value = type.ID;
            return Task.FromResult(result);
        }
    }
}
