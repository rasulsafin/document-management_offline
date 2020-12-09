﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using System.Threading.Tasks;
using MRS.DocumentManagement.Tests.Utility;
using System;
using System.Collections.Generic;
using MRS.DocumentManagement.Interface.Dtos;
using MRS.DocumentManagement.Interface;

namespace MRS.DocumentManagement.Tests
{
    [TestClass]
    public class ObjectiveServiceTests
    {
        public static SharedDatabaseFixture Fixture { get; private set; }

        [ClassInitialize]
        public static async Task Setup(TestContext _)
        {
            Fixture = new SharedDatabaseFixture();
        }

        [ClassCleanup]
        public static void Cleanup()
        {
            Fixture.Dispose();
        }

        // WARNING: Dynamic fields IDs are not set
        private static ObjectiveDto CreateExpectedObjective(ObjectiveToCreateDto o, ID<ObjectiveDto> id, UserDto author, ObjectiveTypeDto type)
        {
            return new ObjectiveDto()
            {
                ID = id,
                Author = author,
                CreationDate = o.CreationDate,
                DueDate = o.DueDate,
                Title = o.Title,
                Description = o.Description,
                ParentObjectiveID = o.ParentObjectiveID,
                ProjectID = o.ProjectID,
                Status = o.Status,
                TaskType = type,
                BimElements = o.BimElements?.ToList() ?? Enumerable.Empty<BimElementDto>(),
                DynamicFields = o.DynamicFields?
                    .Select(x => new DynamicFieldDto() 
                    {
                        Key = x.Key,
                        Type = x.Type,
                        Value = x.Value
                    }).ToList() ?? Enumerable.Empty<DynamicFieldDto>()
            };
        }

        [TestMethod]
        public async Task Complex_objective_test()
        {
            using var transaction = Fixture.Connection.BeginTransaction();
            using (var context = Fixture.CreateContext(transaction))
            {
                var api = new DocumentManagementApi(context);
                var access = await api.Register(new UserToCreateDto("vpupkin", "123", "Vasily Pupkin"));

                var tasktype = await access.ObjectiveTypeService.Add("Задание");
                var errortype = await access.ObjectiveTypeService.Add("Нарушение");

                var project1ID = await access.ProjectService.AddToUser(access.CurrentUser.ID, "Project 1");
                var project2ID = await access.ProjectService.AddToUser(access.CurrentUser.ID, "Project 2");

                var creationTime = DateTime.Parse("2020-11-18T10:50:00.0000000Z");
                var dueTime = creationTime.AddDays(1);

                // 0. Can add objectives
                var newObjective1 = new ObjectiveToCreateDto()
                {
                    AuthorID = access.CurrentUser.ID,
                    CreationDate = creationTime,
                    DueDate = dueTime,
                    Title = "Make cookies",
                    Description = "Mmm, cookies!",
                    Status = ObjectiveStatusDto.Open,
                    TaskType = tasktype,
                    ParentObjectiveID = null,
                    ProjectID = project1ID,
                    BimElements = null,
                    DynamicFields = new List<DynamicFieldToCreateDto>()
                    {
                        new DynamicFieldToCreateDto("df1", "type 1", "val 1"),
                        new DynamicFieldToCreateDto("df2", "type 2", "val 2")
                    }
                };

                var objective1ID = await access.ObjectiveService.Add(newObjective1);
                Assert.IsTrue(objective1ID.IsValid);

                var newObjective2 = new ObjectiveToCreateDto()
                {
                    AuthorID = access.CurrentUser.ID,
                    CreationDate = creationTime,
                    DueDate = dueTime,
                    Title = "Make dough",
                    Description = "Can't make cookies without dough",
                    Status = ObjectiveStatusDto.Open,
                    TaskType = tasktype,
                    ParentObjectiveID = objective1ID,
                    ProjectID = project1ID
                };
                
                var childObjectiveID = await access.ObjectiveService.Add(newObjective2);
                Assert.IsTrue(childObjectiveID.IsValid);

                var newObjective3 = new ObjectiveToCreateDto()
                {
                    AuthorID = access.CurrentUser.ID,
                    CreationDate = creationTime,
                    DueDate = dueTime,
                    Title = "Something is wrong",
                    Description = "Really, something is wrong!",
                    Status = ObjectiveStatusDto.Ready,
                    TaskType = errortype,
                    ParentObjectiveID = null,
                    ProjectID = project2ID
                };

                var objective3ID = await access.ObjectiveService.Add(newObjective3);
                Assert.IsTrue(objective3ID.IsValid);


                // 1. Can query all objectives
                var currentUser = access.CurrentUser;
                var expected = new ObjectiveDto[]
                {
                    CreateExpectedObjective(newObjective1, objective1ID, currentUser, new ObjectiveTypeDto(){ ID = tasktype, Name = "Задание" }),
                    CreateExpectedObjective(newObjective2, childObjectiveID, currentUser, new ObjectiveTypeDto(){ ID = tasktype, Name = "Задание" }),
                    CreateExpectedObjective(newObjective3, objective3ID, currentUser, new ObjectiveTypeDto(){ ID = errortype, Name = "Нарушение" })
                };

                var comparer = new ObjectiveComparer();
                var allObjectives = await access.ObjectiveService.GetAllObjectives();
                CollectionAssert.That.AreEquivalent(expected, allObjectives.ToArray(), comparer);

                //2. Can query objectives by project
                var project1Objectives = await access.ObjectiveService.GetObjectives(project1ID);
                CollectionAssert.That.AreEquivalent(new[] { expected[0], expected[1] }, project1Objectives, comparer);

                var project2Objectives = await access.ObjectiveService.GetObjectives(project2ID);
                CollectionAssert.That.AreEquivalent(new[] { expected[2] }, project2Objectives, comparer);

                // 2. Can find objective
                var found = await access.ObjectiveService.Find(childObjectiveID);
                Assert.IsTrue(comparer.Equals(expected[1], found));

                // 2.1 Can not find not existing objective
                found = await access.ObjectiveService.Find(ID<ObjectiveDto>.InvalidID);
                Assert.IsNull(found);

                // 3. Can remove objective
                var isRemoved = await access.ObjectiveService.Remove(childObjectiveID);
                Assert.IsTrue(isRemoved);

                // 3.1 Can not remove twice
                isRemoved = await access.ObjectiveService.Remove(childObjectiveID);
                Assert.IsFalse(isRemoved);

                // Verify that objective is removed
                allObjectives = await access.ObjectiveService.GetAllObjectives();
                CollectionAssert.That.AreEquivalent(new[] { expected[0], expected[2] }, allObjectives, comparer);

                project1Objectives = await access.ObjectiveService.GetObjectives(project1ID);
                CollectionAssert.That.AreEquivalent(new[] { expected[0] }, project1Objectives, comparer);

                // 4. Can update objective
                var objective = await access.ObjectiveService.Find(objective1ID);
                objective.Status = ObjectiveStatusDto.Ready;
                await access.ObjectiveService.Update(objective);

                var changed = await access.ObjectiveService.Find(objective1ID);
                Assert.AreEqual(ObjectiveStatusDto.Ready, changed.Status);
            }
        }

