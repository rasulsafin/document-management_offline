using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MRS.DocumentManagement;
using MRS.DocumentManagement.Connection.Synchronizator;
using MRS.DocumentManagement.Database.Models;
using MRS.DocumentManagement.Interface.Dtos;
using MRS.DocumentManagement.Interface.Services;
using MRS.DocumentManagement.Tests.Utility;
using MRS.DocumentManagement.Utility;

namespace DocumentManagement.Connection.Tests
{
    [TestClass]
    public class ProjectSynchroTest
    {
        private static IMapper mapper;
        private static ProjectSynchro sychro;
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
                context.SaveChanges();
            });

            Revisions = new RevisionCollection();

            disk = new DiskMock();
            sychro = new ProjectSynchro(disk, Fixture.Context, mapper);
        }

        [TestCleanup]
        public void Cleanup() => Fixture.Dispose();

        [TestMethod]
        public void CheckDBRevision_ProjectSynchro_AddingNonIncludedRecordsToRevisionCollection()
        {
            RevisionCollection actual = new RevisionCollection();
            sychro.CheckDBRevision(actual);

            RevisionCollection expected = new RevisionCollection();
            expected.GetRevision(TableRevision.Projects, 1);
            expected.GetRevision(TableRevision.Projects, 2);

            Assert.IsFalse(disk.RunDelete);
            Assert.IsFalse(disk.RunPull);
            Assert.IsFalse(disk.RunPush);
            AssertHelper.EqualRevisionCollection(expected, actual);
        }

        [TestMethod]
        public void GetRevisions_ProjectSynchro_RemovingACollectionOfRevisionsFromAComplexVariable()
        {
            Revisions.GetRevision(TableRevision.Projects, 1).Rev = 5;
            Revisions.GetRevision(TableRevision.Projects, 2).Rev = 5;
            Revisions.GetRevision(TableRevision.Projects, 3).Delete();

            var actual = sychro.GetRevisions(Revisions);

            var delRev = new Revision(3);
            delRev.Delete();
            var expected = new List<Revision>()
            {
                new Revision(1, 5),
                new Revision(2, 5),
                delRev,
            };

            AssertHelper.EqualList(expected, actual, AssertHelper.EqualRevision);
            Assert.IsFalse(disk.RunDelete);
            Assert.IsFalse(disk.RunPull);
            Assert.IsFalse(disk.RunPush);
        }

        [TestMethod]
        public void SetRevision_ProjectSynchro_SettingAnEntryToAComplexVariable()
        {
            Revisions.GetRevision(TableRevision.Projects, 1).Rev = 5;
            Revisions.GetRevision(TableRevision.Projects, 2).Rev = 5;
            Revisions.GetRevision(TableRevision.Projects, 3).Delete();

            Revision expected = new Revision(2, 25);
            sychro.SetRevision(Revisions, expected);

            var actual = Revisions.GetRevision(TableRevision.Projects, 2);
            AssertHelper.EqualRevision(expected, actual);
            Assert.IsFalse(disk.RunDelete);
            Assert.IsFalse(disk.RunPull);
            Assert.IsFalse(disk.RunPush);
        }

        [TestMethod]
        public void SpecialSynchronization_ProjectSynchro_NoChanges()
        {
            SyncAction actual = new SyncAction();
            SyncAction expected = new SyncAction();
            actual.ID = expected.ID = 1;
            actual.Synchronizer = expected.Synchronizer = nameof(UserSynchro);
            actual.TypeAction = expected.TypeAction = SyncActionType.None;

            actual = sychro.SpecialSynchronization(actual);
            expected.SpecialSynchronization = false;

            AssertHelper.EqualSyncAction(expected, actual);
            Assert.IsFalse(disk.RunDelete);
            Assert.IsFalse(disk.RunPull);
            Assert.IsFalse(disk.RunPush);
        }

        [TestMethod]
        public async Task GetSubSynchroList_ProjectSynchro_Null()
        {
            int id = 1;
            SyncAction action = new SyncAction();
            action.ID = id;

            List<ISynchroTable> actual = await sychro.GetSubSynchroList(action);
            Assert.IsNull(actual);

            Assert.IsFalse(disk.RunDelete);
            Assert.IsFalse(disk.RunPull);
            Assert.IsFalse(disk.RunPush);
        }

        [TestMethod]
        public async Task Special_ProjectSynchro_Exeption()
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
        public async Task DeleteLocal_ProjectSynchro_DeletingEntryFromLocalCollection()
        {
            int id = 1;
            SyncAction action = new SyncAction();
            action.ID = id;
            await sychro.DeleteLocal(action);

            var user = Fixture.Context.Users.Find(id);
            Assert.IsNull(user);

            Assert.IsFalse(disk.RunDelete);
            Assert.IsFalse(disk.RunPull);
            Assert.IsFalse(disk.RunPush);
            Assert.IsTrue(action.IsComplete);
        }

        [TestMethod]
        public async Task DeleteRemote_ProjectSynchro_DeletingEntryFromRemoteCollection()
        {
            int id = 1;
            SyncAction action = new SyncAction();
            action.ID = id;
            await sychro.DeleteRemote(action);

            Assert.IsTrue(disk.RunDelete);
            Assert.IsFalse(disk.RunPull);
            Assert.IsFalse(disk.RunPush);
            Assert.AreEqual(id, disk.LastId);
            Assert.IsTrue(action.IsComplete);
        }

        [TestMethod]
        public async Task Upload_ProjectSynchro_UploadEntryToRemoteCollection()
        {
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
            Assert.IsTrue(action.IsComplete);
        }

        [TestMethod]
        public async Task Upload_ProjectSynchro_UploadEntryToRemoteCollection_ItemCollect()
        {
            int id = 1;
            var items = MockData.DEFAULT_ITEMS;
            Fixture.Context.Items.AddRange(items);
            Fixture.Context.SaveChanges();
            var project = Fixture.Context.Projects.Find(id);
            if (project.Items == null) project.Items = new List<ProjectItem>();
            foreach (var item in items)
            {
                project.Items.Add(new ProjectItem() { ProjectID = project.ID, ItemID = item.ID });
            }

            Fixture.Context.SaveChanges();
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
            Assert.IsTrue(action.IsComplete);
        }

        [TestMethod]
        public async Task Download_ProjectSynchro_UploadEntryToRemoteCollection()
        {
            int id = 1;
            await DownloadTest(id);
        }

        [TestMethod]
        public async Task Download_ProjectSynchro_OverwriteExistingEntryToRemoteCollection()
        {
            int id = 3;
            await DownloadTest(id);
        }

        [TestMethod]
        public async Task Download_ProjectSynchro_OverwriteExistingEntryToRemoteCollectionItemCollect()
        {
            int id = 2;

            var project = Fixture.Context.Projects.Find(id);
            ProjectDto expected = mapper.Map<ProjectDto>(project);
            var expItem = new List<ItemDto>();
            expected.Items = expItem;
            int num = 1;
            foreach (var item in MockData.DEFAULT_ITEMS)
            {
                var itemDto = mapper.Map<ItemDto>(item);
                itemDto.ID = (ID<ItemDto>)num++;
                expItem.Add(itemDto);
            }

            await DownloadProjectTest(expected);
        }

        private async Task DownloadProjectTest(ProjectDto expected)
        {
            disk.Project = expected;

            SyncAction action = new SyncAction();
            action.ID = (int)expected.ID;
            await sychro.Download(action);

            Assert.IsFalse(disk.RunDelete);
            Assert.IsTrue(disk.RunPull);
            Assert.IsFalse(disk.RunPush);
            Assert.AreEqual(action.ID, disk.LastId);
            Assert.IsTrue(action.IsComplete);

            var project = Fixture.Context.Projects.Find(action.ID);
            ProjectDto actual = mapper.Map<ProjectDto>(project);

            AssertHelper.EqualDto(expected, actual);
        }

        private async Task DownloadTest(int id)
        {
            ProjectDto expected = new ProjectDto()
            {
                ID = (ID<ProjectDto>)id,
                Title = "Замок кащея",
                Items = new List<ItemDto>(),
            };
            await DownloadProjectTest(expected);
        }
    }
}
