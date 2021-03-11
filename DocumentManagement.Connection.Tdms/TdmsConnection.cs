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
        internal static TDMSApplication TDMS;

        public Task<ConnectionStatusDto> Connect(ConnectionInfoExternalDto info)
        {
            TDMS = new TDMSApplication();

            if (TDMS.IsLoggedIn)
            {
                if (TDMS.CurrentUser.Login == info.AuthFieldValues[Auth.LOGIN])
                {
                    return GetStatus(info);
                }
                else
                {
                    TDMS.Quit();
                    TDMS = new TDMSApplication();
                }
            }

            try
            {
                var login = info.AuthFieldValues[Auth.LOGIN];
                var password = info.AuthFieldValues[Auth.PASSWORD];
                var server = info.AuthFieldValues[Auth.SERVER];
                var db = info.AuthFieldValues[Auth.DATABASE];

                TDMS.Login(login, password, db, server);
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
            if (TDMS == null)
            {
                return Task.FromResult(new ConnectionStatusDto()
                {
                    Status = RemoteConnectionStatus.NeedReconnect,
                    Message = "NeedReconnect",
                });
            }

            if (TDMS.IsLoggedIn == true)
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

        public void Quit() => TDMS.Quit();

        private ICollection<ObjectiveTypeExternalDto> GetObjectiveTypes()
        {
            return new List<ObjectiveTypeExternalDto>()
                {
                    new ObjectiveTypeExternalDto() { Name = TDMS.ObjectDefs[ObjectTypeID.DEFECT].Description, ExternalId = ObjectTypeID.DEFECT },
                    new ObjectiveTypeExternalDto() { Name = TDMS.ObjectDefs[ObjectTypeID.WORK].Description, ExternalId = ObjectTypeID.WORK },
                };
        }

        private ICollection<EnumerationTypeExternalDto> GetEnumerationTypes()
        {
            var list = new List<EnumerationTypeExternalDto>();
            try
            {
                /// Companies
                var tdmsType = TDMS.ObjectDefs[ObjectTypeID.COMPANY];
                var enumerationType = new EnumerationTypeExternalDto()
                {
                    ExternalID = ObjectTypeID.COMPANY,
                    Name = tdmsType.Description,
                    EnumerationValues = new List<EnumerationValueExternalDto>(),
                };

                var queryCom = TDMS.CreateQuery();
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

        public Task<IConnectionContext> GetContext(ConnectionInfoExternalDto info, DateTime lastSynchronizationDate)
        {
            throw new NotImplementedException();
        }

        public void Dispose() => TDMS.Quit();
    }
}
