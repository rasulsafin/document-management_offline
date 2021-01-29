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

        private DiskTest disk;
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

            disk = new DiskTest();
            sychro = new UserSynchro(disk, Fixture.Context);
        }

        // [TestMethod]
        public async Task AnalysisDownloadTestAsync()
        {
            // Проверить заполнение пустой структуры

            // RevisionCollection remote = new RevisionCollection();
            // remote.GetRevision(TableRevision.Users, 1).Rev = 30;

            // RevisionCollection local = new RevisionCollection();
            // local.GetRevision(TableRevision.Users, 2).Rev = 10;

            // SyncManager.Analysis();

            // List<SyncAction> actual = await SyncHelper.Analysis(local, remote, new UserSyncro());
            // actual.Sort((x, y) => x.ID.CompareTo(y.ID));
            // List<SyncAction> expected = new List<SyncAction>()
            // {
            //    new SyncAction()
            //    {
            //        Synchronizer = nameof(),
            //        TypeAction = TypeSyncAction.Download,
            //        ID = 1,
            //    },
            //    new SyncAction()
            //    {
            //        Synchronizer = nameof(UserSyncro),
            //        TypeAction = TypeSyncAction.Upload,
            //        ID = 2,
            //    },
            // };

            // AssertHelper.EqualList<SyncAction>(expected, actual, AssertHelper.EqualSyncAction);
        }
    }

}
