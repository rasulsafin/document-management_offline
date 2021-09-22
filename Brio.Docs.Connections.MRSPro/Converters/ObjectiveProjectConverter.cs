using Brio.Docs.Connections.MrsPro.Models;
using Brio.Docs.Client;
using Brio.Docs.Client.Dtos;
using System.Threading.Tasks;
using Brio.Docs.Connections.MrsPro.Extensions;
using static Brio.Docs.Connections.MrsPro.Constants;

namespace Brio.Docs.Connections.MrsPro.Converters
{
    internal class ObjectiveProjectConverter : IConverter<ObjectiveExternalDto, Project>
    {
        private readonly IConverter<string, (string id, string type)> idConverter;

        public ObjectiveProjectConverter(IConverter<string, (string id, string type)> idConverter)
        {
            this.idConverter = idConverter;
        }

        public async Task<Project> Convert(ObjectiveExternalDto dto)
        {
            var element = new Project();
            (string id, _) = await idConverter.Convert(dto.ExternalID);
            element.Id = string.IsNullOrEmpty(id) ? null : id;
            element.Owner = dto.AuthorExternalID;
            element.Name = dto.Title;
            element.CreatedDate = dto.CreationDate.ToUnixTime();
            element.Type = ELEMENT_TYPE;

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
