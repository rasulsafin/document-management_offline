using System;
using System.IO;
using System.Threading.Tasks;
using MRS.DocumentManagement.Connection.Bim360.Forge;
using MRS.DocumentManagement.Connection.Bim360.Forge.Models;
using MRS.DocumentManagement.Connection.Bim360.Forge.Models.DataManagement;
using MRS.DocumentManagement.Connection.Bim360.Forge.Services;
using MRS.DocumentManagement.Connection.Bim360.Forge.Utils.Extensions;
using MRS.DocumentManagement.Connection.Utils;
using MRS.DocumentManagement.Interface.Dtos;
using static MRS.DocumentManagement.Connection.Bim360.Forge.Constants;
using Version = MRS.DocumentManagement.Connection.Bim360.Forge.Models.DataManagement.Version;

namespace MRS.DocumentManagement.Connection.Bim360.Synchronization
{
    public class ItemsSyncHelper
    {
        private readonly ItemsService itemsService;
        private readonly ProjectsService projectsService;
        private readonly ObjectsService objectsService;

        public ItemsSyncHelper(ItemsService itemsService, ProjectsService projectsService, ObjectsService objectsService)
        {
            this.itemsService = itemsService;
            this.projectsService = projectsService;
            this.objectsService = objectsService;
        }

        // Replication for steps 5-7 from https://forge.autodesk.com/en/docs/bim360/v1/tutorials/upload-document/
        internal async Task<Item> PostItem(ItemExternalDto item, Folder folder, string projectId)
        {
            var fileName = Path.GetFileName(item.Name);

            // STEP 5. Create a Storage Object.
            var objectToUpload = new StorageObject
            {
                Attributes = new StorageObject.StorageObjectAttributes
                {
                    Name = fileName,
                },
                Relationships = new StorageObject.StorageObjectRelationships
                {
                    Target = new
                    {
                        data = new
                        {
                            type = FOLDER_TYPE,
                            id = folder.ID,
                        },
                    },
                },
            };

            var storage = await projectsService.CreateStorageAsync(projectId, objectToUpload);
            if (storage == default)
                return null;

            // STEP 6. Upload file to storage
            if (!storage.ID.Contains(':') || !storage.ID.Contains('/'))
                return null;

            var parsedId = storage.ParseStorageId();
            var bucketKey = parsedId.bucketKey;
            var hashedName = parsedId.hashedName;

            var documentsFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            var filePath = PathManager.DirectoryName(documentsFolder, PathManager.GetDir(item.Name));
            await objectsService.PutObjectAsync(bucketKey, hashedName, filePath);

            // STEP 7. Create first version
            var version = new Version
            {
                Attributes = new Version.VersionAttributes
                {
                    Name = fileName,
                    Extension = new Extension
                    {
                        Type = AUTODESK_VERSION_FILE_TYPE,
                    },
                },
                Relationships = new Version.VersionRelationships
                {
                    Storage = storage.ToInfo().ToDataContainer(),
                },
            };

            var bimItem = new Item
            {
                Attributes = new Item.ItemAttributes
                {
                    DisplayName = fileName,
                    Extension = new Extension
                    {
                        Type = AUTODESK_ITEM_FILE_TYPE,
                    },
                },
                Relationships = new Item.ItemRelationships
                {
                    Tip = version.ToInfo().ToDataContainer(),
                    Parent = folder.ToInfo().ToDataContainer(),
                },
            };

            var addedItem = await itemsService.PostItemAsync(projectId, bimItem, version);
            if (addedItem.item == null || addedItem.version == null)
                return null;

            return addedItem.item;
        }
    }
}
