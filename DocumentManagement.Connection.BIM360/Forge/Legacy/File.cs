using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Autodesk.Forge;
using Autodesk.Forge.Client;
using Autodesk.Forge.Model;

namespace Forge
{
    public class File : CloudItem<,>
    {
        public static explicit operator DocumentManagement.Item(File file) => new DocumentManagement.Item()
        {
            ID = file.id,
            Name = file.name,
            Parent_id = file.project.id,
            Path = $"{defaultPath}/{file.project.name}/{file.name}"
        };

        public Project project;
        public Storage storage;

        public static string defaultPath = @"C:/Brio MRS/BIM360";


        public Format format
        {
            get
            {
                var extension = Regex.Replace(name, ".*[.]", "");
                Format format;
                Enum.TryParse(extension, out format);
                return format;
            }
        }

        public async Task<Stream> GetFileStreamAsync()
        {
            await AuthenticationService.Instance.CheckAccessAsync();
            var apiInstance = new ObjectsApi(Configuration.Default);
            return await apiInstance.GetObjectAsync(storage.bucketId, storage.fileName);
        }

        public async Task SetVersionAsync(Storage storage)
        {
            var type = "autodesk.bim360:File";
            await AuthenticationService.Instance.CheckAccessAsync();
            var apiInstance = new VersionsApi(Configuration.Default);
            #region body =
            var body = new CreateVersion(
                new JsonApiVersionJsonapi(JsonApiVersionJsonapi.VersionEnum._0),
                new CreateVersionData(
                    CreateVersionData.TypeEnum.Versions,
                    new CreateStorageDataAttributes(
                        name,
                        new BaseAttributesExtensionObject($"{CreateVersionData.TypeEnum.Versions.ToString().ToLower()}:{type}", "1.0")),
                    new CreateVersionDataRelationships(
                        new CreateVersionDataRelationshipsItem(
                            new CreateVersionDataRelationshipsItemData(
                                CreateVersionDataRelationshipsItemData.TypeEnum.Items, id)),
                        new CreateItemRelationshipsStorage(
                            new CreateItemRelationshipsStorageData(CreateItemRelationshipsStorageData.TypeEnum.Objects, storage.id)))));
            #endregion
            await apiInstance.PostVersionAsync(project.id, body);
        }

        public enum Format
        {
            another,
            ifc,
            pdf
        }
    }
}
