using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using System.Threading.Tasks;
using DocumentManagement.Interface.Models;
using DocumentManagement.Tests.Utility;
using System;
using System.Collections.Generic;

namespace DocumentManagement.Tests
{
    [TestClass]
    public class ItemServiceTests
    {
        public static SharedDatabaseFixture Fixture { get; private set; }

        [ClassInitialize]
        public static void Setup(TestContext _)
        {
            Fixture = new SharedDatabaseFixture();
        }

        [ClassCleanup]
        public static void Cleanup()
        {
            Fixture.Dispose();
        }

        [TestMethod]
        public async Task Can_add_and_query_item_for_project()
        {
            using var transaction = Fixture.Connection.BeginTransaction();
            using (var context = Fixture.CreateContext(transaction))
            {
                var api = new DocumentManagementApi(context);
                var access = await api.Register(new NewUser("vpupkin", "123", "Vasily Pupkin"));

                var project1ID = await access.ProjectService.Add(access.CurrentUser.ID, "Project 1");
                var item11ID = await access.ItemService.Add(new NewItem(@"C:\Windows\Temp\abra.tmp", ItemType.File), project1ID);
                var item12ID = await access.ItemService.Add(new NewItem(@"C:\Windows\Temp\cadabra.tmp", ItemType.Bim), project1ID);

                Assert.IsTrue(item11ID.IsValid);
                Assert.IsTrue(item12ID.IsValid);

                var project2ID = await access.ProjectService.Add(access.CurrentUser.ID, "Project 2");
                var item21ID = await access.ItemService.Add(new NewItem(@"C:\Windows\Temp\shvabra.tmp", ItemType.Bim), project2ID);
                var item22ID = await access.ItemService.Add(new NewItem(@"C:\Windows\Temp\dinozavra.tmp", ItemType.Media), project2ID);

                Assert.IsTrue(item21ID.IsValid);
                Assert.IsTrue(item22ID.IsValid);

                var items1 = await access.ItemService.GetItems(project1ID);
                var expected = new Item[]
                {
                    new Item(){ ID = item11ID, ItemType = ItemType.File, Path = @"C:\Windows\Temp\abra.tmp" },
                    new Item(){ ID = item12ID, ItemType = ItemType.Bim, Path = @"C:\Windows\Temp\cadabra.tmp" },
                };
                CollectionAssert.That.AreEquivalent(expected, items1, new ItemComparer());

                var items2 = await access.ItemService.GetItems(project2ID);
                expected = new Item[]
                {
                    new Item(){ ID = item21ID, ItemType = ItemType.Bim, Path = @"C:\Windows\Temp\shvabra.tmp" },
                    new Item(){ ID = item22ID, ItemType = ItemType.Media, Path = @"C:\Windows\Temp\dinozavra.tmp" },
                };
                CollectionAssert.That.AreEquivalent(expected, items2, new ItemComparer());
            }
        }

        [TestMethod]
        public async Task Can_add_and_query_item_for_objective()
        {
            using var transaction = Fixture.Connection.BeginTransaction();
            using (var context = Fixture.CreateContext(transaction))
            {
                var api = new DocumentManagementApi(context);
                var access = await api.Register(new NewUser("vpupkin", "123", "Vasily Pupkin"));

                var project1ID = await access.ProjectService.Add(access.CurrentUser.ID, "Project 1");
                var taskTypeID = await access.ObjectiveTypeService.Add("Task");

                var creationTime = DateTime.Parse("2020-11-18T10:50:00.0000000Z");
                var dueTime = creationTime.AddDays(1);

                var newObjective1 = new NewObjective()
                {
                    AuthorID = access.CurrentUser.ID,
                    CreationDate = creationTime,
                    DueDate = dueTime,
                    Title = "Make cookies",
                    Description = "Mmm, cookies!",
                    Status = ObjectiveStatus.Open,
                    TaskType = taskTypeID,
                    ParentObjectiveID = null,
                    ProjectID = project1ID
                };

                var objective1ID = await access.ObjectiveService.Add(newObjective1);
                Assert.IsTrue(objective1ID.IsValid);

                var item1ID = await access.ItemService.Add(new NewItem(@"C:\Windows\Temp\abra.tmp", ItemType.File), objective1ID);
                var item2ID = await access.ItemService.Add(new NewItem(@"C:\Windows\Temp\cadabra.tmp", ItemType.Bim), objective1ID);
                Assert.IsTrue(item1ID.IsValid);
                Assert.IsTrue(item2ID.IsValid);

                var items = await access.ItemService.GetItems(objective1ID);
                var expected = new Item[]
                {
                    new Item(){ ID = item1ID, ItemType = ItemType.File, Path = @"C:\Windows\Temp\abra.tmp" },
                    new Item(){ ID = item2ID, ItemType = ItemType.Bim, Path = @"C:\Windows\Temp\cadabra.tmp" },
                };
                CollectionAssert.That.AreEquivalent(expected, items, new ItemComparer());
            }
        }

