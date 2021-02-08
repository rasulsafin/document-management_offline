using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MRS.DocumentManagement;
using MRS.DocumentManagement.Connection.Synchronizer;
using MRS.DocumentManagement.Database.Models;
using MRS.DocumentManagement.Interface.Dtos;
using MRS.DocumentManagement.Interface.SyncData;
using MRS.DocumentManagement.Tests.Utility;
using MRS.DocumentManagement.Utility;

namespace DocumentManagement.Connection.Tests
{
    [TestClass]
    public class ItemSynchroTest
    {
        public readonly int idProj = 1;
        private static IMapper mapper;
        private static ItemSynchro sychro;
        private DiskMock disk;

        public RevisionCollection Revisions { get; private set; }

        private static SharedDatabaseFixture Fixture { get; set; }

        [ClassInitialize]
        public static void ClassSetup(TestContext _)
        {
            var mapperConfig = new MapperConfiguration(mc =>
            {
                mc.AddProfile(new MappingProfile());
            });
            mapper = mapperConfig.CreateMapper();
        }

        [TestInitialize]
        public void Setup()
        {
            Fixture = new SharedDatabaseFixture(context =>
            {
                context.Database.EnsureDeleted();
                context.Database.EnsureCreated();

                context.Projects.AddRange(MockData.DEFAULT_PROJECTS);
                context.Items.AddRange(MockData.DEFAULT_ITEMS);
                context.SaveChanges();
            });

            Revisions = new RevisionCollection();
            Revisions.GetRevision(NameTypeRevision.Projects, idProj).Rev = 5;

            disk = new DiskMock();
            sychro = new ItemSynchro(disk, Fixture.Context);
        }

        [TestCleanup]
        public void Cleanup() => Fixture.Dispose();

        [TestMethod]
        public void GetRevisions_Empty_EmptyCollection()
        {
            var expected = new List<Revision>();

            var actual = sychro.GetRevisions(Revisions);

            AssertHelper.EqualList(expected, actual, AssertHelper.EqualRevision);
            Assert.IsFalse(disk.RunDelete);
            Assert.IsFalse(disk.RunPull);
            Assert.IsFalse(disk.RunPush);
        }

        [TestMethod]
        public void GetRevisions_NotEmpty_NotEmptyCollection()
        {
            Revisions.GetRevision(NameTypeRevision.Items, 1).Rev = 5;
            Revisions.GetRevision(NameTypeRevision.Items, 2).Rev = 5;
            Revisions.GetRevision(NameTypeRevision.Items, 3).Delete();
            var delRev = new Revision(3);
            delRev.Delete();
            var expected = new List<Revision>()
            {
                new Revision(1, 5),
                new Revision(2, 5),
                delRev,
            };

            var actual = sychro.GetRevisions(Revisions);

            AssertHelper.EqualList(expected, actual, AssertHelper.EqualRevision);
            Assert.IsFalse(disk.RunDelete);
            Assert.IsFalse(disk.RunPull);
            Assert.IsFalse(disk.RunPush);
        }

        [TestMethod]
        public void SetRevision_NotExistRevision_CorrectAdd()
        {
            int id = 2;
            Revisions.GetRevision(NameTypeRevision.Items, 1).Rev = 5;
            Revisions.GetRevision(NameTypeRevision.Items, 3).Delete();
            Revision expected = new Revision(id, 25);

            sychro.SetRevision(Revisions, expected);

            var actual = Revisions.GetRevision(NameTypeRevision.Items, id);
            AssertHelper.EqualRevision(expected, actual);
            Assert.IsFalse(disk.RunDelete);
            Assert.IsFalse(disk.RunPull);
            Assert.IsFalse(disk.RunPush);
        }

        [TestMethod]
        public void SetRevision_ExistRevision_CorrectRewrite()
        {
            int id = 2;
            Revisions.GetRevision(NameTypeRevision.Items, 1).Rev = 5;
            Revisions.GetRevision(NameTypeRevision.Items, 2).Rev = 5;
            Revisions.GetRevision(NameTypeRevision.Items, 3).Delete();
            Revision expected = new Revision(id, 25);

            sychro.SetRevision(Revisions, expected);

            var actual = Revisions.GetRevision(NameTypeRevision.Items, id);
            AssertHelper.EqualRevision(expected, actual);
            Assert.IsFalse(disk.RunDelete);
            Assert.IsFalse(disk.RunPull);
            Assert.IsFalse(disk.RunPush);
        }

