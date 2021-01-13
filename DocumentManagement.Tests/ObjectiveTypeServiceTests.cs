using AutoMapper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MRS.DocumentManagement.Interface;
using MRS.DocumentManagement.Interface.Dtos;
using MRS.DocumentManagement.Services;
using MRS.DocumentManagement.Tests.Utility;
using MRS.DocumentManagement.Utility;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace MRS.DocumentManagement.Tests
{
    [TestClass]
    public class ObjectiveTypeServiceTests
    {
        private static SharedDatabaseFixture Fixture { get; set; }

        private static ObjectiveTypeService service;
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

                var types = MockData.DEFAULT_OBJECTIVE_TYPES;
                context.ObjectiveTypes.AddRange(types);
                context.SaveChanges();
            });

            service = new ObjectiveTypeService(Fixture.Context, mapper);
        }

        [TestCleanup]
        public void Cleanup()
            => Fixture.Dispose();

        [TestMethod]
        public async Task Add_NewType_ReturnsAddedTypeId()
        {
            var newTypeName = $"newTypeName{Guid.NewGuid()}";

            var result = await service.Add(newTypeName);

            var addedType = Fixture.Context.ObjectiveTypes.FirstOrDefault(t => t.Name == newTypeName);
            Assert.IsNotNull(addedType);
            Assert.AreEqual(addedType.ID, (int)result);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidDataException))]
        public async Task Add_ExistingType_RaisesInvalidDataException()
        {
            var existingType = Fixture.Context.ObjectiveTypes.First();
            
            await service.Add(existingType.Name);

            Assert.Fail();
        }

        [TestMethod]
        public async Task FindById_ExistingType_ReturnsObjectiveType()
        {
            var existingType = Fixture.Context.ObjectiveTypes.First();

            var result = await service.Find(new ID<ObjectiveTypeDto>(existingType.ID));

            Assert.AreEqual(existingType.ID, (int)result.ID);
        }

        [TestMethod]
        public async Task FindById_NotExistingType_ReturnsNull()
        {
            var result = await service.Find(ID<ObjectiveTypeDto>.InvalidID);

            Assert.IsNull(result);
        }

        [TestMethod]
        public async Task FindByName_ExistingType_ReturnsObjectiveType()
        {
            var existingType = Fixture.Context.ObjectiveTypes.First();

            var result = await service.Find(existingType.Name);

            Assert.AreEqual(existingType.Name, result.Name);
        }

        [TestMethod]
        public async Task FindByName_NotExistingType_ReturnsNull()
        {
            var notExistingTypeName = $"invalidName{Guid.NewGuid()}";

            var result = await service.Find(notExistingTypeName);

            Assert.IsNull(result);
        }

        [TestMethod]
        public async Task GetAllObjectiveTypes_NormalWay_ReturnsObjectiveTypesEnumerable()
        {
            var existingType = Fixture.Context.ObjectiveTypes.First();

            var result = await service.Find(existingType.Name);

            Assert.AreEqual(existingType.Name, result.Name);
        }

        [TestMethod]
        public async Task Remove_ExistingType_ReturnsTrueAndRemovesType()
        {
            var existingType = Fixture.Context.ObjectiveTypes.First();

            var result = await service.Remove(new ID<ObjectiveTypeDto>(existingType.ID));

            Assert.IsTrue(result);
            Assert.IsFalse(Fixture.Context.ObjectiveTypes.Any(t => t.ID == existingType.ID));
        }

        [TestMethod]
        public async Task Remove_NotExistingType_ReturnsFalse()
        {
            var result = await service.Remove(ID<ObjectiveTypeDto>.InvalidID);

            Assert.IsFalse(result);
        }



        //        [TestMethod]
        //        public async Task Can_add_new_objective_type()
        //        {
        //            using var transaction = Fixture.Connection.BeginTransaction();
        //            using (var context = Fixture.CreateContext(transaction))
        //            {
        //                var api = new DocumentManagementApi(context);
        //                var access = await api.Register(new UserToCreateDto("vpupkin", "123", "Vasily Pupkin"));

        //                var taskTypeID = await access.ObjectiveTypeService.Add("Задание");
        //                var errorTypeID = await access.ObjectiveTypeService.Add("Нарушение");

        //                Assert.IsTrue(taskTypeID.IsValid);
        //                Assert.IsTrue(errorTypeID.IsValid);
        //            }
        //        }

        //        [TestMethod]
        //        [ExpectedException(typeof(InvalidDataException))]
        //        public async Task Can_not_add_duplicate_objective_type()
        //        {
        //            using var transaction = Fixture.Connection.BeginTransaction();
        //            using (var context = Fixture.CreateContext(transaction))
        //            {
        //                var api = new DocumentManagementApi(context);
        //                var access = await api.Register(new UserToCreateDto("vpupkin", "123", "Vasily Pupkin"));

        //                var id1 = await access.ObjectiveTypeService.Add("Задание");
        //                var id2 = await access.ObjectiveTypeService.Add("Задание");

        //                Assert.Fail();
        //            }
        //        }

        //        [TestMethod]
        //        public async Task Can_query_objective_types()
        //        {
        //            using var transaction = Fixture.Connection.BeginTransaction();
        //            using (var context = Fixture.CreateContext(transaction))
        //            {
        //                var api = new DocumentManagementApi(context);
        //                var access = await api.Register(new UserToCreateDto("vpupkin", "123", "Vasily Pupkin"));

        //                var taskTypeID = await access.ObjectiveTypeService.Add("Задание");
        //                var errorTypeID = await access.ObjectiveTypeService.Add("Нарушение");

        //                var types = await access.ObjectiveTypeService.GetAllObjectiveTypes();
        //                var expected = new ObjectiveTypeDto[] 
        //                {
        //                    new ObjectiveTypeDto(){ ID = taskTypeID, Name = "Задание" },
        //                    new ObjectiveTypeDto(){ ID = errorTypeID, Name = "Нарушение" }
        //                };
        //                var comparer = new DelegateComparer<ObjectiveTypeDto>((x, y) => x.ID == y.ID && x.Name == y.Name);
        //                CollectionAssert.That.AreEquivalent(expected, types, comparer);

        //                var nonexsitent = await access.ObjectiveTypeService.Find(ID<ObjectiveTypeDto>.InvalidID);
        //                Assert.IsNull(nonexsitent);

        //                var item1 = await access.ObjectiveTypeService.Find(taskTypeID);
        //                Assert.IsNotNull(item1);
        //                Assert.IsTrue(comparer.Equals(expected[0], item1));

        //                var item2 = await access.ObjectiveTypeService.Find("Нарушение");
        //                Assert.IsNotNull(item2);
        //                Assert.IsTrue(comparer.Equals(expected[1], item2));
        //            }
        //        }

        //        [TestMethod]
        //        public async Task Can_remove_objective_types()
        //        {
        //            using var transaction = Fixture.Connection.BeginTransaction();
        //            using (var context = Fixture.CreateContext(transaction))
        //            {
        //                var api = new DocumentManagementApi(context);
        //                var access = await api.Register(new UserToCreateDto("vpupkin", "123", "Vasily Pupkin"));

        //                var taskTypeID = await access.ObjectiveTypeService.Add("Задание");
        //                var errorTypeID = await access.ObjectiveTypeService.Add("Нарушение");

        //                var isRemoved = await access.ObjectiveTypeService.Remove(taskTypeID);
        //                Assert.IsTrue(isRemoved);

        //                isRemoved = await access.ObjectiveTypeService.Remove(taskTypeID);
        //                Assert.IsFalse(isRemoved);

        //                var types = await access.ObjectiveTypeService.GetAllObjectiveTypes();
        //                var expected = new ObjectiveTypeDto[]
        //                {
        //                    new ObjectiveTypeDto(){ ID = errorTypeID, Name = "Нарушение" }
        //                };
        //                var comparer = new DelegateComparer<ObjectiveTypeDto>((x, y) => x.ID == y.ID && x.Name == y.Name);
        //                CollectionAssert.That.AreEquivalent(expected, types, comparer);
        //            }
        //        }
    }
}
