using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MRS.DocumentManagement;
using MRS.DocumentManagement.Connection.Tdms;
using MRS.DocumentManagement.Interface.Dtos;

namespace DocumentManagement.Connection.Tdms.Tests
{
    [TestClass]
    public class InitClass
    {
        private static TdmsConnection connection;

        [AssemblyInitialize]
        public static void AssemblyInit(TestContext context)
        {
            connection = new TdmsConnection();
            var connectionInfo = new ConnectionInfoDto
            {
                ID = new ID<ConnectionInfoDto>(1),
                ConnectionType = connection.GetConnectionType(),
                AuthFieldValues = new Dictionary<string, string>()
                {
                    {Auth.LOGIN, "gureva" },
                    {Auth.PASSWORD, "123"},
                    {Auth.DATABASE, "kosmos" },
                    {Auth.SERVER, @"192.168.100.6\sqlkosmos" },
                },
            };

            // Authorize
            var signInTask = connection.Connect(connectionInfo);
            signInTask.Wait();
            if (signInTask.Result.Status != RemoteConnectionStatusDto.OK)
            {
                Assert.Fail("Authorization failed");
            }
        }

        [AssemblyCleanup]
        public static void AssemblyCleanup()
        {
            //connection.Quit();
        }
    }
}
