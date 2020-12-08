using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using System.Threading.Tasks;
using MRS.DocumentManagement.Interface.Services;
using MRS.DocumentManagement.Interface.Dtos;
using MRS.DocumentManagement.Tests.Utility;
using MRS.DocumentManagement.Interface;

namespace MRS.DocumentManagement.Tests
{
    [TestClass]
    public class AuthorizationTests
    {
        public static SharedDatabaseFixture Fixture { get; private set; }

        [ClassInitialize]
        public static void Setup(TestContext _)
        {
            Fixture = new SharedDatabaseFixture();
        }

        [ClassCleanup]
        public static void Cleanup()
        {
            Fixture.Dispose();
        }

        [TestMethod]
        public async Task Can_login_to_current_fixture()
        {
            using var transaction = Fixture.Connection.BeginTransaction();
            using (var context = Fixture.CreateContext(transaction))
            {
                var api = new DocumentManagementApi(context);
                await api.Register(new UserToCreateDto("vpupkin", "123", "Vasily Pupkin"));

                var access = await api.Login("vpupkin", "123");
                Assert.IsNotNull(access);
                Assert.IsTrue(access.CurrentUser.ID.IsValid);
                Assert.AreEqual("vpupkin", access.CurrentUser.Login);
                Assert.AreEqual("Vasily Pupkin", access.CurrentUser.Name);
            }
        }

        [TestMethod]
        public async Task Can_add_and_remove_roles()
        {
            using var transaction = Fixture.Connection.BeginTransaction();
            using (var context = Fixture.CreateContext(transaction))
            {
                var api = new DocumentManagementApi(context);
                var access = await api.Register(new UserToCreateDto("vpupkin", "123", "Vasily Pupkin"));
                Assert.IsNotNull(access.AuthorizationService);

                var user = access.CurrentUser;
                var auth = access.AuthorizationService;
                // 0. Query roles
                var roles = await auth.GetAllRoles();
                Assert.IsNotNull(roles);
                Assert.AreEqual(0, roles.Count());

                // 1. Add new role
                var result = await auth.AddRole(user.ID, "admin");
                Assert.IsTrue(result);
                roles = await auth.GetAllRoles();
                CollectionAssert.AreEqual(new string[] { "admin" }, roles.ToArray());

                // 2. Try add same role
                result = await auth.AddRole(user.ID, "admin");
                Assert.IsFalse(result);
                roles = await auth.GetAllRoles();
                CollectionAssert.AreEqual(new string[] { "admin" }, roles.ToArray());

                // 3. Add new role
                result = await auth.AddRole(user.ID, "operator");
                Assert.IsTrue(result);
                roles = await auth.GetAllRoles();
                CollectionAssert.AreEquivalent(new string[] { "admin", "operator" }, roles.ToArray());

                // 4. Remove role
                result = await auth.RemoveRole(user.ID, "operator");
                Assert.IsTrue(result);
                roles = await auth.GetAllRoles();
                CollectionAssert.AreEquivalent(new string[] { "admin" }, roles.ToArray());

                // 5. Try remove role again
                result = await auth.RemoveRole(user.ID, "operator");
                Assert.IsFalse(result);
                roles = await auth.GetAllRoles();
                CollectionAssert.AreEquivalent(new string[] { "admin" }, roles.ToArray());

                // 6. Remove non-existent role
                result = await auth.RemoveRole(user.ID, "somerole");
                Assert.IsFalse(result);
                roles = await auth.GetAllRoles();
                CollectionAssert.AreEquivalent(new string[] { "admin" }, roles.ToArray());

                // 7. Remove last role
                result = await auth.RemoveRole(user.ID, "admin");
                Assert.IsTrue(result);
                roles = await auth.GetAllRoles();
                Assert.AreEqual(0, roles.Count());
            }
        }

        [TestMethod]
        public async Task Can_query_roles_from_several_users()
        {
            async Task AssertRoles(IAuthorizationService auth, ID<UserDto> id, params string[] roles)
            {
                var eroles = roles ?? Enumerable.Empty<string>();
                CollectionAssert.AreEquivalent(eroles.ToArray(), (await auth.GetUserRoles(id)).ToArray());
                foreach (var role in eroles)
                {
                    Assert.IsTrue(await auth.IsInRole(id, role));
                }
            }

            using var transaction = Fixture.Connection.BeginTransaction();
            using (var context = Fixture.CreateContext(transaction))
            {
                var api = new DocumentManagementApi(context);
                var access = await api.Register(new UserToCreateDto("vpupkin", "123", "Vasily Pupkin"));
                Assert.IsNotNull(access.AuthorizationService);

                var admin = access.CurrentUser;
                var auth = access.AuthorizationService;
                // 0. Add another user
                var operID = await access.UserService.Add(new UserToCreateDto("itaranov", "123", "Ivan Taranov"));
                Assert.IsTrue(operID.IsValid);

                // 1. Add admin role
                Assert.IsTrue(await auth.AddRole(admin.ID, "admin"));
                CollectionAssert.AreEquivalent(new string[] { "admin" }, (await auth.GetAllRoles()).ToArray());
                await AssertRoles(auth, admin.ID, "admin");
                await AssertRoles(auth, operID, null);

                // 2. Add operator role
                Assert.IsTrue(await auth.AddRole(operID, "operator"));
                CollectionAssert.AreEquivalent(new string[] { "admin", "operator" }, (await auth.GetAllRoles()).ToArray());
                await AssertRoles(auth, admin.ID, "admin");
                await AssertRoles(auth, operID, "operator");

                // 3. Add new role to both
                Assert.IsTrue(await auth.AddRole(admin.ID, "cook"));
                Assert.IsTrue(await auth.AddRole(operID, "cook"));
                CollectionAssert.AreEquivalent(new string[] { "admin", "operator", "cook" }, (await auth.GetAllRoles()).ToArray());
                await AssertRoles(auth, admin.ID, "admin", "cook");
                await AssertRoles(auth, operID, "operator", "cook");

                Assert.IsFalse(await auth.IsInRole(admin.ID, "operator"));
                Assert.IsFalse(await auth.IsInRole(operID, "admin"));

                // 4. Remove role from admin
                Assert.IsTrue(await auth.RemoveRole(admin.ID, "cook"));
                CollectionAssert.AreEquivalent(new string[] { "admin", "operator", "cook" }, (await auth.GetAllRoles()).ToArray());
                await AssertRoles(auth, admin.ID, "admin");
                await AssertRoles(auth, operID, "operator", "cook");

                Assert.IsFalse(await auth.IsInRole(admin.ID, "cook"));

                // 5. Remove role from operator
                Assert.IsTrue(await auth.RemoveRole(operID, "cook"));
                CollectionAssert.AreEquivalent(new string[] { "admin", "operator" }, (await auth.GetAllRoles()).ToArray());
                await AssertRoles(auth, admin.ID, "admin");
                await AssertRoles(auth, operID, "operator");

                Assert.IsFalse(await auth.IsInRole(operID, "cook"));

                // 6. Remove last role from operator
                Assert.IsTrue(await auth.RemoveRole(operID, "operator"));
                CollectionAssert.AreEquivalent(new string[] { "admin" }, (await auth.GetAllRoles()).ToArray());
                await AssertRoles(auth, admin.ID, "admin");
                await AssertRoles(auth, operID, null);
            }
        }
    }
}
