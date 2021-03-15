using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MRS.DocumentManagement;
using MRS.DocumentManagement.Database.Models;
using MRS.DocumentManagement.Interface.Dtos;
using MRS.DocumentManagement.Synchronizer;
using MRS.DocumentManagement.Tests.Utility;
using MRS.DocumentManagement.Utility;

namespace DocumentManagement.Connection.Tests
{
    [TestClass]
    public class ObjectiveSynchroTests
    {
        private static IMapper mapper;
        private static ObjectiveSynchro sychro;
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
                var users = MockData.DEFAULT_USERS;
                var projects = MockData.DEFAULT_PROJECTS;
                var objectiveTypes = MockData.DEFAULT_OBJECTIVE_TYPES;
                var objectives = MockData.DEFAULT_OBJECTIVES;

                context.Users.AddRange(users);
                context.Projects.AddRange(projects);
                context.ObjectiveTypes.AddRange(objectiveTypes);

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

            disk = new DiskMock();
            sychro = new ObjectiveSynchro(disk, Fixture.Context, mapper);
        }

        [TestCleanup]
        public void Cleanup() => Fixture.Dispose();

        [TestMethod]
        public void CheckDBRevision_EmptyCollectRevision_AddingNonIncluded()
        {
            RevisionCollection actual = new RevisionCollection();

            sychro.CheckDBRevision(actual);

            RevisionCollection expected = new RevisionCollection();
            expected.GetRevision(NameTypeRevision.Objectives, 1);
            expected.GetRevision(NameTypeRevision.Objectives, 2);
            expected.GetRevision(NameTypeRevision.Objectives, 3);
            Assert.IsFalse(disk.RunDelete);
            Assert.IsFalse(disk.RunPull);
            Assert.IsFalse(disk.RunPush);
            AssertHelper.EqualRevisionCollection(expected, actual);
        }

        [TestMethod]
        public void CheckDBRevision_NotEmptyCollectRevision_NoChange()
        {
            RevisionCollection actual = new RevisionCollection();
            actual.GetRevision(NameTypeRevision.Objectives, 1);
            actual.GetRevision(NameTypeRevision.Objectives, 2);
            actual.GetRevision(NameTypeRevision.Objectives, 3);

            sychro.CheckDBRevision(actual);

            RevisionCollection expected = new RevisionCollection();
            expected.GetRevision(NameTypeRevision.Objectives, 1);
            expected.GetRevision(NameTypeRevision.Objectives, 2);
            expected.GetRevision(NameTypeRevision.Objectives, 3);
            Assert.IsFalse(disk.RunDelete);
            Assert.IsFalse(disk.RunPull);
            Assert.IsFalse(disk.RunPush);
            AssertHelper.EqualRevisionCollection(expected, actual);
        }

        [TestMethod]
        public void GetRevisions_NotEmpty_NotEmptyCollection()
        {
            Revisions.GetRevision(NameTypeRevision.Objectives, 1).Rev = 5;
            Revisions.GetRevision(NameTypeRevision.Objectives, 2).Rev = 5;
            Revisions.GetRevision(NameTypeRevision.Objectives, 3).Delete();

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
            var expected = new List<Revision>();

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
            Revisions.GetRevision(NameTypeRevision.Objectives, 1).Rev = 5;
            Revisions.GetRevision(NameTypeRevision.Objectives, 3).Delete();

            Revision expected = new Revision(id, 25);
            sychro.SetRevision(Revisions, expected);

            var actual = Revisions.GetRevision(NameTypeRevision.Objectives, id);
            AssertHelper.EqualRevision(expected, actual);
            Assert.IsFalse(disk.RunDelete);
            Assert.IsFalse(disk.RunPull);
            Assert.IsFalse(disk.RunPush);
        }

