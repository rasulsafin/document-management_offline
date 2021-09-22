using Brio.Docs.Interface.Dtos;
using System.Collections.Generic;
using TDMS;

namespace Brio.Docs.Connection.Tdms.Mappers
{
    internal class ObjectiveMapper
    {
        private readonly ItemHelper itemHelper = new ItemHelper();
        private readonly TDMSApplication tdms;

        public ObjectiveMapper(TDMSApplication tdms)
        {
            this.tdms = tdms;
        }

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
            SetDynamicField(objectDto, tdmsObject);

            return tdmsObject;
        }

        private IModelMapper<ObjectiveExternalDto, TDMSObject> GetMapper(TDMSObject tdmsObject)
        {
            if (tdmsObject.ObjectDefName == ObjectTypeID.WORK)
                return new JobMapper();
            else if (tdmsObject.ObjectDefName == ObjectTypeID.DEFECT)
                return new DefectMapper(tdms);
            else
                return null;
        }

        private void SetDynamicField(ObjectiveExternalDto objectDto, TDMSObject tdmsObject)
        {
            foreach (var field in objectDto.DynamicFields)
            {
                if (tdmsObject.Attributes.Index[field.ExternalID] == -1)
                    continue;

                switch (field.Type)
                {
                    case DynamicFieldType.BOOL:
                    case DynamicFieldType.FLOAT:
                    case DynamicFieldType.INTEGER:
                    case DynamicFieldType.STRING:
                        tdmsObject.Attributes[field.ExternalID].Value = field.Value;
                        break;
                    case DynamicFieldType.DATE:
                        tdmsObject.Attributes[field.ExternalID].Value = System.DateTime.Parse(field.Value);
                        break;
                    case DynamicFieldType.ENUM:
                        tdmsObject.Attributes[field.ExternalID].Value = tdms.GetObjectByGUID(field.Value);
                        break;
                    case DynamicFieldType.OBJECT:
                        //TODO: ObjectType
                    default:
                        return;
                }
            }
        }
    }
}
