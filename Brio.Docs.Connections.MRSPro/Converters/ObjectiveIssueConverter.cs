using System.Threading.Tasks;
using Brio.Docs.Common;
using Brio.Docs.Connections.MrsPro.Extensions;
using Brio.Docs.Connections.MrsPro.Models;
using Brio.Docs.Integration;
using Brio.Docs.Integration.Dtos;
using static Brio.Docs.Connections.MrsPro.Constants;

namespace Brio.Docs.Connections.MrsPro.Converters
{
    internal class ObjectiveIssueConverter : IConverter<ObjectiveExternalDto, Issue>
    {
        private readonly IConverter<ObjectiveStatus, string> statusConverter;
        private readonly IConverter<string, (string id, string type)> idConverter;

        public ObjectiveIssueConverter(IConverter<ObjectiveStatus, string> statusConverter,
            IConverter<string, (string id, string type)> idConverter)
        {
            this.statusConverter = statusConverter;
            this.idConverter = idConverter;
        }

        public async Task<Issue> Convert(ObjectiveExternalDto dto)
        {
            var element = new Issue();
            (string id, _) = await idConverter.Convert(dto.ExternalID);
            element.Id = string.IsNullOrEmpty(id) ? null : id;
            element.Owner = dto.AuthorExternalID;
            element.Description = dto.Description;
            element.Title = dto.Title;
            element.CreatedDate = dto.CreationDate.ToUnixTime();
            element.DueDate = dto.DueDate.ToUnixTime();
            element.Type = ISSUE_TYPE;
            element.State = await statusConverter.Convert(dto.Status);

            (string parentID, string parentType) = await idConverter.Convert(dto.ParentObjectiveExternalID);
            element.ParentId = string.IsNullOrEmpty(parentID) ? dto.ProjectExternalID : parentID;
            element.ParentType = string.IsNullOrEmpty(parentType) ? ELEMENT_TYPE : parentType;

            // TODO: DynamicFields
            // TODO: Items
            // TODO: BimElements
            return element;
        }
    }
}
