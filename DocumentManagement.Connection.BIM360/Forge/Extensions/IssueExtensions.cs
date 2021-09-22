using Brio.Docs.Connection.Bim360.Forge.Models.Bim360;
using System.Linq;

namespace Brio.Docs.Connection.Bim360.Forge.Utils.Extensions
{
    internal static class IssueExtensions
    {
        public static Issue GetPatchableIssue(this Issue issue)
        {
            if (issue.Attributes?.PermittedAttributes == null)
                return issue;

            var result = new Issue
            {
                ID = issue.ID,
                Type = issue.Type,
                Attributes = new Issue.IssueAttributes(),
            };

            var type = typeof(Issue.IssueAttributes);
            var properties = type
               .GetProperties()
               .ToDictionary(DataMemberUtilities.GetDataMemberName, p => p);

            foreach (var attribute in issue.Attributes.PermittedAttributes)
            {
                if (properties.TryGetValue(attribute, out var property))
                    property.SetValue(result.Attributes, property.GetValue(issue.Attributes));
            }

            return result;
        }
    }
}
