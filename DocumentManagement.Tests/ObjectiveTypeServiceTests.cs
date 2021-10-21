using System;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using MRS.DocumentManagement.Database.Models;
using MRS.DocumentManagement.Interface.Dtos;
using MRS.DocumentManagement.Interface.Exceptions;
using MRS.DocumentManagement.Services;
using MRS.DocumentManagement.Tests.Utility;
using MRS.DocumentManagement.Utility.Mapping;

namespace MRS.DocumentManagement.Tests
{
    [TestClass]
    public class ObjectiveTypeServiceTests
    {
        private static ObjectiveTypeService service;
        private static IMapper mapper;

        private static SharedDatabaseFixture Fixture { get; set; }

        [ClassInitialize]
        public static void ClassSetup(TestContext unused)
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
            service = new ObjectiveTypeService(Fixture.Context, mapper, Mock.Of<ILogger<ObjectiveTypeService>>());
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
        [ExpectedException(typeof(ArgumentException))]
        public async Task Add_ExistingType_RaisesArgumentException()
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
        [ExpectedException(typeof(NotFoundException<ObjectiveType>))]
        public async Task FindById_NotExistingType_ReturnsNull()
        {
            await service.Find(ID<ObjectiveTypeDto>.InvalidID);

            Assert.Fail();
        }

        [TestMethod]
        public async Task FindByName_ExistingType_ReturnsObjectiveType()
        {
            var existingType = Fixture.Context.ObjectiveTypes.First();

            var result = await service.Find(existingType.Name);

            Assert.AreEqual(existingType.Name, result.Name);
        }

        [TestMethod]
        [ExpectedException(typeof(NotFoundException<ObjectiveType>))]
        public async Task FindByName_NotExistingType_RaisesNotFoundException()
        {
            var notExistingTypeName = $"invalidName{Guid.NewGuid()}";

            await service.Find(notExistingTypeName);

            Assert.Fail();
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
        [ExpectedException(typeof(NotFoundException<ObjectiveType>))]
        public async Task Remove_NotExistingType_RaisesNotFoundException()
        {
            var result = await service.Remove(ID<ObjectiveTypeDto>.InvalidID);

            Assert.Fail();
        }
    }
}
