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
using MRS.DocumentManagement.Interface.Services;
using MRS.DocumentManagement.Tests.Utility;
using MRS.DocumentManagement.Utility;

namespace DocumentManagement.Connection.Tests
{
    [TestClass]
    public class ObjectiveSynchroTests : IUserSynchroTests
    {
        private static IMapper mapper;
        private static ObjectiveSynchro sychro;
        private DiskTest disk;

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
                var users = MockData.DEFAULT_USERS;
                var projects = MockData.DEFAULT_PROJECTS;
                var objectiveTypes = MockData.DEFAULT_OBJECTIVE_TYPES;
                var objectives = MockData.DEFAULT_OBJECTIVES;

                // var items = MockData.DEFAULT_ITEMS;
                // var bimElements = MockData.DEFAULT_BIM_ELEMENTS;
                // var dynamicFields = MockData.DEFAULT_DYNAMIC_FIELDS;
                context.Users.AddRange(users);
                context.Projects.AddRange(projects);
                context.ObjectiveTypes.AddRange(objectiveTypes);

                // context.Items.AddRange(items);
                context.SaveChanges();

                if (objectives.Count >= 3 && users.Count >= 2 && projects.Count >= 2 && objectiveTypes.Count >= 2)
                {
                    objectives[0].AuthorID = users[0].ID;
                    objectives[0].ProjectID = projects[0].ID;
                    objectives[0].ObjectiveTypeID = objectiveTypes[0].ID;
                    objectives[1].AuthorID = users[0].ID;
                    objectives[1].ProjectID = projects[0].ID;
                    objectives[1].ObjectiveTypeID = objectiveTypes[0].ID;
                    objectives[2].AuthorID = users[1].ID;
                    objectives[2].ProjectID = projects[1].ID;
                    objectives[2].ObjectiveTypeID = objectiveTypes[1].ID;
                }

