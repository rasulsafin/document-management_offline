using System.IO;
using System.Threading.Tasks;
using Brio.Docs.Connections.Bim360.Forge.Extensions;
using Brio.Docs.Connections.Bim360.Forge.Models;
using Brio.Docs.Connections.Bim360.Forge.Models.DataManagement;
using Brio.Docs.Connections.Bim360.Forge.Services;
using Brio.Docs.Connections.Bim360.Synchronization.Interfaces;
using Brio.Docs.Connections.Bim360.Utilities.Snapshot;
using Brio.Docs.Connections.Bim360.Utilities.Snapshot.Models;
using static Brio.Docs.Connections.Bim360.Forge.Constants;

namespace Brio.Docs.Connections.Bim360.Synchronization.Utilities
{
    internal class ItemsSyncHelper : IItemsUpdater
    {
        private readonly ItemsService itemsService;
        private readonly ProjectsService projectsService;
        private readonly ObjectsService objectsService;
        private readonly VersionsService versionsService;
        private readonly SnapshotGetter snapshotGetter;
        private readonly SnapshotUpdater snapshotUpdater;

        public ItemsSyncHelper(
            ItemsService itemsService,
            ProjectsService projectsService,
            ObjectsService objectsService,
            VersionsService versionsService,
            SnapshotGetter snapshotGetter,
            SnapshotUpdater snapshotUpdater)
        {
            this.itemsService = itemsService;
            this.projectsService = projectsService;
            this.objectsService = objectsService;
            this.versionsService = versionsService;
            this.snapshotGetter = snapshotGetter;
            this.snapshotUpdater = snapshotUpdater;
        }

        public async Task<ItemSnapshot> PostItem(ProjectSnapshot project, string fullPath)
        {
            var posted = await PostItem(fullPath, project.MrsFolderID, project.ID);
            var itemSnapshot = snapshotUpdater.CreateItem(project, posted.item, posted.version);
            return itemSnapshot;
        }

        public async Task<ItemSnapshot> UpdateVersion(ProjectSnapshot project, string itemID, string fullPath)
        {
            var snapshot = snapshotGetter.GetItem(project, itemID);
            var posted = await UpdateVersion(fullPath, project.MrsFolderID, project.ID, snapshot.Entity);
            snapshotUpdater.UpdateItem(project, snapshot, posted.item, posted.version);
            return snapshot;
        }

        public async Task Remove(ProjectSnapshot project, string itemID)
        {
            var itemSnapshot = snapshotGetter.GetItem(project, itemID);

            // Delete uploaded item by marking version as "deleted"
            var deletedVersion = new Version
            {
                Attributes = new Version.VersionAttributes
                {
                    Name = itemSnapshot.Entity.Attributes.DisplayName,
                    Extension = new Extension { Type = AUTODESK_VERSION_DELETED_TYPE },
                },
                Relationships = new Version.VersionRelationships
                {
                    Item = new ObjectInfo
                    {
                        ID = itemID,
                        Type = ITEM_TYPE,
                    }.ToDataContainer(),
                },
            };

            await versionsService.PostVersionAsync(project.ID, deletedVersion);
            snapshotUpdater.RemoveItem(project, itemID);
        }

        // Replication for steps 5, 6, 8 from https://forge.autodesk.com/en/docs/bim360/v1/tutorials/upload-document/
        private async Task<(Item item, Forge.Models.DataManagement.Version version)> UpdateVersion(string fullPath, string folder, string projectId, Item existingItem)
        {
            var fileName = Path.GetFileName(fullPath);
            var version = await CreateVersion(projectId, folder, fullPath, fileName);

            if (version == null)
                return default;

            version.Relationships.Item = existingItem.ToInfo().ToDataContainer();
            return await versionsService.PostVersionAsync(projectId, version);
        }

        // Replication for steps 5-7 from https://forge.autodesk.com/en/docs/bim360/v1/tutorials/upload-document/
        private async Task<(Item item, Version version)> PostItem(string fullPath, string folder, string projectId)
        {
            var fileName = Path.GetFileName(fullPath);
            var version = await CreateVersion(projectId, folder, fullPath, fileName);

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

        private async Task<Forge.Models.DataManagement.Version> CreateVersion(string projectId, string folder, string filePath, string fileName)
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
