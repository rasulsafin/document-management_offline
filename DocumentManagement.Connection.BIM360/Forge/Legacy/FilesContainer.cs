using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Forge
{
    public class FilesContainer
    {
        private Folder parent;

        public FilesContainer(Folder parent)
        {
            this.parent = parent;
        }

        public async Task<List<File>> GetAllAsync()
        {
            await AuthenticationService.Instance.CheckAccessAsync();
            var apiInstance = new FoldersApi(Configuration.Default);
            var result = await apiInstance.GetFolderContentsAsyncWithHttpInfo(parent.project.id, parent.id, new List<string> { "items" });
            return GetFilesFromVersions(result.Data, false);
        }

        public async Task<List<File>> SearchAsync(List<File.Format> formats)
        {
            await AuthenticationService.Instance.CheckAccessAsync();
            var apiInstance = new FoldersApi(Configuration.Default);
            var result = await apiInstance.SearchFolderContentsAsync(parent.project.id, parent.id);
            var formatsString = formats?.Select(x => x.ToString()).ToList();
            return GetFilesFromVersions(result.Data, true, formatsString);
        }

        public async Task CreateFileAsync(string fileName, Storage storage)
        {
            var type = "autodesk.bim360:File";
            var version = "1.0";
            await AuthenticationService.Instance.CheckAccessAsync();
            var apiInstance = new ItemsApi();
            #region var body = 
            var body =
                        new CreateItem(
                        Jsonapi: new JsonApiVersionJsonapi(JsonApiVersionJsonapi.VersionEnum._0),
                        Data: new CreateItemData(
                            Type: CreateItemData.TypeEnum.Items,
                            Attributes: new CreateItemDataAttributes(
                                fileName,
                                Extension: new BaseAttributesExtensionObject(
                                    Type: $"{CreateItemData.TypeEnum.Items.ToString().ToLower()}:{type}",
                                    version)),
                            Relationships: new CreateItemDataRelationships(
                                Tip: new CreateItemDataRelationshipsTip(
                                    Data: new CreateItemDataRelationshipsTipData(
                                        CreateItemDataRelationshipsTipData.TypeEnum.Versions,
                                        CreateItemDataRelationshipsTipData.IdEnum._1)),
                                Parent: new CreateStorageDataRelationshipsTarget(
                                    Data: new StorageRelationshipsTargetData(
                                        StorageRelationshipsTargetData.TypeEnum.Folders,
                                        parent.id)))),
                        Included: new List<CreateItemIncluded>
                        {
                            new CreateItemIncluded(
                                Type: CreateItemIncluded.TypeEnum.Versions,
                                Id: CreateItemIncluded.IdEnum._1,
                                Attributes: new CreateStorageDataAttributes(
                                    fileName,
                                    Extension: new BaseAttributesExtensionObject(
                                        Type: $"{CreateItemIncluded.TypeEnum.Versions.ToString().ToLower()}:{type}",
                                        version)),
                                Relationships: new CreateItemRelationships(
                                    Storage: new CreateItemRelationshipsStorage(
                                        Data: new CreateItemRelationshipsStorageData(
                                            Type: CreateItemRelationshipsStorageData.TypeEnum.Objects,
                                            Id: storage.id))))
                        });
            #endregion
            dynamic result = apiInstance.PostItem(parent.project.id, body);
        }

        private void AddToList(List<File> list, dynamic version, List<dynamic> items)
        {
            var item = items.First(x => x.id == version.relationships.item.data.id);
            var documents = items.FindAll(x =>
                    x.attributes.extension.type.Contains("Document") &&                                              // Find documents
                    x.attributes.extension.data.sourceFileName == item.attributes.extension.data.sourceFileName &&   // that have the same source file
                    x.relationships.parent.data.id == item.relationships.parent.data.id);                            // and located in the same folder
            if (!item.attributes.hidden && !(documents.Count > 0 && documents.All(x => x.attributes.hidden)))
                list.Add(new File
                {
                    id = version.relationships.item.data.id,
                    name = version.attributes.name,
                    project = parent.project,
                    storage = new Storage
                    {
                        id = version.relationships.storage.data.id
                    }
                });
        }

        private List<File> GetFilesFromVersions(dynamic data, bool versionsInData,
            List<string> formats = null)
        {
            var result = new List<File>();
            if (((DynamicJsonResponse)data).GetDynamicMemberNames().Any(x => x == "included"))
            {
                List<dynamic> items = GetList(data, versionsInData ? JsonListTypes.included : JsonListTypes.data);
                List<dynamic> versions = GetList(data, versionsInData ? JsonListTypes.data : JsonListTypes.included);

                foreach (var version in versions)
                    if (version.attributes.extension.type.Contains("File") &&
                        (formats?.Contains(version.attributes.fileType) ?? true))
                        AddToList(result, version, items);
            }
            return result;
        }

        private List<dynamic> GetList(dynamic data, JsonListTypes type)
        {
            switch (type)
            {
                case JsonListTypes.data:
                    return ((DynamicDictionary)data.data).GetValues();
                case JsonListTypes.included:
                    return ((DynamicDictionary)data.included).GetValues();
                default:
                    return null;
            }
        }

        private enum JsonListTypes
        {
            data,
            included
        }
    }
}
