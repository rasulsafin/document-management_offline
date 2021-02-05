using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MRS.DocumentManagement;
using MRS.DocumentManagement.Connection.Synchronizator;
using MRS.DocumentManagement.Interface.Dtos;
using MRS.DocumentManagement.Interface.SyncData;
using MRS.DocumentManagement.Tests.Utility;
using MRS.DocumentManagement.Utility;

namespace DocumentManagement.Connection.Tests
{
    [TestClass]
    public class ObjectiveTypeSynchroTests
    {
        #region Initilise / Cleanup
        private static IMapper mapper;
        private static ObjectiveTypeSynchro sychro;
        private DiskMock disk;

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

                context.ObjectiveTypes.AddRange(MockData.DEFAULT_OBJECTIVE_TYPES);
                context.SaveChanges();
            });

            disk = new DiskMock();
            sychro = new ObjectiveTypeSynchro(disk, Fixture.Context, mapper);
        }

        [TestCleanup]
        public void Cleanup() => Fixture.Dispose();
        #endregion

        [TestMethod]
        public async Task DeleteLocal_NeedDelete_DeletingInDatebase()
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
        }

        [TestMethod]
        public async Task Download_ExistRec_Rewrite()
        {
            var type = Fixture.Context.ObjectiveTypes.Find(1);
            ObjectiveTypeDto expected = mapper.Map<ObjectiveTypeDto>(type);

            expected.Name = "бьючсялытоамт chv/kl hesguhdfkaegjhvc jds vnsdb;litgpo";

            await DownloadObjectiveTypeTest(expected);
        }

        [TestMethod]
        public async Task Download_NotExistRec_AddRecord()
        {
            ObjectiveTypeDto expected = new ObjectiveTypeDto()
            {
                ID = new ID<ObjectiveTypeDto>(10),
                Name = "Новый тип",
            };

            await DownloadObjectiveTypeTest(expected);
        }

        [TestMethod]
        public void GetRevisions_NotEmpty_NotEmptyCollection()
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
        public void GetRevisions_Empty_EmptyCollection()
        {
            var revisions = new RevisionCollection();
            var actual = sychro.GetRevisions(revisions);

            var expected = new List<Revision>();
            AssertHelper.EqualList(expected, actual, AssertHelper.EqualRevision);
            Assert.IsFalse(disk.RunDelete);
            Assert.IsFalse(disk.RunPull);
            Assert.IsFalse(disk.RunPush);
        }

        [TestMethod]
        public async Task GetSubSynchroList_ObjectiveTypeSynchro_Null()
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
        public void SetRevision_ExistRevision_CorrectRewrite()
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
        public void SetRevision_NotExistRevision_CorrectAdd()
        {
            var revisions = new RevisionCollection();
            revisions.GetRevision(TableRevision.ObjectiveTypes, 1).Rev = 5;
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
        public void SpecialSynchronization_ObjectiveTypeSynchro_NoChanges()
        {
            SyncAction actual = new SyncAction();
            SyncAction expected = new SyncAction();
            actual.ID = expected.ID = 1;
            actual.Synchronizer = expected.Synchronizer = nameof(ObjectiveTypeSynchro);
            actual.TypeAction = expected.TypeAction = SyncActionType.None;

            actual = sychro.SpecialSynchronization(actual);
            expected.SpecialSynchronization = false;

            AssertHelper.EqualSyncAction(expected, actual);
            Assert.IsFalse(disk.RunDelete);
            Assert.IsFalse(disk.RunPull);
            Assert.IsFalse(disk.RunPush);
        }

        [TestMethod]
        public async Task Special_ObjectiveTypeSynchro_Exeption()
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
        public async Task Upload_NeedUnload_UploadMethodCall()
        {
            int id = 1;
            var expected = mapper.Map<ObjectiveTypeDto>(Fixture.Context.ObjectiveTypes.Find(id));

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

        [TestMethod]
        public void CheckDBRevision_EmptyCollectRevision_AddingNonIncluded()
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

        [TestMethod]
        public void CheckDBRevision_NotEmptyCollectRevision_NoChange()
        {
            RevisionCollection actual = new RevisionCollection();
            actual.GetRevision(TableRevision.ObjectiveTypes, 1);
            actual.GetRevision(TableRevision.ObjectiveTypes, 2);

            sychro.CheckDBRevision(actual);

            RevisionCollection expected = new RevisionCollection();
            expected.GetRevision(TableRevision.ObjectiveTypes, 1);
            expected.GetRevision(TableRevision.ObjectiveTypes, 2);
            Assert.IsFalse(disk.RunDelete);
            Assert.IsFalse(disk.RunPull);
            Assert.IsFalse(disk.RunPush);
            AssertHelper.EqualRevisionCollection(expected, actual);
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
    }
}
