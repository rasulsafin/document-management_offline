using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MRS.DocumentManagement.Connection.Bim360.Forge;
using MRS.DocumentManagement.Connection.Bim360.Forge.Models;
using MRS.DocumentManagement.Connection.Bim360.Forge.Models.DataManagement;
using MRS.DocumentManagement.Connection.Bim360.Forge.Services;
using MRS.DocumentManagement.Connection.Bim360.Forge.Utils;
using MRS.DocumentManagement.Connection.Bim360.Forge.Utils.Extensions;
using MRS.DocumentManagement.Interface.Dtos;
using static MRS.DocumentManagement.Connection.Bim360.Forge.Constants;
using Version = MRS.DocumentManagement.Connection.Bim360.Forge.Models.DataManagement.Version;

namespace MRS.DocumentManagement.Connection.Bim360.Tests
{
    [TestClass]
    public class LoadTests
    {
        private static readonly string TEST_FILE_PATH = "Resources/IntegrationTestFile.txt";
        private static readonly string TEST_PROJECT_NAME = "Sample Project";

        private static AuthenticationService authService;
        private static HubsService hubsService;
        private static ProjectsService projectsService;
        private static ConnectionInfoDto connectionInfo;
        private static ObjectsService objectsService;
        private static ItemsService itemsService;
        private static FoldersService foldersService;
        private static VersionsService versionsService;
        private static Authenticator authenticator;
        private static ForgeConnection connection;

        private readonly Random random = new Random();
        private readonly string testFileName = Path.GetFileName(TEST_FILE_PATH);

        [ClassInitialize]
        public static void Initialize(TestContext unused)
        {
            connection = new ForgeConnection();
            authService = new AuthenticationService(connection);
            authenticator = new Authenticator(authService);
            hubsService = new HubsService(connection);
            projectsService = new ProjectsService(connection);
            objectsService = new ObjectsService(connection);
            itemsService = new ItemsService(connection);
            foldersService = new FoldersService(connection);
            versionsService = new VersionsService(connection);

            connectionInfo = new ConnectionInfoDto
            {
                ID = new ID<ConnectionInfoDto>(2),
                ConnectionType = new ConnectionTypeDto
                {
                    AppProperties = new Dictionary<string, string>
                    {
                        { "CLIENT_ID", "m5fLEAiDRlW3G7vgnkGGGcg4AABM7hCf" },
                        { "CLIENT_SECRET", "dEGEHfbl9LWmEnd7" },
                        { "RETURN_URL", "http://localhost:8000/oauth/" },
                    },
                    AuthFieldNames = new List<string>
                    {
                        "token",
                        "refreshtoken",
                        "end",
                    },
                    ID = new ID<ConnectionTypeDto>(2),
                    Name = "BIM360",
                },
            };
        }

