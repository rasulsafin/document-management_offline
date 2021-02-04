using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Forge
{
    public class IssuesContainer
    { 
        private IssueType defaultIssueType;
        private Project parent;

        public IssuesContainer(Project parent)
        {
            this.parent = parent;
        }

        public async Task<IssueType> GetDefaultTypeAsync()
        {
            if (defaultIssueType == null)
                defaultIssueType = (await GetTypesAsync())[0];
            return defaultIssueType;
        }

        public async Task<List<IssueType>> GetTypesAsync()
        {
            await AuthenticationService.Instance.CheckAccessAsync();
            var issueTypes = await JsonData<List<IssueType>>.GetDeserializedData("/issues/v1/containers/{container_id}/ng-issue-types?include=subtypes",
                new Dictionary<string, string> { ["container_id"] = parent.issuesContainerId });
            return issueTypes.results;
        }

        private const int itemsOnPage = 100;

        public async Task<List<Issue>> AllAsync()
        {
            await AuthenticationService.Instance.CheckAccessAsync();
            var list = new List<Issue>();
            var attachments = new List<Attachment>();
            var all = false;
            
            // load issues while all pages not already read
            for (var i = 0; !all; i += itemsOnPage)
            {
                var response = await JsonData<List<Issue>, List<CloudItem<,>>>.GetDeserializedData(
                    "/issues/v1/containers/{container_id}/quality-issues?include=attachments&page[limit]={limit}&page[offset]={i}",
                    new Dictionary<string, string>
                    {
                        ["container_id"] = parent.issuesContainerId,
                        ["limit"] = itemsOnPage.ToString(),
                        ["i"] = i.ToString()
                    });
                list.AddRange(response.data);
                if (response.included != null)
                    attachments.AddRange(response.included.Select(x => x.data));
                all = response.meta.page.limit * (response.meta.page.offset / itemsOnPage + 1) >=
                      response.meta.record_count;
            }

            // attach files
            var projectFiles = await parent.rootFolder.files.SearchAsync(null);
            foreach (var attachment in attachments)
            {
                var file = projectFiles.Find(x =>
                    attachment.itemId.Contains(x.storage.fileName) ||
                    attachment.itemId.Contains(x.name) ||
                    attachment.itemId.Contains(x.id)); 
                if (file == null)
                {
                    break;
                }
                file.name = attachment.name;
                list.Find(x => x.id == attachment.issueId).attachments.Add(file);
            }

            return list;
        }

        public async Task<Issue> AddAsync(Issue issue)
        {
            await AuthenticationService.Instance.CheckAccessAsync();
            var response = await JsonData<Issue>.PostSerializedData("/issues/v1/containers/{container_id}/quality-issues",
                 new Dictionary<string, string> { ["container_id"] = parent.issuesContainerId }, issue);
            return null;
        }


        public async Task<Issue> ChangeAsync(Issue issue)
        {
            await AuthenticationService.Instance.CheckAccessAsync();
            // Delete info for JSON request.
            var save = new
            {
                issue.attributes.ng_issue_type_id,
                issue.attributes.created_at,
            };
            issue.attributes.ng_issue_type_id = null;
            issue.attributes.created_at = null;

            try
            {
                // Request.
                var response = await JsonData<Issue>.PatchSerializedData("/issues/v1/containers/{container_id}/quality-issues/{issue_id}",
                    new Dictionary<string, string> { ["container_id"] = parent.issuesContainerId, ["issue_id"] = issue.id }, issue);
                return null;
            }
            finally
            {
                // Recover deleted info.
                issue.attributes.ng_issue_type_id = save.ng_issue_type_id;
                issue.attributes.created_at = save.created_at;
            }
        }
    }
}
