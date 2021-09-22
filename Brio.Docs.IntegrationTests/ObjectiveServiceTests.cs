using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Brio.Docs.Client;
using Brio.Docs.Database.Models;
using Brio.Docs.Client.Dtos;
using Brio.Docs.Client.Exceptions;
using Brio.Docs.Common;
using Brio.Docs.Common.Dtos;
using Brio.Docs.Services;
using Brio.Docs.Tests.Utility;
using Brio.Docs.Utility;
using Brio.Docs.Utility.Mapping;
using Brio.Docs.Utility.Mapping.Resolvers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Brio.Docs.Database.Extensions;

namespace Brio.Docs.Tests
{
    [TestClass]
    public class ObjectiveServiceTests
    {
        private static ObjectiveService service;
        private static IMapper mapper;
        private ServiceProvider serviceProvider;

        private static SharedDatabaseFixture Fixture { get; set; }

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
                        ObjectiveID = firstObjective.ID,
                    });
                });
                context.SaveChanges();
            });

            IServiceCollection services = new ServiceCollection();
            services.AddTransient(
                x => new DynamicFieldModelToDtoValueResolver(
                    Fixture.Context,
                    mapper,
                    Mock.Of<ILogger<DynamicFieldModelToDtoValueResolver>>()));
            services.AddLogging();
            services.AddMappingResolvers();
            services.AddAutoMapper(typeof(MappingProfile));
            services.AddSingleton(x => Fixture.Context);
            serviceProvider = services.BuildServiceProvider();
            mapper = serviceProvider.GetService<IMapper>();

            service = new ObjectiveService(
                Fixture.Context,
                mapper,
                new ItemHelper(Mock.Of<ILogger<ItemHelper>>()),
                new DynamicFieldHelper(Fixture.Context, mapper, Mock.Of<ILogger<DynamicFieldHelper>>()),
                Mock.Of<ILogger<ObjectiveService>>());
        }

        [TestCleanup]
        public void Cleanup()
        {
            Fixture.Dispose();
            serviceProvider.Dispose();
        }

        [TestMethod]
        public async Task Add_NewObjectiveToCreate_ReturnsObjectiveToList()
        {
            var title = $"Test issue {Guid.NewGuid()}";
            var status = ObjectiveStatus.Open;
            var type = Fixture.Context.ObjectiveTypes.First();
            var project = Fixture.Context.Projects.Unsynchronized().First();
            var objectivesCount = Fixture.Context.Objectives.Unsynchronized().Count();
            var objectiveToCreate = new ObjectiveToCreateDto
            {
                Title = title,
                Description = "created for test purpose only",
                Status = status,
                ObjectiveTypeID = new ID<ObjectiveTypeDto>(type.ID),
                ProjectID = new ID<ProjectDto>(project.ID),
            };

            var result = await service.Add(objectiveToCreate);

            Assert.AreEqual(title, result.Title);
            Assert.AreEqual(status, result.Status);
            Assert.AreEqual(type.ID, (int)result.ObjectiveType.ID);
            Assert.AreEqual(objectivesCount + 1, Fixture.Context.Objectives.Unsynchronized().Count());
        }

        [TestMethod]
        public async Task Add_NewObjectiveToCreateWithExistingBimElements_ReturnsObjectiveToListAndDoesntCreateBimElements()
        {
            var title = $"Test issue {Guid.NewGuid()}";
            var status = ObjectiveStatus.Open;
            var type = Fixture.Context.ObjectiveTypes.First();
            var project = Fixture.Context.Projects.Unsynchronized().First();
            var objectivesCount = Fixture.Context.Objectives.Unsynchronized().Count();
            var bimElementsCount = Fixture.Context.BimElements.Count();
            var existingBimElement = Fixture.Context.BimElements.First();
            var bimList = new List<BimElementDto>
            {
                new BimElementDto
                {
                    GlobalID = existingBimElement.GlobalID,
                    ElementName = existingBimElement.ElementName,
                    ParentName = existingBimElement.ParentName,
                },
            };
            var objectiveToCreate = new ObjectiveToCreateDto
            {
                Title = title,
                Description = "created for test purpose only",
                Status = status,
                ObjectiveTypeID = new ID<ObjectiveTypeDto>(type.ID),
                ProjectID = new ID<ProjectDto>(project.ID),
                BimElements = bimList,
            };

            var result = await service.Add(objectiveToCreate);

            var addedObjective = Fixture.Context.Objectives.Unsynchronized().First(o => o.Title == title);
            Assert.AreEqual(title, result.Title);
            Assert.AreEqual(status, result.Status);
            Assert.AreEqual(type.ID, (int)result.ObjectiveType.ID);
            Assert.AreEqual(objectivesCount + 1, Fixture.Context.Objectives.Unsynchronized().Count());
            Assert.AreEqual(bimList.Count, addedObjective.BimElements.Count);
            Assert.AreEqual(bimElementsCount, Fixture.Context.BimElements.Count());
        }

        [TestMethod]
        public async Task Add_NewObjectiveToCreateWithNotExistingBimElements_ReturnsObjectiveToListAndCreatesBimElements()
        {
            var title = $"Test issue {Guid.NewGuid()}";
            var status = ObjectiveStatus.Open;
            var type = Fixture.Context.ObjectiveTypes.First();
            var project = Fixture.Context.Projects.Unsynchronized().First();
            var objectivesCount = Fixture.Context.Objectives.Unsynchronized().Count();
            var bimElementsCount = Fixture.Context.BimElements.Count();
            var bimList = new List<BimElementDto>
            {
                new BimElementDto
                {
                    GlobalID = $"uniqueGlobalId{Guid.NewGuid()}",
                    ElementName = "Doorway",
                    ParentName = "MEGA",
                },
            };
            var objectiveToCreate = new ObjectiveToCreateDto
            {
                Title = title,
                Description = "created for test purpose only",
                Status = status,
                ObjectiveTypeID = new ID<ObjectiveTypeDto>(type.ID),
                ProjectID = new ID<ProjectDto>(project.ID),
                BimElements = bimList,
            };

            var result = await service.Add(objectiveToCreate);

            var addedObjective = Fixture.Context.Objectives.Unsynchronized().First(o => o.Title == title);
            Assert.AreEqual(title, result.Title);
            Assert.AreEqual(status, result.Status);
            Assert.AreEqual(type.ID, (int)result.ObjectiveType.ID);
            Assert.AreEqual(objectivesCount + 1, Fixture.Context.Objectives.Unsynchronized().Count());
            Assert.AreEqual(bimList.Count, addedObjective.BimElements.Count);
            Assert.AreEqual(bimElementsCount + bimList.Count, Fixture.Context.BimElements.Count());
        }

        [TestMethod]
        public async Task Add_NewObjectiveToCreateWithDifferentBimElements_ReturnsObjectiveToListAndCreatesNotExistingBimElements()
        {
            var title = $"Test issue {Guid.NewGuid()}";
            var status = ObjectiveStatus.Open;
            var type = Fixture.Context.ObjectiveTypes.First();
            var project = Fixture.Context.Projects.Unsynchronized().First();
            var objectivesCount = Fixture.Context.Objectives.Unsynchronized().Count();
            var bimElementsCount = Fixture.Context.BimElements.Count();
            var existingBimElement = Fixture.Context.BimElements.First();
            var bimList = new List<BimElementDto>
            {
                    new BimElementDto
                    {
                        GlobalID = existingBimElement.GlobalID,
                        ElementName = existingBimElement.ElementName,
                        ParentName = existingBimElement.ParentName,
                    },
                    new BimElementDto
                    {
                        GlobalID = $"uniqueGlobalId{Guid.NewGuid()}",
                        ElementName = "Floor",
                        ParentName = "Home",
                    },
            };
            var objectiveToCreate = new ObjectiveToCreateDto
            {
                Title = title,
                Description = "created for test purpose only",
                Status = status,
                ObjectiveTypeID = new ID<ObjectiveTypeDto>(type.ID),
                ProjectID = new ID<ProjectDto>(project.ID),
                BimElements = bimList,
            };

            var result = await service.Add(objectiveToCreate);

            var addedObjective = Fixture.Context.Objectives.Unsynchronized().First(o => o.Title == title);
            Assert.AreEqual(title, result.Title);
            Assert.AreEqual(status, result.Status);
            Assert.AreEqual(type.ID, (int)result.ObjectiveType.ID);
            Assert.AreEqual(objectivesCount + 1, Fixture.Context.Objectives.Unsynchronized().Count());
            Assert.AreEqual(bimList.Count, addedObjective.BimElements.Count);
            Assert.AreEqual(bimElementsCount + 1, Fixture.Context.BimElements.Count());
        }

        [TestMethod]
        public async Task Add_NewObjectiveToCreateWithItems_ReturnsObjectiveToListAndAddsItemsToDb()
        {
            var title = $"Test issue {Guid.NewGuid()}";
            var status = ObjectiveStatus.Open;
            var type = Fixture.Context.ObjectiveTypes.First();
            var project = Fixture.Context.Projects.Unsynchronized().First();
            var objectivesCount = Fixture.Context.Objectives.Unsynchronized().Count();
            var bimElementsCount = Fixture.Context.BimElements.Count();
            var existingBimElement = Fixture.Context.BimElements.First();
            var dbItem = Fixture.Context.Items.Unsynchronized().First();
            var items = new List<ItemDto>
            {
                new ItemDto
                {
                    ID = new ID<ItemDto>(dbItem.ID),
                    ItemType = (ItemType)dbItem.ItemType,
                    RelativePath = dbItem.RelativePath,
                },
            };
            var objectiveToCreate = new ObjectiveToCreateDto
            {
                Title = title,
                Description = "created for test purpose only",
                Status = status,
                ObjectiveTypeID = new ID<ObjectiveTypeDto>(type.ID),
                ProjectID = new ID<ProjectDto>(project.ID),
                Items = items,
            };

            var result = await service.Add(objectiveToCreate);

            var addedObjective = Fixture.Context.Objectives.Unsynchronized().First(o => o.Title == title);
            Assert.AreEqual(title, result.Title);
            Assert.AreEqual(status, result.Status);
            Assert.AreEqual(type.ID, (int)result.ObjectiveType.ID);
            Assert.AreEqual(objectivesCount + 1, Fixture.Context.Objectives.Unsynchronized().Count());
            Assert.AreEqual(items.Count, addedObjective.Items.Count);
        }

        [TestMethod]
        public async Task Add_NewObjectiveToCreateWithDynamicFields_ReturnsObjectiveToListAndAddsDynamicFieldsToDb()
        {
            var title = $"Test issue {Guid.NewGuid()}";
            var status = ObjectiveStatus.Open;
            var type = Fixture.Context.ObjectiveTypes.First();
            var project = Fixture.Context.Projects.Unsynchronized().First();
            var objectivesCount = Fixture.Context.Objectives.Unsynchronized().Count();
            var bimElementsCount = Fixture.Context.BimElements.Count();
            var existingBimElement = Fixture.Context.BimElements.First();
            var dynamicFields = new List<DynamicFieldDto>
            {
                MockData.DEFAULT_DYNAMIC_FIELDS_TO_CREATE[0],
                MockData.DEFAULT_DYNAMIC_FIELDS_TO_CREATE[1],
            };
            var objectiveToCreate = new ObjectiveToCreateDto
            {
                Title = title,
                Description = "created for test purpose only",
                Status = status,
                ObjectiveTypeID = new ID<ObjectiveTypeDto>(type.ID),
                ProjectID = new ID<ProjectDto>(project.ID),
                DynamicFields = dynamicFields,
            };

            var result = await service.Add(objectiveToCreate);

            var addedObjective = Fixture.Context.Objectives.Unsynchronized().First(o => o.Title == title);
            Assert.AreEqual(title, result.Title);
            Assert.AreEqual(status, result.Status);
            Assert.AreEqual(type.ID, (int)result.ObjectiveType.ID);
            Assert.AreEqual(objectivesCount + 1, Fixture.Context.Objectives.Unsynchronized().Count());
            Assert.AreEqual(dynamicFields.Count, addedObjective.DynamicFields.Count);
        }

        [TestMethod]
        public async Task Add_NewObjectiveToCreateWithAllAdditionalFields_ReturnsObjectiveToListAndAddsAdditionalFieldsToDb()
        {
            var title = $"Test issue {Guid.NewGuid()}";
            var status = ObjectiveStatus.Open;
            var type = Fixture.Context.ObjectiveTypes.First();
            var project = Fixture.Context.Projects.Unsynchronized().First();
            var objectivesCount = Fixture.Context.Objectives.Unsynchronized().Count();
            var bimElementsCount = Fixture.Context.BimElements.Count();
            var existingBimElement = Fixture.Context.BimElements.First();
            var dbItem = Fixture.Context.Items.Unsynchronized().First();
            var bimList = new List<BimElementDto>
            {
                new BimElementDto
                {
                    GlobalID = existingBimElement.GlobalID,
                    ElementName = existingBimElement.ElementName,
                    ParentName = existingBimElement.ParentName,
                },
                new BimElementDto
                {
                    GlobalID = $"uniqueGlobalId{Guid.NewGuid()}",
                    ElementName = "Floor",
                    ParentName = "Home",
                },
            };
            var items = new List<ItemDto>
            {
                new ItemDto
                {
                    ID = new ID<ItemDto>(dbItem.ID),
                    ItemType = (ItemType)dbItem.ItemType,
                    RelativePath = dbItem.RelativePath,
                },
            };
            var dynamicFields = new List<DynamicFieldDto>
            {
                MockData.DEFAULT_DYNAMIC_FIELDS_TO_CREATE[0],
                MockData.DEFAULT_DYNAMIC_FIELDS_TO_CREATE[1],
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
                DynamicFields = dynamicFields,
            };

            var result = await service.Add(objectiveToCreate);

            var addedObjective = Fixture.Context.Objectives.Unsynchronized().First(o => o.Title == title);
            Assert.AreEqual(title, result.Title);
            Assert.AreEqual(status, result.Status);
            Assert.AreEqual(type.ID, (int)result.ObjectiveType.ID);
            Assert.AreEqual(objectivesCount + 1, Fixture.Context.Objectives.Unsynchronized().Count());
            Assert.AreEqual(items.Count, addedObjective.Items.Count);
            Assert.AreEqual(dynamicFields.Count, addedObjective.DynamicFields.Count);
            Assert.AreEqual(bimList.Count, addedObjective.BimElements.Count);
            Assert.AreEqual(bimElementsCount + 1, Fixture.Context.BimElements.Count());
        }

        [TestMethod]
        public async Task Find_ExistingObjective_ReturnsObjectiveWithIncludedFields()
        {
            var existingObjective = Fixture.Context.Objectives.Unsynchronized()
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
        [ExpectedException(typeof(NotFoundException<Objective>))]
        public async Task Find_NotExistingObjective_RaisesNotFoundException()
        {
            var result = await service.Find(ID<ObjectiveDto>.InvalidID);

            Assert.Fail();
        }

        [TestMethod]
        public async Task GetObjectives_ExistingProjectWithObjectives_ReturnsEnumerableWithProjectObjectives()
        {
            var existingProject = Fixture.Context.Projects.Unsynchronized().First(p => p.Objectives.Count > 0);
            var dtoId = new ID<ProjectDto>(existingProject.ID);

            var result = await service.GetObjectives(dtoId, new Client.Filters.ObjectiveFilterParameters());

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Items.Any());
        }

        [TestMethod]
        [ExpectedException(typeof(NotFoundException<Project>))]
        public async Task GetObjectives_NotExistingProject_RaisesNotFoundException()
        {
            var dtoId = ID<ProjectDto>.InvalidID;

            var result = await service.GetObjectives(dtoId, new Client.Filters.ObjectiveFilterParameters());

            Assert.Fail();
        }

        [TestMethod]
        public async Task Remove_ExistingObjective_ReturnsTrueAndDeletesRelatedBridges()
        {
            var existingObjective = Fixture.Context.Objectives.Unsynchronized()
                .First(o => o.DynamicFields.Count > 0
                            && o.BimElements.Count > 0
                            && o.Items.Count > 0);
            var startObjectiveItemsCount = Fixture.Context.ObjectiveItems.Count();
            var relatedObjectiveItemsCount = Fixture.Context.ObjectiveItems.Where(oi => oi.ObjectiveID == existingObjective.ID).Count();
            var startDynamicFieldsCount = Fixture.Context.DynamicFields.Unsynchronized().Count();
            var relatedDynamicFields = Fixture.Context.DynamicFields.Unsynchronized().Where(f => f.ObjectiveID == existingObjective.ID).Count();
            var startBeoCount = Fixture.Context.BimElementObjectives.Count();
            var relatedBeo = Fixture.Context.BimElementObjectives.Where(beo => beo.ObjectiveID == existingObjective.ID).Count();
            var dtoId = new ID<ObjectiveDto>(existingObjective.ID);

            var result = await service.Remove(dtoId);

            Assert.IsTrue(result);
            Assert.AreEqual(startObjectiveItemsCount - relatedObjectiveItemsCount, Fixture.Context.ObjectiveItems.Count());
            Assert.AreEqual(startDynamicFieldsCount - relatedDynamicFields, Fixture.Context.DynamicFields.Unsynchronized().Count());
            Assert.AreEqual(startBeoCount - relatedBeo, Fixture.Context.BimElementObjectives.Count());
        }

        [TestMethod]
        [ExpectedException(typeof(NotFoundException<Objective>))]
        public async Task Remove_NotExistingObjective_RaisesNotFoundException()
        {
            var dtoId = ID<ObjectiveDto>.InvalidID;

            await service.Remove(dtoId);

            Assert.Fail();
        }

        [TestMethod]
        public async Task Update_ExistingObjectiveWithoutAdditionalFields_ReturnsTrueAndClearsAdditionalFields()
        {
            var existingObjective = Fixture.Context.Objectives.Unsynchronized()
                .First(o => o.DynamicFields.Count > 0
                            && o.BimElements.Count > 0
                            && o.Items.Count > 0);
            var newDescription = $"newDescription{Guid.NewGuid()}";
            var newTitle = $"newTitle{Guid.NewGuid()}";
            var newStatus = (ObjectiveStatus)existingObjective.Status != ObjectiveStatus.InProgress
                            ? ObjectiveStatus.InProgress
                            : ObjectiveStatus.Ready;
            var newProject = Fixture.Context.Projects.Unsynchronized().First(p => p.ID != existingObjective.ProjectID);
            var changedObjective = new ObjectiveDto
            {
                AuthorID = new ID<UserDto>(existingObjective.AuthorID!.Value),
                CreationDate = existingObjective.CreationDate,
                DueDate = existingObjective.DueDate,
                Description = newDescription,
                ID = new ID<ObjectiveDto>(existingObjective.ID),
                ObjectiveTypeID = new ID<ObjectiveTypeDto>(existingObjective.ObjectiveTypeID),
                ProjectID = new ID<ProjectDto>(existingObjective.ProjectID),
                Status = newStatus,
                Title = newTitle,
            };

            var result = await service.Update(changedObjective);

            var updatedObjective = Fixture.Context.Objectives.Unsynchronized().First(o => o.ID == existingObjective.ID);
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
            var existingObjective = Fixture.Context.Objectives.Unsynchronized()
                .First(o => o.DynamicFields.Count > 0
                            && o.BimElements.Count > 0
                            && o.Items.Count > 0);
            var newDescription = $"newDescription{Guid.NewGuid()}";
            var newTitle = $"newTitle{Guid.NewGuid()}";
            var newStatus = (ObjectiveStatus)existingObjective.Status != ObjectiveStatus.InProgress
                            ? ObjectiveStatus.InProgress
                            : ObjectiveStatus.Ready;
            var newProject = Fixture.Context.Projects.Unsynchronized().First(p => p.ID != existingObjective.ProjectID);
            var guid = Guid.NewGuid();
            var newDynamicFields = new List<DynamicFieldDto>
            {
                new DynamicFieldDto
                {
                    Name = $"name{guid}",
                    Value = $"value{guid}",
                    Type = DynamicFieldType.STRING,
                },
            };
            var firstDynamicField = existingObjective.DynamicFields.First();
            var existingDynamicFields = new List<DynamicFieldDto>
            {
                new DynamicFieldDto
                {
                    Name = firstDynamicField.Name,
                    Value = firstDynamicField.Value,
                    Type = DynamicFieldType.STRING,
                },
            };
            var deletingDynamicFieldsCount = existingObjective.DynamicFields.Count - 1;
            var startDynamicFieldsCount = Fixture.Context.DynamicFields.Unsynchronized().Count();
            var dynamicFields = new List<DynamicFieldDto>();
            dynamicFields.AddRange(newDynamicFields);
            dynamicFields.AddRange(existingDynamicFields);
            var changedObjective = new ObjectiveDto
            {
                AuthorID = new ID<UserDto>(existingObjective.AuthorID!.Value),
                CreationDate = existingObjective.CreationDate,
                DueDate = existingObjective.DueDate,
                Description = newDescription,
                DynamicFields = dynamicFields,
                ID = new ID<ObjectiveDto>(existingObjective.ID),
                ObjectiveTypeID = new ID<ObjectiveTypeDto>(existingObjective.ObjectiveTypeID),
                ProjectID = new ID<ProjectDto>(existingObjective.ProjectID),
                Status = newStatus,
                Title = newTitle,
            };

            var result = await service.Update(changedObjective);

            var updatedObjective = Fixture.Context.Objectives.Unsynchronized().First(o => o.ID == existingObjective.ID);
            Assert.IsTrue(result);
            Assert.AreEqual(newDescription, updatedObjective.Description);
            Assert.AreEqual((int)newStatus, updatedObjective.Status);
            Assert.AreEqual(newTitle, updatedObjective.Title);
            Assert.AreEqual(startDynamicFieldsCount - deletingDynamicFieldsCount + newDynamicFields.Count, Fixture.Context.DynamicFields.Unsynchronized().Count());
            Assert.AreEqual(dynamicFields.Count, changedObjective.DynamicFields.Count());
            updatedObjective.DynamicFields.ToList().ForEach(df =>
            {
                Assert.IsTrue(dynamicFields.Any(cdf => CompareDynamicFieldDtoToModel(cdf, df)));
            });
        }

        [TestMethod]
        public async Task Update_ExistingObjectiveWithBimElements_ReturnsTrueAndUpdatesBimElements()
        {
            var existingObjective = Fixture.Context.Objectives.Unsynchronized()
                .First(o => o.DynamicFields.Count > 0
                            && o.BimElements.Count > 0
                            && o.Items.Count > 0);
            var newDescription = $"newDescription{Guid.NewGuid()}";
            var newTitle = $"newTitle{Guid.NewGuid()}";
            var newStatus = (ObjectiveStatus)existingObjective.Status != ObjectiveStatus.InProgress
                            ? ObjectiveStatus.InProgress
                            : ObjectiveStatus.Ready;
            var newProject = Fixture.Context.Projects.Unsynchronized().First(p => p.ID != existingObjective.ProjectID);
            var startBimElementsCount = Fixture.Context.BimElementObjectives.Count();
            var deletingBimElementsCount = existingObjective.BimElements.Count - 1;
            var firstBimElement = existingObjective.BimElements.First().BimElement;
            var changedBimElements = new List<BimElementDto>
            {
                 new BimElementDto
                    {
                        GlobalID = firstBimElement.GlobalID,
                        ElementName = firstBimElement.ElementName,
                        ParentName = firstBimElement.ParentName,
                    },
                 new BimElementDto
                    {
                        GlobalID = $"uniqueGlobalId{Guid.NewGuid()}",
                        ElementName = firstBimElement.ElementName,
                        ParentName = firstBimElement.ParentName,
                    },
            };
            var newBimElementsCount = changedBimElements.Count - 1;
            var changedObjective = new ObjectiveDto
            {
                AuthorID = new ID<UserDto>(existingObjective.AuthorID!.Value),
                BimElements = changedBimElements,
                CreationDate = existingObjective.CreationDate,
                DueDate = existingObjective.DueDate,
                Description = newDescription,
                ID = new ID<ObjectiveDto>(existingObjective.ID),
                ObjectiveTypeID = new ID<ObjectiveTypeDto>(existingObjective.ObjectiveTypeID),
                ProjectID = new ID<ProjectDto>(existingObjective.ProjectID),
                Status = newStatus,
                Title = newTitle,
            };

            var result = await service.Update(changedObjective);

            var updatedObjective = Fixture.Context.Objectives.Unsynchronized().First(o => o.ID == existingObjective.ID);
            Assert.IsTrue(result);
            Assert.AreEqual(newDescription, updatedObjective.Description);
            Assert.AreEqual((int)newStatus, updatedObjective.Status);
            Assert.AreEqual(newTitle, updatedObjective.Title);
            Assert.AreEqual(changedBimElements.Count, updatedObjective.BimElements.Count);
            Assert.AreEqual(startBimElementsCount - deletingBimElementsCount + newBimElementsCount, Fixture.Context.BimElementObjectives.Count());
            updatedObjective.BimElements.ToList().ForEach(be =>
            {
                Assert.IsTrue(changedBimElements.Any(cbe => cbe.GlobalID == be.BimElement.GlobalID
                                                            && cbe.ParentName == be.BimElement.ParentName));
            });
        }

        [TestMethod]
        public async Task Update_ExistingObjectiveWithItems_ReturnsTrueAndUpdatesItems()
        {
            var existingObjective = Fixture.Context.Objectives.Unsynchronized()
                .First(o => o.DynamicFields.Count > 0
                            && o.BimElements.Count > 0
                            && o.Items.Count > 0);
            var newDescription = $"newDescription{Guid.NewGuid()}";
            var newTitle = $"newTitle{Guid.NewGuid()}";
            var newStatus = (ObjectiveStatus)existingObjective.Status != ObjectiveStatus.InProgress
                            ? ObjectiveStatus.InProgress
                            : ObjectiveStatus.Ready;
            var newProject = Fixture.Context.Projects.Unsynchronized().First(p => p.ID != existingObjective.ProjectID);
            var guid = Guid.NewGuid();
            var newItems = new List<ItemDto>
            {
                new ItemDto
                {
                    ItemType = ItemType.Media,
                    RelativePath = $"Name{guid}",
                },
            };
            var firstItem = existingObjective.Items.First().Item;
            var deletingItemsCount = existingObjective.Items.Count - 1;
            var existingItems = new List<ItemDto>
            {
                new ItemDto
                {
                    ID = new ID<ItemDto>(firstItem.ID),
                    ItemType = (ItemType)firstItem.ItemType,
                    RelativePath = firstItem.RelativePath,
                },
            };
            var startItemsCount = existingObjective.Items.Count;
            var objectiveItemsLinksCount = Fixture.Context.ObjectiveItems.Count();
            var items = new List<ItemDto>();
            items.AddRange(newItems);
            items.AddRange(existingItems);
            var changedObjective = new ObjectiveDto
            {
                AuthorID = new ID<UserDto>(existingObjective.AuthorID!.Value),
                CreationDate = existingObjective.CreationDate,
                DueDate = existingObjective.DueDate,
                Description = newDescription,
                ID = new ID<ObjectiveDto>(existingObjective.ID),
                Items = items,
                ObjectiveTypeID = new ID<ObjectiveTypeDto>(existingObjective.ObjectiveTypeID),
                ProjectID = new ID<ProjectDto>(existingObjective.ProjectID),
                Status = newStatus,
                Title = newTitle,
            };

            var result = await service.Update(changedObjective);

            var updatedObjective = Fixture.Context.Objectives.Unsynchronized().First(o => o.ID == existingObjective.ID);
            Assert.IsTrue(result);
            Assert.AreEqual(newDescription, updatedObjective.Description);
            Assert.AreEqual((int)newStatus, updatedObjective.Status);
            Assert.AreEqual(newTitle, updatedObjective.Title);
            Assert.AreEqual(startItemsCount - deletingItemsCount + newItems.Count, updatedObjective.Items.Count);
            Assert.AreEqual(objectiveItemsLinksCount - deletingItemsCount + newItems.Count, Fixture.Context.ObjectiveItems.Count());
            Assert.AreEqual(updatedObjective.Items.Count, items.Count);
            updatedObjective.Items.Select(oi => oi.Item).ToList().ForEach(i =>
            {
                Assert.IsTrue(items.Any(ci => (int)ci.ItemType == i.ItemType
                                                    && ci.RelativePath == i.RelativePath));
            });
        }

        [TestMethod]
        public async Task Update_ExistingObjectiveWithAdditionalFields_ReturnsTrueAndUpdatesAdditionalFields()
        {
            var existingObjective = Fixture.Context.Objectives.Unsynchronized()
                .First(o => o.DynamicFields.Count > 0
                            && o.BimElements.Count > 0
                            && o.Items.Count > 0);
            var newDescription = $"newDescription{Guid.NewGuid()}";
            var newTitle = $"newTitle{Guid.NewGuid()}";
            var newStatus = (ObjectiveStatus)existingObjective.Status != ObjectiveStatus.InProgress
                            ? ObjectiveStatus.InProgress
                            : ObjectiveStatus.Ready;
            var newProject = Fixture.Context.Projects.Unsynchronized().First(p => p.ID != existingObjective.ProjectID);
            var guid = Guid.NewGuid();

            // Dynamic fields
            var newDynamicFields = new List<DynamicFieldDto>
            {
                new DynamicFieldDto
                {
                    Name = $"name{guid}",
                    Value = $"value{guid}",
                    Type = DynamicFieldType.STRING,
                },
            };
            var firstDynamicField = existingObjective.DynamicFields.First();
            var existingDynamicFields = new List<DynamicFieldDto>
            {
                new DynamicFieldDto
                {
                    Name = firstDynamicField.Name,
                    Value = firstDynamicField.Value,
                    Type = DynamicFieldType.STRING,
                },
            };
            var deletingDynamicFieldsCount = existingObjective.DynamicFields.Count - 1;
            var startDynamicFieldsCount = Fixture.Context.DynamicFields.Unsynchronized().Count();
            var dynamicFields = new List<DynamicFieldDto>();
            dynamicFields.AddRange(newDynamicFields);
            dynamicFields.AddRange(existingDynamicFields);

            // Items
            var newItems = new List<ItemDto>
            {
                new ItemDto
                {
                    ItemType = ItemType.Media,
                    RelativePath = $"Name{guid}",
                },
            };
            var firstItem = existingObjective.Items.First().Item;
            var deletingItemsCount = existingObjective.Items.Count - 1;
            var existingItems = new List<ItemDto>
            {
                new ItemDto
                {
                    ID = new ID<ItemDto>(firstItem.ID),
                    ItemType = (ItemType)firstItem.ItemType,
                    RelativePath = firstItem.RelativePath,
                },
            };
            var startItemsCount = existingObjective.Items.Count;
            var items = new List<ItemDto>();
            items.AddRange(newItems);

            // Bim elements
            items.AddRange(existingItems);
            var startBimElementsCount = Fixture.Context.BimElementObjectives.Count();
            var deletingBimElementsCount = existingObjective.BimElements.Count - 1;
            var firstBimElement = existingObjective.BimElements.First().BimElement;
            var changedBimElements = new List<BimElementDto>
            {
                 new BimElementDto
                    {
                        GlobalID = firstBimElement.GlobalID,
                        ElementName = firstBimElement.ElementName,
                        ParentName = firstBimElement.ParentName,
                    },
                 new BimElementDto
                    {
                        GlobalID = $"uniqueGlobalId{Guid.NewGuid()}",
                        ElementName = "Floor",
                        ParentName = "Home",
                    },
            };
            var newBimElementsCount = changedBimElements.Count - 1;

            var changedObjective = new ObjectiveDto
            {
                AuthorID = new ID<UserDto>(existingObjective.AuthorID!.Value),
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
                Title = newTitle,
            };

            var result = await service.Update(changedObjective);

            var updatedObjective = Fixture.Context.Objectives.Unsynchronized().First(o => o.ID == existingObjective.ID);
            Assert.IsTrue(result);
            Assert.AreEqual(newDescription, updatedObjective.Description);
            Assert.AreEqual((int)newStatus, updatedObjective.Status);
            Assert.AreEqual(newTitle, updatedObjective.Title);
            Assert.AreEqual(startItemsCount - deletingItemsCount + newItems.Count, updatedObjective.Items.Count);
            Assert.AreEqual(updatedObjective.Items.Count, items.Count);
            Assert.AreEqual(startDynamicFieldsCount - deletingDynamicFieldsCount + newDynamicFields.Count, Fixture.Context.DynamicFields.Unsynchronized().Count());
            Assert.AreEqual(dynamicFields.Count, changedObjective.DynamicFields.Count());
            Assert.AreEqual(changedBimElements.Count, updatedObjective.BimElements.Count);
            Assert.AreEqual(startBimElementsCount - deletingBimElementsCount + newBimElementsCount, Fixture.Context.BimElementObjectives.Count());
        }

        [TestMethod]
        [ExpectedException(typeof(NotFoundException<Objective>))]
        public async Task Update_NotExistingObjective_RaisesNotFoundException()
        {
            var notExistingObjective = new ObjectiveDto { ID = ID<ObjectiveDto>.InvalidID };

            await service.Update(notExistingObjective);

            Assert.Fail();
        }

        private bool CompareDynamicFieldDtoToModel(DynamicFieldDto dto, DynamicField model)
        {
            return dto.Name == model.Name
                && dto.Type == (DynamicFieldType)Enum.Parse(typeof(DynamicFieldType), model.Type);
        }
    }
}
