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
        private static SharedDatabaseFixture Fixture { get; set; }
        private static IMapper mapper;
        private static ItemHelper helper;
        private static ItemComparer comparer;

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
            var objectiveType = typeof(Objective);
            var parentId = -1;
            var item = new ItemDto { ID = new ID<ItemDto>(existingItem.ID) };

            var result = await helper.CheckItemToLink(context, mapper, item, objectiveType, parentId);

            Assert.IsNotNull(result);
            Assert.IsTrue(comparer.NotNullEquals(existingItem, result));
        }

        [TestMethod]
        public async Task CheckItemToLink_ExistingItemNotLinkedToProjectParent_ReturnsItem()
        {
            var context = Fixture.Context;
            var existingItem = context.Items.Unsynchronized().First();
            var objectiveType = typeof(Project);
            var parentId = -1;
            var item = new ItemDto { ID = new ID<ItemDto>(existingItem.ID) };

            var result = await helper.CheckItemToLink(context, mapper, item, objectiveType, parentId);

            Assert.IsNotNull(result);
            Assert.IsTrue(comparer.NotNullEquals(existingItem, result));
        }

        [TestMethod]
        public async Task CheckItemToLink_ExistingItemLinkedToObjectiveParent_ReturnsNull()
        {
            var context = Fixture.Context;
            var existingItem = context.Items.Unsynchronized().First(i => context.ObjectiveItems.Any(oi => oi.ItemID == i.ID));
            var objectiveType = typeof(Objective);
            var parentId = existingItem.Objectives.First().ObjectiveID;
            var item = new ItemDto { ID = new ID<ItemDto>(existingItem.ID) };

            var result = await helper.CheckItemToLink(context, mapper, item, objectiveType, parentId);

            Assert.IsNull(result);
        }

        [TestMethod]
        public async Task CheckItemToLink_ExistingItemLinkedToProjectParent_ReturnsNull()
        {
            var context = Fixture.Context;
            var existingItem = context.Items.Unsynchronized().First(i => i.ProjectID != null);
            var projectType = typeof(Project);
            var parentId = existingItem.ProjectID ?? -1;
            var item = new ItemDto { ID = new ID<ItemDto>(existingItem.ID) };

            var result = await helper.CheckItemToLink(context, mapper, item, projectType, parentId);

            Assert.IsNull(result);
        }

        [TestMethod]
        public async Task CheckItemToLink_NotExistingItemWithObjectiveParent_ReturnsItemAddedToDb()
        {
            var context = Fixture.Context;
            var guid = Guid.NewGuid();
            var externalId = $"ExternalItemId{guid}";
            var name = $"Name{guid}";
            var itemType = ItemTypeDto.Bim;
            var objectiveType = typeof(Objective);
            var parentId = context.Objectives.Unsynchronized().First().ID;
            var itemsCount = context.Items.Unsynchronized().Count();
            var item = new ItemDto { ExternalID = externalId, ItemType = itemType, Name = name };

            var result = await helper.CheckItemToLink(context, mapper, item, objectiveType, parentId);

            var addedItem = context.Items.Unsynchronized().FirstOrDefault(i => i.ExternalID == externalId
                                                                               && i.ItemType == (int)itemType
                                                                               && i.RelativePath == name);
            Assert.IsNotNull(result);
            Assert.IsNotNull(addedItem);
            Assert.AreEqual(itemsCount + 1, context.Items.Unsynchronized().Count());
        }

        [TestMethod]
        public async Task CheckItemToLink_NotExistingItemWithProjectParent_ReturnsItemAddedToDb()
        {
            var context = Fixture.Context;
            var guid = Guid.NewGuid();
            var externalId = $"ExternalItemId{guid}";
            var name = $"Name{guid}";
            var itemType = ItemTypeDto.Bim;
            var projectType = typeof(Project);
            var parentId = context.Projects.Unsynchronized().First().ID;
            var itemsCount = context.Items.Unsynchronized().Count();
            var item = new ItemDto { ExternalID = externalId, ItemType = itemType, Name = name };

            var result = await helper.CheckItemToLink(context, mapper, item, projectType, parentId);

            var addedItem = context.Items.Unsynchronized().FirstOrDefault(i => i.ExternalID == externalId
                                                                                                         && i.ItemType == (int)itemType
                                                                                                         && i.RelativePath == name);
            Assert.IsNotNull(result);
            Assert.IsNotNull(addedItem);
            Assert.AreEqual(itemsCount + 1, context.Items.Unsynchronized().Count());
        }
    }
}