        [TestMethod]
        public async Task Can_update_bim_elements()
        {
            using var transaction = Fixture.Connection.BeginTransaction();
            using (var context = Fixture.CreateContext(transaction))
            {
                var api = new DocumentManagementApi(context);
                var access = await api.Register(new UserToCreateDto("vpupkin", "123", "Vasily Pupkin"));

                var tasktype = await access.ObjectiveTypeService.Add("Задание");
                var project1ID = await access.ProjectService.AddToUser(access.CurrentUser.ID, "Project 1");

                var userProjects = await access.ProjectService.GetUserProjects(access.CurrentUser.ID);
                var item1 = await access.ItemService.Add(new ItemToCreateDto(@"C:\Windows\Temp\abra.tmp", ItemTypeDto.File), project1ID);

                var creationTime = DateTime.Parse("2020-11-18T10:50:00.0000000Z");
                var dueTime = creationTime.AddDays(1);

                var newObjective1 = new ObjectiveToCreateDto()
                {
                    AuthorID = access.CurrentUser.ID,
                    CreationDate = creationTime,
                    DueDate = dueTime,
                    Title = "Make cookies",
                    Description = "Mmm, cookies!",
                    Status = ObjectiveStatusDto.Open,
                    TaskType = tasktype,
                    ParentObjectiveID = null,
                    ProjectID = project1ID,
                    BimElements = null
                };

                var objID = await access.ObjectiveService.Add(newObjective1);

                var obj = await access.ObjectiveService.Find(objID);
                Assert.IsNotNull(obj.BimElements);
                Assert.AreEqual(0, obj.BimElements.Count());

                obj.BimElements = new List<BimElementDto>()
                {
                    new BimElementDto() { ItemID = item1, GlobalID = "BIM1" },
                    new BimElementDto() { ItemID = item1, GlobalID = "BIM2" }
                };
                await access.ObjectiveService.Update(obj);

                var bimComparer = new DelegateComparer<BimElementDto>((x, y) => x.ItemID == y.ItemID && x.GlobalID == y.GlobalID);

                var added = await access.ObjectiveService.Find(objID);
                CollectionAssert.That.AreEquivalent(obj.BimElements, added.BimElements, bimComparer);

                obj.BimElements = new List<BimElementDto>()
                {
                    new BimElementDto() { ItemID = item1, GlobalID = "BIM1" },
                    new BimElementDto() { ItemID = item1, GlobalID = "BIM3" },
                    new BimElementDto() { ItemID = item1, GlobalID = "BIM4" }
                };
                await access.ObjectiveService.Update(obj);
                added = await access.ObjectiveService.Find(objID);
                CollectionAssert.That.AreEquivalent(obj.BimElements, added.BimElements, bimComparer);
            }
        }

        [TestMethod]
        public async Task Can_query_requred_dynamic_fields()
        {
            using var transaction = Fixture.Connection.BeginTransaction();
            using (var context = Fixture.CreateContext(transaction))
            {
                var api = new DocumentManagementApi(context);
                var access = await api.Register(new UserToCreateDto("vpupkin", "123", "Vasily Pupkin"));

                //TODO: how to test?
                Assert.Fail();
            }
        }
    }
}
