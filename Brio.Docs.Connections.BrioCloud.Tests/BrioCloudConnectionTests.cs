using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Brio.Docs.Common.Dtos;
using Brio.Docs.Connections.BrioCloud.Synchronization;
using Brio.Docs.Integration.Dtos;
using Microsoft.VisualStudio.TestTools.UnitTesting;

[assembly: Parallelize(Workers = 0, Scope = ExecutionScope.MethodLevel)]

namespace Brio.Docs.Connections.BrioCloud.Tests
{
    [TestClass]
    public class BrioCloudConnectionTests
    {
        private const string NAME_CONNECTION = "Brio-Cloud";
        private const string CLIENT_ID = "CLIENT_ID";
        private const string CLIENT_SECRET = "CLIENT_SECRET";

        private const string VALID_USERNAME = "avsingaevskiy";
        private const string VALID_PASSWORD = "AndreyS186";

        private static ConnectionInfoExternalDto validInfo;

        [ClassInitialize]
        public static void ClassInitialize(TestContext unused)
        {
            validInfo = new ConnectionInfoExternalDto()
            {
                ConnectionType = new ConnectionTypeExternalDto()
                {
                    Name = NAME_CONNECTION,
                    AppProperties = new Dictionary<string, string>
                    {
                        { CLIENT_ID, VALID_USERNAME },
                        { CLIENT_SECRET, VALID_PASSWORD },
                    },
                },
            };
        }

        [TestMethod]
        public async Task Connect_ValidCredentials_OK()
        {
            BrioCloudConnection connection = new BrioCloudConnection();

            var expectedResult = RemoteConnectionStatus.OK;
            var result = await connection.Connect(validInfo, default);

            Assert.AreEqual(expectedResult, result.Status);
        }

        [TestMethod]
        [DataRow("avsingaevskiy", "AndreyS187")]
        [DataRow("avsingaevskij", "AndreyS186")]
        public async Task Connect_InvalidCredentials_Error(string username, string password)
        {
            ConnectionInfoExternalDto info = new ConnectionInfoExternalDto()
            {
                ConnectionType = new ConnectionTypeExternalDto()
                {
                    Name = NAME_CONNECTION,
                    AppProperties = new Dictionary<string, string>
                    {
                        { CLIENT_ID, username },
                        { CLIENT_SECRET, password },
                    },
                },
            };

            BrioCloudConnection connection = new BrioCloudConnection();
            var expectedResult = RemoteConnectionStatus.Error;

            var result = await connection.Connect(info, default);

            Assert.AreEqual(expectedResult, result.Status);
        }

        [TestMethod]
        public async Task GetStatus_ManagerInitiated_OK()
        {
            BrioCloudConnection connection = new BrioCloudConnection();
            await connection.Connect(validInfo, default);

            var expectedResult = RemoteConnectionStatus.OK;
            var result = await connection.GetStatus(validInfo);

            Assert.AreEqual(expectedResult, result.Status);
        }

        [TestMethod]
        public async Task GetStatus_ManagerNotInitiated_NeedReconnect()
        {
            BrioCloudConnection connection = new BrioCloudConnection();

            var expectedResult = RemoteConnectionStatus.NeedReconnect;
            var result = await connection.GetStatus(validInfo);

            Assert.AreEqual(expectedResult, result.Status);
        }

        [TestMethod]
        public async Task UpdateConnectionInfo_WrongObjectiveTypeWithoutGuid_NewObjectiveTypeNewGuid()
        {
            string objectiveType = "WrongObjectiveType";
            BrioCloudConnection connection = new BrioCloudConnection();
            var info = new ConnectionInfoExternalDto()
            {
                ConnectionType = new ConnectionTypeExternalDto()
                {
                    Name = NAME_CONNECTION,
                    AppProperties = new Dictionary<string, string>
                    {
                        { CLIENT_ID, VALID_USERNAME },
                        { CLIENT_SECRET, VALID_PASSWORD },
                    },
                    ObjectiveTypes = new List<ObjectiveTypeExternalDto>
                    {
                        new ObjectiveTypeExternalDto { Name = objectiveType, ExternalId = objectiveType },
                    },
                },
            };
            await connection.Connect(info, default);

            var expectedResult = "BrioCloudIssue";
            var connectionInfo = await connection.UpdateConnectionInfo(info);

            Assert.IsFalse(string.IsNullOrWhiteSpace(connectionInfo.UserExternalID));

            var result = connectionInfo.ConnectionType.ObjectiveTypes.FirstOrDefault();

            Assert.AreEqual(expectedResult, result.Name);
            Assert.AreEqual(expectedResult, result.ExternalId);
        }

