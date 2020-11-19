using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DocumentManagement.Interface.Models;
using DocumentManagement.Interface;
using DocumentManagement.Tests.Utility;

namespace DocumentManagement.Tests
{
    [TestClass]
    public class UserServiceTests
    {
        public static SharedDatabaseFixture Fixture { get; private set; }

        [ClassInitialize]
        public static async Task Setup(TestContext _)
        {
            Fixture = new SharedDatabaseFixture();
        }

        [ClassCleanup]
        public static void Cleanup()
        {
            Fixture.Dispose();
        }

        [TestMethod]
        public async Task Check_get_current_user()
        {
            using var transaction = Fixture.Connection.BeginTransaction();
            using (var context = Fixture.CreateContext(transaction))
            {
                var api = new DocumentManagementApi(context);
                var access = await api.Register(new NewUser("vpupkin", "123", "Vasily Pupkin"));

                var comparer = new UserComparer(ignoreIDs: true);
                var current = new User((ID<User>)0, "vpupkin", "Vasily Pupkin");
                Assert.IsTrue(comparer.Equals(access.CurrentUser, current));
            }
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidDataException))]
        public async Task Can_not_add_user_with_duplicate_login()
        {
            using var transaction = Fixture.Connection.BeginTransaction();
            using (var context = Fixture.CreateContext(transaction))
            {
                var api = new DocumentManagementApi(context);
                var access = await api.Register(new NewUser("vpupkin", "123", "Vasily Pupkin"));

                await access.UserService.Add(new NewUser("vpupkin", "312", "Vladimir Pupkin"));
                Assert.Fail();
            }
        }

        [TestMethod]
        public async Task Are_user_specific_roles_deleted_when_user_is_deleted()
        {
            using var transaction = Fixture.Connection.BeginTransaction();
            using (var context = Fixture.CreateContext(transaction))
            {
                var api = new DocumentManagementApi(context);
                var access = await api.Register(new NewUser("vpupkin", "123", "Vasily Pupkin"));
                Assert.IsNotNull(access.AuthorizationService);

                var userService = access.UserService;
                var auth = access.AuthorizationService;
                // 0. Assign initial roles
                await auth.AddRole(access.CurrentUser.ID, "admin");
                await auth.AddRole(access.CurrentUser.ID, "cook");

                var userID = await userService.Add(new NewUser("itaranov", "123", "Ivan Taranov"));
                Assert.IsTrue(await auth.AddRole(userID, "operator"));
                Assert.IsTrue(await auth.AddRole(userID, "cook"));
                CollectionAssert.AreEquivalent(new string[] { "admin", "operator", "cook" }, (await auth.GetAllRoles()).ToArray());

                // 1. Remove user - operator role should be deleted
                Assert.IsTrue(await userService.Delete(userID));
                CollectionAssert.AreEquivalent(new string[] { "admin", "cook" }, (await auth.GetAllRoles()).ToArray());
            }
        }

        [TestMethod]
        public async Task Can_add_and_remove_new_users()
        {
            using var transaction = Fixture.Connection.BeginTransaction();
            using (var context = Fixture.CreateContext(transaction))
            {
                var api = new DocumentManagementApi(context);
                var access = await api.Register(new NewUser("vpupkin", "123", "Vasily Pupkin"));
                Assert.IsNotNull(access.UserService);

                var userService = access.UserService;

                var userCreds = new List<NewUser>
                {
                    new NewUser("vpupkin", "123", "Vasily Pupkin"),
                    new NewUser("itaranov", "321", "Ivan Taranov"),
                    new NewUser("ppshezdetsky", "333", "Pshek Pshezdetsky")
                };

                // Add new users
                var userID1 = await userService.Add(userCreds[1]);
                var userID2 = await userService.Add(userCreds[2]);

                Assert.IsTrue(userID1.IsValid);
                Assert.IsTrue(userID2.IsValid);

                var users = await userService.GetAllUsers();
                Assert.AreEqual(3, users.Count());
                foreach (var user in users)
                {
                    var creds = userCreds.Single(x => x.Login == user.Login);
                    Assert.AreEqual(creds.Name, user.Name);
                }

                // Remove first user
                Assert.IsTrue(await userService.Delete(userID1));
                Assert.AreEqual(2, (await userService.GetAllUsers()).Count());

                // Try remove already removed user
                Assert.IsFalse(await userService.Delete(userID1));
                Assert.AreEqual(2, (await userService.GetAllUsers()).Count());

                // Remove second user
                Assert.IsTrue(await userService.Delete(userID2));
                Assert.AreEqual(1, (await userService.GetAllUsers()).Count());
            }
        }

        [TestMethod]
        public async Task Can_user_delete_himself()
        {
            using var transaction = Fixture.Connection.BeginTransaction();
            using (var context = Fixture.CreateContext(transaction))
            {
                var api = new DocumentManagementApi(context);
                var access = await api.Register(new NewUser("vpupkin", "123", "Vasily Pupkin"));

                var result = await access.UserService.Delete(access.CurrentUser.ID);
                Assert.IsTrue(result);

                var comparer = new UserComparer(ignoreIDs: false);
                Assert.IsTrue(comparer.Equals(access.CurrentUser, User.Anonymous));

                var roles = await access.AuthorizationService.GetUserRoles(access.CurrentUser.ID);
                Assert.IsTrue(roles.Count() == 0);
            }
        }

        [TestMethod]
        public async Task Can_update_self_user_data()
        {
            using var transaction = Fixture.Connection.BeginTransaction();
            using (var context = Fixture.CreateContext(transaction))
            {
                var api = new DocumentManagementApi(context);
                var access = await api.Register(new NewUser("vpupkin", "123", "Vasily Pupkin"));

                await access.UserService.Update(new User(access.CurrentUser.ID, "vpopkin", "Vasily Popkin"));

                Assert.AreEqual("vpopkin", access.CurrentUser.Login);
                Assert.AreEqual("Vasily Popkin", access.CurrentUser.Name);
            }
        }

        [TestMethod]
        public async Task Can_update_another_user_data()
        {
            using var transaction = Fixture.Connection.BeginTransaction();
            using (var context = Fixture.CreateContext(transaction))
            {
                var api = new DocumentManagementApi(context);
                var access = await api.Register(new NewUser("vpupkin", "123", "Vasily Pupkin"));

                var userID = await access.UserService.Add(new NewUser("itaranov", "123", "Ivan Taranov"));

                Assert.IsTrue(await access.UserService.Exists("itaranov"));
                Assert.IsFalse(await access.UserService.Exists("bwillis"));

                await access.UserService.Update(new User(userID, "bwillis", "Bruce Willis"));

                Assert.IsFalse(await access.UserService.Exists("itaranov"));
                Assert.IsTrue(await access.UserService.Exists("bwillis"));
            }
        }

        [TestMethod]
        public async Task Can_verify_password()
        {
            using var transaction = Fixture.Connection.BeginTransaction();
            using (var context = Fixture.CreateContext(transaction))
            {
                var api = new DocumentManagementApi(context);
                var access = await api.Register(new NewUser("vpupkin", "123", "Vasily Pupkin"));

                Assert.IsTrue(await access.UserService.VerifyPassword(access.CurrentUser.ID, "123"));
                Assert.IsFalse(await access.UserService.VerifyPassword(access.CurrentUser.ID, "321"));
            }
        }

        [TestMethod]
        public async Task Can_update_password()
        {
            using var transaction = Fixture.Connection.BeginTransaction();
            using (var context = Fixture.CreateContext(transaction))
            {
                var api = new DocumentManagementApi(context);
                var access = await api.Register(new NewUser("vpupkin", "123", "Vasily Pupkin"));

                await access.UserService.UpdatePassword(access.CurrentUser.ID, "321");
                Assert.IsTrue(await access.UserService.VerifyPassword(access.CurrentUser.ID, "321"));
                Assert.IsFalse(await access.UserService.VerifyPassword(access.CurrentUser.ID, "123"));
            }
        }

        [TestMethod]
        public async Task Can_find_by_id()
        {
            using var transaction = Fixture.Connection.BeginTransaction();
            using (var context = Fixture.CreateContext(transaction))
            {
                var api = new DocumentManagementApi(context);
                var access = await api.Register(new NewUser("vpupkin", "123", "Vasily Pupkin"));

                var userID = await access.UserService.Add(new NewUser("itaranov", "123", "Ivan Taranov"));

                var user = await access.UserService.Find(userID);
                Assert.AreEqual("itaranov", user.Login);
                Assert.AreEqual("Ivan Taranov", user.Name);
            }
        }

        [TestMethod]
        public async Task Can_find_by_login()
        {
            using var transaction = Fixture.Connection.BeginTransaction();
            using (var context = Fixture.CreateContext(transaction))
            {
                var api = new DocumentManagementApi(context);
                var access = await api.Register(new NewUser("vpupkin", "123", "Vasily Pupkin"));

                var userID = await access.UserService.Add(new NewUser("itaranov", "123", "Ivan Taranov"));

                var user = await access.UserService.Find("itaranov");
                Assert.AreEqual("itaranov", user.Login);
                Assert.AreEqual("Ivan Taranov", user.Name);
            }
        }

        [TestMethod]
        public async Task Can_check_existing_by_id()
        {
            using var transaction = Fixture.Connection.BeginTransaction();
            using (var context = Fixture.CreateContext(transaction))
            {
                var api = new DocumentManagementApi(context);
                var access = await api.Register(new NewUser("vpupkin", "123", "Vasily Pupkin"));

                Assert.IsTrue(await access.UserService.Exists(access.CurrentUser.ID));

                var userID = await access.UserService.Add(new NewUser("itaranov", "123", "Ivan Taranov"));

                Assert.IsTrue(await access.UserService.Exists(access.CurrentUser.ID));
                Assert.IsTrue(await access.UserService.Exists(userID));

                await access.UserService.Delete(userID);

                Assert.IsTrue(await access.UserService.Exists(access.CurrentUser.ID));
                Assert.IsFalse(await access.UserService.Exists(userID));
            }
        }

        [TestMethod]
        public async Task Can_check_existing_by_login()
        {
            using var transaction = Fixture.Connection.BeginTransaction();
            using (var context = Fixture.CreateContext(transaction))
            {
                var api = new DocumentManagementApi(context);
                var access = await api.Register(new NewUser("vpupkin", "123", "Vasily Pupkin"));

                Assert.IsTrue(await access.UserService.Exists("vpupkin"));
                Assert.IsFalse(await access.UserService.Exists("itaranov"));

                await access.UserService.Add(new NewUser("itaranov", "123", "Ivan Taranov"));

                Assert.IsTrue(await access.UserService.Exists("vpupkin"));
                Assert.IsTrue(await access.UserService.Exists("itaranov"));
            }
        }
    }
}
