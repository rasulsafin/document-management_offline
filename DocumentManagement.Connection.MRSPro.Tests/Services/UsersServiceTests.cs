﻿using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MRS.DocumentManagement.Connection.MrsPro.Services;
using MRS.DocumentManagement.Interface.Dtos;

namespace MRS.DocumentManagement.Connection.MrsPro.Tests.Services
{
    [TestClass]
    public class UsersServiceTests
    {
        private static UsersService service;
        private static ServiceProvider serviceProvider;

        [ClassInitialize]
        public static void Init(TestContext unused)
        {
            var delay = Task.Delay(TestConstants.MILLISECONDS_TIME_DELAY);
            delay.Wait();

            var services = new ServiceCollection();
            services.AddMrsPro();
            services.AddLogging(x => x.SetMinimumLevel(LogLevel.None));
            serviceProvider = services.BuildServiceProvider();
            service = serviceProvider.GetService<UsersService>();
            var authenticator = serviceProvider.GetService<AuthenticationService>();

            // Authorize
            var email = "asidorov@briogroup.ru";
            var password = "GhundU72!c";
            var companyCode = "skprofitgroup";
            var signInTask = authenticator.SignInAsync(email, password, companyCode);
            signInTask.Wait();
            var result = signInTask.Result;
            if (result.Status != RemoteConnectionStatus.OK)
                Assert.Fail("Authorization failed");
        }

        [ClassCleanup]
        public static void ClassCleanup()
            => serviceProvider.Dispose();

        [TestInitialize]
        public async Task Setup()
        => await Task.Delay(TestConstants.MILLISECONDS_TIME_DELAY);

        [TestMethod]
        public async Task TryGetCurrentUser_ReturnCurrentUserId()
        {
            var result = await service.GetMe();

            Assert.IsNotNull(result);
        }
    }
}
