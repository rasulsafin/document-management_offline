using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MRS.DocumentManagement.Connection.Synchronizator;
using MRS.DocumentManagement.Interface.Services;
using MRS.DocumentManagement.Tests.Utility;
using MRS.DocumentManagement.Utility;

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
