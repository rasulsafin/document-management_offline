using Brio.Docs.Connection.LementPro.Services;
using Brio.Docs.Interface.Dtos;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Brio.Docs.Connection.LementPro.Tests.IntegrationTests.Services
{
    [TestClass]
    public class AuthenticationServiceTests
    {
        private static AuthenticationService service;
        private static ServiceProvider serviceProvider;

        [ClassInitialize]
        public static void Init(TestContext unused)
        {
            var services = new ServiceCollection();
            services.AddLementPro();
            services.AddLogging(x => x.SetMinimumLevel(LogLevel.None));
            serviceProvider = services.BuildServiceProvider();
            service = serviceProvider.GetService<AuthenticationService>();
        }

        [ClassCleanup]
        public static void ClassCleanup()
            => serviceProvider.Dispose();

        [TestMethod]
        public async Task SignInAsync_CorrectCredentials_SuccessfulSignIn()
        {
            var login = "diismagilov";
            var password = "DYZDFMwZ";
            var connectionInfo = new ConnectionInfoExternalDto
            {
                AuthFieldValues = new Dictionary<string, string>
                {
                    { "login", login },
                    { "password", password },
                },
            };

            var (authStatus, updatedInfo) = await service.SignInAsync(connectionInfo);

            Assert.IsTrue(authStatus.Status == RemoteConnectionStatus.OK);
            Assert.IsNotNull(updatedInfo);
        }

        [TestMethod]
        public async Task SignInAsync_IncorrectCredentials_FailedSignIn()
        {
            var login = "diismagilov";
            var password = $"incorrectPasssword{Guid.NewGuid()}";
            var connectionInfo = new ConnectionInfoExternalDto
            {
                AuthFieldValues = new Dictionary<string, string>
                {
                    { "login", login },
                    { "password", password },
                },
            };

            var (authStatus, updatedInfo) = await service.SignInAsync(connectionInfo);

            Assert.IsTrue(authStatus.Status != RemoteConnectionStatus.OK);
            Assert.IsNull(updatedInfo);
        }
    }
}
