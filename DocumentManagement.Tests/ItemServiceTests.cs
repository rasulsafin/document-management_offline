using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MRS.DocumentManagement.Database;
using MRS.DocumentManagement.Database.Extensions;
using MRS.DocumentManagement.Database.Models;
using MRS.DocumentManagement.Interface.Dtos;
using MRS.DocumentManagement.Services;
using MRS.DocumentManagement.Tests.Utility;
using MRS.DocumentManagement.Utility;

namespace MRS.DocumentManagement.Tests
{
    [TestClass]
    public class ItemServiceTests
    {

        private static SharedDatabaseFixture Fixture { get; set; }
        private static DMContext Context => Fixture.Context;

        private static ItemService service;
        private static IMapper mapper;

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

                var projects = MockData.DEFAULT_PROJECTS;
                var objectives = MockData.DEFAULT_OBJECTIVES;
                var items = MockData.DEFAULT_ITEMS;
                var objectiveTypes = MockData.DEFAULT_OBJECTIVE_TYPES;
                context.Projects.AddRange(projects);
                context.ObjectiveTypes.AddRange(objectiveTypes);
                context.SaveChanges();

                objectives.ForEach(o =>
                {
                    o.ProjectID = projects[0].ID;
                    o.ObjectiveTypeID = objectiveTypes[0].ID;
                });
                context.Objectives.AddRange(objectives);

                items[0].ProjectID = projects[0].ID;
                items[1].ProjectID = projects[0].ID;

                context.Items.AddRange(items);
                context.SaveChanges();

                context.ObjectiveItems.AddRange(new List<ObjectiveItem>
                {
                    new ObjectiveItem { ItemID = items[0].ID, ObjectiveID = objectives[0].ID },
                    new ObjectiveItem { ItemID = items[1].ID, ObjectiveID = objectives[0].ID },
                });

