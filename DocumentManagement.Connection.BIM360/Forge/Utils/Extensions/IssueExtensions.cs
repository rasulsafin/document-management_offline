using System.Linq;
using MRS.DocumentManagement.Connection.Bim360.Forge.Models;
using static MRS.DocumentManagement.Connection.Bim360.Forge.Models.Issue;

namespace MRS.DocumentManagement.Connection.Bim360.Forge.Utils.Extensions
{
    public static class IssueExtensions
    {
        public static Issue GetPatchableIssue(this Issue issue)
        {
            if (issue.Attributes?.PermittedAttributes == null)
                return issue;

            var result = new Issue
            {
                ID = issue.ID,
                Type = issue.Type,
                Attributes = new IssueAttributes(),
            };

            var type = typeof(IssueAttributes);
            var properties = type
                    .GetProperties()
                    .ToDictionary(p => type.GetDataMemberName(p), p => p);

            foreach (var attribute in issue.Attributes.PermittedAttributes)
            {
                if (properties.TryGetValue(attribute, out var property))
                    property.SetValue(result.Attributes, property.GetValue(issue.Attributes));
            }

            return result;
        }
    }
}
