using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading.Tasks;
using DocumentManagement.Tests.Utility;

namespace DocumentManagement.Tests
{
    [TestClass]
    public class DocumentManagementAPITests
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
        public async Task Can_register_new_user()
        {
            using var transaction = Fixture.Connection.BeginTransaction();
            using (var context = Fixture.CreateContext(transaction))
            {
                var api = new DocumentManagementApi(context);
                var access = await api.Register(new Interface.Models.NewUser("vpupkin", "abracadabra", "Vasily Pupkin"));
                Assert.IsNotNull(access);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task Can_not_register_empty_login()
        {
            using var transaction = Fixture.Connection.BeginTransaction();
            using (var context = Fixture.CreateContext(transaction))
            {
                var api = new DocumentManagementApi(context);
                var access = await api.Register(new Interface.Models.NewUser(null, "123", "Name"));
                Assert.Fail();
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task Can_not_register_empty_password()
        {
            using var transaction = Fixture.Connection.BeginTransaction();
            using (var context = Fixture.CreateContext(transaction))
            {
                var api = new DocumentManagementApi(context);
                var access = await api.Register(new Interface.Models.NewUser("login", null, "Name"));
                Assert.Fail();
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task Can_not_register_empty_name()
        {
            using var transaction = Fixture.Connection.BeginTransaction();
            using (var context = Fixture.CreateContext(transaction))
            {
                var api = new DocumentManagementApi(context);
                var access = await api.Register(new Interface.Models.NewUser("login", "123", null));
                Assert.Fail();
            }
        }

        [TestMethod]
        public async Task Can_login_with_valid_creds()
        {
            using var transaction = Fixture.Connection.BeginTransaction();
            using (var context = Fixture.CreateContext(transaction))
            {
                var api = new DocumentManagementApi(context);
                await api.Register(new Interface.Models.NewUser("vpupkin", "123456", "Vasily Pupkin"));

                var accessLog = await api.Login("vpupkin", "123456");
                Assert.IsNotNull(accessLog);
            }
        }

        [TestMethod]
        public async Task Can_not_login_with_invalid_password()
        {
            using var transaction = Fixture.Connection.BeginTransaction();
            using (var context = Fixture.CreateContext(transaction))
            {
                var api = new DocumentManagementApi(context);
                await api.Register(new Interface.Models.NewUser("vpupkin", "123456", "Vasily Pupkin"));

                var accessLog = await api.Login("vpupkin", "abracadabra");
                Assert.IsNull(accessLog);
            }
        }

        [TestMethod]
        public async Task Can_not_login_with_invalid_login()
        {
            using var transaction = Fixture.Connection.BeginTransaction();
            using (var context = Fixture.CreateContext(transaction))
            {
                var api = new DocumentManagementApi(context);
                await api.Register(new Interface.Models.NewUser("vpupkin", "123456", "Vasily Pupkin"));

                var accessLog = await api.Login("itaranov", "123456");
                Assert.IsNull(accessLog);
            }
        }

        [TestMethod]
        public async Task Can_not_login_with_empty_password()
        {
            using var transaction = Fixture.Connection.BeginTransaction();
            using (var context = Fixture.CreateContext(transaction))
            {
                var api = new DocumentManagementApi(context);
                await api.Register(new Interface.Models.NewUser("vpupkin", "123456", "Vasily Pupkin"));

                var access1 = await api.Login("vpupkin", string.Empty);
                Assert.IsNull(access1);

                var access2 = await api.Login("vpupkin", null);
                Assert.IsNull(access2);
            }
        }
    }
}
