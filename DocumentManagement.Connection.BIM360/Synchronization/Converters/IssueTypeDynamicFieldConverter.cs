using System.Linq;
using System.Threading.Tasks;
using MRS.DocumentManagement.Connection.Bim360.Forge.Models;
using MRS.DocumentManagement.Connection.Bim360.Forge.Utils.Extensions;
using MRS.DocumentManagement.Interface.Dtos;
using static MRS.DocumentManagement.Connection.Bim360.Forge.Constants;

namespace MRS.DocumentManagement.Connection.Bim360.Synchronization.Converters
{
    internal class IssueTypeDynamicFieldConverter : IConverter<IssueType, DynamicFieldExternalDto>
    {
        public Task<DynamicFieldExternalDto> Convert(IssueType type)
        {
            return Task.FromResult(
                new DynamicFieldExternalDto
                {
                    ExternalID =
                        typeof(Issue.IssueAttributes).GetDataMemberName(nameof(Issue.IssueAttributes.NgIssueTypeID)),
                    Name = STANDARD_NG_TYPES.Value.Name,
                    Value = STANDARD_NG_TYPES.Value.EnumerationValues.FirstOrDefault(x => x.ExternalID == type.Title)
                      ?.ExternalID ?? UNDEFINED_NG_TYPE.ExternalID,
                    Type = DynamicFieldType.ENUM,
                });
        }
    }
}