                context.Objectives.AddRange(objectives);
                context.SaveChanges();
            });

            Revisions = new RevisionCollection();

            disk = new DiskTest();
            sychro = new ObjectiveSynchro(disk, Fixture.Context, mapper);
        }

        [TestCleanup]
        public void Cleanup() => Fixture.Dispose();

        [TestMethod]
        public void CheckDBRevisionTest()
        {
            RevisionCollection actual = new RevisionCollection();
            sychro.CheckDBRevision(actual);

            RevisionCollection expected = new RevisionCollection();
            expected.GetRevision(TableRevision.Objectives, 1);
            expected.GetRevision(TableRevision.Objectives, 2);
            expected.GetRevision(TableRevision.Objectives, 3);

            Assert.IsFalse(disk.RunDelete);
            Assert.IsFalse(disk.RunPull);
            Assert.IsFalse(disk.RunPush);
            AssertHelper.EqualRevisionCollection(expected, actual);
        }

        [TestMethod]
        public void GetRevisionsTest()
        {
            Revisions.GetRevision(TableRevision.Objectives, 1).Rev = 5;
            Revisions.GetRevision(TableRevision.Objectives, 2).Rev = 5;
            Revisions.GetRevision(TableRevision.Objectives, 3).Delete();

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
        public void SetRevisionTest()
        {
            int id = 2;
            Revisions.GetRevision(TableRevision.Objectives, 1).Rev = 5;
            Revisions.GetRevision(TableRevision.Objectives, 2).Rev = 5;
            Revisions.GetRevision(TableRevision.Objectives, 3).Delete();

            Revision expected = new Revision(id, 25);
            sychro.SetRevision(Revisions, expected);

            var actual = Revisions.GetRevision(TableRevision.Objectives, id);
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
            actual.Synchronizer = expected.Synchronizer = nameof(ObjectiveSynchro);
            actual.TypeAction = expected.TypeAction = TypeSyncAction.Download;
            expected.SpecialSynchronization = false;

            actual = sychro.SpecialSynchronization(actual);

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

            Assert.IsFalse(disk.RunDelete);
            Assert.IsFalse(disk.RunPull);
            Assert.IsFalse(disk.RunPush);
            var exist = Fixture.Context.Objectives.Any(x => x.ID == action.ID);
            exist = exist || Fixture.Context.BimElementObjectives.Any(x => x.ObjectiveID == action.ID);
            exist = exist || Fixture.Context.ObjectiveItems.Any(x => x.ObjectiveID == action.ID);
            Assert.IsFalse(exist);
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
            int id = 1;
            var objective = Fixture.Context.Objectives.Find(id);
            ObjectiveDto expected = mapper.Map<ObjectiveDto>(objective);
            SyncAction action = new SyncAction();
            action.ID = id;
            await sychro.Upload(action);

            Assert.IsFalse(disk.RunDelete);
            Assert.IsFalse(disk.RunPull);
            Assert.IsTrue(disk.RunPush);
            Assert.AreEqual(id, disk.LastId);

            ObjectiveDto actual = disk.Objective;

            AssertHelper.EqualDto(expected, actual);
        }

        [TestMethod]
        public async Task UploadTest_ItemCollect()
        {
            int id = 1;
            var objective = Fixture.Context.Objectives.Find(id);
            Fixture.Context.Items.AddRange(MockData.DEFAULT_ITEMS);
            Fixture.Context.SaveChanges();
            objective.Items = new List<ObjectiveItem>();
            foreach (var item in Fixture.Context.Items)
            {
                objective.Items.Add(new ObjectiveItem() { ObjectiveID = id, ItemID = item.ID });
            }

            Fixture.Context.SaveChanges();

            ObjectiveDto expected = mapper.Map<ObjectiveDto>(objective);
            SyncAction action = new SyncAction();
            action.ID = id;
            await sychro.Upload(action);

            Assert.IsFalse(disk.RunDelete);
            Assert.IsFalse(disk.RunPull);
            Assert.IsTrue(disk.RunPush);
            Assert.AreEqual(id, disk.LastId);

            ObjectiveDto actual = disk.Objective;

            AssertHelper.EqualDto(expected, actual);
        }

        [TestMethod]
        public async Task UploadTest_BimCollect()
        {
            int id = 1;
            var objective = Fixture.Context.Objectives.Find(id);
            Fixture.Context.Items.AddRange(MockData.DEFAULT_ITEMS);
            Fixture.Context.SaveChanges();

            var itemID = Fixture.Context.Items.Where(x => x.ItemType == (int)ItemTypeDto.Bim).FirstOrDefault().ID;
            foreach (var bimElem in MockData.DEFAULT_BIM_ELEMENTS)
            {
                bimElem.ItemID = itemID;
                Fixture.Context.BimElements.Add(bimElem);
            }

            Fixture.Context.SaveChanges();

            objective.BimElements = new List<BimElementObjective>();
            foreach (var bim in Fixture.Context.BimElements)
            {
                objective.BimElements.Add(new BimElementObjective() { ObjectiveID = id, BimElementID = bim.ID });
            }

            Fixture.Context.SaveChanges();

            ObjectiveDto expected = mapper.Map<ObjectiveDto>(objective);
            SyncAction action = new SyncAction();
            action.ID = id;
            await sychro.Upload(action);

            Assert.IsFalse(disk.RunDelete);
            Assert.IsFalse(disk.RunPull);
            Assert.IsTrue(disk.RunPush);
            Assert.AreEqual(id, disk.LastId);

            ObjectiveDto actual = disk.Objective;

            AssertHelper.EqualDto(expected, actual);
        }

        [TestMethod]
        public async Task UploadTest_DynamicCollect()
        {
            int id = 1;
            var objective = Fixture.Context.Objectives.Find(id);

            // Fixture.Context.DynamicFields.AddRange(MockData.DEFAULT_DYNAMIC_FIELDS);
            // Fixture.Context.SaveChanges();
            objective.DynamicFields = new List<DynamicField>();
            foreach (var dynamic in Fixture.Context.DynamicFields)
            {
                objective.DynamicFields.Add(dynamic);
            }

            Fixture.Context.SaveChanges();

            ObjectiveDto expected = mapper.Map<ObjectiveDto>(objective);
            SyncAction action = new SyncAction();
            action.ID = id;
            await sychro.Upload(action);

            Assert.IsFalse(disk.RunDelete);
            Assert.IsFalse(disk.RunPull);
            Assert.IsTrue(disk.RunPush);
            Assert.AreEqual(id, disk.LastId);

            ObjectiveDto actual = disk.Objective;

            AssertHelper.EqualDto(expected, actual);
        }

        [TestMethod]
        public async Task DownloadTestExist()
        {
            int id = 1;
            await DownloadTest(id);
        }

        [TestMethod]
        public async Task DownloadTestNotExist()
        {
            int id = 5;
            var objective = MockData.DEFAULT_OBJECTIVES.First();
            objective.ID = id;
            objective.ProjectID = 1;
            objective.AuthorID = 1;
            objective.ObjectiveTypeID = 1;
            ObjectiveDto expected = mapper.Map<ObjectiveDto>(objective);

            await DownloadObjectiveTest(expected);
        }

        [TestMethod]
        public async Task DownloadTest_ItemCollect()
        {
            int id = 2;
            var objective = Fixture.Context.Objectives.Find(id);
            ObjectiveDto expected = mapper.Map<ObjectiveDto>(objective);
            var expItem = new List<ItemDto>();
            expected.Items = expItem;
            int num = 1;
            foreach (var item in MockData.DEFAULT_ITEMS)
            {
                var itemDto = mapper.Map<ItemDto>(item);
                itemDto.ID = (ID<ItemDto>)num++;
                expItem.Add(itemDto);
            }

            await DownloadObjectiveTest(expected);
        }

        [TestMethod]
        public async Task DownloadTest_BimElementCollect_NoFileBim()
        {
            int id = 2;
            var objective = Fixture.Context.Objectives.Find(id);
            ObjectiveDto expected = mapper.Map<ObjectiveDto>(objective);
            var expBim = new List<BimElementDto>();
            expected.BimElements = expBim;

            // int num = 1;
            foreach (var bim in MockData.DEFAULT_BIM_ELEMENTS)
            {
                var bimDto = mapper.Map<BimElementDto>(bim);

                bimDto.ItemID = (ID<ItemDto>)3;
                expBim.Add(bimDto);
            }

            disk.Objective = expected;

            SyncAction action = new SyncAction();
            action.ID = (int)expected.ID;
            await sychro.Download(action);

            Assert.IsFalse(disk.RunDelete);
            Assert.IsTrue(disk.RunPull);
            Assert.IsFalse(disk.RunPush);
            Assert.AreEqual(action.ID, disk.LastId);

            objective = Fixture.Context.Objectives.Find(action.ID);
            ObjectiveDto actual = mapper.Map<ObjectiveDto>(objective);

            expected.BimElements = Enumerable.Empty<BimElementDto>();
            AssertHelper.EqualDto(expected, actual);
        }

        [TestMethod]
        public async Task DownloadTest_BimElementCollect_YesFileBim()
        {
            int id = 2;
            var objective = Fixture.Context.Objectives.Find(id);
            Fixture.Context.Items.AddRange(MockData.DEFAULT_ITEMS);
            Fixture.Context.SaveChanges();

            var item = Fixture.Context.Items.First(x => x.ItemType == (int)ItemTypeDto.File);
            ObjectiveDto expected = mapper.Map<ObjectiveDto>(objective);
            var expBim = new List<BimElementDto>();
            expected.BimElements = expBim;

            // int num = 1;
            foreach (var bim in MockData.DEFAULT_BIM_ELEMENTS)
            {
                var bimDto = mapper.Map<BimElementDto>(bim);

                bimDto.ItemID = (ID<ItemDto>)item.ID;
                expBim.Add(bimDto);
            }

            await DownloadObjectiveTest(expected);
        }

        [TestMethod]
        public async Task DownloadTest_DynamicCollect()
        {
            int id = 2;
            var objective = Fixture.Context.Objectives.Find(id);
            ObjectiveDto expected = mapper.Map<ObjectiveDto>(objective);
            var expDyn = new List<DynamicFieldDto>();
            expected.DynamicFields = expDyn;
            int num = 1;
            foreach (var dyn in MockData.DEFAULT_DYNAMIC_FIELDS)
            {
                var dynDto = mapper.Map<DynamicFieldDto>(dyn);

                dynDto.ID = (ID<DynamicFieldDto>)num++;
                expDyn.Add(dynDto);
            }

            await DownloadObjectiveTest(expected);
        }

        [TestMethod]
        public async Task StopedDownloadAction_NoKeyProject_Test()
        {
            int id = 5;
            var objective = MockData.DEFAULT_OBJECTIVES[0];
            objective.ID = id;
            objective.ProjectID = 5;
            objective.AuthorID = 1;
            objective.ObjectiveTypeID = 1;
            ObjectiveDto expected = mapper.Map<ObjectiveDto>(objective);
            await StopedAction(expected);
        }

        [TestMethod]
        public async Task StopedDownloadAction_NoKeyAuthor_Test()
        {
            int id = 5;
            var objective = MockData.DEFAULT_OBJECTIVES[0];
            objective.ID = id;
            objective.ProjectID = 1;
            objective.AuthorID = 5;
            objective.ObjectiveTypeID = 1;
            ObjectiveDto expected = mapper.Map<ObjectiveDto>(objective);
            await StopedAction(expected);
        }

        [TestMethod]
        public async Task StopedDownloadAction_NoKeyObjectiveType_Test()
        {
            int id = 5;
            var objective = MockData.DEFAULT_OBJECTIVES[0];
            objective.ID = id;
            objective.ProjectID = 1;
            objective.AuthorID = 1;
            objective.ObjectiveTypeID = 5;
            ObjectiveDto expected = mapper.Map<ObjectiveDto>(objective);
            await StopedAction(expected);
        }

        private async Task StopedAction(ObjectiveDto expected)
        {
            disk.Objective = expected;

            SyncAction action = new SyncAction();
            action.ID = (int)expected.ID;

            await sychro.Download(action);

            Assert.IsFalse(disk.RunDelete);
            Assert.IsTrue(disk.RunPull);
            Assert.IsFalse(disk.RunPush);
            Assert.IsFalse(action.IsComplete);
            Assert.AreEqual(action.ID, disk.LastId);

            ObjectiveDto actual = mapper.Map<ObjectiveDto>(action.Data);

            AssertHelper.EqualDto(expected, actual);
        }

        private async Task DownloadTest(int id)
        {
            var objective = Fixture.Context.Objectives.Find(id);
            ObjectiveDto expected = mapper.Map<ObjectiveDto>(objective);

            await DownloadObjectiveTest(expected);
        }

        private async Task DownloadObjectiveTest(ObjectiveDto expected)
        {
            disk.Objective = expected;

            SyncAction action = new SyncAction();
            action.ID = (int)expected.ID;
            await sychro.Download(action);

            Assert.IsFalse(disk.RunDelete);
            Assert.IsTrue(disk.RunPull);
            Assert.IsFalse(disk.RunPush);
            Assert.IsTrue(action.IsComplete);
            Assert.AreEqual(action.ID, disk.LastId);

            var objective = Fixture.Context.Objectives.Find(action.ID);
            ObjectiveDto actual = mapper.Map<ObjectiveDto>(objective);

            AssertHelper.EqualDto(expected, actual);
        }
    }
}
