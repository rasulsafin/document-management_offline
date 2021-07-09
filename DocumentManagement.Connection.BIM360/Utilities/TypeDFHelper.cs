using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MRS.DocumentManagement.Connection.Bim360.Forge;
using MRS.DocumentManagement.Connection.Bim360.Forge.Models;
using MRS.DocumentManagement.Connection.Bim360.Forge.Services;
using MRS.DocumentManagement.Interface.Dtos;
using Newtonsoft.Json;

namespace MRS.DocumentManagement.Connection.Bim360.Utilities
{
    public class TypeDFHelper
    {
        private static readonly EnumerationValueExternalDto UNDEFINED_NG_TYPE = new ()
        {
            ExternalID = "undefined-bim360-ng-type-2021.07.09",
            Value = "Undefined",
        };

        private static readonly string ENUM_EXTERNAL_ID = "ng_issue_type_id,ng_issue_subtype_id";
        private static readonly string DISPLAY_NAME = "Type";

        private readonly IssuesService issuesService;
        private readonly HubsService hubsService;
        private readonly ProjectsService projectsService;

        public TypeDFHelper(
            IssuesService issuesService,
            HubsService hubsService,
            ProjectsService projectsService)
        {
            this.issuesService = issuesService;
            this.hubsService = hubsService;
            this.projectsService = projectsService;
        }

        public static string ID => ENUM_EXTERNAL_ID;

        internal static IEnumerable<IGrouping<string, (IssueType type, IssueSubtype subtype)>> GetGroupedTypes(
            IEnumerable<(IssueType type, IssueSubtype subtype)> allTypes)
            => allTypes
               .Select(
                    item => (displayName: GetDisplayName(item.type, item.subtype),
                        tupleTypes: (item.type, item.subtype)))
               .GroupBy(
                    x => x.displayName,
                    x => x.tupleTypes,
                    StringComparer.CurrentCultureIgnoreCase);

        internal static string GetExternalID(IEnumerable<(IssueType type, IssueSubtype subtype)> types)
            => JsonConvert.SerializeObject(
                types.Select(x => (type: x.type.ID, subtype: x.subtype.ID))
                   .OrderBy(id => id.type)
                   .ThenBy(id => id.subtype)
                   .ToList());

        internal static IEnumerable<(string type, string subtype)> DeserializeID(string externalID)
        {
            try
            {
                return string.IsNullOrWhiteSpace(externalID)
                    ? Enumerable.Empty<(string type, string subtype)>()
                    : JsonConvert.DeserializeObject<IEnumerable<(string type, string subtype)>>(externalID);
            }
            catch (Exception)
            {
                return Enumerable.Empty<(string type, string subtype)>();
            }
        }

        internal static DynamicFieldExternalDto GetDefault()
            => new ()
            {
                ExternalID = ENUM_EXTERNAL_ID,
                Name = DISPLAY_NAME,
                Type = DynamicFieldType.ENUM,
                Value = UNDEFINED_NG_TYPE.ExternalID,
            };

        internal async Task<EnumerationTypeExternalDto> GetTypeEnumeration()
        {
            var types = new List<(IssueType type, IssueSubtype subtype)>();

            foreach (var hub in await hubsService.GetHubsAsync())
            {
                foreach (var project in (await projectsService.GetProjectsAsync(hub.ID)).Where(
                    p => p.Attributes.Name != Constants.INTEGRATION_TEST_PROJECT))
                {
                    types.AddRange(
                        (await issuesService.GetIssueTypesAsync(project.Relationships.IssuesContainer.Data.ID))
                       .SelectMany(x => x.Subtypes.Select(y => (x, y))));
                }
            }

            var values = GetGroupedTypes(types)
               .Select(
                    x => new EnumerationValueExternalDto
                    {
                        ExternalID = GetExternalID(x),
                        Value = x.Key,
                    })
               .Append(UNDEFINED_NG_TYPE)
               .ToList();
            var enumType = new EnumerationTypeExternalDto
            {
                ExternalID = ENUM_EXTERNAL_ID,
                Name = DISPLAY_NAME,
                EnumerationValues = values,
            };
            return enumType;
        }

        private static string GetDisplayName(IssueType type, IssueSubtype subtype)
            => string.Equals(type.Title, subtype.Title, StringComparison.Ordinal)
                ? type.Title
                : $"{type.Title}: {subtype.Title}";
    }
}
