using System.Collections.Generic;
using System.Threading.Tasks;

namespace Forge.Legacy
{
    public class Folder : CloudItem<,>
    {
        public Project project;
        public FilesContainer files;

        public Folder()
        {
            files = new FilesContainer(this);
        }

        public async Task<List<Folder>> GetAllFoldersAsync()
        {
            await AuthenticationService.Instance.CheckAccessAsync();
            var apiInstance = new FoldersApi(Configuration.Default);
            var result = await apiInstance.GetFolderContentsAsyncWithHttpInfo(project.id, id, new List<string> { "folders" });
            var listFolders = new List<Folder>();
            foreach (var item in result.Data.data.Items())
            {
                dynamic value = item.Value;
                if (value.type == "folders")
                    // Add folder.
                    listFolders.Add(new Folder
                    {
                        id = value.id,
                        name = value.attributes.name,
                        project = project
                    });
            }
            return listFolders;
        }
    }
}
