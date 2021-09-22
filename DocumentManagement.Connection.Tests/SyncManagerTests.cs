using AutoMapper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Brio.Docs.Synchronizer;
using Brio.Docs.Tests.Utility;
using Brio.Docs.Utility;

namespace DocumentManagement.Connection.Tests
{
    [TestClass]
    public class SyncManagerTests
    {
        private static SharedDatabaseFixture Fixture { get; set; }

        private DiskMock disk;
        private static IMapper mapper;
        private static UserSynchro sychro;

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

                context.Users.AddRange(MockData.DEFAULT_USERS);
                context.SaveChanges();
            });

            disk = new DiskMock();
            sychro = new UserSynchro(disk, Fixture.Context);
        }
    }
}