        /// <summary>
        /// Test based on https://forge.autodesk.com/en/docs/bim360/v1/tutorials/upload-document/ step-by-step tutorial.
        /// </summary>
        [TestMethod]
        public async Task UploadAndDeleteFile_Bim360IsWorkingForgeConnected_Success()
        {
            // Authorize
            var authorizationResult = (await authenticator.SignInAsync(connectionInfo)).authStatus;
            if (authorizationResult.Status != RemoteConnectionStatus.OK)
                Assert.Fail("Authorization failed");

            connection.Token = connectionInfo.AuthFieldValues[TOKEN_AUTH_NAME];

            // STEP 1. Find hub with projects
            var hub = (await hubsService.GetHubsAsync()).FirstOrDefault();
            if (hub == default)
                Assert.Fail("Hubs are empty");

            // STEP 2. Choose first project
            // TODO decide which project should be used for end-to-end tests
            var project = (await projectsService.GetProjectsAsync(hub.ID)).FirstOrDefault(p => p.Attributes.Name == TEST_PROJECT_NAME);
            if (project == default)
                Assert.Fail("Projects in hubs are empty");

            // STEP 3. Find the Folder ID
            var topFolder = (await projectsService.GetTopFoldersAsync(hub.ID, project.ID)).LastOrDefault();
            if (topFolder == default)
                Assert.Fail("Top folders in project are empty");

            // STEP 4. Find the nested Folder ID
            var folder = (await foldersService.GetFoldersAsync(project.ID, topFolder.ID)).FirstOrDefault();
            if (folder == default)
                Assert.Fail("Top folder is empty");

            // STEP 5. Create a Storage Object.
            var objectToUpload = new StorageObject
            {
                Attributes = new StorageObject.StorageObjectAttributes
                {
                    Name = testFileName,
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

            var storage = await projectsService.CreateStorageAsync(project.ID, objectToUpload);
            if (storage == default)
                Assert.Fail("Storage creating failed");

            // STEP 6. Upload file to storage
            if (!storage.ID.Contains(':') || !storage.ID.Contains('/'))
                Assert.Fail("Storage ID has incorrect format");

            var parsedId = storage.ParseStorageId();
            var bucketKey = parsedId.bucketKey;
            var hashedName = parsedId.hashedName;

            await objectsService.PutObjectAsync(bucketKey, hashedName, TEST_FILE_PATH);

            // STEP 7. Create first version
            var version = new Version
            {
                Attributes = new Version.VersionAttributes
                {
                    Name = testFileName,
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

            var item = new Item
            {
                Attributes = new Item.ItemAttributes
                {
                    DisplayName = testFileName,
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

            var addedItem = await itemsService.PostItemAsync(project.ID, item, version);

            if (addedItem.item == null || addedItem.version == null)
                Assert.Fail("Adding item failed");

            // Delete uploaded item by marking version as "deleted"
            var deletedVersion = new Version
            {
                Attributes = new Version.VersionAttributes
                {
                    Name = testFileName,
                    Extension = new Extension { Type = AUTODESK_VERSION_DELETED_TYPE },
                },
                Relationships = new Version.VersionRelationships
                {
                    Item = addedItem.item.ToInfo().ToDataContainer(),
                },
            };

            var deleteResult = await versionsService.PostVersionAsync(project.ID, deletedVersion);
            if (deleteResult.item == null || deleteResult.version == null)
                Assert.Fail("Deleting item failed");
        }

        [TestMethod]
        public async Task DownloadRandomItem_Bim360IsWorkingForgeConnected_Success()
        {
            // Authorize
            var authorizationResult = (await authenticator.SignInAsync(connectionInfo)).authStatus;
            if (authorizationResult.Status != RemoteConnectionStatus.OK)
                Assert.Fail("Authorization failed");

            connection.Token = connectionInfo.AuthFieldValues[TOKEN_AUTH_NAME];

            // STEP 1. Find hub with projects
            var hub = (await hubsService.GetHubsAsync()).FirstOrDefault();
            if (hub == default)
                Assert.Fail("Hubs are empty");

            // STEP 2. Choose project
            var project = (await projectsService.GetProjectsAsync(hub.ID))
                    .FirstOrDefault(x => x.Attributes.Name == TEST_PROJECT_NAME);
            if (project == default)
                Assert.Fail("Testing project doesn't exist");

            // Step 3: Find the resource item in a project.
            var root = project.Relationships.RootFolder.Data;
            if (root == null)
                Assert.Fail("Can't take root folder");
            var files = await foldersService.SearchAsync(project.ID,
                    root.ID);
            if (files == null || files.Count == 0)
                Assert.Fail("Files are empty");
            var file = files[random.Next(files.Count)];

            // Step 4: Download the item.
            var storage = file.version.Relationships.Storage?.Data
                    .ToObject<StorageObject,
                            StorageObject.StorageObjectAttributes,
                            StorageObject.StorageObjectRelationships>();
            if (storage == null)
                Assert.Fail("Can't take storage of file");
            var (bucketKey, hashedName) = storage.ParseStorageId();
            var fileInfo = await objectsService.GetAsync(bucketKey, hashedName, file.item.Attributes.DisplayName);
            if (!fileInfo.Exists)
                Assert.Fail("File doesn't exist");
            fileInfo.Delete();
        }

        /// <summary>
        /// Test based on https://forge.autodesk.com/en/docs/bim360/v1/tutorials/upload-document/ step-by-step tutorial.
        /// </summary>
        [TestMethod]
        public async Task UploadFileUpdateVersionDeleteFile_Bim360IsWorkingForgeConnected_Success()
        {
            // Authorize
            var authorizationResult = (await authenticator.SignInAsync(connectionInfo)).authStatus;
            if (authorizationResult.Status != RemoteConnectionStatus.OK)
                Assert.Fail("Authorization failed");

            connection.Token = connectionInfo.AuthFieldValues[TOKEN_AUTH_NAME];

            // STEP 1. Find hub with projects
            var hub = (await hubsService.GetHubsAsync()).FirstOrDefault();
            if (hub == default)
                Assert.Fail("Hubs are empty");

            // STEP 2. Choose first project
            // TODO decide which project should be used for end-to-end tests
            var project = (await projectsService.GetProjectsAsync(hub.ID)).FirstOrDefault(p => p.Attributes.Name == TEST_PROJECT_NAME);
            if (project == default)
                Assert.Fail("Projects in hubs are empty");

            // STEP 3. Find the Folder ID
            var topFolder = (await projectsService.GetTopFoldersAsync(hub.ID, project.ID)).LastOrDefault();
            if (topFolder == default)
                Assert.Fail("Top folders in project are empty");

            // STEP 4. Find the nested Folder ID
            var folder = (await foldersService.GetFoldersAsync(project.ID, topFolder.ID)).FirstOrDefault();
            if (folder == default)
                Assert.Fail("Top folder is empty");

            // STEP 5. Create a Storage Object
            var objectToUpload = new StorageObject
            {
                Attributes = new StorageObject.StorageObjectAttributes
                {
                    Name = testFileName,
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

            var firstStorage = await projectsService.CreateStorageAsync(project.ID, objectToUpload);
            if (firstStorage == default)
                Assert.Fail("Storage creating failed");

            // STEP 6. Upload file to storage
            if (!firstStorage.ID.Contains(':') || !firstStorage.ID.Contains('/'))
                Assert.Fail("Storage ID has incorrect format");

            var parsedId = firstStorage.ParseStorageId();
            var bucketKey = parsedId.bucketKey;
            var hashedName = parsedId.hashedName;

            await objectsService.PutObjectAsync(bucketKey, hashedName, TEST_FILE_PATH);

            // STEP 7. Create first version
            var version = new Version
            {
                Attributes = new Version.VersionAttributes
                {
                    Name = testFileName,
                    Extension = new Extension
                    {
                        Type = AUTODESK_VERSION_FILE_TYPE,
                    },
                },
                Relationships = new Version.VersionRelationships
                {
                    Storage = firstStorage.ToInfo().ToDataContainer(),
                },
            };

            var item = new Item
            {
                Attributes = new Item.ItemAttributes
                {
                    DisplayName = testFileName,
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

            var addedItem = await itemsService.PostItemAsync(project.ID, item, version);

            if (addedItem.item == null || addedItem.version == null)
                Assert.Fail("Adding item failed");

            // To update version for uploaded item there is need to repeat step 5 and 6 and then call updating version.
            // REPEAT STEP 5: Creating new storage for the same item
            var secondStorage = await projectsService.CreateStorageAsync(project.ID, objectToUpload);
            if (secondStorage == default)
                Assert.Fail("Storage creating failed");

            // REPEAT STEP 6: Upload the item
            if (!secondStorage.ID.Contains(':') || !secondStorage.ID.Contains('/'))
                Assert.Fail("Storage ID has incorrect format");

            var secondStorageParsedId = secondStorage.ParseStorageId();
            var secondStorageBucketKey = secondStorageParsedId.bucketKey;
            var secondStorageHashedName = secondStorageParsedId.hashedName;

            await objectsService.PutObjectAsync(secondStorageBucketKey, secondStorageHashedName, TEST_FILE_PATH);

            // STEP 8: Update version
            var updatedVersion = new Version
            {
                Attributes = new Version.VersionAttributes
                {
                    Name = testFileName,
                    Extension = new Extension { Type = AUTODESK_VERSION_FILE_TYPE },
                },
                Relationships = new Version.VersionRelationships
                {
                    Item = addedItem.item.ToInfo().ToDataContainer(),
                    Storage = secondStorage.ToInfo().ToDataContainer(),
                },
            };

            var updateResult = await versionsService.PostVersionAsync(project.ID, updatedVersion);
            if (updateResult.item == null || updateResult.version == null)
                Assert.Fail("Updating item failed");

            // Delete uploaded item by marking version as "deleted"
            var deletedVersion = new Version
            {
                Attributes = new Version.VersionAttributes
                {
                    Name = testFileName,
                    Extension = new Extension { Type = AUTODESK_VERSION_DELETED_TYPE },
                },
                Relationships = new Version.VersionRelationships
                {
                    Item = addedItem.item.ToInfo().ToDataContainer(),
                },
            };

            var deleteResult = await versionsService.PostVersionAsync(project.ID, deletedVersion);
            if (deleteResult.item == null || deleteResult.version == null)
                Assert.Fail("Deleting item failed");
        }
    }
}
