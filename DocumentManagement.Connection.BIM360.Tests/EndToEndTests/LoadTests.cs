using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MRS.DocumentManagement.Connection.BIM360.Forge;
using MRS.DocumentManagement.Connection.BIM360.Forge.Models;
using MRS.DocumentManagement.Connection.BIM360.Forge.Models.DataManagement;
using MRS.DocumentManagement.Connection.BIM360.Forge.Services;
using MRS.DocumentManagement.Interface.Dtos;
using static MRS.DocumentManagement.Connection.BIM360.Forge.Constants;

namespace MRS.DocumentManagement.Connection.BIM360.Tests
{
    [TestClass]
    public class LoadTests
    {
        private static readonly string TEST_FILE_PATH = "AdditionalData//TestIcon.png";

        private AuthenticationService authService;
        private HubsService hubsService;
        private ProjectsService projectsService;
        private RemoteConnectionInfoDto connectionInfo;
        private ForgeConnection connection;
        private ObjectsService objectsService;
        private ItemsService itemsService;

        private string[] authData;

        [ClassInitialize]
        public void Initialize(AuthenticationService authService,
            HubsService hubsService,
            ProjectsService projectsService,
            ObjectsService objectsService,
            ItemsService itemsService,
            ForgeConnection connection)
        {
            authData = File.ReadAllLines("AdditionalData//AuthData.json");
            this.authService = authService;
            this.hubsService = hubsService;
            this.connection = connection;
            this.projectsService = projectsService;
            this.objectsService = objectsService;
            this.itemsService = itemsService;
            var authDataDictionary = new Dictionary<string, string>();
            foreach (var data in authData)
            {
                var parsedLine = data.Split(':', System.StringSplitOptions.TrimEntries);

                authDataDictionary.Add(parsedLine[0].Trim('\"'), parsedLine[1]);
            }

            connectionInfo = new RemoteConnectionInfoDto { AuthFieldValues = authDataDictionary };
        }

        /// <summary>
        /// Test based on https://forge.autodesk.com/en/docs/data/v2/tutorials/upload-file/ step-by-step tutorial
        /// </summary>
        [TestMethod]
        public async Task Can_upload_item()
        {
            // Authorize
            await authService.SignInAsync(connectionInfo);
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

            var version = new Version
            {
                Attributes = new Version.VersionAttributes
                {
                    Name = TEST_FILE_PATH,
                    Extension = new Extension { Type = AUTODESK_FILE_TYPE },
                },
                Relationships = new Version.VersionRelationships
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
    }
}
