using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MRS.DocumentManagement.Interface.Dtos;
using TDMS;

namespace MRS.DocumentManagement.Connection.Tdms.Mappers
{
    public class DefectMapper : IModelMapper<ObjectiveExternalDto, TDMSObject>
    {
        private static readonly string SPLITTER = ";";

        public ObjectiveExternalDto ToDto(TDMSObject tdmsObject)
        {
            ObjectiveExternalDto objectiveDto = new ObjectiveExternalDto();

            objectiveDto.ExternalID = tdmsObject.GUID;
            objectiveDto.ProjectExternalID = tdmsObject.Attributes[AttributeID.OBJECT_LINK].Object.GUID;
            objectiveDto.ParentObjectiveExternalID = tdmsObject.Parent.GUID;
            objectiveDto.AuthorExternalID = tdmsObject.CreateUser.SysName;
            objectiveDto.ObjectiveType = new ObjectiveTypeExternalDto()
            {
                Name = ObjectTypeID.DEFECT,
            };
            objectiveDto.CreationDate = Convert.ToDateTime(tdmsObject.Attributes[AttributeID.START_DATE].Value);
            objectiveDto.DueDate = Convert.ToDateTime(tdmsObject.Attributes[AttributeID.DUE_DATE].Value);

            if (tdmsObject.Attributes.Index[AttributeID.GUID] == -1)
                tdmsObject.Attributes.Create(AttributeID.GUID);
            else
                objectiveDto.BimElements = GetBimElemenents(tdmsObject);

            objectiveDto.Title = tdmsObject.Attributes[AttributeID.NORM_DOC].Value.ToString();
            objectiveDto.Description = tdmsObject.Attributes[AttributeID.DESCRIPTION].Value.ToString();
            objectiveDto.Status = GetStatus(tdmsObject.StatusName);
            objectiveDto.DynamicFields = new List<DynamicFieldExternalDto>(); // TODO: DynamicField

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

            if (objectDto.BimElements != null)
            {
                if (model.Attributes.Index[AttributeID.GUID] == -1)
                    model.Attributes.Create(AttributeID.GUID);

                model.Attributes[AttributeID.GUID].Value = SetBimElements(objectDto.BimElements);
            }

            var parent = TdmsConnection.TDMS.GetObjectByGUID(objectDto.ParentObjectiveExternalID);

            model.Attributes[AttributeID.OBJECT_LINK].Value = parent.Attributes[AttributeID.OBJECT_LINK];
            model.Attributes[AttributeID.JOB_LINK].Value = parent;
            model.Attributes[AttributeID.PARENT].Value = parent;

            model.Attributes[AttributeID.NUMBER].Value = parent.Objects.ObjectsByDef(ObjectTypeID.DEFECT).Count;

            // TODO: DynamicField
            // model.Attributes[AttributeID.BUILDER].Value = ;

            return model;
        }

        private string SetBimElements(ICollection<BimElementExternalDto> bimElements)
        {
            if (bimElements == null)
                return string.Empty;

            return string.Join(SPLITTER, bimElements.Select(b => b.GlobalID));
        }

        private string SetStatus(ObjectiveStatus status) => status switch
        {
            ObjectiveStatus.Ready => StatusID.DEFECT_DONE,
            ObjectiveStatus.InProgress => StatusID.DEFECT_INPROGRESS,
            _ => StatusID.DEFECT_CREATED,
        };

        private ObjectiveStatus GetStatus(string statusTDMS)
          =>  statusTDMS == StatusID.DEFECT_DONE ? ObjectiveStatus.Ready :
              statusTDMS == StatusID.DEFECT_INPROGRESS ? ObjectiveStatus.InProgress :
              statusTDMS == StatusID.DEFECT_CREATED ? ObjectiveStatus.Open :
              ObjectiveStatus.Undefined;

        private TDMSUser GetUser(string id) =>
            TdmsConnection.TDMS.Users.Cast<TDMSUser>().FirstOrDefault(u => u.SysName == id);

        private ICollection<BimElementExternalDto> GetBimElemenents(TDMSObject tdmsObject)
        {
            var list = new List<BimElementExternalDto>();
            var links = tdmsObject.Attributes[AttributeID.GUID].Value.ToString().Split(SPLITTER, StringSplitOptions.RemoveEmptyEntries);
            foreach (string link in links)
            {
                list.Add(new BimElementExternalDto()
                {
                    GlobalID = link,
                });
            }

            return list;
        }
    }
}