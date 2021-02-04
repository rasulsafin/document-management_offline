using System.Collections.Generic;
using System.Threading.Tasks;
using Forge;

public class UsersContainer
{
    private Project parent;

    public UsersContainer(Project parent)
    {
        this.parent = parent;
    }

    public async Task<User> MeAsync()
    {
        await AuthenticationService.Instance.CheckAccessAsync();
        var response = await JsonData<User>.GetDeserializedData("/issues/v1/containers/{container_id}/users/me",
            new Dictionary<string, string> { ["container_id"] = parent.issuesContainerId });
        response.data.name = response.data.attributes.name;
        return response.data;
    }
}
