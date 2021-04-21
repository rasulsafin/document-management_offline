using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MRS.DocumentManagement.Interface;
using MRS.DocumentManagement.Interface.Dtos;

namespace MRS.DocumentManagement.Connection.Tdms.Tests
{
    [TestClass]
    public class InitClass
    {
        private static TdmsConnection connection;
        public static IConnectionContext connectionContext;

        [AssemblyInitialize]
        public static void AssemblyInit(TestContext context)
        {
            connection = new TdmsConnection();
            var connectionMeta = new TdmsConnectionMeta();
            var connectionInfo = new ConnectionInfoExternalDto
            {
                ConnectionType = connectionMeta.GetConnectionTypeInfo(),
                AuthFieldValues = new Dictionary<string, string>
                {
                    { Auth.LOGIN, "gureva" },
                    { Auth.PASSWORD, "123" },
                    { Auth.DATABASE, "kosmos" },
                    { Auth.SERVER, @"192.168.100.6\sqlkosmos" },
                },
            };

            // Authorize
            var signInTask = connection.Connect(connectionInfo);
            signInTask.Wait();
            if (signInTask.Result.Status != RemoteConnectionStatus.OK)
            {
                Assert.Fail("Authorization failed");
            }

            connectionContext = connection.GetContext(connectionInfo).Result;
            Assert.IsNotNull(connectionContext);
        }

        [AssemblyCleanup]
        public static void AssemblyCleanup()
        {
            //connection.Quit();
        }
    }
}
