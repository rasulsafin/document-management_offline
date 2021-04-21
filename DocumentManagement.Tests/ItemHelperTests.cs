using System;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MRS.DocumentManagement.Database.Extensions;
using MRS.DocumentManagement.Database.Models;
using MRS.DocumentManagement.Interface.Dtos;
using MRS.DocumentManagement.Tests.Utility;
using MRS.DocumentManagement.Utility;

namespace MRS.DocumentManagement.Tests
{
    [TestClass]
    public class ItemHelperTests
    {
        private static IMapper mapper;
        private static ItemHelper helper;
        private static ItemComparer comparer;

        private static SharedDatabaseFixture Fixture { get; set; }

        [TestInitialize]
        public void Setup()
        {
            helper = new ItemHelper();
            comparer = new ItemComparer();
            var mapperConfig = new MapperConfiguration(mc => mc.AddProfile(new MappingProfile()));
            mapper = mapperConfig.CreateMapper();

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
                context.Items.AddRange(items);
                context.SaveChanges();

                context.ObjectiveItems.Add(new ObjectiveItem { ItemID = items[0].ID, ObjectiveID = objectives[0].ID });
                context.SaveChanges();
            });
        }

        [TestCleanup]
        public void Cleanup()
            => Fixture.Dispose();

        [TestMethod]
        public async Task CheckItemToLink_ExistingItemNotLinkedToObjectiveParent_ReturnsItem()
        {
            var context = Fixture.Context;
            var existingItem = context.Items.Unsynchronized().First();
            var parent = context.Objectives.First();
            var item = new ItemDto { ID = new ID<ItemDto>(existingItem.ID) };

            var result = await helper.CheckItemToLink(context, mapper, item, parent);

            Assert.IsNotNull(result);
            Assert.IsTrue(comparer.NotNullEquals(existingItem, result));
        }

        [TestMethod]
        public async Task CheckItemToLink_ExistingItemLinkedToObjectiveAndNotLinkedToProjectParent_ReturnsItem()
        {
            var context = Fixture.Context;
            var parent = context.Projects.First();
            var existingItem = context.Items.First(x => x.ProjectID == parent.ID);
            existingItem.ProjectID = null;
            context.Update(existingItem);
            await context.SaveChangesAsync();
            var item = new ItemDto { ID = new ID<ItemDto>(existingItem.ID) };

            var result = await helper.CheckItemToLink(context, mapper, item, parent);

            Assert.IsNotNull(result);
            Assert.IsTrue(comparer.NotNullEquals(existingItem, result));
        }

        [TestMethod]
        public async Task CheckItemToLink_ExistingItemLinkedToObjectiveParent_ReturnsNull()
        {
            var context = Fixture.Context;
            var existingItem = context.Items.Unsynchronized().First(i => context.ObjectiveItems.Any(oi => oi.ItemID == i.ID));
            var parent = existingItem.Objectives.First().Objective;
            var item = new ItemDto { ID = new ID<ItemDto>(existingItem.ID) };

            var result = await helper.CheckItemToLink(context, mapper, item, parent);

            Assert.IsNull(result);
        }

        [TestMethod]
        public async Task CheckItemToLink_ExistingItemLinkedToProjectParent_ReturnsNull()
        {
            var context = Fixture.Context;
            var existingItem = context.Items.Unsynchronized().First(i => i.ProjectID != null);
            var parent = existingItem.Project;
            var item = new ItemDto { ID = new ID<ItemDto>(existingItem.ID) };

            var result = await helper.CheckItemToLink(context, mapper, item, parent);

            Assert.IsNull(result);
        }

        [TestMethod]
        public async Task CheckItemToLink_NotExistingItemWithObjectiveParent_ReturnsItemAddedToDb()
        {
            var context = Fixture.Context;
            var guid = Guid.NewGuid();
            var name = $"Name{guid}";
            var itemType = ItemType.Bim;
            var parent = context.Objectives.First();
            var itemsCount = context.Items.Count();
            var item = new ItemDto { ItemType = itemType, RelativePath = name };

            var result = await helper.CheckItemToLink(context, mapper, item, parent);

            var addedItem = context.Items
               .Unsynchronized()
               .FirstOrDefault(i => i.ItemType == (int)itemType && i.RelativePath == name);

            Assert.IsNotNull(result);
            Assert.IsNotNull(addedItem);
            Assert.AreEqual(itemsCount + 1, context.Items.Unsynchronized().Count());
        }

        [TestMethod]
        public async Task CheckItemToLink_NotExistingItemWithProjectParent_ReturnsItemAddedToDb()
        {
            var context = Fixture.Context;
            var guid = Guid.NewGuid();
            var name = $"Name{guid}";
            var itemType = ItemType.Bim;
            var parentId = context.Projects.First();
            var itemsCount = context.Items.Count();
            var item = new ItemDto { ItemType = itemType, RelativePath = name };

            var result = await helper.CheckItemToLink(context, mapper, item, parentId);

            var addedItem = context.Items
               .Unsynchronized()
               .FirstOrDefault(i => i.ItemType == (int)itemType && i.RelativePath == name);

            Assert.IsNotNull(result);
            Assert.IsNotNull(addedItem);
            Assert.AreEqual(itemsCount + 1, context.Items.Unsynchronized().Count());
        }
    }
}
