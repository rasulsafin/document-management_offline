using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MRS.DocumentManagement.Interface;
using MRS.DocumentManagement.Interface.Dtos;
using TDMS;

namespace MRS.DocumentManagement.Connection.Tdms
{
    public class TdmsConnection : IConnection, IDisposable
    {
        private static TDMSApplication tdms;

        public Task<ConnectionStatusDto> Connect(ConnectionInfoExternalDto info)
        {
            tdms = new TDMSApplication();

            if (tdms.IsLoggedIn)
            {
                if (tdms.CurrentUser.Login == info.AuthFieldValues[Auth.LOGIN])
                {
                    return GetStatus(info);
                }
                else
                {
                    tdms.Quit();
                    tdms = new TDMSApplication();
                }
            }

            try
            {
                var login = info.AuthFieldValues[Auth.LOGIN];
                var password = info.AuthFieldValues[Auth.PASSWORD];
                var server = info.AuthFieldValues[Auth.SERVER];
                var db = info.AuthFieldValues[Auth.DATABASE];

                tdms.Login(login, password, db, server);
            }
            catch (Exception e)
            {
                return Task.FromResult(new ConnectionStatusDto()
                {
                    Status = RemoteConnectionStatus.Error,
                    Message = e.Message,
                });
            }

            return GetStatus(info);
        }

        public Task<ConnectionInfoExternalDto> UpdateConnectionInfo(ConnectionInfoExternalDto info)
        {
            info.EnumerationTypes = GetEnumerationTypes();
            info.ConnectionType.ObjectiveTypes = GetObjectiveTypes();

            return Task.FromResult(info);
        }

        public Task<ConnectionStatusDto> GetStatus(ConnectionInfoExternalDto info)
        {
            if (tdms == null)
            {
                return Task.FromResult(new ConnectionStatusDto()
                {
                    Status = RemoteConnectionStatus.NeedReconnect,
                    Message = "NeedReconnect",
                });
            }

            if (tdms.IsLoggedIn == true)
            {
                return Task.FromResult(new ConnectionStatusDto()
                {
                    Status = RemoteConnectionStatus.OK,
                    Message = "Ok",
                });
            }

            return Task.FromResult(new ConnectionStatusDto()
            {
                Status = RemoteConnectionStatus.Error,
                Message = "Error",
            });
        }

        public Task<bool> IsAuthDataCorrect(ConnectionInfoExternalDto info)
        {
            throw new NotImplementedException();
        }

        public ConnectionTypeExternalDto GetConnectionType()
        {
            var type = new ConnectionTypeExternalDto
            {
                Name = "tdms",
                AuthFieldNames = new List<string>() { Auth.LOGIN, Auth.PASSWORD, Auth.SERVER, Auth.DATABASE },
                AppProperties = new Dictionary<string, string>(),
            };

            return type;
        }

        public void Quit() => tdms.Quit();

        private ICollection<ObjectiveTypeExternalDto> GetObjectiveTypes()
        {
            return new List<ObjectiveTypeExternalDto>()
                {
                    new ObjectiveTypeExternalDto() { Name = tdms.ObjectDefs[ObjectTypeID.DEFECT].Description, ExternalId = ObjectTypeID.DEFECT },
                    new ObjectiveTypeExternalDto() { Name = tdms.ObjectDefs[ObjectTypeID.WORK].Description, ExternalId = ObjectTypeID.WORK },
                };
        }

        private ICollection<EnumerationTypeExternalDto> GetEnumerationTypes()
        {
            var list = new List<EnumerationTypeExternalDto>();
            try
            {
                /// Companies
                var tdmsType = tdms.ObjectDefs[ObjectTypeID.COMPANY];
                var enumerationType = new EnumerationTypeExternalDto()
                {
                    ExternalID = ObjectTypeID.COMPANY,
                    Name = tdmsType.Description,
                    EnumerationValues = new List<EnumerationValueExternalDto>(),
                };

                var queryCom = tdms.CreateQuery();
                queryCom.AddCondition(TDMSQueryConditionType.tdmQueryConditionObjectDef, ObjectTypeID.COMPANY);

                foreach (TDMSObject contractor in queryCom.Objects)
                {
                    enumerationType.EnumerationValues.Add(new EnumerationValueExternalDto()
                    {
                        ExternalID = contractor.GUID,
                        Value = contractor.Description,
                    });
                }

                list.Add(enumerationType);
            }
            catch
            {
            }

            return list;
        }

        public void Dispose() => tdms.Quit();

        public async Task<IConnectionContext> GetContext(ConnectionInfoExternalDto info)
        {
            await Connect(info);
            return new TdmsConnectionContext(tdms);
        }

        public Task<IConnectionStorage> GetStorage(ConnectionInfoExternalDto info)
        {
            throw new NotImplementedException();
        }
    }
}