        [TestMethod]
        public async Task Can_link_items_to_project()
        {
            using var transaction = Fixture.Connection.BeginTransaction();
            using (var context = Fixture.CreateContext(transaction))
            {
                var api = new DocumentManagementApi(context);
                var access = await api.Register(new NewUser("vpupkin", "123", "Vasily Pupkin"));

                var project1 = await access.ProjectService.Add(access.CurrentUser.ID, "Project 1");
                var project2 = await access.ProjectService.Add(access.CurrentUser.ID, "Project 2");

                var item1 = await access.ItemService.Add(new NewItem(@"C:\Windows\Temp\abra.tmp", ItemType.File), project1);
                var item2 = await access.ItemService.Add(new NewItem(@"C:\Windows\Temp\cadabra.tmp", ItemType.Bim), project1);
                var item3 = await access.ItemService.Add(new NewItem(@"C:\Windows\Temp\shvabra.tmp", ItemType.Bim), project2);
                var item4 = await access.ItemService.Add(new NewItem(@"C:\Windows\Temp\dinozavra.tmp", ItemType.Media), project2);

                var expected = new Item[]
                {
                    new Item(){ ID = item1, ItemType = ItemType.File, Path = @"C:\Windows\Temp\abra.tmp" },
                    new Item(){ ID = item2, ItemType = ItemType.Bim, Path = @"C:\Windows\Temp\cadabra.tmp" },
                    new Item(){ ID = item3, ItemType = ItemType.Bim, Path = @"C:\Windows\Temp\shvabra.tmp" },
                    new Item(){ ID = item4, ItemType = ItemType.Media, Path = @"C:\Windows\Temp\dinozavra.tmp" },
                };

                var project1items = await access.ItemService.GetItems(project1);
                CollectionAssert.That.AreEquivalent(new[] { expected[0], expected[1] }, project1items, new ItemComparer());
                var project2items = await access.ItemService.GetItems(project2);
                CollectionAssert.That.AreEquivalent(new[] { expected[2], expected[3] }, project2items, new ItemComparer());

                //try link already linked
                await access.ItemService.Link(item1, project1);
                //should be no changes
                project1items = await access.ItemService.GetItems(project1);
                CollectionAssert.That.AreEquivalent(new[] { expected[0], expected[1] }, project1items, new ItemComparer());
                project2items = await access.ItemService.GetItems(project2);
                CollectionAssert.That.AreEquivalent(new[] { expected[2], expected[3] }, project2items, new ItemComparer());

                //cross link files
                await access.ItemService.Link(item1, project2);
                await access.ItemService.Link(item2, project2);
                await access.ItemService.Link(item3, project1);

                project1items = await access.ItemService.GetItems(project1);
                CollectionAssert.That.AreEquivalent(new[] { expected[0], expected[1], expected[2] }, project1items, new ItemComparer());
                project2items = await access.ItemService.GetItems(project2);
                CollectionAssert.That.AreEquivalent(new[] { expected[2], expected[3], expected[0], expected[1] }, project2items, new ItemComparer());

                //unlink files
                await access.ItemService.Unlink(item4, project2);
                project2items = await access.ItemService.GetItems(project2);
                CollectionAssert.That.AreEquivalent(new[] { expected[2], expected[0], expected[1] }, project2items, new ItemComparer());

                //try unlink alredy not linked item - should be no changes
                await access.ItemService.Unlink(item4, project2);
                project2items = await access.ItemService.GetItems(project2);
                CollectionAssert.That.AreEquivalent(new[] { expected[2], expected[0], expected[1] }, project2items, new ItemComparer());
            }
        }

