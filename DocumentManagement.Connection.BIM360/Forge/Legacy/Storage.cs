using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Forge
{
    public class Storage
    {
        public string id;
        public string fileName => id.Split('/').Last();
        public string bucketId => id.Split(':').Last().Split('/').First();

        public async Task UploadAsync(Stream fileStream)
        {
            await AuthenticationService.Instance.CheckAccessAsync();
            var apiInstance = new ObjectsApi(Configuration.Default);
            var result = await apiInstance.UploadObjectAsync(bucketId, fileName, 100, fileStream);
        }

        public static async Task<Storage> CreateAsync(Folder folder, string fileName)
        {
            await AuthenticationService.Instance.CheckAccessAsync();
            var apiInstance = new ProjectsApi();
            #region var body = 
            var body = new CreateStorage(
                Jsonapi: new JsonApiVersionJsonapi(JsonApiVersionJsonapi.VersionEnum._0),
                Data: new CreateStorageData(
                    Type: CreateStorageData.TypeEnum.Objects,
                    Attributes: new CreateStorageDataAttributes(
                       Name: fileName,
                       Extension: new BaseAttributesExtensionObject("", "")),
                    Relationships: new CreateStorageDataRelationships(
                        Target: new CreateStorageDataRelationshipsTarget(
                            Data: new StorageRelationshipsTargetData(
                                Type: StorageRelationshipsTargetData.TypeEnum.Folders,
                                Id: folder.id)))));
            #endregion
            dynamic result = apiInstance.PostStorage(folder.project.id, body);
            return new Storage { id = result.data.id };
        }
    }
}
