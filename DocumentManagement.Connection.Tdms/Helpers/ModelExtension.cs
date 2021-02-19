using System;
using System.Collections.Generic;
using MRS.DocumentManagement.Interface.Dtos;
using TDMS;

namespace MRS.DocumentManagement.Connection.Tdms.Helpers
{
    public enum ObjectType
    {
        JOB,
        DEFECT,
    }

    public static class ModelExtension
    {
        public static ObjectiveDto ToObjective(this TDMSObject tdmsObject, ObjectType objectType)
        {
            return objectType switch
            {
                ObjectType.JOB => tdmsObject.FromJob(),
                ObjectType.DEFECT => tdmsObject.FromDefect(),
                _ => default,
            };
        }

        private static ObjectiveDto FromJob(this TDMSObject tdmsObject)
        {
            if (tdmsObject.Attributes.Index[AttributeID.GUID] == -1)
                tdmsObject.Attributes.Create(AttributeID.GUID);

            ObjectiveDto objectiveDto = new ObjectiveDto();

            objectiveDto.ID = new ID<ObjectiveDto>(-1);
            objectiveDto.ProjectID = new ID<ProjectDto>(-1);
            objectiveDto.AuthorID = new ID<UserDto>(-1);
            objectiveDto.ObjectiveTypeID = new ID<ObjectiveTypeDto>(-1);

            objectiveDto.CreationDate = Convert.ToDateTime(tdmsObject.Attributes[AttributeID.START_DATE].Value);
            objectiveDto.DueDate = Convert.ToDateTime(tdmsObject.Attributes[AttributeID.DUE_DATE].Value);
            objectiveDto.Title = tdmsObject.Attributes[AttributeID.WORK_NAME].Value.ToString();
            objectiveDto.Description = string.Empty;

            objectiveDto.Status = GetStatus(tdmsObject.StatusName);
            objectiveDto.Items = new List<ItemDto>();
            objectiveDto.DynamicFields = new List<DynamicFieldDto>(); // Volumes, Operations,
            objectiveDto.BimElements = new List<BimElementDto>();

            return new ObjectiveDto();
        }

        private static ObjectiveDto FromDefect(this TDMSObject tdmsObject)
        {
            if (tdmsObject.Attributes.Index[AttributeID.GUID] == -1)
                tdmsObject.Attributes.Create(AttributeID.GUID);

            ObjectiveDto objectiveDto = new ObjectiveDto();

            objectiveDto.ID = new ID<ObjectiveDto>(-1);
            objectiveDto.ProjectID = new ID<ProjectDto>(-1);
            objectiveDto.ParentObjectiveID = new ID<ObjectiveDto>(-1);
            objectiveDto.AuthorID = new ID<UserDto>(-1);
            objectiveDto.ObjectiveTypeID = new ID<ObjectiveTypeDto>(-1);
            objectiveDto.CreationDate = Convert.ToDateTime(tdmsObject.Attributes[AttributeID.START_DATE].Value);
            objectiveDto.DueDate = Convert.ToDateTime(tdmsObject.Attributes[AttributeID.DUE_DATE].Value);
            objectiveDto.Title = tdmsObject.Attributes[AttributeID.NORM_DOC].Value.ToString();
            objectiveDto.Description = tdmsObject.Attributes[AttributeID.DESCRIPTION].Value.ToString();
            objectiveDto.Status = ObjectiveStatus.Undefined;
            objectiveDto.Items = new List<ItemDto>();
            objectiveDto.DynamicFields = new List<DynamicFieldDto>();
            objectiveDto.BimElements = new List<BimElementDto>();

            return objectiveDto;
        }

        private static ObjectiveStatus GetStatus(string statusTDMS)
        {
            return statusTDMS == StatusID.WORK_COMPLETED || statusTDMS == StatusID.DEFECT_DONE ? ObjectiveStatus.Ready :
                    statusTDMS == StatusID.WORK_IN_PROGRESS ? ObjectiveStatus.InProgress :
                    statusTDMS == StatusID.WORK_LATE ? ObjectiveStatus.Late :
                    statusTDMS == StatusID.WORK_OPEN ? ObjectiveStatus.Open :
                    ObjectiveStatus.Undefined;
        }

    }
}
