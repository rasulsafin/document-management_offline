using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using System.Threading.Tasks;
using MRS.DocumentManagement.Tests.Utility;
using System;
using System.Collections.Generic;
using MRS.DocumentManagement.Interface.Dtos;
using MRS.DocumentManagement.Interface;
using MRS.DocumentManagement.Services;
using AutoMapper;
using MRS.DocumentManagement.Utility;
using MRS.DocumentManagement.Database.Models;

namespace MRS.DocumentManagement.Tests
{
    [TestClass]
    public class ObjectiveServiceTests
    {
        private static SharedDatabaseFixture Fixture { get; set; }

        private static ObjectiveService service;
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

                var users = MockData.DEFAULT_USERS;
                var projects = MockData.DEFAULT_PROJECTS;
                var objectiveTypes = MockData.DEFAULT_OBJECTIVE_TYPES;
                var objectives = MockData.DEFAULT_OBJECTIVES;
                var items = MockData.DEFAULT_ITEMS;
                var bimElements = MockData.DEFAULT_BIM_ELEMENTS;
                var dynamicFields = MockData.DEFAULT_DYNAMIC_FIELDS;
                context.Users.AddRange(users);
                context.Projects.AddRange(projects);
                context.ObjectiveTypes.AddRange(objectiveTypes);
                context.Items.AddRange(items);
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

                var bimFile = items.First(i => i.ItemType == (int)ItemTypeDto.Bim);
                bimElements.ForEach(e => e.ItemID = bimFile.ID);
                context.BimElements.AddRange(bimElements);

                context.Objectives.AddRange(objectives);
                context.SaveChanges();