                context.SaveChanges();
            });

            service = new ItemService(Fixture.Context, mapper);
        }

        [TestCleanup]
        public void Cleanup()
            => Fixture.Dispose();

        [TestMethod]
        public async Task Find_ExistingItem_ReturnsItem()
        {
            var existingItem = Context.Items.Unsynchronized().First();
            var dtoId = new ID<ItemDto>(existingItem.ID);

            var result = await service.Find(dtoId);

            Assert.AreEqual(dtoId, result.ID);
            Assert.AreEqual(existingItem.RelativePath, result.RelativePath);
            Assert.AreEqual(existingItem.ItemType, (int)result.ItemType);
        }

        [TestMethod]
        public async Task Find_NotExistingItem_ReturnsNull()
        {
            var dtoId = ID<ItemDto>.InvalidID;

            var result = await service.Find(dtoId);

            Assert.IsNull(result);
        }

        [TestMethod]
        public async Task GetProjectItems_ExistingProjectWithItems_ReturnsEnumerableWithItems()
        {
            var existingProject = Context.Projects.Include(x => x.Items).First(p => p.Items.Any());
            var projectItems = existingProject.Items;

            var result = await service.GetItems(new ID<ProjectDto>(existingProject.ID));

            Assert.AreEqual(projectItems.Count(), result.Count());
            projectItems.ToList().ForEach(i =>
            {
                Assert.IsTrue(result.Any(ri => (int)ri.ID == i.ID
                                               && (int)ri.ItemType == i.ItemType
                                               && ri.RelativePath == i.RelativePath));
            });
        }

        [TestMethod]
        public async Task GetProjectItems_ExistingProjectWithoutItems_ReturnsEmptyEnumerable()
        {
            var existingProject = Context.Projects.Unsynchronized().First(p => !p.Items.Any());

            var result = await service.GetItems(new ID<ProjectDto>(existingProject.ID));

            Assert.IsFalse(result.Any());
        }

        [TestMethod]
        public async Task GetProjectItems_NotExistingProject_ReturnsEmptyEnumerable()
        {
            var result = await service.GetItems(ID<ProjectDto>.InvalidID);

            Assert.IsFalse(result.Any());
        }

        [TestMethod]
        public async Task GetObjectiveItems_ExistingObjectiveWithItems_ReturnsEnumerableWithItems()
        {
            var existingObjective = Context.Objectives.Unsynchronized().First(o => o.Items.Any());
            var objectiveItems = existingObjective.Items.Select(oi => oi.Item);

            var result = await service.GetItems(new ID<ObjectiveDto>(existingObjective.ID));

            Assert.AreEqual(objectiveItems.Count(), result.Count());
            objectiveItems.ToList().ForEach(i =>
            {
                Assert.IsTrue(result.Any(ri => (int)ri.ID == i.ID
                                               && (int)ri.ItemType == i.ItemType
                                               && ri.RelativePath == i.RelativePath));
            });
        }

        [TestMethod]
        public async Task GetObjectiveItems_ExistingObjectiveWithoutItems_ReturnsEmptyEnumerable()
        {
            var existingObjective = Context.Objectives.Unsynchronized().First(o => !o.Items.Any());

            var result = await service.GetItems(new ID<ObjectiveDto>(existingObjective.ID));

            Assert.IsFalse(result.Any());
        }

        [TestMethod]
        public async Task GetObjectiveItems_NotExistingObjective_ReturnsEmptyEnumerable()
        {
            var result = await service.GetItems(ID<ObjectiveDto>.InvalidID);

            Assert.IsFalse(result.Any());
        }

        [TestMethod]
        public async Task Update_ExistingItem_ReturnsTrue()
        {
            var existingItem = Context.Items.Unsynchronized().First();
            var guid = Guid.NewGuid();
            var newItemType = existingItem.ItemType != 1 ? 1 : 2;
            var newName = $"newName{guid}";
            var item = new ItemDto
            {
                ID = new ID<ItemDto>(existingItem.ID),
                ItemType = (ItemType)newItemType,
                RelativePath = newName,
            };

            var result = await service.Update(item);

            var updatedItem = Context.Items.Unsynchronized().First(i => i.ID == existingItem.ID);
            Assert.IsTrue(result);
            Assert.IsTrue(updatedItem.ItemType == newItemType);
            Assert.IsTrue(updatedItem.RelativePath == newName);
        }

        [TestMethod]
        public async Task Update_NotExistingItem_ReturnsFalse()
        {
            var item = new ItemDto { ID = ID<ItemDto>.InvalidID };

            var result = await service.Update(item);

            Assert.IsFalse(result);
        }

        //        //[TestMethod]
        //        //public async Task Can_add_and_query_item_for_project()
        //        //{
        //        //    using var transaction = Fixture.Connection.BeginTransaction();
        //        //    using (var context = Fixture.CreateContext(transaction))
        //        //    {
        //        //        var api = new DocumentManagementApi(context);
        //        //        var access = await api.Register(new UserToCreateDto("vpupkin", "123", "Vasily Pupkin"));

        //        //        var project1ID = await access.ProjectService.AddToUser(access.CurrentUser.ID, "Project 1");
        //        //        var item11ID = await access.ItemService.Add(new ItemToCreateDto(@"C:\Windows\Temp\abra.tmp", "externalId", ItemTypeDto.File), project1ID);
        //        //        var item12ID = await access.ItemService.Add(new ItemToCreateDto(@"C:\Windows\Temp\cadabra.tmp", "externalId", ItemTypeDto.Bim), project1ID);

        //        //        Assert.IsTrue(item11ID.IsValid);
        //        //        Assert.IsTrue(item12ID.IsValid);

        //        //        var project2ID = await access.ProjectService.AddToUser(access.CurrentUser.ID, "Project 2");
        //        //        var item21ID = await access.ItemService.Add(new ItemToCreateDto(@"C:\Windows\Temp\shvabra.tmp", "externalId", ItemTypeDto.Bim), project2ID);
        //        //        var item22ID = await access.ItemService.Add(new ItemToCreateDto(@"C:\Windows\Temp\dinozavra.tmp", "externalId", ItemTypeDto.Media), project2ID);

        //        //        Assert.IsTrue(item21ID.IsValid);
        //        //        Assert.IsTrue(item22ID.IsValid);

        //        //        var items1 = await access.ItemService.GetItems(project1ID);
        //        //        var expected = new ItemDto[]
        //        //        {
        //        //            new ItemDto(){ ID = item11ID, ItemType = ItemTypeDto.File, Name = @"C:\Windows\Temp\abra.tmp" },
        //        //            new ItemDto(){ ID = item12ID, ItemType = ItemTypeDto.Bim, Name = @"C:\Windows\Temp\cadabra.tmp" },
        //        //        };
        //        //        CollectionAssert.That.AreEquivalent(expected, items1, new ItemComparer());

        //        //        var items2 = await access.ItemService.GetItems(project2ID);
        //        //        expected = new ItemDto[]
        //        //        {
        //        //            new ItemDto(){ ID = item21ID, ItemType = ItemTypeDto.Bim, Name = @"C:\Windows\Temp\shvabra.tmp" },
        //        //            new ItemDto(){ ID = item22ID, ItemType = ItemTypeDto.Media, Name = @"C:\Windows\Temp\dinozavra.tmp" },
        //        //        };
        //        //        CollectionAssert.That.AreEquivalent(expected, items2, new ItemComparer());
        //        //    }
        //        //}

        //        //[TestMethod]
        //        //public async Task Can_add_and_query_item_for_objective()
        //        //{
        //        //    using var transaction = Fixture.Connection.BeginTransaction();
        //        //    using (var context = Fixture.CreateContext(transaction))
        //        //    {
        //        //        var api = new DocumentManagementApi(context);
        //        //        var access = await api.Register(new UserToCreateDto("vpupkin", "123", "Vasily Pupkin"));

        //        //        var project1ID = await access.ProjectService.AddToUser(access.CurrentUser.ID, "Project 1");
        //        //        var taskTypeID = await access.ObjectiveTypeService.Add("Task");

        //        //        var creationTime = DateTime.Parse("2020-11-18T10:50:00.0000000Z");
        //        //        var dueTime = creationTime.AddDays(1);

        //        //        var newObjective1 = new ObjectiveToCreateDto()
        //        //        {
        //        //            AuthorID = access.CurrentUser.ID,
        //        //            CreationDate = creationTime,
        //        //            DueDate = dueTime,
        //        //            Title = "Make cookies",
        //        //            Description = "Mmm, cookies!",
        //        //            Status = ObjectiveStatus.Open,
        //        //            ObjectiveTypeID = taskTypeID,
        //        //            ParentObjectiveID = null,
        //        //            ProjectID = project1ID
        //        //        };

        //        //        var objective1ID = await access.ObjectiveService.Add(newObjective1);
        //        //        Assert.IsTrue(objective1ID.IsValid);

        //        //        var item1ID = await access.ItemService.Add(new ItemToCreateDto(@"C:\Windows\Temp\abra.tmp", "externalId", ItemTypeDto.File), objective1ID);
        //        //        var item2ID = await access.ItemService.Add(new ItemToCreateDto(@"C:\Windows\Temp\cadabra.tmp", "externalId", ItemTypeDto.Bim), objective1ID);
        //        //        Assert.IsTrue(item1ID.IsValid);
        //        //        Assert.IsTrue(item2ID.IsValid);

        //        //        var items = await access.ItemService.GetItems(objective1ID);
        //        //        var expected = new ItemDto[]
        //        //        {
        //        //            new ItemDto(){ ID = item1ID, ItemType = ItemTypeDto.File, Name = @"C:\Windows\Temp\abra.tmp" },
        //        //            new ItemDto(){ ID = item2ID, ItemType = ItemTypeDto.Bim, Name = @"C:\Windows\Temp\cadabra.tmp" },
        //        //        };
        //        //        CollectionAssert.That.AreEquivalent(expected, items, new ItemComparer());
        //        //    }
        //        //}

        //        [TestMethod]
        //        public async Task Can_link_items_to_project()
        //        {
        //            //using var transaction = Fixture.Connection.BeginTransaction();
        //            //using (var context = Fixture.CreateContext(transaction))
        //            //{
        //            //    var api = new DocumentManagementApi(context);
        //            //    var access = await api.Register(new UserToCreateDto("vpupkin", "123", "Vasily Pupkin"));

        //            //    var project1 = await access.ProjectService.AddToUser(access.CurrentUser.ID, "Project 1");
        //            //    var project2 = await access.ProjectService.AddToUser(access.CurrentUser.ID, "Project 2");

        //            //    var item1 = await access.ItemService.Add(new ItemToCreateDto(@"C:\Windows\Temp\abra.tmp", "externalId", ItemTypeDto.File), project1);
        //            //    var item2 = await access.ItemService.Add(new ItemToCreateDto(@"C:\Windows\Temp\cadabra.tmp", "externalId", ItemTypeDto.Bim), project1);
        //            //    var item3 = await access.ItemService.Add(new ItemToCreateDto(@"C:\Windows\Temp\shvabra.tmp", "externalId", ItemTypeDto.Bim), project2);
        //            //    var item4 = await access.ItemService.Add(new ItemToCreateDto(@"C:\Windows\Temp\dinozavra.tmp", "externalId", ItemTypeDto.Media), project2);

        //            //    var expected = new ItemDto[]
        //            //    {
        //            //        new ItemDto(){ ID = item1, ItemType = ItemTypeDto.File, Name = @"C:\Windows\Temp\abra.tmp" },
        //            //        new ItemDto(){ ID = item2, ItemType = ItemTypeDto.Bim, Name = @"C:\Windows\Temp\cadabra.tmp" },
        //            //        new ItemDto(){ ID = item3, ItemType = ItemTypeDto.Bim, Name = @"C:\Windows\Temp\shvabra.tmp" },
        //            //        new ItemDto(){ ID = item4, ItemType = ItemTypeDto.Media, Name = @"C:\Windows\Temp\dinozavra.tmp" },
        //            //    };

        //            //    var project1items = await access.ItemService.GetItems(project1);
        //            //    CollectionAssert.That.AreEquivalent(new[] { expected[0], expected[1] }, project1items, new ItemComparer());
        //            //    var project2items = await access.ItemService.GetItems(project2);
        //            //    CollectionAssert.That.AreEquivalent(new[] { expected[2], expected[3] }, project2items, new ItemComparer());

        //            //    //try link already linked
        //            //    await access.ItemService.Link(item1, project1);
        //            //    //should be no changes
        //            //    project1items = await access.ItemService.GetItems(project1);
        //            //    CollectionAssert.That.AreEquivalent(new[] { expected[0], expected[1] }, project1items, new ItemComparer());
        //            //    project2items = await access.ItemService.GetItems(project2);
        //            //    CollectionAssert.That.AreEquivalent(new[] { expected[2], expected[3] }, project2items, new ItemComparer());

        //            //    //cross link files
        //            //    await access.ItemService.Link(item1, project2);
        //            //    await access.ItemService.Link(item2, project2);
        //            //    await access.ItemService.Link(item3, project1);

        //            //    project1items = await access.ItemService.GetItems(project1);
        //            //    CollectionAssert.That.AreEquivalent(new[] { expected[0], expected[1], expected[2] }, project1items, new ItemComparer());
        //            //    project2items = await access.ItemService.GetItems(project2);
        //            //    CollectionAssert.That.AreEquivalent(new[] { expected[2], expected[3], expected[0], expected[1] }, project2items, new ItemComparer());

        //            //    //unlink files
        //            //    await access.ItemService.Unlink(item4, project2);
        //            //    project2items = await access.ItemService.GetItems(project2);
        //            //    CollectionAssert.That.AreEquivalent(new[] { expected[2], expected[0], expected[1] }, project2items, new ItemComparer());

        //            //    //try unlink alredy not linked item - should be no changes
        //            //    await access.ItemService.Unlink(item4, project2);
        //            //    project2items = await access.ItemService.GetItems(project2);
        //            //    CollectionAssert.That.AreEquivalent(new[] { expected[2], expected[0], expected[1] }, project2items, new ItemComparer());
        //            //}
        //        }

        //        [TestMethod]
        //        public async Task Can_link_items_to_objective()
        //        {
        //            //using var transaction = Fixture.Connection.BeginTransaction();
        //            //using (var context = Fixture.CreateContext(transaction))
        //            //{
        //            //    var api = new DocumentManagementApi(context);
        //            //    var access = await api.Register(new UserToCreateDto("vpupkin", "123", "Vasily Pupkin"));

        //            //    var project = await access.ProjectService.AddToUser(access.CurrentUser.ID, "Project 1");
        //            //    var taskType = await access.ObjectiveTypeService.Add("Task");

        //            //    var creationTime = DateTime.Parse("2020-11-18T10:50:00.0000000Z");
        //            //    var dueTime = creationTime.AddDays(1);

        //            //    var newObjective1 = new ObjectiveToCreateDto()
        //            //    {
        //            //        AuthorID = access.CurrentUser.ID,
        //            //        CreationDate = creationTime,
        //            //        DueDate = dueTime,
        //            //        Title = "Make cookies",
        //            //        Description = "Mmm, cookies!",
        //            //        Status = ObjectiveStatus.Open,
        //            //        ObjectiveTypeID = taskType,
        //            //        ParentObjectiveID = null,
        //            //        ProjectID = project
        //            //    };

        //            //    var newObjective2 = new ObjectiveToCreateDto()
        //            //    {
        //            //        AuthorID = access.CurrentUser.ID,
        //            //        CreationDate = creationTime,
        //            //        DueDate = dueTime,
        //            //        Title = "Погладить кота",
        //            //        Description = "Погладь кота!",
        //            //        Status = ObjectiveStatus.Open,
        //            //        ObjectiveTypeID = taskType,
        //            //        ParentObjectiveID = null,
        //            //        ProjectID = project
        //            //    };

        //            //    var objective1 = await access.ObjectiveService.Add(newObjective1);
        //            //    var objective2 = await access.ObjectiveService.Add(newObjective2);

        //            //    var item1 = await access.ItemService.Add(new ItemToCreateDto(@"C:\Windows\Temp\abra.tmp", "externalId", ItemTypeDto.File), project);
        //            //    var item2 = await access.ItemService.Add(new ItemToCreateDto(@"C:\Windows\Temp\cadabra.tmp", "externalId", ItemTypeDto.Bim), project);
        //            //    var item3 = await access.ItemService.Add(new ItemToCreateDto(@"C:\Windows\Temp\shvabra.tmp", "externalId", ItemTypeDto.Bim), objective1);
        //            //    var item4 = await access.ItemService.Add(new ItemToCreateDto(@"C:\Windows\Temp\dinozavra.tmp", "externalId", ItemTypeDto.Media), objective2);

        //            //    var expected = new ItemDto[]
        //            //    {
        //            //        new ItemDto(){ ID = item1, ItemType = ItemTypeDto.File, Name = @"C:\Windows\Temp\abra.tmp" },
        //            //        new ItemDto(){ ID = item2, ItemType = ItemTypeDto.Bim, Name = @"C:\Windows\Temp\cadabra.tmp" },
        //            //        new ItemDto(){ ID = item3, ItemType = ItemTypeDto.Bim, Name = @"C:\Windows\Temp\shvabra.tmp" },
        //            //        new ItemDto(){ ID = item4, ItemType = ItemTypeDto.Media, Name = @"C:\Windows\Temp\dinozavra.tmp" },
        //            //    };

        //            //    var obj1items = await access.ItemService.GetItems(objective1);
        //            //    CollectionAssert.That.AreEquivalent(new[] { expected[2] }, obj1items, new ItemComparer());
        //            //    var obj2items = await access.ItemService.GetItems(objective2);
        //            //    CollectionAssert.That.AreEquivalent(new[] { expected[3] }, obj2items, new ItemComparer());

        //            //    //cross link files
        //            //    await access.ItemService.Link(item1, objective1);
        //            //    await access.ItemService.Link(item1, objective2);
        //            //    await access.ItemService.Link(item2, objective2);

        //            //    obj1items = await access.ItemService.GetItems(objective1);
        //            //    CollectionAssert.That.AreEquivalent(new[] { expected[0], expected[2] }, obj1items, new ItemComparer());
        //            //    obj2items = await access.ItemService.GetItems(objective2);
        //            //    CollectionAssert.That.AreEquivalent(new[] { expected[0], expected[1], expected[3] }, obj2items, new ItemComparer());

        //            //    //unlink files
        //            //    await access.ItemService.Unlink(item4, objective2);
        //            //    obj2items = await access.ItemService.GetItems(objective2);
        //            //    CollectionAssert.That.AreEquivalent(new[] { expected[0], expected[1] }, obj2items, new ItemComparer());
        //            //}
        //        }

        //        //[TestMethod]
        //        //public async Task Can_find_items()
        //        //{
        //        //    using var transaction = Fixture.Connection.BeginTransaction();
        //        //    using (var context = Fixture.CreateContext(transaction))
        //        //    {
        //        //        var api = new DocumentManagementApi(context);
        //        //        var access = await api.Register(new UserToCreateDto("vpupkin", "123", "Vasily Pupkin"));

        //        //        var project1 = await access.ProjectService.AddToUser(access.CurrentUser.ID, "Project 1");

        //        //        var item1 = await access.ItemService.Add(new ItemToCreateDto(@"C:\Windows\Temp\abra.tmp", "externalId", ItemTypeDto.File), project1);
        //        //        var item2 = await access.ItemService.Add(new ItemToCreateDto(@"C:\Windows\Temp\cadabra.tmp", "externalId", ItemTypeDto.Bim), project1);
        //        //        var item3 = await access.ItemService.Add(new ItemToCreateDto(@"C:\Windows\Temp\shvabra.tmp", "externalId", ItemTypeDto.Media), project1);

        //        //        var expected = new ItemDto[]
        //        //        {
        //        //            new ItemDto(){ ID = item1, ItemType = ItemTypeDto.File, Name = @"C:\Windows\Temp\abra.tmp" },
        //        //            new ItemDto(){ ID = item2, ItemType = ItemTypeDto.Bim, Name = @"C:\Windows\Temp\cadabra.tmp" },
        //        //            new ItemDto(){ ID = item3, ItemType = ItemTypeDto.Media, Name = @"C:\Windows\Temp\shvabra.tmp" },
        //        //        };

        //        //        var comparer = new ItemComparer();

        //        //        var found = await access.ItemService.Find(item1);
        //        //        Assert.IsTrue(comparer.Equals(expected[0], found));
        //        //    }
        //        //}

        //        //[TestMethod]
        //        //public async Task Can_update_items()
        //        //{
        //        //    using var transaction = Fixture.Connection.BeginTransaction();
        //        //    using (var context = Fixture.CreateContext(transaction))
        //        //    {
        //        //        var api = new DocumentManagementApi(context);
        //        //        var access = await api.Register(new UserToCreateDto("vpupkin", "123", "Vasily Pupkin"));

        //        //        var project1 = await access.ProjectService.AddToUser(access.CurrentUser.ID, "Project 1");
        //        //        var item1 = await access.ItemService.Add(new ItemToCreateDto(@"C:\Windows\Temp\abra.tmp", "externalId", ItemTypeDto.File), project1);

        //        //        await access.ItemService.Update(new ItemDto() { ID = item1, ItemType = ItemTypeDto.Bim, Name = "cadabra.txt" });

        //        //        var found = await access.ItemService.Find(item1);
        //        //        Assert.IsNotNull(found);
        //        //        Assert.AreEqual(item1, found.ID);
        //        //        Assert.AreEqual("cadabra.txt", found.Name);
        //        //        Assert.AreEqual(ItemTypeDto.Bim, found.ItemType);
        //        //    }
        //        //}
    }
}
