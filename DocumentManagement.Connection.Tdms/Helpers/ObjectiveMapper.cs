using System;
using System.Collections.Generic;
using System.Linq;
using MRS.DocumentManagement.Interface.Dtos;
using TDMS;

namespace MRS.DocumentManagement.Connection.Tdms.Helpers
{
    internal class ObjectiveMapper
    {
        public ObjectiveExternalDto ToDto(TDMSObject tdmsObject)
        {
            return GetMapper(tdmsObject)?.ToDto(tdmsObject);
        }

        public TDMSObject ToModel(ObjectiveExternalDto objectDto, TDMSObject tdmsObject)
        {
            return GetMapper(tdmsObject)?.ToModel(objectDto, tdmsObject);
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
