using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MRS.DocumentManagement.Interface.Dtos;
using TDMS;

namespace MRS.DocumentManagement.Connection.Tdms.Helpers
{
    public class DefectMapper : IModelMapper<ObjectiveExternalDto, TDMSObject>
    {
        public ObjectiveExternalDto ToDto(TDMSObject tdmsObject)
        {
            if (tdmsObject.Attributes.Index[AttributeID.GUID] == -1)
                tdmsObject.Attributes.Create(AttributeID.GUID);

            var project = tdmsObject.Attributes[AttributeID.OBJECT_LINK].Object.GUID;
            var parent = tdmsObject.Parent.GUID;

            ObjectiveExternalDto objectiveDto = new ObjectiveExternalDto();

            objectiveDto.ExternalID = tdmsObject.GUID;
            objectiveDto.ProjectExternalID = project;
            objectiveDto.ParentObjectiveExternalID = parent;
            objectiveDto.AuthorExternalID = tdmsObject.CreateUser.SysName;
            objectiveDto.ObjectiveType = new ObjectiveTypeExternalDto()
            {
                Name = tdmsObject.ObjectDefName,
            };
            objectiveDto.CreationDate = Convert.ToDateTime(tdmsObject.Attributes[AttributeID.START_DATE].Value);
            objectiveDto.DueDate = Convert.ToDateTime(tdmsObject.Attributes[AttributeID.DUE_DATE].Value);
            objectiveDto.Title = tdmsObject.Attributes[AttributeID.NORM_DOC].Value.ToString();
            objectiveDto.Description = tdmsObject.Attributes[AttributeID.DESCRIPTION].Value.ToString();
            objectiveDto.Status = ObjectiveStatus.Undefined;
            objectiveDto.Items = new List<ItemExternalDto>();
            objectiveDto.DynamicFields = new List<DynamicFieldExternalDto>();
            objectiveDto.BimElements = new List<BimElementExternalDto>(); // TODO: 

            return objectiveDto;
        }

        public TDMSObject ToModel(ObjectiveExternalDto objectDto, TDMSObject model)
        {
            model.StatusName = SetStatus(objectDto.Status);

            model.Attributes[AttributeID.NORM_DOC].Value = objectDto.Title;
            model.Attributes[AttributeID.START_DATE].Value = objectDto.CreationDate;
            model.Attributes[AttributeID.DUE_DATE].Value = objectDto.DueDate;
            model.Attributes[AttributeID.DESCRIPTION].Value = objectDto.Description;
            model.Attributes[AttributeID.AUTHOR].Value = GetUser(objectDto.AuthorExternalID);

            // TODO: DynamicField
            // model.Attributes[AttributeID.BUILDER].Value = ;

            if (objectDto.BimElements != null)
            {
                if (model.Attributes.Index[AttributeID.GUID] == -1)
                    model.Attributes.Create(AttributeID.GUID);

                model.Attributes[AttributeID.GUID].Value = objectDto.BimElements.ToString(); // TODO: Serialize to JSON
            }

            var parent = TdmsConnection.tdms.GetObjectByGUID(objectDto.ParentObjectiveExternalID);

            model.Attributes[AttributeID.OBJECT_LINK].Value = parent.Attributes[AttributeID.OBJECT_LINK];
            model.Attributes[AttributeID.JOB_LINK].Value = parent;
            model.Attributes[AttributeID.PARENT].Value = parent;

            model.Attributes[AttributeID.NUMBER].Value = parent.Objects.ObjectsByDef(ObjectTypeID.DEFECT).Count;

            // TODO: Files.

            return model;
        }

        private string SetStatus(ObjectiveStatus status) => status switch
        {
            ObjectiveStatus.Ready => StatusID.DEFECT_DONE,
            ObjectiveStatus.InProgress => StatusID.DEFECT_INPROGRESS,
            _ => StatusID.DEFECT_CREATED,
        };

        private TDMSUser GetUser(string id) => TdmsConnection.tdms.Users.Cast<TDMSUser>().FirstOrDefault(u => u.SysName == id);
    }
}