        [TestMethod]
        public async Task Can_link_items_to_objective()
        {
            using var transaction = Fixture.Connection.BeginTransaction();
            using (var context = Fixture.CreateContext(transaction))
            {
                var api = new DocumentManagementApi(context);
                var access = await api.Register(new NewUser("vpupkin", "123", "Vasily Pupkin"));

                var project = await access.ProjectService.Add(access.CurrentUser.ID, "Project 1");
                var taskType = await access.ObjectiveTypeService.Add("Task");

                var creationTime = DateTime.Parse("2020-11-18T10:50:00.0000000Z");
                var dueTime = creationTime.AddDays(1);

                var newObjective1 = new NewObjective()
                {
                    AuthorID = access.CurrentUser.ID,
                    CreationDate = creationTime,
                    DueDate = dueTime,
                    Title = "Make cookies",
                    Description = "Mmm, cookies!",
                    Status = ObjectiveStatus.Open,
                    TaskType = taskType,
                    ParentObjectiveID = null,
                    ProjectID = project
                };

                var newObjective2 = new NewObjective()
                {
                    AuthorID = access.CurrentUser.ID,
                    CreationDate = creationTime,
                    DueDate = dueTime,
                    Title = "Погладить кота",
                    Description = "Погладь кота!",
                    Status = ObjectiveStatus.Open,
                    TaskType = taskType,
                    ParentObjectiveID = null,
                    ProjectID = project
                };

                var objective1 = await access.ObjectiveService.Add(newObjective1);
                var objective2 = await access.ObjectiveService.Add(newObjective2);

                var item1 = await access.ItemService.Add(new NewItem(@"C:\Windows\Temp\abra.tmp", ItemType.File), project);
                var item2 = await access.ItemService.Add(new NewItem(@"C:\Windows\Temp\cadabra.tmp", ItemType.Bim), project);
                var item3 = await access.ItemService.Add(new NewItem(@"C:\Windows\Temp\shvabra.tmp", ItemType.Bim), objective1);
                var item4 = await access.ItemService.Add(new NewItem(@"C:\Windows\Temp\dinozavra.tmp", ItemType.Media), objective2);

                var expected = new Item[]
                {
                    new Item(){ ID = item1, ItemType = ItemType.File, Path = @"C:\Windows\Temp\abra.tmp" },
                    new Item(){ ID = item2, ItemType = ItemType.Bim, Path = @"C:\Windows\Temp\cadabra.tmp" },
                    new Item(){ ID = item3, ItemType = ItemType.Bim, Path = @"C:\Windows\Temp\shvabra.tmp" },
                    new Item(){ ID = item4, ItemType = ItemType.Media, Path = @"C:\Windows\Temp\dinozavra.tmp" },
                };

                var obj1items = await access.ItemService.GetItems(objective1);
                CollectionAssert.That.AreEquivalent(new[] { expected[2] }, obj1items, new ItemComparer());
                var obj2items = await access.ItemService.GetItems(objective2);
                CollectionAssert.That.AreEquivalent(new[] { expected[3] }, obj2items, new ItemComparer());

                //cross link files
                await access.ItemService.Link(item1, objective1);
                await access.ItemService.Link(item1, objective2);
                await access.ItemService.Link(item2, objective2);

                obj1items = await access.ItemService.GetItems(objective1);
                CollectionAssert.That.AreEquivalent(new[] { expected[0], expected[2] }, obj1items, new ItemComparer());
                obj2items = await access.ItemService.GetItems(objective2);
                CollectionAssert.That.AreEquivalent(new[] { expected[0], expected[1], expected[3] }, obj2items, new ItemComparer());

                //unlink files
                await access.ItemService.Unlink(item4, objective2);
                obj2items = await access.ItemService.GetItems(objective2);
                CollectionAssert.That.AreEquivalent(new[] { expected[0], expected[1] }, obj2items, new ItemComparer());
            }
        }

        [TestMethod]
        public async Task Can_find_items()
        {
            using var transaction = Fixture.Connection.BeginTransaction();
            using (var context = Fixture.CreateContext(transaction))
            {
                var api = new DocumentManagementApi(context);
                var access = await api.Register(new NewUser("vpupkin", "123", "Vasily Pupkin"));

                var project1 = await access.ProjectService.Add(access.CurrentUser.ID, "Project 1");

                var item1 = await access.ItemService.Add(new NewItem(@"C:\Windows\Temp\abra.tmp", ItemType.File), project1);
                var item2 = await access.ItemService.Add(new NewItem(@"C:\Windows\Temp\cadabra.tmp", ItemType.Bim), project1);
                var item3 = await access.ItemService.Add(new NewItem(@"C:\Windows\Temp\shvabra.tmp", ItemType.Media), project1);

                var expected = new Item[]
                {
                    new Item(){ ID = item1, ItemType = ItemType.File, Path = @"C:\Windows\Temp\abra.tmp" },
                    new Item(){ ID = item2, ItemType = ItemType.Bim, Path = @"C:\Windows\Temp\cadabra.tmp" },
                    new Item(){ ID = item3, ItemType = ItemType.Media, Path = @"C:\Windows\Temp\shvabra.tmp" },
                };

                var comparer = new ItemComparer();

                var found = await access.ItemService.Find(item1);
                Assert.IsTrue(comparer.Equals(expected[0], found));

                found = await access.ItemService.Find(@"C:\Windows\Temp\shvabra.tmp");
                Assert.IsTrue(comparer.Equals(expected[2], found));

                found = await access.ItemService.Find("somenonexistentcrap");
                Assert.IsNull(found);
            }
        }

        [TestMethod]
        public async Task Can_update_items()
        {
            using var transaction = Fixture.Connection.BeginTransaction();
            using (var context = Fixture.CreateContext(transaction))
            {
                var api = new DocumentManagementApi(context);
                var access = await api.Register(new NewUser("vpupkin", "123", "Vasily Pupkin"));

                var project1 = await access.ProjectService.Add(access.CurrentUser.ID, "Project 1");
                var item1 = await access.ItemService.Add(new NewItem(@"C:\Windows\Temp\abra.tmp", ItemType.File), project1);

                await access.ItemService.Update(new Item() { ID = item1, ItemType = ItemType.Bim, Path = "cadabra.txt" });

                var found = await access.ItemService.Find("cadabra.txt");
                Assert.IsNotNull(found);
                Assert.AreEqual(item1, found.ID);
                Assert.AreEqual("cadabra.txt", found.Path);
                Assert.AreEqual(ItemType.Bim, found.ItemType);
            }
        }
    }
}
