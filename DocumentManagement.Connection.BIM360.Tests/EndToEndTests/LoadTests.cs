using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
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
        private static readonly string TEST_FILE_PATH = "AdditionalData//TestIcon.png";

        private static AuthenticationService authService;
        private static HubsService hubsService;
        private static ProjectsService projectsService;
        private static ConnectionInfoDto connectionInfo;
        private static ObjectsService objectsService;
        private static ItemsService itemsService;
        private static Authenticator authenticator;
        private static ForgeConnection connection;
        private static FoldersService foldersService;

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
        /// Test based on https://forge.autodesk.com/en/docs/data/v2/tutorials/upload-file/ step-by-step tutorial
        /// </summary>
        [TestMethod]
        public async Task Can_upload_item()
        {
            // Authorize
            var authorizationResult = await authenticator.SignInAsync(connectionInfo);
            if (authorizationResult.Status != RemoteConnectionStatusDto.OK)
                Assert.Fail();

            connection.Token = connectionInfo.AuthFieldValues[TOKEN_AUTH_NAME];

            // STEP 1. Find hub with projects
            var hub = (await hubsService.GetHubsAsync()).FirstOrDefault(h => h.Relationships.Projects.Count > 0);
            if (hub == default)
                Assert.Fail();

            // STEP 2. Choose first project
            // TODO decide which project should be used for end-to-end tests
            var project = (await projectsService.GetProjectsAsync(hub.ID)).FirstOrDefault();
            if (project == default)
                Assert.Fail();

            // STEP 3. Create storage
            // Choose folder
            var folder = (await projectsService.GetTopFoldersAsync(hub.ID, project.ID)).LastOrDefault();

            var objectToUpload = new StorageObject
            {
                Attributes = new StorageObject.StorageObjectAttributes { Name = TEST_FILE_PATH },
                Relationships = new StorageObject.StorageObjectRelationships
                {
                    Target = new
                    {
                        data = folder,
                    },
                },
            };

            var storage = await projectsService.CreateStorageAsync(project.ID, objectToUpload);
            if (storage == default)
                Assert.Fail();

            // STEP 4. Upload file to storage
            if (!storage.ID.Contains(':') || !storage.ID.Contains('/'))
                Assert.Fail();

            var parsedId = storage.ParseStorageId();
            var bucketKey = parsedId.bucketKey;
            var hashedName = parsedId.hashedName;

            await objectsService.PutObjectAsync(bucketKey, hashedName, TEST_FILE_PATH);

            // STEP 5. Create first version
            var item = new Item
            {
                Attributes = new Item.ItemAttributes
                {
                    DisplayName = TEST_FILE_PATH,
                    Extension = new Extension { Type = AUTODESK_FILE_TYPE },
                },
                Relationships = new Item.ItemRelationships
                {
                    Parent = folder,
                },
            };

            var version = new Forge.Models.DataManagement.Version
            {
                Attributes = new Forge.Models.DataManagement.Version.VersionAttributes
                {
                    Name = TEST_FILE_PATH,
                    Extension = new Extension { Type = AUTODESK_FILE_TYPE },
                },
                Relationships = new Forge.Models.DataManagement.Version.VersionRelationships
                {
                    Storage = new
                    {
                        data = storage,
                    },
                },
            };

            var addedItem = await itemsService.PostItemAsync(project.ID, item, version);

            if (addedItem.item == null || addedItem.version == null)
                Assert.Fail();
        }

        [TestMethod]
        public async Task CanDownloadRandomItem()
        {
            // Authorize
            var authorizationResult = await authenticator.SignInAsync(connectionInfo);
            if (authorizationResult.Status != RemoteConnectionStatusDto.OK)
                Assert.Fail();

            connection.Token = connectionInfo.AuthFieldValues["Token"];

            // STEP 1. Find hub with projects
            var hub = (await hubsService.GetHubsAsync()).FirstOrDefault(h => h.Relationships.Projects.Count > 0);
            if (hub == default)
                Assert.Fail();

            // STEP 2. Choose first project
            // TODO decide which project should be used for end-to-end tests
            var project = (await projectsService.GetProjectsAsync(hub.ID)).FirstOrDefault();
            if (project == default)
                Assert.Fail();

            // Step 3: Find the resource item in a project folder.
            var root = ((JToken)project.Relationships.RootFolder.data).ToObject<Folder>();

            if (root == null)
                Assert.Fail();

            var files = await foldersService.SearchAsync(project.ID,
                    root.ID,
                    new[] { (typeof(Item).GetDataMemberName(nameof(Item.Type)), ITEM_TYPE) });

            var file = files[random.Next(files.Count)];

            // Step 6: Download the item.
            var storage = ((JToken)file.version.Relationships.Storage.data).ToObject<StorageObject>();

            if (storage == null)
                Assert.Fail();

            (var bucketKey, var hashedName) = storage.ParseStorageId();
            var directory = Path.GetTempPath();
            var fileInfo = await objectsService.GetAsync(bucketKey, hashedName, directory);

            if (!fileInfo.Exists)
                Assert.Fail();
        }
    }
    }
}
