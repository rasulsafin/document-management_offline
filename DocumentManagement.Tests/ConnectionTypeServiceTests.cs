using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using MRS.DocumentManagement.Interface;
using MRS.DocumentManagement.Interface.Dtos;
using MRS.DocumentManagement.Interface.Services;
using MRS.DocumentManagement.Services;
using MRS.DocumentManagement.Tests.Utility;
using MRS.DocumentManagement.Utility;
using MRS.DocumentManagement.Utility.Mapping;

namespace MRS.DocumentManagement.Tests
{
    [TestClass]
    public class ConnectionTypeServiceTests
    {
        private static IConnectionTypeService service;
        private static IMapper mapper;

        private static SharedDatabaseFixture Fixture { get; set; }

        [ClassInitialize]
        public static void ClassSetup(TestContext _)
        {
            IServiceCollection services = new ServiceCollection();

            var mock = new Mock<CryptographyHelper>();
            services.AddTransient<CryptographyHelper>(sp => mock.Object);

            services.AddAutoMapper(typeof(MappingProfile));
            IServiceProvider serviceProvider = services.BuildServiceProvider();
            mapper = serviceProvider.GetService<IMapper>();
        }

        [TestInitialize]
        public void Setup()
        {
            Fixture = new SharedDatabaseFixture(context =>
            {
                context.Database.EnsureDeleted();
                context.Database.EnsureCreated();

                var types = MockData.DEFAULT_CONNECTION_TYPES;
                context.ConnectionTypes.AddRange(types);
                context.SaveChanges();
            });

            service = new ConnectionTypeService(Fixture.Context, mapper, Mock.Of<ILogger<ConnectionTypeService>>());
        }

        [TestCleanup]
        public void Cleanup()
            => Fixture.Dispose();

        [TestMethod]
        public async Task Add_NewType_ReturnsAddedTypeId()
        {
            var newTypeName = $"newTypeName{Guid.NewGuid()}";

            var result = await service.Add(newTypeName);

            var addedType = Fixture.Context.ConnectionTypes.FirstOrDefault(t => t.Name == newTypeName);
            Assert.IsNotNull(addedType);
            Assert.AreEqual(addedType.ID, (int)result);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidDataException))]
        public async Task Add_ExistingType_RaisesInvalidDataException()
        {
            var existingType = Fixture.Context.ConnectionTypes.First();

            await service.Add(existingType.Name);

            Assert.Fail();
        }

        [TestMethod]
        public async Task FindById_ExistingType_ReturnsConnectionType()
        {
            var existingType = Fixture.Context.ConnectionTypes.First();

            var result = await service.Find(new ID<ConnectionTypeDto>(existingType.ID));

            Assert.AreEqual(existingType.ID, (int)result.ID);
        }

        [TestMethod]
        public async Task FindById_NotExistingType_ReturnsNull()
        {
            var result = await service.Find(ID<ConnectionTypeDto>.InvalidID);

            Assert.IsNull(result);
        }

        [TestMethod]
        public async Task FindByName_ExistingType_ReturnsConnectionType()
        {
            var existingType = Fixture.Context.ConnectionTypes.First();

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
        public async Task GetAllConnectionTypes_NormalWay_ReturnsConnectionTypesEnumerable()
        {
            var existingType = Fixture.Context.ConnectionTypes.First();

            var result = await service.Find(existingType.Name);

            Assert.AreEqual(existingType.Name, result.Name);
        }

        [TestMethod]
        public async Task Remove_ExistingType_ReturnsTrueAndRemovesType()
        {
            var existingType = Fixture.Context.ConnectionTypes.First();

            var result = await service.Remove(new ID<ConnectionTypeDto>(existingType.ID));

            Assert.IsTrue(result);
            Assert.IsFalse(Fixture.Context.ConnectionTypes.Any(t => t.ID == existingType.ID));
        }

        [TestMethod]
        public async Task Remove_NotExistingType_ReturnsFalse()
        {
            var result = await service.Remove(ID<ConnectionTypeDto>.InvalidID);

            Assert.IsFalse(result);
        }
    }
}
