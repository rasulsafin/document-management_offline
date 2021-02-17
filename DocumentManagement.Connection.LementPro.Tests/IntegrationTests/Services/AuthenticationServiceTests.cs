using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MRS.DocumentManagement.Connection.LementPro.Services;
using MRS.DocumentManagement.Connection.LementPro.Utilities;
using MRS.DocumentManagement.Interface.Dtos;

namespace MRS.DocumentManagement.Connection.LementPro.Tests
{
    [TestClass]
    public class AuthenticationServiceTests
    {
        private static AuthenticationService service;

        [ClassInitialize]
        public static void Init(TestContext unused)
            => service = new AuthenticationService(new HttpRequestUtility(new NetConnector()));

        [ClassCleanup]
        public static void ClassCleanup()
            => service.Dispose();

        [TestMethod]
        public async Task SignInAsync_CorrectCredentials_SuccessfulSignIn()
        {
            var login = "diismagilov";
            var password = "DYZDFMwZ";
            var connectionInfo = new ConnectionInfoDto
            {
                AuthFieldValues = new Dictionary<string, string>
                {
                    { "login", login },
                    { "password", password },
                },
            };

            var (authStatus, updatedInfo) = await service.SignInAsync(connectionInfo);

            Assert.IsTrue(authStatus.Status == RemoteConnectionStatusDto.OK);
            Assert.IsNotNull(updatedInfo);
        }

        [TestMethod]
        public async Task SignInAsync_IncorrectCredentials_FailedSignIn()
        {
            var login = "diismagilov";
            var password = $"incorrectPasssword{Guid.NewGuid()}";
            var connectionInfo = new ConnectionInfoDto
            {
                AuthFieldValues = new Dictionary<string, string>
                {
                    { "login", login },
                    { "password", password },
                },
            };

            var (authStatus, updatedInfo) = await service.SignInAsync(connectionInfo);

            Assert.IsTrue(authStatus.Status != RemoteConnectionStatusDto.OK);
            Assert.IsNull(updatedInfo);
        }
    }
}
