using System.IO;
using System.Threading.Tasks;
using MRS.DocumentManagement.Connection.Bim360.Forge.Models;
using MRS.DocumentManagement.Connection.Bim360.Forge.Models.DataManagement;
using MRS.DocumentManagement.Connection.Bim360.Forge.Services;
using MRS.DocumentManagement.Connection.Bim360.Forge.Utils.Extensions;
using MRS.DocumentManagement.Connection.Bim360.Synchronization.Helpers.Snapshot;
using MRS.DocumentManagement.Interface.Dtos;
using static MRS.DocumentManagement.Connection.Bim360.Forge.Constants;
using Version = MRS.DocumentManagement.Connection.Bim360.Forge.Models.DataManagement.Version;

namespace MRS.DocumentManagement.Connection.Bim360.Synchronization.Utilities
{
    public class ItemsSyncHelper
    {
        private readonly ItemsService itemsService;
        private readonly ProjectsService projectsService;
        private readonly ObjectsService objectsService;
        private readonly VersionsService versionsService;

        public ItemsSyncHelper(ItemsService itemsService, ProjectsService projectsService, ObjectsService objectsService, VersionsService versionsService)
        {
            this.itemsService = itemsService;
            this.projectsService = projectsService;
            this.objectsService = objectsService;
            this.versionsService = versionsService;
        }

        internal async Task<(Item item, Version version)> PostItem(ProjectSnapshot project, ItemExternalDto item)
        {
            var posted = await PostItem(item, project.MrsFolderID, project.ID);
            project.Items.Add(posted.item.ID, new ItemSnapshot(posted.item) { Version = posted.version });
            return posted;
        }

        internal async Task<(Item item, Version version)> UpdateVersion(ProjectSnapshot project, ItemExternalDto item)
        {
            var snapshot = project.Items[item.ExternalID];
            var posted = await UpdateVersion(item, project.MrsFolderID, project.ID, snapshot.Entity);

            if (snapshot.ID != posted.item.ID)
            {
                project.Items.Remove(snapshot.ID);
                project.Items.Add(posted.item.ID, new ItemSnapshot(posted.item) { Version = posted.version });
            }
            else
            {
                snapshot.Entity = posted.item;
                snapshot.Version = posted.version;
            }

            return posted;
        }

        internal async Task Remove(string projectID, Item item)
        {
            // Delete uploaded item by marking version as "deleted"
            var deletedVersion = new Version
            {
                Attributes = new Version.VersionAttributes
                {
                    Name = item.Attributes.DisplayName,
                    Extension = new Extension { Type = AUTODESK_VERSION_DELETED_TYPE },
                },
                Relationships = new Version.VersionRelationships
                {
                    Item = new ObjectInfo
                    {
                        ID = item.ID,
                        Type = ITEM_TYPE,
                    }.ToDataContainer(),
                },
            };

            await versionsService.PostVersionAsync(projectID, deletedVersion);
        }

        // Replication for steps 5, 6, 8 from https://forge.autodesk.com/en/docs/bim360/v1/tutorials/upload-document/
        private async Task<(Item item, Version version)> UpdateVersion(ItemExternalDto item, string folder, string projectId, Item existingItem)
        {
            var fileName = Path.GetFileName(item.FullPath);
            var version = await CreateVersion(projectId, folder, item.FullPath, fileName);

            if (version == null)
                return default;

            version.Relationships.Item = existingItem.ToInfo().ToDataContainer();
            return await versionsService.PostVersionAsync(projectId, version);
        }

        // Replication for steps 5-7 from https://forge.autodesk.com/en/docs/bim360/v1/tutorials/upload-document/
        private async Task<(Item item, Version version)> PostItem(ItemExternalDto item, string folder, string projectId)
        {
            var fileName = Path.GetFileName(item.FullPath);
            var version = await CreateVersion(projectId, folder, item.FullPath, fileName);

            if (version == null)
                return default;

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
                    Parent = new ObjectInfo
                    {
                        ID = folder,
                        Type = FOLDER_TYPE,
                    }.ToDataContainer(),
                },
            };

            var addedItem = await itemsService.PostItemAsync(projectId, bimItem, version);
            if (addedItem.item == null || addedItem.version == null)
                return default;

            return addedItem;
        }

        private async Task<Version> CreateVersion(string projectId, string folder, string filePath, string fileName)
        {
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
                            id = folder,
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
            return version;
        }
    }
}
