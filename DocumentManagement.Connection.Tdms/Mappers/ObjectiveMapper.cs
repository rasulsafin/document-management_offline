using MRS.DocumentManagement.Interface.Dtos;
using TDMS;

namespace MRS.DocumentManagement.Connection.Tdms.Mappers
{
    internal class ObjectiveMapper
    {
        private readonly ItemHelper itemHelper = new ItemHelper();

        public ObjectiveExternalDto ToDto(TDMSObject tdmsObject)
        {
            try
            {
                var dto = GetMapper(tdmsObject)?.ToDto(tdmsObject);
                dto.Items = itemHelper.GetItems(tdmsObject);
                return dto;
            } catch
            {
                return null;
            }
        }

        public TDMSObject ToModel(ObjectiveExternalDto objectDto, TDMSObject tdmsObject)
        {
            GetMapper(tdmsObject)?.ToModel(objectDto, tdmsObject);
            itemHelper.SetItems(tdmsObject, objectDto.Items);

            return tdmsObject;
        }

        private IModelMapper<ObjectiveExternalDto, TDMSObject> GetMapper(TDMSObject tdmsObject)
        {
            if (tdmsObject.ObjectDefName == ObjectTypeID.WORK)
                return new JobMapper();
            else if (tdmsObject.ObjectDefName == ObjectTypeID.DEFECT)
                return new DefectMapper();
            else
                return null;
        }
    }
}
