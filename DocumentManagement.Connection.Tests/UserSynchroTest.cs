using AutoMapper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MRS.DocumentManagement.Connection;
using MRS.DocumentManagement.Connection.Synchronizator;
using MRS.DocumentManagement.Utility;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DocumentManagement.Connection.Tests
{
    [TestClass]
    public class UserSynchroTest
    {
        private static SharedDatabaseFixture Fixture { get; set; }
        public RevisionCollection Revisions { get; private set; }

        private static IMapper mapper;
        private static UserSychro sychro;

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

            Revisions = new RevisionCollection();

            var disk = new DiskTest();
            sychro = new UserSychro(disk, Fixture.Context);
        }

        [TestMethod]
        public void GetRevisionsTest()
        {
            Revisions.GetUser(1).Rev = 5;
            Revisions.GetUser(2).Rev = 5;
            Revisions.GetUser(3).Delete();

            var actual = sychro.GetRevisions(Revisions);

            var delRev = new Revision(3);
            delRev.Delete();
            var expected = new List<Revision>()
            {
                new Revision(1,5),
                new Revision(2,5),
                delRev,
            };

            AssertHelper.EqualList(expected, actual, AssertHelper.EqualRevision);
        }

        [TestMethod]
        public void SetRevisionTest()
        {
            Revisions.GetUser(1).Rev = 5;
            Revisions.GetUser(2).Rev = 5;
            Revisions.GetUser(3).Delete();

            Revision expected = new Revision(2, 25);
            sychro.SetRevision(Revisions, expected);

            var actual = Revisions.GetUser(2);
            AssertHelper.EqualRevision(expected, actual);
        }

        [TestMethod]
        public void SpecialSynchronizationTest()
        {
            SyncAction actual = new SyncAction();
            SyncAction expected = new SyncAction();
            actual.ID = expected.ID = 1;
            actual.Synchronizer = expected.Synchronizer = nameof(UserSychro);
            actual.TypeAction = expected.TypeAction =  TypeSyncAction.None;

            actual = sychro.SpecialSynchronization(actual);
            expected.SpecialSynchronization = false;

            AssertHelper.EqualSyncAction(expected, actual);
        }

        [TestMethod]
        public async Task GetSubSynchroListTest()
        {
            var sub = await sychro.GetSubSynchroList(1);
            Assert.IsNull(sub);
        }

        [TestMethod]
        public async Task SpecialTest()
        {
            SyncAction action = new SyncAction();
            await Assert.ThrowsExceptionAsync<NotImplementedException>(() =>
            {
                return sychro.Special(action);
            });

        }

        [TestMethod]
        public async Task SpecialTest()
        {
            SyncAction action = new SyncAction();
            await Assert.ThrowsExceptionAsync<NotImplementedException>(() =>
            {
                return sychro.DeleteLocal(action);
            });
        }

        internal class DiskTest : IDiskManager
        {
            public DiskTest()
            {
            }

            public Task<T> Pull<T>(string id)
            {
                throw new System.NotImplementedException();
            }

            public Task<bool> Push<T>(T @object, string id)
            {
                throw new System.NotImplementedException();
            }
        }
    }
}
