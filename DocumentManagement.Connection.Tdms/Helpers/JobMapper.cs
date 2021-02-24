using MRS.DocumentManagement.Interface.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TDMS;

namespace MRS.DocumentManagement.Connection.Tdms.Helpers
{
    public class JobMapper : IModelMapper<ObjectiveExternalDto, TDMSObject>
    {
        public ObjectiveExternalDto ToDto(TDMSObject tdmsObject)
        {
            if (tdmsObject.Attributes.Index[AttributeID.GUID] == -1)
                tdmsObject.Attributes.Create(AttributeID.GUID);

            ObjectiveExternalDto objectiveDto = new ObjectiveExternalDto();

            //objectiveDto.ID = new ID<ObjectiveDto>(-1);
            //objectiveDto.ProjectID = new ID<ProjectDto>(-1);
            //objectiveDto.AuthorID = new ID<UserDto>(-1);
            //objectiveDto.ObjectiveTypeID = new ID<ObjectiveTypeDto>(-1);

            objectiveDto.CreationDate = Convert.ToDateTime(tdmsObject.Attributes[AttributeID.START_DATE].Value);
            objectiveDto.DueDate = Convert.ToDateTime(tdmsObject.Attributes[AttributeID.DUE_DATE].Value);
            objectiveDto.Title = tdmsObject.Attributes[AttributeID.WORK_NAME].Value.ToString();
            objectiveDto.Description = string.Empty;

            objectiveDto.Status = GetStatus(tdmsObject.StatusName);
            objectiveDto.Items = new List<ItemExternalDto>();
            objectiveDto.DynamicFields = new List<DynamicFieldExternalDto>(); // Volumes, Operations,
            objectiveDto.BimElements = new List<BimElementExternalDto>();

            return new ObjectiveExternalDto();
        }

        public TDMSObject ToModel(ObjectiveExternalDto objectDto, TDMSObject model)
        {
            throw new NotImplementedException();
        }

        private ObjectiveStatus GetStatus(string statusTDMS)
            => statusTDMS == StatusID.WORK_COMPLETED || statusTDMS == StatusID.DEFECT_DONE ? ObjectiveStatus.Ready :
                statusTDMS == StatusID.WORK_IN_PROGRESS ? ObjectiveStatus.InProgress :
                statusTDMS == StatusID.WORK_LATE ? ObjectiveStatus.Late :
                statusTDMS == StatusID.WORK_OPEN ? ObjectiveStatus.Open :
                ObjectiveStatus.Undefined;
    }
}
