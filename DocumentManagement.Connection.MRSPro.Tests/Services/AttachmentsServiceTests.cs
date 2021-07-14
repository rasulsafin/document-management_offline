using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MRS.DocumentManagement.Connection.MrsPro.Services;
using MRS.DocumentManagement.Interface.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static MRS.DocumentManagement.Connection.MrsPro.Tests.TestConstants;

namespace MRS.DocumentManagement.Connection.MrsPro.Tests.Services
{
    [TestClass]
    public class AttachmentsServiceTests
    {
        private static AttachmentsService service;
        private static ServiceProvider serviceProvider;

        [ClassInitialize]
        public static void Init(TestContext unused)
        {
            var delay = Task.Delay(MILLISECONDS_TIME_DELAY);
            delay.Wait();

            var services = new ServiceCollection();
            services.AddMrsPro();
            services.AddLogging(x => x.SetMinimumLevel(LogLevel.None));
            serviceProvider = services.BuildServiceProvider();
            service = serviceProvider.GetService<AttachmentsService>();
            var authenticator = serviceProvider.GetService<AuthenticationService>();

            // Authorize
            var email = TEST_EMAIL;
            var password = TEST_PASSWORD;
            var companyCode = TEST_COMPANY;
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
        => await Task.Delay(MILLISECONDS_TIME_DELAY);

    }
}
