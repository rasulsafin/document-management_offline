using Brio.Docs.Client;
using Brio.Docs.Client.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TDMS;

namespace Brio.Docs.Connections.Tdms
{
    public class TdmsConnection : IConnection, IDisposable
    {
        private static TDMSApplication tdms;

        public Task<ConnectionStatusDto> Connect(ConnectionInfoExternalDto info, CancellationToken token)
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
            info.UserExternalID = tdms.CurrentUser.SysName;

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

        public void Quit() => tdms.Quit();

        private ICollection<ObjectiveTypeExternalDto> GetObjectiveTypes()
        {
            return new List<ObjectiveTypeExternalDto>()
                {
                    new ObjectiveTypeExternalDto()
                    {
                        Name = tdms.ObjectDefs[ObjectTypeID.DEFECT].Description,
                        ExternalId = ObjectTypeID.DEFECT,
                        DefaultDynamicFields = GetDefectDefaultDynamicFields(),
                    },
                    new ObjectiveTypeExternalDto() { Name = tdms.ObjectDefs[ObjectTypeID.WORK].Description, ExternalId = ObjectTypeID.WORK },
                };
        }

        private ICollection<DynamicFieldExternalDto> GetDefectDefaultDynamicFields()
        {
            var list = new List<DynamicFieldExternalDto>();

            var definitions = tdms.ObjectDefs;
            string defaultCompanyValue = GetEnumerationTypes()
                        .Where(x => x.ExternalID == ObjectTypeID.COMPANY)
                        .FirstOrDefault().EnumerationValues
                        .FirstOrDefault()?.ExternalID;

            var defectDef = definitions.Cast<TDMSObjectDef>().FirstOrDefault(x => x.SysName == ObjectTypeID.DEFECT);
            list.AddIsNotNull(ConstructDynamicFieldDto(
                defectDef,
                AttributeID.BUILDER,
                DynamicFieldType.ENUM,
                defaultCompanyValue));
            list.AddIsNotNull(ConstructDynamicFieldDto(
                defectDef,
                AttributeID.COMPANY,
                DynamicFieldType.ENUM,
                defaultCompanyValue));

            return list;
        }

        private DynamicFieldExternalDto ConstructDynamicFieldDto(TDMSObjectDef objectDef, string attributeId, DynamicFieldType type, string defaultValue)
        {
            var attributeDef = objectDef.AttributeDefs.Cast<TDMSAttributeDef>().FirstOrDefault(a => a.SysName == attributeId);
            if (attributeDef == null)
                return null;

            return new DynamicFieldExternalDto()
            {
                ExternalID = attributeDef.SysName,
                Name = attributeDef.Description,
                Type = type,
                Value = defaultValue,
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
            return new TdmsConnectionContext(tdms);
        }

        public async Task<IConnectionStorage> GetStorage(ConnectionInfoExternalDto info)
        {
            return new TdmsStorage(tdms);
        }
    }
}
