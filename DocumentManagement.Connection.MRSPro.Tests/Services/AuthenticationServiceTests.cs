using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MRS.DocumentManagement.Connection.MrsPro.Services;
using MRS.DocumentManagement.Interface.Dtos;

namespace MRS.DocumentManagement.Connection.MrsPro.Tests.Services
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
            services.AddMrsPro();
            services.AddLogging(x => x.SetMinimumLevel(LogLevel.None));
            serviceProvider = services.BuildServiceProvider();
            service = serviceProvider.GetService<AuthenticationService>();
        }

        [ClassCleanup]
        public static void ClassCleanup()
            => serviceProvider.Dispose();

        [TestInitialize]
        public async Task Setup()
         => await Task.Delay(5000);

        [TestMethod]
        public async Task SignInAsync_CorrectCredentials_SuccessfulSignIn()
        {
            var email = "asidorov@briogroup.ru";
            var password = "GhundU72!c";
            var companyCode = "skprofitgroup";

            var (userExternalID, authStatus) = await service.SignInAsync(email, password, companyCode);

            Assert.IsTrue(authStatus.Status == RemoteConnectionStatus.OK);
            Assert.IsNotNull(userExternalID);
        }

        [TestMethod]
        [DataRow("asidorov@briogroup.ru", "incorrectPasssword", "skprofitgroup", DisplayName = "Incorrect password")]
        [DataRow("incorrectEmail", "GhundU72!c", "skprofitgroup", DisplayName = "Incorrect email")]
        [DataRow("asidorov@briogroup.ru", "GhundU72!c", "incorrectCompany", DisplayName = "Incorrect company")]
        [DataRow(null, null, null, DisplayName = "Null values")]
        public async Task SignInAsync_IncorrectCredentials_FailedSignIn(string emailValue, string passwordValue, string companyValue)
        {
            var email = emailValue;
            var password = passwordValue;
            var companyCode = companyValue;

            var (userExternalID, authStatus) = await service.SignInAsync(email, password, companyCode);

            Assert.IsTrue(authStatus.Status != RemoteConnectionStatus.OK);
            Assert.IsNull(userExternalID);
        }
    }
}
