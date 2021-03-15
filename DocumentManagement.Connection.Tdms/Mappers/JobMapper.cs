using System;
using System.Collections.Generic;
using System.Linq;
using MRS.DocumentManagement.Interface.Dtos;
using TDMS;

namespace MRS.DocumentManagement.Connection.Tdms.Mappers
{
    public class JobMapper : IModelMapper<ObjectiveExternalDto, TDMSObject>
    {
        public ObjectiveExternalDto ToDto(TDMSObject tdmsObject)
        {
            ObjectiveExternalDto objectiveDto = new ObjectiveExternalDto();

            objectiveDto.ExternalID = tdmsObject.GUID;
            objectiveDto.ProjectExternalID = tdmsObject.Attributes[AttributeID.OBJECT_LINK].Object.GUID;
            objectiveDto.AuthorExternalID = tdmsObject.CreateUser.SysName;
            objectiveDto.ObjectiveType = new ObjectiveTypeExternalDto()
            {
                Name = ObjectTypeID.WORK,
            };
            objectiveDto.CreationDate = Convert.ToDateTime(tdmsObject.Attributes[AttributeID.START_DATE].Value);
            objectiveDto.DueDate = Convert.ToDateTime(tdmsObject.Attributes[AttributeID.DUE_DATE].Value);

            objectiveDto.Title = tdmsObject.Attributes[AttributeID.WORK_NAME].Value.ToString();
            objectiveDto.Description = tdmsObject.Description;
            objectiveDto.Status = GetStatus(tdmsObject.StatusName);
            objectiveDto.DynamicFields = GetDynamicFields(tdmsObject);
            objectiveDto.BimElements = GetBimElemenents(tdmsObject);

            return objectiveDto;
        }

        public TDMSObject ToModel(ObjectiveExternalDto objectDto, TDMSObject model)
        {
            // TODO: Dynamic fields as Volumes and Operations
            // Can only change status and dynamic fields.
            model.StatusName = SetStatus(objectDto.Status);
            return model;
        }

        private ICollection<BimElementExternalDto> GetBimElemenents(TDMSObject tdmsObject)
        {
            var list = new List<BimElementExternalDto>();
            var links = tdmsObject.ReferencedBy.Cast<TDMSObject>().Where(x => x.ObjectDefName == ObjectTypeID.LINK);
            foreach (TDMSObject link in links)
            {
                list.Add(new BimElementExternalDto()
                {
                    GlobalID = link.Attributes[AttributeID.ENTITY_GLOBAL_ID].Value.ToString(),
                });
            }

            return list;
        }

        private ObjectiveStatus GetStatus(string statusTDMS)
            => statusTDMS == StatusID.WORK_COMPLETED ? ObjectiveStatus.Ready :
                statusTDMS == StatusID.WORK_IN_PROGRESS ? ObjectiveStatus.InProgress :
                statusTDMS == StatusID.WORK_LATE ? ObjectiveStatus.Late :
                statusTDMS == StatusID.WORK_OPEN ? ObjectiveStatus.Open :
                ObjectiveStatus.Undefined;

        private string SetStatus(ObjectiveStatus status)
            => status switch
            {
                ObjectiveStatus.Ready => StatusID.WORK_COMPLETED,
                ObjectiveStatus.InProgress => StatusID.WORK_IN_PROGRESS,
                ObjectiveStatus.Late => StatusID.WORK_LATE,
                ObjectiveStatus.Open => StatusID.WORK_OPEN,
                _ => StatusID.WORK_OPEN,
            };

        private ICollection<DynamicFieldExternalDto> GetDynamicFields(TDMSObject tdmsObject)
        {
            //// TODO: Dynamic fields
            //// - Volumes (DF):
            ////    - Volume (float)
            ////    - Price (float)
            ////    - Ammount (float)
            ////    - Status (bool)
            //// - Operations (DF):
            ////    - name (string),
            ////    - status (bool)

            return new List<DynamicFieldExternalDto>() { };
        }
    }
}