        [TestMethod]
        public void SetRevision_ExistRevision_CorrectRewrite()
        {
            int id = 2;
            Revisions.GetRevision(NameTypeRevision.Objectives, 1).Rev = 5;
            Revisions.GetRevision(NameTypeRevision.Objectives, 2).Rev = 5;
            Revisions.GetRevision(NameTypeRevision.Objectives, 3).Delete();

            Revision expected = new Revision(id, 25);
            sychro.SetRevision(Revisions, expected);

            var actual = Revisions.GetRevision(NameTypeRevision.Objectives, id);
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

            Assert.IsFalse(disk.RunDelete);
            Assert.IsFalse(disk.RunPull);
            Assert.IsFalse(disk.RunPush);
            var exist = Fixture.Context.Objectives.Any(x => x.ID == action.ID);
            exist = exist || Fixture.Context.BimElementObjectives.Any(x => x.ObjectiveID == action.ID);
            exist = exist || Fixture.Context.ObjectiveItems.Any(x => x.ObjectiveID == action.ID);
            Assert.IsFalse(exist);
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
        public async Task Upload_NeedUnloadNotCollection_UnloadMethodCall()
        {
            int id = 1;
            var objective = Fixture.Context.Objectives.Find(id);
            SyncAction action = new SyncAction();
            action.ID = id;

            await sychro.Upload(action);

            ObjectiveDto expected = mapper.Map<ObjectiveDto>(objective);
            Assert.IsFalse(disk.RunDelete);
            Assert.IsFalse(disk.RunPull);
            Assert.IsTrue(disk.RunPush);
            Assert.AreEqual(id, disk.LastId);

            ObjectiveDto actual = disk.Objective;

            AssertHelper.EqualDto(expected, actual);
        }

        [TestMethod]
        public async Task Upload_NeedUnloadAndItemCollection_UnloadMethodCall()
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
        public async Task Upload_NeedUnloadAndBimElementCollection_UnloadMethodCall()
        {
            int id = 1;
            var objective = Fixture.Context.Objectives.Find(id);
            Fixture.Context.Items.AddRange(MockData.DEFAULT_ITEMS);
            Fixture.Context.SaveChanges();

            var itemName = Fixture.Context.Items.Where(x => x.ItemType == (int)ItemTypeDto.Bim).FirstOrDefault().RelativePath;
            foreach (var bimElem in MockData.DEFAULT_BIM_ELEMENTS)
            {
                bimElem.ParentName = itemName;
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
        public async Task Upload_NeedUnloadAndDynamicFieldCollection_UnloadMethodCall()
        {
            int id = 1;
            var objective = Fixture.Context.Objectives.Find(id);

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
        public async Task Download_ExistRec_Rewrite()
        {
            int id = 1;
            await DownloadTest(id);
        }

        [TestMethod]
        public async Task Download_NotExistRec_AddRecord()
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
        public async Task Download_ExistRecAndItemCollect_RewriteRecordAddCollect()
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
        public async Task Download_ExistRecAndNotBimElementCollect_RewriteRecord()
        {
            int id = 2;
            var objective = Fixture.Context.Objectives.Find(id);
            ObjectiveDto expected = mapper.Map<ObjectiveDto>(objective);
            var expBim = new List<BimElementDto>();
            expected.BimElements = expBim;

            foreach (var bim in MockData.DEFAULT_BIM_ELEMENTS)
            {
                var bimDto = mapper.Map<BimElementDto>(bim);

                bimDto.ParentName = "Имя Файла которого пока нет";
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

            AssertHelper.EqualDto(expected, actual);
        }

        [TestMethod]
        public async Task Download_ExistRecAndBimElementCollect_RewriteRecordAddCollect()
        {
            int id = 2;
            var objective = Fixture.Context.Objectives.Find(id);
            Fixture.Context.Items.AddRange(MockData.DEFAULT_ITEMS);
            Fixture.Context.SaveChanges();

            var item = Fixture.Context.Items.First(x => x.ItemType == (int)ItemTypeDto.File);
            ObjectiveDto expected = mapper.Map<ObjectiveDto>(objective);
            var expBim = new List<BimElementDto>();
            expected.BimElements = expBim;

            foreach (var bim in MockData.DEFAULT_BIM_ELEMENTS)
            {
                var bimDto = mapper.Map<BimElementDto>(bim);

                bimDto.ParentName = item.RelativePath;
                expBim.Add(bimDto);
            }

            await DownloadObjectiveTest(expected);
        }

        [TestMethod]
        public async Task Download_ExistRecAndDynamicFieldCollect_RewriteRecordAddCollect()
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
        public async Task Download_NotExistProject_NotCompleteAction()
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
        public async Task Download_NotExistUser_NotCompleteAction()
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
        public async Task Download_NotExistObjectiveType_NotCompleteAction()
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
