using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MRS.DocumentManagement;
using MRS.DocumentManagement.Connection;
using MRS.DocumentManagement.Connection.Synchronizator;
using MRS.DocumentManagement.Database.Models;
using MRS.DocumentManagement.Interface.Dtos;
using MRS.DocumentManagement.Interface.Services;
using MRS.DocumentManagement.Tests.Utility;
using MRS.DocumentManagement.Utility;

namespace DocumentManagement.Connection.Tests
{
    [TestClass]
    public class ObjectiveTypeSynchroTests : IUserSynchroTests
    {
        #region Initilise / Cleanup
        private static SharedDatabaseFixture Fixture { get; set; }

        private DiskTest disk;
        private static IMapper mapper;
        private static ObjectiveTypeSynchro sychro;

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

                context.ObjectiveTypes.AddRange(MockData.DEFAULT_OBJECTIVE_TYPES);
                context.SaveChanges();
            });

            // Revisions = new RevisionCollection();
            disk = new DiskTest();
            sychro = new ObjectiveTypeSynchro(disk, Fixture.Context, mapper);
        }

        [TestCleanup]
        public void Cleanup() => Fixture.Dispose();
        #endregion

        [TestMethod]
        public async Task DeleteLocalTest()
        {
            int id = 1;
            SyncAction action = new SyncAction();
            action.ID = id;
            await sychro.DeleteLocal(action);

            var user = Fixture.Context.ObjectiveTypes.Find(id);
            Assert.IsNull(user);

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
        public async Task DownloadTestExist()
        {
            var type = Fixture.Context.ObjectiveTypes.Find(1);
            ObjectiveTypeDto expected = mapper.Map<ObjectiveTypeDto>(type);

            expected.Name = "бьючсялытоамт chv/kl hesguhdfkaegjhvc jds vnsdb;litgpo";

            await DownloadObjectiveTypeTest(expected);
        }

        [TestMethod]
        public async Task DownloadTestNotExist()
        {
            ObjectiveTypeDto expected = new ObjectiveTypeDto()
            {
                ID = new ID<ObjectiveTypeDto>(10),
                Name = "Новый тип",
            };

            await DownloadObjectiveTypeTest(expected);
        }

        [TestMethod]
        public void GetRevisionsTest()
        {
            var revisions = new RevisionCollection();
            revisions.GetRevision(TableRevision.ObjectiveTypes, 1).Rev = 5;
            revisions.GetRevision(TableRevision.ObjectiveTypes, 2).Rev = 5;
            revisions.GetRevision(TableRevision.ObjectiveTypes, 3).Delete();

            var actual = sychro.GetRevisions(revisions);

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
        public async Task GetSubSynchroListTest()
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
        public void SetRevisionTest()
        {
            var revisions = new RevisionCollection();
            revisions.GetRevision(TableRevision.ObjectiveTypes, 1).Rev = 5;
            revisions.GetRevision(TableRevision.ObjectiveTypes, 2).Rev = 5;
            revisions.GetRevision(TableRevision.ObjectiveTypes, 3).Delete();

            Revision expected = new Revision(2, 25);
            sychro.SetRevision(revisions, expected);

            var actual = revisions.GetRevision(TableRevision.ObjectiveTypes, 2);
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
            actual.Synchronizer = expected.Synchronizer = nameof(ObjectiveTypeSynchro);
            actual.TypeAction = expected.TypeAction = TypeSyncAction.None;

            actual = sychro.SpecialSynchronization(actual);
            expected.SpecialSynchronization = false;

            AssertHelper.EqualSyncAction(expected, actual);
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
        public async Task UploadTest()
        {
            int id = 1;
            var expected = mapper.Map<ObjectiveTypeDto>(Fixture.Context.ObjectiveTypes.Find(id));
            //var expected = mapper.Map<ObjectiveTypeDto>(MockData.DEFAULT_OBJECTIVE_TYPES[0]);

            SyncAction action = new SyncAction();
            action.ID = id;

            await sychro.Upload(action);
            var actual = disk.ObjectiveType;

            Assert.IsFalse(disk.RunDelete);
            Assert.IsFalse(disk.RunPull);
            Assert.IsTrue(disk.RunPush);
            Assert.IsTrue(action.IsComplete);

            AssertHelper.EqualDto(expected, actual);
        }

        private async Task DownloadObjectiveTypeTest(ObjectiveTypeDto expected)
        {
            disk.ObjectiveType = expected;

            SyncAction action = new SyncAction();
            action.ID = (int)expected.ID;
            await sychro.Download(action);

            Assert.IsFalse(disk.RunDelete);
            Assert.IsTrue(disk.RunPull);
            Assert.IsFalse(disk.RunPush);
            Assert.AreEqual(action.ID, disk.LastId);

            var objectiveType = Fixture.Context.ObjectiveTypes.Find(action.ID);
            var actual = mapper.Map<ObjectiveTypeDto>(objectiveType);

            AssertHelper.EqualDto(expected, actual);
        }

        [TestMethod]
        public void CheckDBRevisionTest()
        {
            RevisionCollection actual = new RevisionCollection();
            sychro.CheckDBRevision(actual);

            RevisionCollection expected = new RevisionCollection();
            expected.GetRevision(TableRevision.ObjectiveTypes, 1);
            expected.GetRevision(TableRevision.ObjectiveTypes, 2);

            Assert.IsFalse(disk.RunDelete);
            Assert.IsFalse(disk.RunPull);
            Assert.IsFalse(disk.RunPush);
            AssertHelper.EqualRevisionCollection(expected, actual);
        }
    }
}
