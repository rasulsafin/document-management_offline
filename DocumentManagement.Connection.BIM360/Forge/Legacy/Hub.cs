using System.Collections.Generic;
using System.Threading.Tasks;

namespace Forge.Legacy
{
    public class Hub : CloudItem<,>
    {
        public static async Task<List<Hub>> GetHubsAsync(List<string> filterId = null, List<string> filterExtensionType = null)
        {
            await AuthenticationService.Instance.CheckAccessAsync();
            var apiInstance = new HubsApi(Configuration.Default);

            var result = await apiInstance.GetHubsAsyncWithHttpInfo(filterId, filterExtensionType);
            var list = new List<Hub>();
            foreach (var item in result.Data.data.Items())
            {
                dynamic value = item.Value;
                list.Add(new Hub
                {
                    id = value.id,
                    name = value.attributes.name
                });
            }
            return list;

        }

        public async Task<List<Project>> GetProjectsAsync()
        {
            await AuthenticationService.Instance.CheckAccessAsync();
            var apiInstance = new ProjectsApi(Configuration.Default);
            var response = await apiInstance.GetHubProjectsAsyncWithHttpInfo(id);

            var projects = new List<Project>();
            foreach (var item in response.Data.data.Items())
            {
                dynamic value = item.Value;
                var rootFolder = new Folder { id = value.relationships.rootFolder.data.id };
                var project = new Project(rootFolder)
                {
                    id = value.id,
                    name = value.attributes.name,
                    issuesContainerId = value.relationships.issues.data.id
                };
                projects.Add(project);
            }
            return projects;
        }
    }
}