        [TestMethod]
        public async Task DeleteLocal_NeedDelete_DeletingInDatebase()
        {
            int id = 1;
            SyncAction action = new SyncAction();
            action.ID = id;
            await sychro.DeleteLocal(action);
            Fixture.Context.SaveChanges();

            Item item = Fixture.Context.Items.Find(id);
            Assert.IsNull(item);
            List<ProjectItem> projItems = Fixture.Context.ProjectItems.Where(x => x.ItemID == id).ToList();
            Assert.AreEqual(0, projItems.Count);

            Assert.IsFalse(disk.RunDelete);
            Assert.IsFalse(disk.RunPull);
            Assert.IsFalse(disk.RunPush);
            Assert.IsTrue(action.IsComplete);
        }

        [TestMethod]
        public async Task DeleteRemote_NeedDelete_DeletingMethodCall()
        {
            int id = 1;
            var item = Fixture.Context.Items.Find(id);
            disk.Item = mapper.Map<ItemDto>(item);
            SyncAction action = new SyncAction();
            action.ID = id;
            await sychro.DeleteRemote(action);

            Assert.IsTrue(disk.RunDelete);
            Assert.IsTrue(disk.RunPull);
            Assert.IsFalse(disk.RunPush);
            Assert.IsTrue(action.IsComplete);

            Assert.AreEqual(id, disk.LastId);
        }

        [TestMethod]
        public async Task Upload_NeedUnload_UploadMethodCall()
        {
            int id = 1;
            var item = Fixture.Context.Items.Find(id);
            ItemDto expected = mapper.Map<ItemDto>(item);
            SyncAction action = new SyncAction();
            action.ID = id;

            await sychro.Upload(action);

            Assert.IsFalse(disk.RunDelete);
            Assert.IsFalse(disk.RunPull);
            Assert.IsTrue(disk.RunPush);
            Assert.IsTrue(action.IsComplete);

            Assert.AreEqual(id, disk.LastId);
            ItemDto actual = disk.Item;
            AssertHelper.EqualDto(expected, actual);
        }

        [TestMethod]
        public void CheckDBRevision_EmptyRevisionCollection_AddingNonIncludedRecords()
        {
            RevisionCollection actual = new RevisionCollection();
            RevisionCollection expected = new RevisionCollection();
            expected.GetRevision(NameTypeRevision.Items, 1);
            expected.GetRevision(NameTypeRevision.Items, 2);
            expected.GetRevision(NameTypeRevision.Items, 3);

            sychro.CheckDBRevision(actual);

            Assert.IsFalse(disk.RunDelete);
            Assert.IsFalse(disk.RunPull);
            Assert.IsFalse(disk.RunPush);

            AssertHelper.EqualRevisionCollection(expected, actual);
        }

        [TestMethod]
        public void CheckDBRevision_NotEmptyRevisionCollection_AddingNonIncludedRecords()
        {
            RevisionCollection actual = new RevisionCollection();
            actual.GetRevision(NameTypeRevision.Items, 1);
            actual.GetRevision(NameTypeRevision.Items, 2);
            actual.GetRevision(NameTypeRevision.Items, 3);
            RevisionCollection expected = new RevisionCollection();
            expected.GetRevision(NameTypeRevision.Items, 1);
            expected.GetRevision(NameTypeRevision.Items, 2);
            expected.GetRevision(NameTypeRevision.Items, 3);

            sychro.CheckDBRevision(actual);

            Assert.IsFalse(disk.RunDelete);
            Assert.IsFalse(disk.RunPull);
            Assert.IsFalse(disk.RunPush);
            AssertHelper.EqualRevisionCollection(expected, actual);
        }

        [TestMethod]
        public async Task Download_ExistRec_Rewrite()
        {
            int id = 1;
            await DownloadTest(id);
        }

        [TestMethod]
        public async Task Download_NotExistRec_AddRecord()
        {
            int id = 5;
            await DownloadTest(id);
        }

        private async Task DownloadTest(int id)
        {
            disk.Item = new ItemDto()
            {
                ID = (ID<ItemDto>)id,
                Name = "Замок кащея",
                ExternalItemId = "Замок кащея",
                ItemType = ItemTypeDto.File,
            };
            ItemDto expected = disk.Item;

            SyncAction action = new SyncAction();
            action.ID = id;
            await sychro.Download(action);

            Assert.IsFalse(disk.RunDelete);
            Assert.IsTrue(disk.RunPull);
            Assert.IsFalse(disk.RunPush);
            Assert.IsTrue(action.IsComplete);

            Assert.AreEqual(id, disk.LastId);

            var item = Fixture.Context.Items.Find(id);
            ItemDto actual = mapper.Map<ItemDto>(item);

            AssertHelper.EqualDto(expected, actual);
        }
    }
}
