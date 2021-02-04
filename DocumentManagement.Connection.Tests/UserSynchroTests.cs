using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MRS.DocumentManagement.Connection.Synchronizator;
using MRS.DocumentManagement.Interface.Dtos;
using MRS.DocumentManagement.Interface.Services;
using MRS.DocumentManagement.Tests.Utility;
using MRS.DocumentManagement.Utility;

namespace DocumentManagement.Connection.Tests
{
    [TestClass]
    public class UserSynchroTests
    {
        private static IMapper mapper;
        private static UserSynchro sychro;
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

                context.Users.AddRange(MockData.DEFAULT_USERS);
                context.SaveChanges();
            });

            Revisions = new RevisionCollection();

            disk = new DiskMock();
            sychro = new UserSynchro(disk, Fixture.Context);
        }

        [TestCleanup]
        public void Cleanup() => Fixture.Dispose();

        [TestMethod]
        public void CheckDBRevision_EmptyRevisionCollection_AddingNonIncludedRecords()
        {
            RevisionCollection actual = new RevisionCollection();
            sychro.CheckDBRevision(actual);

            RevisionCollection expected = new RevisionCollection();
            expected.GetRevision(TableRevision.Users, 1);
            expected.GetRevision(TableRevision.Users, 2);
            expected.GetRevision(TableRevision.Users, 3);
            expected.GetRevision(TableRevision.Users, 4);
            Assert.IsFalse(disk.RunDelete);
            Assert.IsFalse(disk.RunPull);
            Assert.IsFalse(disk.RunPush);
            AssertHelper.EqualRevisionCollection(expected, actual);
        }

        [TestMethod]
        public void CheckDBRevision_NotEmptyRevisionCollection_AddingNonIncludedRecords()
        {
            RevisionCollection actual = new RevisionCollection();
            actual.GetRevision(TableRevision.Users, 1);
            actual.GetRevision(TableRevision.Users, 2);
            actual.GetRevision(TableRevision.Users, 3);
            actual.GetRevision(TableRevision.Users, 4);

            sychro.CheckDBRevision(actual);

            RevisionCollection expected = new RevisionCollection();
            expected.GetRevision(TableRevision.Users, 1);
            expected.GetRevision(TableRevision.Users, 2);
            expected.GetRevision(TableRevision.Users, 3);
            expected.GetRevision(TableRevision.Users, 4);
            Assert.IsFalse(disk.RunDelete);
            Assert.IsFalse(disk.RunPull);
            Assert.IsFalse(disk.RunPush);
            AssertHelper.EqualRevisionCollection(expected, actual);
        }

        [TestMethod]
        public void GetRevisions_NotEmpty_NotEmptyCollection()
        {
            Revisions.GetRevision(TableRevision.Users, 1).Rev = 5;
            Revisions.GetRevision(TableRevision.Users, 2).Rev = 5;
            Revisions.GetRevision(TableRevision.Users, 3).Delete();

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
        public void GetRevisions_Empty_EmptyCollection()
        {
            var actual = sychro.GetRevisions(Revisions);

            var delRev = new Revision(3);
            delRev.Delete();
            var expected = new List<Revision>();

            AssertHelper.EqualList(expected, actual, AssertHelper.EqualRevision);
            Assert.IsFalse(disk.RunDelete);
            Assert.IsFalse(disk.RunPull);
            Assert.IsFalse(disk.RunPush);
        }

        [TestMethod]
        public void SetRevision_ExistRevision_CorrectRewrite()
        {
            Revisions.GetRevision(TableRevision.Users, 1).Rev = 5;
            Revisions.GetRevision(TableRevision.Users, 2).Rev = 5;
            Revisions.GetRevision(TableRevision.Users, 3).Delete();

            Revision expected = new Revision(2, 25);
            sychro.SetRevision(Revisions, expected);

            var actual = Revisions.GetRevision(TableRevision.Users, 2);
            AssertHelper.EqualRevision(expected, actual);
            Assert.IsFalse(disk.RunDelete);
            Assert.IsFalse(disk.RunPull);
            Assert.IsFalse(disk.RunPush);
        }

        [TestMethod]
        public void SetRevision_NotExistRevision_CorrectAdd()
        {
            Revisions.GetRevision(TableRevision.Users, 1).Rev = 5;
            Revisions.GetRevision(TableRevision.Users, 3).Delete();

            Revision expected = new Revision(2, 25);
            sychro.SetRevision(Revisions, expected);

            var actual = Revisions.GetRevision(TableRevision.Users, 2);
            AssertHelper.EqualRevision(expected, actual);
            Assert.IsFalse(disk.RunDelete);
            Assert.IsFalse(disk.RunPull);
            Assert.IsFalse(disk.RunPush);
        }

        [TestMethod]
        public void SpecialSynchronization_UserSynchro_NoChanges()
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
        public async Task GetSubSynchroList_UserSynchro_Null()
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
        public async Task Special_UserSynchro_Exeption()
        {
            SyncAction action = new SyncAction();
            await Assert.ThrowsExceptionAsync<NotImplementedException>(() =>
            {
                return sychro.Special(action);
            });
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

            var user = Fixture.Context.Users.Find(id);
            Assert.IsNull(user);

            Assert.IsFalse(disk.RunDelete);
            Assert.IsFalse(disk.RunPull);
            Assert.IsFalse(disk.RunPush);
            Assert.IsTrue(action.IsComplete);
        }

        [TestMethod]
        public async Task DeleteRemote_NeedDelete_DeletingMethodCall()
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
        public async Task Upload_NeedUnload_UploadMethodCall()
        {
            int id = 1;
            SyncAction action = new SyncAction();
            action.ID = id;

            await sychro.Upload(action);

            Assert.IsFalse(disk.RunDelete);
            Assert.IsFalse(disk.RunPull);
            Assert.IsTrue(disk.RunPush);
            Assert.AreEqual(id, disk.LastId);

            var user = Fixture.Context.Users.Find(id);
            UserDto expected = mapper.Map<UserDto>(user);
            UserDto actual = disk.User.ToDto();
            AssertHelper.EqualDto(expected, actual);
            Assert.IsTrue(action.IsComplete);
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
            var user = MockData.DEFAULT_USERS.Find(x => x.ID == id);
            disk.User = new UserSynchro.UserSync()
            {
                ID = id,
                Login = "hty007",
                Name = "Ренат Мирошников",
            };
            UserDto expected = disk.User.ToDto();

            SyncAction action = new SyncAction();
            action.ID = id;
            await sychro.Download(action);

            Assert.IsFalse(disk.RunDelete);
            Assert.IsTrue(disk.RunPull);
            Assert.IsFalse(disk.RunPush);
            Assert.AreEqual(id, disk.LastId);
            Assert.IsTrue(action.IsComplete);

            user = Fixture.Context.Users.Find(id);
            UserDto actual = mapper.Map<UserDto>(user);

            AssertHelper.EqualDto(expected, actual);
        }
    }
}
