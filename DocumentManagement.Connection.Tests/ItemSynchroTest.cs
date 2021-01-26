using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MRS.DocumentManagement;
using MRS.DocumentManagement.Connection.Synchronizator;
using MRS.DocumentManagement.Database.Models;
using MRS.DocumentManagement.Interface.Dtos;
using MRS.DocumentManagement.Tests.Utility;
using MRS.DocumentManagement.Utility;

namespace DocumentManagement.Connection.Tests
{

    [TestClass]
    public class ItemSynchroTest
    {
        private static SharedDatabaseFixture Fixture { get; set; }

        public RevisionCollection Revisions { get; private set; }

        private DiskTest disk;
        private static IMapper mapper;
        private static ItemSynchro sychro;

        public readonly int idProj = 1;

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
                var project = context.Projects.FirstOrDefault();
                var items = context.Items.ToList();
                foreach (var item in items)
                {
                    var link = new ProjectItem();
                    link.ItemID = item.ID;
                    link.ProjectID = project.ID;
                    project.Items.Add(link);
                }

                context.SaveChanges();
            });

            Revisions = new RevisionCollection();
            Revisions.GetProject(idProj).Rev = 5;

            disk = new DiskTest();
            sychro = new ItemSynchro(disk, Fixture.Context);
        }

        [TestCleanup]
        public void Cleanup() => Fixture.Dispose();

        [TestMethod]
        public void GetRevisionsTest()
        {
            Revisions.GetItem(1).Rev = 5;
            Revisions.GetItem(2).Rev = 5;
            Revisions.GetItem(3).Delete();

            var actual = sychro.GetRevisions(Revisions);

            var delRev = new Revision(3);
            delRev.Delete();
            var expected = new List<Revision>()
            {
                new Revision(1,5),
                new Revision(2,5),
                delRev,
            };

            AssertHelper.EqualList(expected, actual, AssertHelper.EqualRevision);
            Assert.IsFalse(disk.RunDelete);
            Assert.IsFalse(disk.RunPull);
            Assert.IsFalse(disk.RunPush);
        }

        [TestMethod]
        public void SetRevisionTest()
        {
            int id = 2;
            Revisions.GetItem(1).Rev = 5;
            Revisions.GetItem(2).Rev = 5;
            Revisions.GetItem(3).Delete();

            Revision expected = new Revision(id, 25);
            sychro.SetRevision(Revisions, expected);

            var actual = Revisions.GetItem(id);
            AssertHelper.EqualRevision(expected, actual);
            Assert.IsFalse(disk.RunDelete);
            Assert.IsFalse(disk.RunPull);
            Assert.IsFalse(disk.RunPush);
        }

        [TestMethod]
        public void SpecialSynchronizationTest()
        {
            SyncAction actual = new SyncAction();
            SyncAction expected = new SyncAction();
            actual.ID = expected.ID = 1;
            actual.Synchronizer = expected.Synchronizer = nameof(ItemSynchro);
            actual.TypeAction = expected.TypeAction = TypeSyncAction.None;

            actual = sychro.SpecialSynchronization(actual);
            expected.SpecialSynchronization = false;

            AssertHelper.EqualSyncAction(expected, actual);
            Assert.IsFalse(disk.RunDelete);
            Assert.IsFalse(disk.RunPull);
            Assert.IsFalse(disk.RunPush);
        }

        [TestMethod]
        public async Task GetSubSynchroListTest()
        {
            SyncAction action = new SyncAction();
            action.ID = 1;
            var sub = await sychro.GetSubSynchroList(action);
            Assert.IsNull(sub);

            Assert.IsFalse(disk.RunDelete);
            Assert.IsFalse(disk.RunPull);
            Assert.IsFalse(disk.RunPush);
        }

        [TestMethod]
        public async Task SpecialTest()
        {
            SyncAction action = new SyncAction();
            await Assert.ThrowsExceptionAsync<NotImplementedException>(async () =>
            {
                await sychro.Special(action);
            });
            Assert.IsFalse(disk.RunDelete);
            Assert.IsFalse(disk.RunPull);
            Assert.IsFalse(disk.RunPush);
        }

        [TestMethod]
        public async Task DeleteLocalTest()
        {
            int id = 1;
            SyncAction action = new SyncAction();
            action.ID = id;
            await sychro.DeleteLocal(action);

            Item item = Fixture.Context.Items.Find(id);
            Assert.IsNull(item);
            List<ProjectItem> projItems = Fixture.Context.ProjectItems.Where(x => x.ItemID == id).ToList();
            Assert.AreEqual(0, projItems.Count);

            Assert.IsFalse(disk.RunDelete);
            Assert.IsFalse(disk.RunPull);
            Assert.IsFalse(disk.RunPush);
        }

        [TestMethod]
        public async Task DeleteRemoteTest()
        {
            int id = 1;
            SyncAction action = new SyncAction();
            action.ID = id;
            await sychro.DeleteRemote(action);

            Assert.IsTrue(disk.RunDelete);
            Assert.IsFalse(disk.RunPull);
            Assert.IsFalse(disk.RunPush);
            Assert.AreEqual(id, disk.LastId);
        }

        [TestMethod]
        public async Task UploadTest()
        {
            // Assert.Fail("Что тут будет происходить пока не доконца понятно!");
            int id = 1;
            var project = Fixture.Context.Projects.Find(id);
            ProjectDto expected = mapper.Map<ProjectDto>(project);
            SyncAction action = new SyncAction();
            action.ID = id;

            await sychro.Upload(action);

            Assert.IsFalse(disk.RunDelete);
            Assert.IsFalse(disk.RunPull);
            Assert.IsTrue(disk.RunPush);
            Assert.AreEqual(id, disk.LastId);
            ProjectDto actual = disk.Project;
            AssertHelper.EqualDto(expected, actual);
        }

        [TestMethod]
        public async Task DownloadTestExist()
        {
            int id = 1;
            await DownloadTest(id);
        }

        private async Task DownloadTest(int id)
        {
            disk.Project = new ProjectDto()
            {
                ID = (ID<ProjectDto>)id,
                Title = "Замок кащея",
                Items = new List<ItemDto>(),
            };
            ProjectDto expected = disk.Project;

            SyncAction action = new SyncAction();
            action.ID = id;
            await sychro.Download(action);

            Assert.IsFalse(disk.RunDelete);
            Assert.IsTrue(disk.RunPull);
            Assert.IsFalse(disk.RunPush);
            Assert.AreEqual(id, disk.LastId);

            var project = Fixture.Context.Projects.Find(id);
            ProjectDto actual = mapper.Map<ProjectDto>(project);

            AssertHelper.EqualDto(expected, actual);
        }

        [TestMethod]
        public async Task DownloadTestNotExist()
        {
            int id = 3;
            await DownloadTest(id);
        }

    }
}