                var dynamicField = dynamicFields[0];
                var firstObjective = objectives[0];
                dynamicField.ObjectiveID = firstObjective.ID;
                context.DynamicFields.Add(dynamicField);
                context.ObjectiveItems.AddRange(new ObjectiveItem { ItemID = items[0].ID, ObjectiveID = firstObjective.ID });
                bimElements.ForEach(e =>
                {
                    context.BimElementObjectives.Add(new BimElementObjective
                    {
                        BimElementID = e.ID,
                        ObjectiveID = firstObjective.ID
                    });
                });
                context.SaveChanges();
            });

            service = new ObjectiveService(Fixture.Context, mapper, new ItemHelper(), new SyncService(Fixture.Context));
        }

        [TestCleanup]
        public void Cleanup()
            => Fixture.Dispose();

        [TestMethod]
        public async Task Add_NewObjectiveToCreate_ReturnsObjectiveToList()
        {
            var title = $"Test issue {Guid.NewGuid()}";
            var status = ObjectiveStatus.Open;
            var type = Fixture.Context.ObjectiveTypes.First();
            var project = Fixture.Context.Projects.First();
            var objectivesCount = Fixture.Context.Objectives.Count();
            var objectiveToCreate = new ObjectiveToCreateDto
            {
                Title = title,
                Description = "created for test purpose only",
                Status = status,
                ObjectiveTypeID = new ID<ObjectiveTypeDto>(type.ID),
                ProjectID = new ID<ProjectDto>(project.ID)
            };

            var result = await service.Add(objectiveToCreate);

            Assert.AreEqual(title, result.Title);
            Assert.AreEqual(status, result.Status);
            Assert.AreEqual(type.ID, (int)result.ObjectiveType.ID);
            Assert.AreEqual(objectivesCount + 1, Fixture.Context.Objectives.Count());
        }

        [TestMethod]
        public async Task Add_NewObjectiveToCreateWithExistingBimElements_ReturnsObjectiveToListAndDoesntCreateBimElements()
        {
            var title = $"Test issue {Guid.NewGuid()}";
            var status = ObjectiveStatus.Open;
            var type = Fixture.Context.ObjectiveTypes.First();
            var project = Fixture.Context.Projects.First();
            var objectivesCount = Fixture.Context.Objectives.Count();
            var bimElementsCount = Fixture.Context.BimElements.Count();
            var existingBimElement = Fixture.Context.BimElements.First();
            var bimList = new List<BimElementDto>
            {
                new BimElementDto
                {
                    GlobalID = existingBimElement.GlobalID,
                    ItemID = new ID<ItemDto>(existingBimElement.ItemID)
                }
            };
            var objectiveToCreate = new ObjectiveToCreateDto
            {
                Title = title,
                Description = "created for test purpose only",
                Status = status,
                ObjectiveTypeID = new ID<ObjectiveTypeDto>(type.ID),
                ProjectID = new ID<ProjectDto>(project.ID),
                BimElements = bimList
            };

            var result = await service.Add(objectiveToCreate);

            var addedObjective = Fixture.Context.Objectives.First(o => o.Title == title);
            Assert.AreEqual(title, result.Title);
            Assert.AreEqual(status, result.Status);
            Assert.AreEqual(type.ID, (int)result.ObjectiveType.ID);
            Assert.AreEqual(objectivesCount + 1, Fixture.Context.Objectives.Count());
            Assert.AreEqual(bimList.Count, addedObjective.BimElements.Count);
            Assert.AreEqual(bimElementsCount, Fixture.Context.BimElements.Count());
        }

        [TestMethod]
        public async Task Add_NewObjectiveToCreateWithNotExistingBimElements_ReturnsObjectiveToListAndCreatesBimElements()
        {
            var title = $"Test issue {Guid.NewGuid()}";
            var status = ObjectiveStatus.Open;
            var type = Fixture.Context.ObjectiveTypes.First();
            var project = Fixture.Context.Projects.First();
            var objectivesCount = Fixture.Context.Objectives.Count();
            var bimElementsCount = Fixture.Context.BimElements.Count();
            var bimList = new List<BimElementDto>
            {
                new BimElementDto
                {
                    GlobalID = $"uniqueGlobalId{Guid.NewGuid()}",
                    ItemID = new ID<ItemDto>(1)
                }
            };
            var objectiveToCreate = new ObjectiveToCreateDto
            {
                Title = title,
                Description = "created for test purpose only",
                Status = status,
                ObjectiveTypeID = new ID<ObjectiveTypeDto>(type.ID),
                ProjectID = new ID<ProjectDto>(project.ID),
                BimElements = bimList
            };

            var result = await service.Add(objectiveToCreate);

            var addedObjective = Fixture.Context.Objectives.First(o => o.Title == title);
            Assert.AreEqual(title, result.Title);
            Assert.AreEqual(status, result.Status);
            Assert.AreEqual(type.ID, (int)result.ObjectiveType.ID);
            Assert.AreEqual(objectivesCount + 1, Fixture.Context.Objectives.Count());
            Assert.AreEqual(bimList.Count, addedObjective.BimElements.Count);
            Assert.AreEqual(bimElementsCount + bimList.Count, Fixture.Context.BimElements.Count());
        }

        [TestMethod]
        public async Task Add_NewObjectiveToCreateWithDifferentBimElements_ReturnsObjectiveToListAndCreatesNotExistingBimElements()
        {
            var title = $"Test issue {Guid.NewGuid()}";
            var status = ObjectiveStatus.Open;
            var type = Fixture.Context.ObjectiveTypes.First();
            var project = Fixture.Context.Projects.First();
            var objectivesCount = Fixture.Context.Objectives.Count();
            var bimElementsCount = Fixture.Context.BimElements.Count();
            var existingBimElement = Fixture.Context.BimElements.First();
            var bimList = new List<BimElementDto>
            {
                    new BimElementDto
                    {
                        GlobalID = existingBimElement.GlobalID,
                        ItemID = new ID<ItemDto>(existingBimElement.ItemID)
                    },
                    new BimElementDto
                    {
                        GlobalID = $"uniqueGlobalId{Guid.NewGuid()}",
                        ItemID = new ID<ItemDto>(1)
                    }
            };
            var objectiveToCreate = new ObjectiveToCreateDto
            {
                Title = title,
                Description = "created for test purpose only",
                Status = status,
                ObjectiveTypeID = new ID<ObjectiveTypeDto>(type.ID),
                ProjectID = new ID<ProjectDto>(project.ID),
                BimElements = bimList
            };

            var result = await service.Add(objectiveToCreate);

            var addedObjective = Fixture.Context.Objectives.First(o => o.Title == title);
            Assert.AreEqual(title, result.Title);
            Assert.AreEqual(status, result.Status);
            Assert.AreEqual(type.ID, (int)result.ObjectiveType.ID);
            Assert.AreEqual(objectivesCount + 1, Fixture.Context.Objectives.Count());
            Assert.AreEqual(bimList.Count, addedObjective.BimElements.Count);
            Assert.AreEqual(bimElementsCount + 1, Fixture.Context.BimElements.Count());
        }

        [TestMethod]
        public async Task Add_NewObjectiveToCreateWithItems_ReturnsObjectiveToListAndAddsItemsToDb()
        {
            var title = $"Test issue {Guid.NewGuid()}";
            var status = ObjectiveStatus.Open;
            var type = Fixture.Context.ObjectiveTypes.First();
            var project = Fixture.Context.Projects.First();
            var objectivesCount = Fixture.Context.Objectives.Count();
            var bimElementsCount = Fixture.Context.BimElements.Count();
            var existingBimElement = Fixture.Context.BimElements.First();
            var dbItem = Fixture.Context.Items.First();
            var items = new List<ItemDto>
            {
                new ItemDto
                {
                    ExternalItemId = dbItem.ExternalItemId,
                    ID = new ID<ItemDto>(dbItem.ID),
                    ItemType = (ItemTypeDto)dbItem.ItemType,
                    Name = dbItem.Name
                }
            };
            var objectiveToCreate = new ObjectiveToCreateDto
            {
                Title = title,
                Description = "created for test purpose only",
                Status = status,
                ObjectiveTypeID = new ID<ObjectiveTypeDto>(type.ID),
                ProjectID = new ID<ProjectDto>(project.ID),
                Items = items
            };

            var result = await service.Add(objectiveToCreate);

            var addedObjective = Fixture.Context.Objectives.First(o => o.Title == title);
            Assert.AreEqual(title, result.Title);
            Assert.AreEqual(status, result.Status);
            Assert.AreEqual(type.ID, (int)result.ObjectiveType.ID);
            Assert.AreEqual(objectivesCount + 1, Fixture.Context.Objectives.Count());
            Assert.AreEqual(items.Count, addedObjective.Items.Count);
        }

        [TestMethod]
        public async Task Add_NewObjectiveToCreateWithDynamicFields_ReturnsObjectiveToListAndAddsDynamicFieldsToDb()
        {
            var title = $"Test issue {Guid.NewGuid()}";
            var status = ObjectiveStatus.Open;
            var type = Fixture.Context.ObjectiveTypes.First();
            var project = Fixture.Context.Projects.First();
            var objectivesCount = Fixture.Context.Objectives.Count();
            var bimElementsCount = Fixture.Context.BimElements.Count();
            var existingBimElement = Fixture.Context.BimElements.First();
            var dynamicFields = new List<DynamicFieldToCreateDto>
            {
                MockData.DEFAULT_DYNAMIC_FIELDS_TO_CREATE[0],
                MockData.DEFAULT_DYNAMIC_FIELDS_TO_CREATE[1]
            };
            var objectiveToCreate = new ObjectiveToCreateDto
            {
                Title = title,
                Description = "created for test purpose only",
                Status = status,
                ObjectiveTypeID = new ID<ObjectiveTypeDto>(type.ID),
                ProjectID = new ID<ProjectDto>(project.ID),
                DynamicFields = dynamicFields
            };

            var result = await service.Add(objectiveToCreate);

            var addedObjective = Fixture.Context.Objectives.First(o => o.Title == title);
            Assert.AreEqual(title, result.Title);
            Assert.AreEqual(status, result.Status);
            Assert.AreEqual(type.ID, (int)result.ObjectiveType.ID);
            Assert.AreEqual(objectivesCount + 1, Fixture.Context.Objectives.Count());
            Assert.AreEqual(dynamicFields.Count, addedObjective.DynamicFields.Count);
        }

        [TestMethod]
        public async Task Add_NewObjectiveToCreateWithAllAdditionalFields_ReturnsObjectiveToListAndAddsAdditionalFieldsToDb()
        {
            var title = $"Test issue {Guid.NewGuid()}";
            var status = ObjectiveStatus.Open;
            var type = Fixture.Context.ObjectiveTypes.First();
            var project = Fixture.Context.Projects.First();
            var objectivesCount = Fixture.Context.Objectives.Count();
            var bimElementsCount = Fixture.Context.BimElements.Count();
            var existingBimElement = Fixture.Context.BimElements.First();
            var dbItem = Fixture.Context.Items.First();
            var bimList = new List<BimElementDto>
            {
                    new BimElementDto
                    {
                        GlobalID = existingBimElement.GlobalID,
                        ItemID = new ID<ItemDto>(existingBimElement.ItemID)
                    },
                    new BimElementDto
                    {
                        GlobalID = $"uniqueGlobalId{Guid.NewGuid()}",
                        ItemID = new ID<ItemDto>(1)
                    }
            };
            var items = new List<ItemDto>
            {
                new ItemDto
                {
                    ExternalItemId = dbItem.ExternalItemId,
                    ID = new ID<ItemDto>(dbItem.ID),
                    ItemType = (ItemTypeDto)dbItem.ItemType,
                    Name = dbItem.Name
                }
            };
            var dynamicFields = new List<DynamicFieldToCreateDto>
            {
                MockData.DEFAULT_DYNAMIC_FIELDS_TO_CREATE[0],
                MockData.DEFAULT_DYNAMIC_FIELDS_TO_CREATE[1]
            };
            var objectiveToCreate = new ObjectiveToCreateDto
            {
                Title = title,
                Description = "created for test purpose only",
                Status = status,
                ObjectiveTypeID = new ID<ObjectiveTypeDto>(type.ID),
                ProjectID = new ID<ProjectDto>(project.ID),
                Items = items,
                BimElements = bimList,
                DynamicFields = dynamicFields
            };

            var result = await service.Add(objectiveToCreate);

            var addedObjective = Fixture.Context.Objectives.First(o => o.Title == title);
            Assert.AreEqual(title, result.Title);
            Assert.AreEqual(status, result.Status);
            Assert.AreEqual(type.ID, (int)result.ObjectiveType.ID);
            Assert.AreEqual(objectivesCount + 1, Fixture.Context.Objectives.Count());
            Assert.AreEqual(items.Count, addedObjective.Items.Count);
            Assert.AreEqual(dynamicFields.Count, addedObjective.DynamicFields.Count);
            Assert.AreEqual(bimList.Count, addedObjective.BimElements.Count);
            Assert.AreEqual(bimElementsCount + 1, Fixture.Context.BimElements.Count());
        }

        [TestMethod]
        public async Task Find_ExistingObjective_ReturnsObjectiveWithIncludedFields()
        {
            var existingObjective = Fixture.Context.Objectives
                .First(o => o.DynamicFields.Count > 0
                            && o.BimElements.Count > 0
                            && o.Items.Count > 0);
            var dtoId = new ID<ObjectiveDto>(existingObjective.ID);

            var result = await service.Find(dtoId);

            Assert.IsNotNull(result);
            Assert.AreEqual(dtoId, result.ID);
            Assert.AreEqual(existingObjective.AuthorID, (int)result.AuthorID);
            Assert.AreEqual(existingObjective.ProjectID, (int)result.ProjectID);
            Assert.AreEqual(existingObjective.ObjectiveTypeID, (int)result.ObjectiveTypeID);
            Assert.AreEqual(existingObjective.DynamicFields.Count, result.DynamicFields.Count());
            Assert.AreEqual(existingObjective.BimElements.Count, result.BimElements.Count());
            Assert.AreEqual(existingObjective.Items.Count, result.Items.Count());
        }

        [TestMethod]
        public async Task Find_NotExistingObjective_ReturnsNull()
        {
            var result = await service.Find(ID<ObjectiveDto>.InvalidID);

            Assert.IsNull(result);
        }

        [TestMethod]
        public async Task GetObjectives_ExistingProjectWithObjectives_ReturnsEnumerableWithProjectObjectives()
        {
            var existingProject = Fixture.Context.Projects.First(p => p.Objectives.Count > 0);
            var dtoId = new ID<ProjectDto>(existingProject.ID);

            var result = await service.GetObjectives(dtoId);

            Assert.IsNotNull(result);
            Assert.AreEqual(existingProject.Objectives.Count, result.Count());
        }

        [TestMethod]
        public async Task GetObjectives_NotExistingProject_ReturnsEmptyEnumerable()
        {
            var dtoId = ID<ProjectDto>.InvalidID;

            var result = await service.GetObjectives(dtoId);

            Assert.IsFalse(result.Any());
        }

        [TestMethod]
        public async Task Remove_ExistingObjective_ReturnsTrueAndDeletesRelatedBridges()
        {
            var existingObjective = Fixture.Context.Objectives
                .First(o => o.DynamicFields.Count > 0
                            && o.BimElements.Count > 0
                            && o.Items.Count > 0);
            var startObjectiveItemsCount = Fixture.Context.ObjectiveItems.Count();
            var relatedObjectiveItemsCount = Fixture.Context.ObjectiveItems.Where(oi => oi.ObjectiveID == existingObjective.ID).Count();
            var startDynamicFieldsCount = Fixture.Context.DynamicFields.Count();
            var relatedDynamicFields = Fixture.Context.DynamicFields.Where(f => f.ObjectiveID == existingObjective.ID).Count();
            var startBeoCount = Fixture.Context.BimElementObjectives.Count();
            var relatedBeo = Fixture.Context.BimElementObjectives.Where(beo => beo.ObjectiveID == existingObjective.ID).Count();
            var dtoId = new ID<ObjectiveDto>(existingObjective.ID);

            var result = await service.Remove(dtoId);

            Assert.IsTrue(result);
            Assert.AreEqual(startObjectiveItemsCount - relatedObjectiveItemsCount, Fixture.Context.ObjectiveItems.Count());
            Assert.AreEqual(startDynamicFieldsCount - relatedDynamicFields, Fixture.Context.DynamicFields.Count());
            Assert.AreEqual(startBeoCount - relatedBeo, Fixture.Context.BimElementObjectives.Count());
        }

        [TestMethod]
        public async Task Remove_NotExistingObjective_ReturnsFalse()
        {
            var dtoId = ID<ObjectiveDto>.InvalidID;

            var result = await service.Remove(dtoId);

            Assert.IsFalse(result);
        }

        [TestMethod]
        public async Task Update_ExistingObjectiveWithoutAdditionalFields_ReturnsTrueAndClearsAdditionalFields()
        {
            var existingObjective = Fixture.Context.Objectives
                .First(o => o.DynamicFields.Count > 0
                            && o.BimElements.Count > 0
                            && o.Items.Count > 0);
            var newDescription = $"newDescription{Guid.NewGuid()}";
            var newTitle = $"newTitle{Guid.NewGuid()}";
            var newStatus = (ObjectiveStatus)existingObjective.Status != ObjectiveStatus.InProgress
                            ? ObjectiveStatus.InProgress
                            : ObjectiveStatus.Ready;
            var newProject = Fixture.Context.Projects.First(p => p.ID != existingObjective.ProjectID);
            var changedObjective = new ObjectiveDto
            {
                AuthorID = new ID<UserDto>(existingObjective.AuthorID.Value),
                CreationDate = existingObjective.CreationDate,
                DueDate = existingObjective.DueDate,
                Description = newDescription,
                ID = new ID<ObjectiveDto>(existingObjective.ID),
                ObjectiveTypeID = new ID<ObjectiveTypeDto>(existingObjective.ObjectiveTypeID),
                ProjectID = new ID<ProjectDto>(existingObjective.ProjectID),
                Status = newStatus,
                Title = newTitle
            };

            var result = await service.Update(changedObjective);

            var updatedObjective = Fixture.Context.Objectives.First(o => o.ID == existingObjective.ID);
            Assert.IsTrue(result);
            Assert.AreEqual(newDescription, updatedObjective.Description);
            Assert.AreEqual((int)newStatus, updatedObjective.Status);
            Assert.AreEqual(newTitle, updatedObjective.Title);
            Assert.AreEqual(0, existingObjective.BimElements.Count);
            Assert.AreEqual(0, existingObjective.Items.Count);
            Assert.AreEqual(0, existingObjective.DynamicFields.Count);
        }

        [TestMethod]
        public async Task Update_ExistingObjectiveWithDynamicFields_ReturnsTrueAndUpdatesDynamicFields()
        {
            var existingObjective = Fixture.Context.Objectives
                .First(o => o.DynamicFields.Count > 0
                            && o.BimElements.Count > 0
                            && o.Items.Count > 0);
            var newDescription = $"newDescription{Guid.NewGuid()}";
            var newTitle = $"newTitle{Guid.NewGuid()}";
            var newStatus = (ObjectiveStatus)existingObjective.Status != ObjectiveStatus.InProgress
                            ? ObjectiveStatus.InProgress
                            : ObjectiveStatus.Ready;
            var newProject = Fixture.Context.Projects.First(p => p.ID != existingObjective.ProjectID);
            var guid = Guid.NewGuid();
            var newDynamicFields = new List<DynamicFieldDto>
            {
                new DynamicFieldDto
                {
                    Key = $"key{guid}",
                    Type = $"type{guid}",
                    Value = $"value{guid}"
                }
            };
            var firstDynamicField = existingObjective.DynamicFields.First();
            var existingDynamicFields = new List<DynamicFieldDto>
            {
                new DynamicFieldDto
                {
                    Key = firstDynamicField.Key,
                    Type = firstDynamicField.Type,
                    Value = firstDynamicField.Value
                }
            };
            var deletingDynamicFieldsCount = existingObjective.DynamicFields.Count - 1;
            var startDynamicFieldsCount = Fixture.Context.DynamicFields.Count();
            var dynamicFields = new List<DynamicFieldDto>();
            dynamicFields.AddRange(newDynamicFields);
            dynamicFields.AddRange(existingDynamicFields);
            var changedObjective = new ObjectiveDto
            {
                AuthorID = new ID<UserDto>(existingObjective.AuthorID.Value),
                CreationDate = existingObjective.CreationDate,
                DueDate = existingObjective.DueDate,
                Description = newDescription,
                DynamicFields = dynamicFields,
                ID = new ID<ObjectiveDto>(existingObjective.ID),
                ObjectiveTypeID = new ID<ObjectiveTypeDto>(existingObjective.ObjectiveTypeID),
                ProjectID = new ID<ProjectDto>(existingObjective.ProjectID),
                Status = newStatus,
                Title = newTitle
            };

            var result = await service.Update(changedObjective);

            var updatedObjective = Fixture.Context.Objectives.First(o => o.ID == existingObjective.ID);
            Assert.IsTrue(result);
            Assert.AreEqual(newDescription, updatedObjective.Description);
            Assert.AreEqual((int)newStatus, updatedObjective.Status);
            Assert.AreEqual(newTitle, updatedObjective.Title);
            Assert.AreEqual(startDynamicFieldsCount - deletingDynamicFieldsCount + newDynamicFields.Count, Fixture.Context.DynamicFields.Count());
            Assert.AreEqual(dynamicFields.Count, changedObjective.DynamicFields.Count());
            updatedObjective.DynamicFields.ToList().ForEach(df =>
            {
                Assert.IsTrue(dynamicFields.Any(cdf => cdf.Key == df.Key
                                                    && cdf.Type == df.Type
                                                    && cdf.Value == df.Value));
            });
        }

        [TestMethod]
        public async Task Update_ExistingObjectiveWithBimElements_ReturnsTrueAndUpdatesBimElements()
        {
            var existingObjective = Fixture.Context.Objectives
                .First(o => o.DynamicFields.Count > 0
                            && o.BimElements.Count > 0
                            && o.Items.Count > 0);
            var newDescription = $"newDescription{Guid.NewGuid()}";
            var newTitle = $"newTitle{Guid.NewGuid()}";
            var newStatus = (ObjectiveStatus)existingObjective.Status != ObjectiveStatus.InProgress
                            ? ObjectiveStatus.InProgress
                            : ObjectiveStatus.Ready;
            var newProject = Fixture.Context.Projects.First(p => p.ID != existingObjective.ProjectID);
            var startBimElementsCount = Fixture.Context.BimElementObjectives.Count();
            var deletingBimElementsCount = existingObjective.BimElements.Count - 1;
            var firstBimElement = existingObjective.BimElements.First().BimElement;
            var changedBimElements = new List<BimElementDto>
            {
                new BimElementDto
                {
                    GlobalID = firstBimElement.GlobalID,
                    ItemID = new ID<ItemDto>(firstBimElement.ItemID)
                },
                new BimElementDto
                {
                    GlobalID = $"newBimsGlobalId{Guid.NewGuid()}",
                    ItemID = new ID<ItemDto>(firstBimElement.ItemID)
                }
            };
            var newBimElementsCount = changedBimElements.Count - 1;
            var changedObjective = new ObjectiveDto
            {
                AuthorID = new ID<UserDto>(existingObjective.AuthorID.Value),
                BimElements = changedBimElements,
                CreationDate = existingObjective.CreationDate,
                DueDate = existingObjective.DueDate,
                Description = newDescription,
                ID = new ID<ObjectiveDto>(existingObjective.ID),
                ObjectiveTypeID = new ID<ObjectiveTypeDto>(existingObjective.ObjectiveTypeID),
                ProjectID = new ID<ProjectDto>(existingObjective.ProjectID),
                Status = newStatus,
                Title = newTitle
            };

            var result = await service.Update(changedObjective);

            var updatedObjective = Fixture.Context.Objectives.First(o => o.ID == existingObjective.ID);
            Assert.IsTrue(result);
            Assert.AreEqual(newDescription, updatedObjective.Description);
            Assert.AreEqual((int)newStatus, updatedObjective.Status);
            Assert.AreEqual(newTitle, updatedObjective.Title);
            Assert.AreEqual(changedBimElements.Count, updatedObjective.BimElements.Count);
            Assert.AreEqual(startBimElementsCount - deletingBimElementsCount + newBimElementsCount, Fixture.Context.BimElementObjectives.Count());
            updatedObjective.BimElements.ToList().ForEach(be =>
            {
                Assert.IsTrue(changedBimElements.Any(cbe => cbe.GlobalID == be.BimElement.GlobalID
                                                            && (int)cbe.ItemID == be.BimElement.ItemID));
            });
        }

        [TestMethod]
        public async Task Update_ExistingObjectiveWithItems_ReturnsTrueAndUpdatesItems()
        {
            var existingObjective = Fixture.Context.Objectives
                .First(o => o.DynamicFields.Count > 0
                            && o.BimElements.Count > 0
                            && o.Items.Count > 0);
            var newDescription = $"newDescription{Guid.NewGuid()}";
            var newTitle = $"newTitle{Guid.NewGuid()}";
            var newStatus = (ObjectiveStatus)existingObjective.Status != ObjectiveStatus.InProgress
                            ? ObjectiveStatus.InProgress
                            : ObjectiveStatus.Ready;
            var newProject = Fixture.Context.Projects.First(p => p.ID != existingObjective.ProjectID);
            var guid = Guid.NewGuid();
            var newItems = new List<ItemDto>
            {
                new ItemDto
                {
                    ItemType = ItemTypeDto.Media,
                    ExternalItemId = $"ExternalItemId{guid}",
                    Name = $"Name{guid}"
                }
            };
            var firstItem = existingObjective.Items.First().Item;
            var deletingItemsCount = existingObjective.Items.Count - 1;
            var existingItems = new List<ItemDto>
            {
                new ItemDto
                {
                    ID = new ID<ItemDto>(firstItem.ID),
                    ExternalItemId = firstItem.ExternalItemId,
                    ItemType = (ItemTypeDto)firstItem.ItemType,
                    Name = firstItem.Name
                }
            };
            var startItemsCount = existingObjective.Items.Count;
            var items = new List<ItemDto>();
            items.AddRange(newItems);
            items.AddRange(existingItems);
            var changedObjective = new ObjectiveDto
            {
                AuthorID = new ID<UserDto>(existingObjective.AuthorID.Value),
                CreationDate = existingObjective.CreationDate,
                DueDate = existingObjective.DueDate,
                Description = newDescription,
                ID = new ID<ObjectiveDto>(existingObjective.ID),
                Items = items,
                ObjectiveTypeID = new ID<ObjectiveTypeDto>(existingObjective.ObjectiveTypeID),
                ProjectID = new ID<ProjectDto>(existingObjective.ProjectID),
                Status = newStatus,
                Title = newTitle
            };

            var result = await service.Update(changedObjective);

            var updatedObjective = Fixture.Context.Objectives.First(o => o.ID == existingObjective.ID);
            Assert.IsTrue(result);
            Assert.AreEqual(newDescription, updatedObjective.Description);
            Assert.AreEqual((int)newStatus, updatedObjective.Status);
            Assert.AreEqual(newTitle, updatedObjective.Title);
            Assert.AreEqual(startItemsCount - deletingItemsCount + newItems.Count, updatedObjective.Items.Count);
            Assert.AreEqual(updatedObjective.Items.Count, items.Count);
            updatedObjective.Items.Select(oi => oi.Item).ToList().ForEach(i =>
            {
                Assert.IsTrue(items.Any(ci => ci.ExternalItemId == i.ExternalItemId
                                                    && (int)ci.ItemType == i.ItemType
                                                    && ci.Name == i.Name));
            });
        }

        [TestMethod]
        public async Task Update_ExistingObjectiveWithAdditionalFields_ReturnsTrueAndUpdatesAdditionalFields()
        {
            var existingObjective = Fixture.Context.Objectives
                .First(o => o.DynamicFields.Count > 0
                            && o.BimElements.Count > 0
                            && o.Items.Count > 0);
            var newDescription = $"newDescription{Guid.NewGuid()}";
            var newTitle = $"newTitle{Guid.NewGuid()}";
            var newStatus = (ObjectiveStatus)existingObjective.Status != ObjectiveStatus.InProgress
                            ? ObjectiveStatus.InProgress
                            : ObjectiveStatus.Ready;
            var newProject = Fixture.Context.Projects.First(p => p.ID != existingObjective.ProjectID);
            var guid = Guid.NewGuid();
            #region Dynamic Fields
            var newDynamicFields = new List<DynamicFieldDto>
            {
                new DynamicFieldDto
                {
                    Key = $"key{guid}",
                    Type = $"type{guid}",
                    Value = $"value{guid}"
                }
            };
            var firstDynamicField = existingObjective.DynamicFields.First();
            var existingDynamicFields = new List<DynamicFieldDto>
            {
                new DynamicFieldDto
                {
                    Key = firstDynamicField.Key,
                    Type = firstDynamicField.Type,
                    Value = firstDynamicField.Value
                }
            };
            var deletingDynamicFieldsCount = existingObjective.DynamicFields.Count - 1;
            var startDynamicFieldsCount = Fixture.Context.DynamicFields.Count();
            var dynamicFields = new List<DynamicFieldDto>();
            dynamicFields.AddRange(newDynamicFields);
            dynamicFields.AddRange(existingDynamicFields); 
            #endregion
            #region Items
            var newItems = new List<ItemDto>
            {
                new ItemDto
                {
                    ItemType = ItemTypeDto.Media,
                    ExternalItemId = $"ExternalItemId{guid}",
                    Name = $"Name{guid}"
                }
            };
            var firstItem = existingObjective.Items.First().Item;
            var deletingItemsCount = existingObjective.Items.Count - 1;
            var existingItems = new List<ItemDto>
            {
                new ItemDto
                {
                    ID = new ID<ItemDto>(firstItem.ID),
                    ExternalItemId = firstItem.ExternalItemId,
                    ItemType = (ItemTypeDto)firstItem.ItemType,
                    Name = firstItem.Name
                }
            };
            var startItemsCount = existingObjective.Items.Count;
            var items = new List<ItemDto>();
            items.AddRange(newItems); 
            #endregion
            #region BIM elements
            items.AddRange(existingItems); var startBimElementsCount = Fixture.Context.BimElementObjectives.Count();
            var deletingBimElementsCount = existingObjective.BimElements.Count - 1;
            var firstBimElement = existingObjective.BimElements.First().BimElement;
            var changedBimElements = new List<BimElementDto>
            {
                new BimElementDto
                {
                    GlobalID = firstBimElement.GlobalID,
                    ItemID = new ID<ItemDto>(firstBimElement.ItemID)
                },
                new BimElementDto
                {
                    GlobalID = $"newBimsGlobalId{Guid.NewGuid()}",
                    ItemID = new ID<ItemDto>(firstBimElement.ItemID)
                }
            };
            var newBimElementsCount = changedBimElements.Count - 1; 
            #endregion
            var changedObjective = new ObjectiveDto
            {
                AuthorID = new ID<UserDto>(existingObjective.AuthorID.Value),
                BimElements = changedBimElements,
                CreationDate = existingObjective.CreationDate,
                DueDate = existingObjective.DueDate,
                Description = newDescription,
                DynamicFields = dynamicFields,
                ID = new ID<ObjectiveDto>(existingObjective.ID),
                Items = items,
                ObjectiveTypeID = new ID<ObjectiveTypeDto>(existingObjective.ObjectiveTypeID),
                ProjectID = new ID<ProjectDto>(existingObjective.ProjectID),
                Status = newStatus,
                Title = newTitle
            };

            var result = await service.Update(changedObjective);

            var updatedObjective = Fixture.Context.Objectives.First(o => o.ID == existingObjective.ID);
            Assert.IsTrue(result);
            Assert.AreEqual(newDescription, updatedObjective.Description);
            Assert.AreEqual((int)newStatus, updatedObjective.Status);
            Assert.AreEqual(newTitle, updatedObjective.Title);
            Assert.AreEqual(startItemsCount - deletingItemsCount + newItems.Count, updatedObjective.Items.Count);
            Assert.AreEqual(updatedObjective.Items.Count, items.Count);
            Assert.AreEqual(startDynamicFieldsCount - deletingDynamicFieldsCount + newDynamicFields.Count, Fixture.Context.DynamicFields.Count());
            Assert.AreEqual(dynamicFields.Count, changedObjective.DynamicFields.Count());
            Assert.AreEqual(changedBimElements.Count, updatedObjective.BimElements.Count);
            Assert.AreEqual(startBimElementsCount - deletingBimElementsCount + newBimElementsCount, Fixture.Context.BimElementObjectives.Count());
        }

        [TestMethod]
        public async Task Update_NotExistingObjective_ReturnsFalse()
        {
            var notExistingObjective = new ObjectiveDto { ID = ID<ObjectiveDto>.InvalidID };

            var result = await service.Update(notExistingObjective);

            Assert.IsFalse(result);
        }

        //        // WARNING: Dynamic fields IDs are not set
        //        private static ObjectiveDto CreateExpectedObjective(ObjectiveToCreateDto o, ID<ObjectiveDto> id, UserDto author, ObjectiveTypeDto type)
        //        {
        //            return new ObjectiveDto()
        //            {
        //                ID = id,
        //               // AuthorID = author,
        //                CreationDate = o.CreationDate,
        //                DueDate = o.DueDate,
        //                Title = o.Title,
        //                Description = o.Description,
        //                ParentObjectiveID = o.ParentObjectiveID,
        //                ProjectID = o.ProjectID,
        //                Status = o.Status,
        //                ObjectiveType = type,
        //                BimElements = o.BimElements?.ToList() ?? Enumerable.Empty<BimElementDto>(),
        //                DynamicFields = o.DynamicFields?
        //                    .Select(x => new DynamicFieldDto() 
        //                    {
        //                        Key = x.Key,
        //                        Type = x.Type,
        //                        Value = x.Value
        //                    }).ToList() ?? Enumerable.Empty<DynamicFieldDto>()
        //            };
        //        }

        //        [TestMethod]
        //        public async Task Complex_objective_test()
        //        {
        //            using var transaction = Fixture.Connection.BeginTransaction();
        //            using (var context = Fixture.CreateContext(transaction))
        //            {
        //                var api = new DocumentManagementApi(context);
        //                var access = await api.Register(new UserToCreateDto("vpupkin", "123", "Vasily Pupkin"));

        //                var tasktype = await access.ObjectiveTypeService.Add("Задание");
        //                var errortype = await access.ObjectiveTypeService.Add("Нарушение");

        //                var project1ID = await access.ProjectService.AddToUser(access.CurrentUser.ID, "Project 1");
        //                var project2ID = await access.ProjectService.AddToUser(access.CurrentUser.ID, "Project 2");

        //                var creationTime = DateTime.Parse("2020-11-18T10:50:00.0000000Z");
        //                var dueTime = creationTime.AddDays(1);

        //                // 0. Can add objectives
        //                var newObjective1 = new ObjectiveToCreateDto()
        //                {
        //                    AuthorID = access.CurrentUser.ID,
        //                    CreationDate = creationTime,
        //                    DueDate = dueTime,
        //                    Title = "Make cookies",
        //                    Description = "Mmm, cookies!",
        //                    Status = ObjectiveStatus.Open,
        //                    ObjectiveTypeID = tasktype,
        //                    ParentObjectiveID = null,
        //                    ProjectID = project1ID,
        //                    BimElements = null,
        //                    DynamicFields = new List<DynamicFieldToCreateDto>()
        //                    {
        //                        new DynamicFieldToCreateDto("df1", "type 1", "val 1"),
        //                        new DynamicFieldToCreateDto("df2", "type 2", "val 2")
        //                    }
        //                };

        //                var objective1ID = await access.ObjectiveService.Add(newObjective1);
        //                Assert.IsTrue(objective1ID.IsValid);

        //                var newObjective2 = new ObjectiveToCreateDto()
        //                {
        //                    AuthorID = access.CurrentUser.ID,
        //                    CreationDate = creationTime,
        //                    DueDate = dueTime,
        //                    Title = "Make dough",
        //                    Description = "Can't make cookies without dough",
        //                    Status = ObjectiveStatus.Open,
        //                    ObjectiveTypeID = tasktype,
        //                    ParentObjectiveID = objective1ID,
        //                    ProjectID = project1ID
        //                };

        //                var childObjectiveID = await access.ObjectiveService.Add(newObjective2);
        //                Assert.IsTrue(childObjectiveID.IsValid);

        //                var newObjective3 = new ObjectiveToCreateDto()
        //                {
        //                    AuthorID = access.CurrentUser.ID,
        //                    CreationDate = creationTime,
        //                    DueDate = dueTime,
        //                    Title = "Something is wrong",
        //                    Description = "Really, something is wrong!",
        //                    Status = ObjectiveStatus.Ready,
        //                    ObjectiveTypeID = errortype,
        //                    ParentObjectiveID = null,
        //                    ProjectID = project2ID
        //                };

        //                var objective3ID = await access.ObjectiveService.Add(newObjective3);
        //                Assert.IsTrue(objective3ID.IsValid);


        //                // 1. Can query all objectives
        //                var currentUser = access.CurrentUser;
        //                var expected = new ObjectiveDto[]
        //                {
        //                    CreateExpectedObjective(newObjective1, objective1ID, currentUser, new ObjectiveTypeDto(){ ID = tasktype, Name = "Задание" }),
        //                    CreateExpectedObjective(newObjective2, childObjectiveID, currentUser, new ObjectiveTypeDto(){ ID = tasktype, Name = "Задание" }),
        //                    CreateExpectedObjective(newObjective3, objective3ID, currentUser, new ObjectiveTypeDto(){ ID = errortype, Name = "Нарушение" })
        //                };

        //                var comparer = new ObjectiveComparer();
        //                var allObjectives = await access.ObjectiveService.GetAllObjectives();
        //               // CollectionAssert.That.AreEquivalent(expected, allObjectives.ToArray(), comparer);

        //                //2. Can query objectives by project
        //                var project1Objectives = await access.ObjectiveService.GetObjectives(project1ID);
        //                CollectionAssert.That.AreEquivalent(new[] { expected[0], expected[1] }, project1Objectives, comparer);

        //                var project2Objectives = await access.ObjectiveService.GetObjectives(project2ID);
        //                CollectionAssert.That.AreEquivalent(new[] { expected[2] }, project2Objectives, comparer);

        //                // 2. Can find objective
        //                var found = await access.ObjectiveService.Find(childObjectiveID);
        //                Assert.IsTrue(comparer.Equals(expected[1], found));

        //                // 2.1 Can not find not existing objective
        //                found = await access.ObjectiveService.Find(ID<ObjectiveDto>.InvalidID);
        //                Assert.IsNull(found);

        //                // 3. Can remove objective
        //                var isRemoved = await access.ObjectiveService.Remove(childObjectiveID);
        //                Assert.IsTrue(isRemoved);

        //                // 3.1 Can not remove twice
        //                isRemoved = await access.ObjectiveService.Remove(childObjectiveID);
        //                Assert.IsFalse(isRemoved);

        //                // Verify that objective is removed
        //                //allObjectives = await access.ObjectiveService.GetAllObjectives();
        //               // CollectionAssert.That.AreEquivalent(new[] { expected[0], expected[2] }, allObjectives, comparer);

        //                project1Objectives = await access.ObjectiveService.GetObjectives(project1ID);
        //                CollectionAssert.That.AreEquivalent(new[] { expected[0] }, project1Objectives, comparer);

        //                // 4. Can update objective
        //                var objective = await access.ObjectiveService.Find(objective1ID);
        //                objective.Status = ObjectiveStatus.Ready;
        //                await access.ObjectiveService.Update(objective);

        //                var changed = await access.ObjectiveService.Find(objective1ID);
        //                Assert.AreEqual(ObjectiveStatus.Ready, changed.Status);
        //            }
        //        }

        //        [TestMethod]
        //        public async Task Can_update_bim_elements()
        //        {
        //            using var transaction = Fixture.Connection.BeginTransaction();
        //            using (var context = Fixture.CreateContext(transaction))
        //            {
        //                var api = new DocumentManagementApi(context);
        //                var access = await api.Register(new UserToCreateDto("vpupkin", "123", "Vasily Pupkin"));

        //                var tasktype = await access.ObjectiveTypeService.Add("Задание");
        //                var project1ID = await access.ProjectService.AddToUser(access.CurrentUser.ID, "Project 1");

        //                var userProjects = await access.ProjectService.GetUserProjects(access.CurrentUser.ID);
        //                var item1 = await access.ItemService.Add(new ItemToCreateDto(@"C:\Windows\Temp\abra.tmp", "externalId", ItemTypeDto.File), project1ID);

        //                var creationTime = DateTime.Parse("2020-11-18T10:50:00.0000000Z");
        //                var dueTime = creationTime.AddDays(1);

        //                var newObjective1 = new ObjectiveToCreateDto()
        //                {
        //                    AuthorID = access.CurrentUser.ID,
        //                    CreationDate = creationTime,
        //                    DueDate = dueTime,
        //                    Title = "Make cookies",
        //                    Description = "Mmm, cookies!",
        //                    Status = ObjectiveStatus.Open,
        //                    ObjectiveTypeID = tasktype,
        //                    ParentObjectiveID = null,
        //                    ProjectID = project1ID,
        //                    BimElements = null
        //                };

        //                var objID = await access.ObjectiveService.Add(newObjective1);

        //                var obj = await access.ObjectiveService.Find(objID);
        //                Assert.IsNotNull(obj.BimElements);
        //                Assert.AreEqual(0, obj.BimElements.Count());

        //                obj.BimElements = new List<BimElementDto>()
        //                {
        //                    new BimElementDto() { ItemID = item1, GlobalID = "BIM1" },
        //                    new BimElementDto() { ItemID = item1, GlobalID = "BIM2" }
        //                };
        //                await access.ObjectiveService.Update(obj);

        //                var bimComparer = new DelegateComparer<BimElementDto>((x, y) => x.ItemID == y.ItemID && x.GlobalID == y.GlobalID);

        //                var added = await access.ObjectiveService.Find(objID);
        //                CollectionAssert.That.AreEquivalent(obj.BimElements, added.BimElements, bimComparer);

        //                obj.BimElements = new List<BimElementDto>()
        //                {
        //                    new BimElementDto() { ItemID = item1, GlobalID = "BIM1" },
        //                    new BimElementDto() { ItemID = item1, GlobalID = "BIM3" },
        //                    new BimElementDto() { ItemID = item1, GlobalID = "BIM4" }
        //                };
        //                await access.ObjectiveService.Update(obj);
        //                added = await access.ObjectiveService.Find(objID);
        //                CollectionAssert.That.AreEquivalent(obj.BimElements, added.BimElements, bimComparer);
        //            }
        //        }

        //        [TestMethod]
        //        public async Task Can_query_requred_dynamic_fields()
        //        {
        //            using var transaction = Fixture.Connection.BeginTransaction();
        //            using (var context = Fixture.CreateContext(transaction))
        //            {
        //                var api = new DocumentManagementApi(context);
        //                var access = await api.Register(new UserToCreateDto("vpupkin", "123", "Vasily Pupkin"));

        //                //TODO: how to test?
        //                Assert.Fail();
        //            }
        //        }
    }
}
