using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Forge.Models;
using Forge.Models.Authentication;
using Newtonsoft.Json;

namespace Forge
{
    public class IssuesService
    {
        public async Task<List<Issue>> GetIssuesAsync()
        {
            var request = new HttpRequestMessage(HttpMethod.Get, "https://developer.api.autodesk.com/issues/v1/containers/:container_id/quality-issues")
            {
                Content = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("client_id", appProperyClientId),
                    new KeyValuePair<string, string>("client_secret", appProperyClientSecret),
                    new KeyValuePair<string, string>("grant_type", "authorization_code"),
                    new KeyValuePair<string, string>("code", code),
                    new KeyValuePair<string, string>("redirect_uri", appProperyCallBackUrl),
                }),
            };
            request.RequestUri.Query
            
            
            var response = await client.SendAsync(request);
            response.EnsureSuccessStatusCode();
            var data = await response.Content.ReadAsStringAsync();
            //return JsonConvert.DeserializeObject<Token>(data);
            
            
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
    }
}