        [TestMethod]
        public async Task UpdateConnectionInfo_WrongObjectiveTypeWithGuid_NewObjectiveTypeOldGuid()
        {
            string objectiveType = "WrongObjectiveType";
            var userExternalId = Guid.NewGuid().ToString();
            BrioCloudConnection connection = new BrioCloudConnection();
            var info = new ConnectionInfoExternalDto()
            {
                ConnectionType = new ConnectionTypeExternalDto()
                {
                    Name = NAME_CONNECTION,
                    AppProperties = new Dictionary<string, string>
                    {
                        { CLIENT_ID, VALID_USERNAME },
                        { CLIENT_SECRET, VALID_PASSWORD },
                    },
                    ObjectiveTypes = new List<ObjectiveTypeExternalDto>
                    {
                        new ObjectiveTypeExternalDto { Name = objectiveType, ExternalId = objectiveType },
                    },
                },
                UserExternalID = userExternalId,
            };
            await connection.Connect(info, default);

            var expectedResult = "BrioCloudIssue";
            var connectionInfo = await connection.UpdateConnectionInfo(info);

            Assert.AreEqual(userExternalId, connectionInfo.UserExternalID);

            var result = connectionInfo.ConnectionType.ObjectiveTypes.FirstOrDefault();

            Assert.AreEqual(expectedResult, result.Name);
            Assert.AreEqual(expectedResult, result.ExternalId);
        }

        [TestMethod]
        public async Task UpdateConnectionInfo_RightObjectiveTypeWithGuid_OldObjectiveTypeOldGuid()
        {
            string objectiveType = "BrioCloudIssue";
            var userExternalId = Guid.NewGuid().ToString();
            BrioCloudConnection connection = new BrioCloudConnection();
            var info = new ConnectionInfoExternalDto()
            {
                ConnectionType = new ConnectionTypeExternalDto()
                {
                    Name = NAME_CONNECTION,
                    AppProperties = new Dictionary<string, string>
                    {
                        { CLIENT_ID, VALID_USERNAME },
                        { CLIENT_SECRET, VALID_PASSWORD },
                    },
                    ObjectiveTypes = new List<ObjectiveTypeExternalDto>
                    {
                        new ObjectiveTypeExternalDto { Name = objectiveType, ExternalId = objectiveType },
                    },
                },
                UserExternalID = userExternalId,
            };
            await connection.Connect(info, default);

            var connectionInfo = await connection.UpdateConnectionInfo(info);

            Assert.AreEqual(userExternalId, connectionInfo.UserExternalID);

            var result = connectionInfo.ConnectionType.ObjectiveTypes.FirstOrDefault();

            Assert.AreEqual(objectiveType, result.Name);
            Assert.AreEqual(objectiveType, result.ExternalId);
        }

        [TestMethod]
        public async Task UpdateConnectionInfo_RightObjectiveTypeWithoutGuid_OldObjectiveTypeNewGuid()
        {
            string objectiveType = "BrioCloudIssue";
            BrioCloudConnection connection = new BrioCloudConnection();
            var info = new ConnectionInfoExternalDto()
            {
                ConnectionType = new ConnectionTypeExternalDto()
                {
                    Name = NAME_CONNECTION,
                    AppProperties = new Dictionary<string, string>
                    {
                        { CLIENT_ID, VALID_USERNAME },
                        { CLIENT_SECRET, VALID_PASSWORD },
                    },
                    ObjectiveTypes = new List<ObjectiveTypeExternalDto>
                    {
                        new ObjectiveTypeExternalDto { Name = objectiveType, ExternalId = objectiveType },
                    },
                },
            };
            await connection.Connect(info, default);

            var connectionInfo = await connection.UpdateConnectionInfo(info);

            Assert.IsFalse(string.IsNullOrWhiteSpace(connectionInfo.UserExternalID));

            var result = connectionInfo.ConnectionType.ObjectiveTypes.FirstOrDefault();

            Assert.AreEqual(objectiveType, result.Name);
            Assert.AreEqual(objectiveType, result.ExternalId);
        }
    }
}
