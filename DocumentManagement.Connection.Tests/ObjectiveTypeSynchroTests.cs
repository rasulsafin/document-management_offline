using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MRS.DocumentManagement;
using MRS.DocumentManagement.Connection;
using MRS.DocumentManagement.Connection.Synchronizator;
using MRS.DocumentManagement.Database.Models;
using MRS.DocumentManagement.Interface.Dtos;
using MRS.DocumentManagement.Interface.Services;
using MRS.DocumentManagement.Tests.Utility;
using MRS.DocumentManagement.Utility;

namespace DocumentManagement.Connection.Tests
{
    [TestClass]
    public class ObjectiveTypeSynchroTests : IUserSynchroTests
    {
        #region Initilise / Cleanup
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

                context.ObjectiveTypes.AddRange(MockData.DEFAULT_OBJECTIVE_TYPES);
                context.SaveChanges();
            });

            Revisions = new RevisionCollection();

            disk = new DiskTest();
            sychro = new UserSynchro(disk, Fixture.Context);
        }

        [TestCleanup]
        public void Cleanup() => Fixture.Dispose(); 
        #endregion

        [TestMethod]
        public Task DeleteLocalTest()
        {
            throw new NotImplementedException();
        }

        [TestMethod]
        public Task DeleteRemoteTest()
        {
            throw new NotImplementedException();
        }

        [TestMethod]
        public Task DownloadTestExist()
        {
            throw new NotImplementedException();
        }

        [TestMethod]
        public Task DownloadTestNotExist()
        {
            throw new NotImplementedException();
        }

        [TestMethod]
        public void GetRevisionsTest()
        {
            throw new NotImplementedException();
        }

        public Task GetSubSynchroListTest()
        {
            throw new NotImplementedException();
        }

        [TestMethod]
        public void SetRevisionTest()
        {
            throw new NotImplementedException();
        }

        [TestMethod]
        public void SpecialSynchronizationTest()
        {
            throw new NotImplementedException();
        }

        [TestMethod]
        public Task SpecialTest()
        {
            throw new NotImplementedException();
        }

        [TestMethod]
        public async Task UploadTest()
        {
            int id = 1;
            var expected = mapper.Map<ObjectiveTypeDto>(Fixture.Context.ObjectiveTypes.Find(id));
            //var expected = mapper.Map<ObjectiveTypeDto>(MockData.DEFAULT_OBJECTIVE_TYPES[0]);


            SyncAction action = new SyncAction();
            action.ID = id;

            await sychro.Upload(action);
            var actual = disk.ObjectiveType;

            Assert.IsFalse(disk.RunDelete);
            Assert.IsFalse(disk.RunPull);
            Assert.IsTrue(disk.RunPush);
            Assert.IsTrue(action.IsComplete);

        }
    }
}
