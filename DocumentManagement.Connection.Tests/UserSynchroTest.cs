using AutoMapper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MRS.DocumentManagement.Connection;
using MRS.DocumentManagement.Connection.Synchronizator;
using MRS.DocumentManagement.Interface.Dtos;
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

        private DiskTest disk;
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

            disk = new DiskTest();
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
            Assert.IsFalse(disk.RunDelete);
            Assert.IsFalse(disk.RunPull);
            Assert.IsFalse(disk.RunPush);
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
            Assert.IsFalse(disk.RunDelete);
            Assert.IsFalse(disk.RunPull);
            Assert.IsFalse(disk.RunPush);
        }

        [TestMethod]
        public void SpecialSynchronizationTest()
        {
            SyncAction actual = new SyncAction();
            SyncAction expected = new SyncAction();
            actual.ID = expected.ID = 1;
            actual.Synchronizer = expected.Synchronizer = nameof(UserSychro);
            actual.TypeAction = expected.TypeAction = TypeSyncAction.None;

            actual = sychro.SpecialSynchronization(actual);
            expected.SpecialSynchronization = false;

            AssertHelper.EqualSyncAction(expected, actual);
            Assert.IsFalse(disk.RunDelete);
            Assert.IsFalse(disk.RunPull);
            Assert.IsFalse(disk.RunPush);
        }

        [TestMethod]
        public async Task GetSubSynchroListTest()
        {
            SyncAction action = new SyncAction();
            action.ID = 1;
            var sub = await sychro.GetSubSynchroList(action);
            Assert.IsNull(sub);

            Assert.IsFalse(disk.RunDelete);
            Assert.IsFalse(disk.RunPull);
            Assert.IsFalse(disk.RunPush);
        }

        [TestMethod]
        public async Task SpecialTest()
        {
            SyncAction action = new SyncAction();
            await Assert.ThrowsExceptionAsync<NotImplementedException>(() =>
            {
                return sychro.Special(action);
            });
            Assert.IsFalse(disk.RunDelete);
            Assert.IsFalse(disk.RunPull);
            Assert.IsFalse(disk.RunPush);
        }

        [TestMethod]
        public async Task DeleteLocalTest()
        {
            int id = 1;
            SyncAction action = new SyncAction();
            action.ID = id;
            await sychro.DeleteLocal(action);

            var user = Fixture.Context.Users.Find(id);
            Assert.IsNull(user);

            Assert.IsFalse(disk.RunDelete);
            Assert.IsFalse(disk.RunPull);
            Assert.IsFalse(disk.RunPush);
        }

        [TestMethod]
        public async Task DeleteRemoteTest()
        {
            int id = 1;
            SyncAction action = new SyncAction();
            action.ID = id;
            await sychro.DeleteRemote(action);

            Assert.IsTrue(disk.RunDelete);
            Assert.IsFalse(disk.RunPull);
            Assert.IsFalse(disk.RunPush);
            Assert.AreEqual(id, disk.LastId);
        }

        [TestMethod]
        public async Task UploadTest()
        {
            int id = 1;
            SyncAction action = new SyncAction();
            action.ID = id;
            await sychro.Upload(action);

            Assert.IsFalse(disk.RunDelete);
            Assert.IsFalse(disk.RunPull);
            Assert.IsTrue(disk.RunPush);
            Assert.AreEqual(id, disk.LastId);

            var user = Fixture.Context.Users.Find(id);

            UserDto expected = mapper.Map<UserDto>(user);
            var actual = disk.User;

            AssertHelper.EqualDto(expected, actual);
        }

        internal class DiskTest : IDiskManager
        {
            public DiskTest()
            {
            }

            public UserDto User { get; set; }

            public bool RunDelete { get; private set; }

            public bool RunPull { get; private set; }

            public bool RunPush { get; private set; }

            public int LastId { get; private set; }

            public Task Delete<T>(string id)
            {
                RunDelete = true;
                if (int.TryParse(id, out int num))
                {
                    if (User is T usr)
                        LastId = num;
                }

                return Task.CompletedTask;
            }

            public Task<T> Pull<T>(string id)
            {
                RunPull = true;
                if (int.TryParse(id, out int num))
                {
                    LastId = num;
                    if (User is T usr)
                        return Task.FromResult(usr);
                }

                return Task.FromResult<T>(default);
            }

            public Task<bool> Push<T>(T @object, string id)
            {
                RunPush = true;
                if (int.TryParse(id, out int num))
                {
                    LastId = num;
                    if (@object is UserDto usr)
                    {
                        User = usr;
                        return Task.FromResult(true);
                    }
                }

                return Task.FromResult(false);
            }
        }
    }
}
