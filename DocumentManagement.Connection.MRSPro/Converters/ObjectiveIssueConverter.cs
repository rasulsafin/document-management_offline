using System.Threading.Tasks;
using MRS.DocumentManagement.Connection.MrsPro.Extensions;
using MRS.DocumentManagement.Connection.MrsPro.Models;
using MRS.DocumentManagement.Interface;
using MRS.DocumentManagement.Interface.Dtos;
using static MRS.DocumentManagement.Connection.MrsPro.Constants;

namespace MRS.DocumentManagement.Connection.MrsPro.Converters
{
    internal class ObjectiveIssueConverter : IConverter<ObjectiveExternalDto, Issue>
    {
        private readonly IConverter<ObjectiveStatus, string> statusConverter;

        public ObjectiveIssueConverter(IConverter<ObjectiveStatus, string> statusConverter)
        {
            this.statusConverter = statusConverter;
        }

        public async Task<Issue> Convert(ObjectiveExternalDto dto)
        {
            var element = new Issue();
            element.Id = dto.ExternalID?.Split(ID_SPLITTER)[0]; //TODO: fix it, if ids with be ancestry
            element.Owner = dto.AuthorExternalID;
            element.Description = dto.Description;
            element.Title = dto.Title;
            element.CreatedDate = dto.CreationDate.ToUnixTime();
            element.DueDate = dto.DueDate.ToUnixTime();
            element.Type = ISSUE_TYPE;
            element.State = await statusConverter.Convert(dto.Status);

            var parentData = dto.ParentObjectiveExternalID?.Split(ID_SPLITTER); // TODO: fix it, if ids with be ancestry
            element.ParentId = parentData?[0] ?? dto.ProjectExternalID;
            element.ParentType = parentData?[1] ?? ELEMENT_TYPE;

            // TODO: DynamicFields
            // TODO: Items
            // TODO: BimElements
            return element;
        }
    }
}
