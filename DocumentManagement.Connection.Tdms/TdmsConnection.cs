using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MRS.DocumentManagement.Connection.Tdms.Helpers;
using MRS.DocumentManagement.Interface.Dtos;
using TDMS;

namespace MRS.DocumentManagement.Connection.Tdms
{
    public class TdmsConnection : IConnection
    {
        private static TDMSApplication tdms;

        public Task<ConnectionStatusDto> Connect(ConnectionInfoDto info)
        {
            tdms = new TDMSApplication();
            var login = info.AuthFieldValues[Auth.LOGIN];
            var password = info.AuthFieldValues[Auth.PASSWORD];
            var server = info.AuthFieldValues[Auth.SERVER];
            var db = info.AuthFieldValues[Auth.DATABASE];

            if (tdms.IsLoggedIn)
                tdms.Quit();

            try
            {
                tdms.Login(login, password, db, server);
            }
            catch
            {
                return Task.FromResult(new ConnectionStatusDto()
                {
                    Status = RemoteConnectionStatusDto.Error,
                    Message = "Error",
                });
            }

            return GetStatus(info);
        }

        public Task<ConnectionInfoDto> UpdateConnectionInfo(ConnectionInfoDto info)
        {
            info.EnumerationTypes = GetEnumerationTypes();

            return Task.FromResult(info);
        }

        public Task<ConnectionStatusDto> GetStatus(ConnectionInfoDto info)
        {
            if (tdms == null)
            {
                return Task.FromResult(new ConnectionStatusDto()
                {
                    Status = RemoteConnectionStatusDto.NeedReconnect,
                    Message = "NeedReconnect",
                });
            }

            if (tdms.IsLoggedIn == true)
            {
                return Task.FromResult(new ConnectionStatusDto()
                {
                    Status = RemoteConnectionStatusDto.OK,
                    Message = "Ok",
                });
            }

            return Task.FromResult(new ConnectionStatusDto()
            {
                Status = RemoteConnectionStatusDto.Error,
                Message = "Error",
            });
        }

        public Task<bool> IsAuthDataCorrect(ConnectionInfoDto info)
        {
            return Task.FromResult(true);
        }

        public ConnectionTypeDto GetConnectionType()
        {
            var type = new ConnectionTypeDto
            {
                Name = "tdms",
                AuthFieldNames = new List<string>() { Auth.LOGIN, Auth.PASSWORD, Auth.SERVER, Auth.DATABASE },
                AppProperties = new Dictionary<string, string>(),
                ObjectiveTypes = GetObjectiveTypes(),
                EnumerationTypes = GetEnumerationTypes(),
            };

            return type;
        }

        private ICollection<ObjectiveTypeDto> GetObjectiveTypes()
        {
            return new List<ObjectiveTypeDto>()
                {
                    new ObjectiveTypeDto() { Name = tdms.ObjectDefs[ObjectTypeID.DEFECT].Description },
                    new ObjectiveTypeDto() { Name = tdms.ObjectDefs[ObjectTypeID.WORK].Description },
                };
        }

        private ICollection<EnumerationTypeDto> GetEnumerationTypes()
        {
            var tdmsType = tdms.ObjectDefs[ObjectTypeID.COMPANY];
            var enumerationType = new EnumerationTypeDto()
            {
                ExternalId = tdmsType.SysName,
                Name = tdmsType.Description,
                EnumerationValues = new List<EnumerationValueDto>(),
            };

            var queryCom = tdms.CreateQuery();
            queryCom.AddCondition(TDMSQueryConditionType.tdmQueryConditionObjectDef, ObjectTypeID.COMPANY);

            foreach (TDMSObject contractor in queryCom.Objects)
            {
                enumerationType.EnumerationValues.Add(new EnumerationValueDto()
                {
                    ExternalId = contractor.GUID,
                    Value = contractor.Description,
                });
            }

            return new List<EnumerationTypeDto>() { enumerationType };
        }
    }
}
