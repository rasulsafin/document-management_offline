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
using Newtonsoft.Json.Linq;
using static MRS.DocumentManagement.Connection.Bim360.Forge.Constants;
using Version = MRS.DocumentManagement.Connection.Bim360.Forge.Models.DataManagement.Version;

namespace MRS.DocumentManagement.Connection.Bim360.Tests
{
    [TestClass]
    public class LoadTests
    {
        private static readonly string TEST_FILE_PATH = "My Test Folder/124.txt";

        private static AuthenticationService authService;
        private static HubsService hubsService;
        private static ProjectsService projectsService;
        private static ConnectionInfoDto connectionInfo;
        private static ObjectsService objectsService;
        private static ItemsService itemsService;
        private static FoldersService foldersService;
        private static Authenticator authenticator;
        private static ForgeConnection connection;

        private readonly Random random = new Random();

        [ClassInitialize]
        public static void Initialize(TestContext _)
        {
            connection = new ForgeConnection();
            authService = new AuthenticationService(connection);
            authenticator = new Authenticator(authService);
            hubsService = new HubsService(connection);
            projectsService = new ProjectsService(connection);
            objectsService = new ObjectsService(connection);
            itemsService = new ItemsService(connection);
            foldersService = new FoldersService(connection);

            connectionInfo = new ConnectionInfoDto
            {
                ID = new ID<ConnectionInfoDto>(2),
                ConnectionType = new ConnectionTypeDto
                {
                    AppProperty = new Dictionary<string, string>
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
        /// Test based on https://forge.autodesk.com/en/docs/bim360/v1/tutorials/upload-document/ step-by-step tutorial
        /// </summary>
        [TestMethod]
        public async Task Can_upload_item()
        {
            // Authorize
            var authorizationResult = await authenticator.SignInAsync(connectionInfo);
            if (authorizationResult.Status != RemoteConnectionStatusDto.OK)
                Assert.Fail("Authorization failed");

            connection.Token = connectionInfo.AuthFieldValues[TOKEN_AUTH_NAME];

            // STEP 1. Find hub with projects
            var hub = (await hubsService.GetHubsAsync()).FirstOrDefault();
            if (hub == default)
                Assert.Fail("Hubs are empty");

            // STEP 2. Choose first project
            // TODO decide which project should be used for end-to-end tests
            var project = (await projectsService.GetProjectsAsync(hub.ID)).FirstOrDefault();
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
                    Name = TEST_FILE_PATH.Split('/').Last(),
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

            var version = new Forge.Models.DataManagement.Version
            {
                ID = "1",
                Attributes = new Forge.Models.DataManagement.Version.VersionAttributes
                {
                    Name = TEST_FILE_PATH.Split('/').Last(),
                    Extension = new
                    {
                        type = AUTODESK_VERSION_FILE_TYPE,
                        version = "1.0",
                    },
                },
                Relationships = new Forge.Models.DataManagement.Version.VersionRelationships
                {
                    Storage = new
                    {
                        data = new
                        {
                            type = OBJECT_TYPE,
                            id = storage.ID,
                        },
                    },
                },
            };

            var item = new Item
            {
                Attributes = new Item.ItemAttributes
                {
                    DisplayName = TEST_FILE_PATH.Split('/').Last(),
                    Extension = new
                    {
                        type = AUTODESK_ITEM_FILE_TYPE,
                        version = "1.0",
                    },
                },
                Relationships = new Item.ItemRelationships
                {
                    Tip = new
                    {
                        data = new
                        {
                            type = VERSION_TYPE,
                            id = "1",
                        },
                    },
                    Parent = new
                    {
                        data = new
                        {
                            type = FOLDER_TYPE,
                            id = folder.ID,
                        },
                    },
                },
            };

            var addedItem = await itemsService.PostItemAsync(project.ID, item, version);

            if (addedItem.item == null || addedItem.version == null)
                Assert.Fail("Adding item failed");
        }

        [TestMethod]
        public async Task CanDownloadRandomItem()
        {
            // Authorize
            var authorizationResult = await authenticator.SignInAsync(connectionInfo);
            if (authorizationResult.Status != RemoteConnectionStatusDto.OK)
                Assert.Fail("Authorization failed");

            connection.Token = connectionInfo.AuthFieldValues[TOKEN_AUTH_NAME];

            // STEP 1. Find hub with projects
            var hub = (await hubsService.GetHubsAsync()).FirstOrDefault();
            if (hub == default)
                Assert.Fail("Hubs are empty");

            // STEP 2. Choose first project
            // TODO decide which project should be used for end-to-end tests
            var project = (await projectsService.GetProjectsAsync(hub.ID)).FirstOrDefault();
            if (project == default)
                Assert.Fail();

            // STEP 3. Find the Folder ID
            var topFolders = await projectsService.GetTopFoldersAsync(hub.ID, project.ID);
            if (topFolders == default)
                Assert.Fail("Top folders in project are empty");

            // Step 3: Find the resource item in a project folder.
            var root = ((JToken)project.Relationships.RootFolder.data).ToObject<Folder>();

            if (root == null)
                Assert.Fail();

            List<(Version version, Item item)> files = null;

            foreach (var folder in topFolders)
            {
                files = await foldersService.SearchAsync(project.ID,
                        folder.ID,
                        Array.Empty<(string filteringField, string filteringValue)>());

                        // new[] { (typeof(Item).GetDataMemberName(nameof(Item.Type)), ITEM_TYPE) });
                if (files != null && files.Count > 0)
                    break;
            }

            if (files == null || files.Count == 0)
                Assert.Fail();

            var file = files[random.Next(files.Count)];

            if (root == null)
                Assert.Fail();

            // Step 6: Download the item.
            var storage = ((JToken)file.version.Relationships.Storage.data).ToObject<StorageObject>();

            if (storage == null)
                Assert.Fail();

            (var bucketKey, var hashedName) = storage.ParseStorageId();
            var fileInfo = await objectsService.GetAsync(bucketKey, hashedName, file.item.Attributes.DisplayName);

            if (!fileInfo.Exists)
                Assert.Fail();
        }
    }
}
