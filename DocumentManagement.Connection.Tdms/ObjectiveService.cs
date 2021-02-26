using System;
using System.Collections.Generic;
using System.Linq;
using MRS.DocumentManagement.Connection.Tdms.Mappers;
using MRS.DocumentManagement.Interface.Dtos;
using TDMS;

namespace MRS.DocumentManagement.Connection.Tdms
{
    public class ObjectiveService
    {
        private readonly ObjectiveMapper mapper = new ObjectiveMapper();

        public ObjectiveExternalDto Get(string id)
        {
            try
            {
                TDMSObject objective = TdmsConnection.TDMS.GetObjectByGUID(id);
                return mapper.ToDto(objective);
            }
            catch
            {
                return null;
            }
        }

        public ObjectiveExternalDto Add(ObjectiveExternalDto objectiveDto)
        {
            try
            {
                string parent = string.Empty;
                string type = string.Empty;

                if (objectiveDto.ObjectiveType.Name == ObjectTypeID.DEFECT)
                {
                    parent = objectiveDto.ParentObjectiveExternalID;
                    type = ObjectTypeID.DEFECT;
                }
                else
                {
                    // Cannot create Job from Local
                    throw new NotImplementedException();
                }

                if (string.IsNullOrEmpty(parent))
                    throw new Exception();

                TDMSObject obj = TdmsConnection.TDMS.GetObjectByGUID(parent);
                TDMSObject objective = obj.Objects.Create(type);
                obj.Update();
                mapper.ToModel(objectiveDto, objective);
                objective.Update();
                obj.Update();

                return mapper.ToDto(objective);
            }
            catch
            {
                return default;
            }
        }

        public ObjectiveExternalDto Update(ObjectiveExternalDto objectiveDto)
        {
            try
            {
                TDMSObject objective = TdmsConnection.TDMS.GetObjectByGUID(objectiveDto.ExternalID);
                if (objective == null)
                    throw new NullReferenceException();

                mapper.ToModel(objectiveDto, objective);
                objective.Update();

                return mapper.ToDto(objective);
            }
            catch
            {
                return default;
            }
        }

        public bool Remove(string id)
        {
            try
            {
                TDMSObject obj = TdmsConnection.TDMS.GetObjectByGUID(id);
                var parent = obj.Parent;
                obj.Erase();
                parent?.Update();

                return true;
            }
            catch
            {
                return false;
            }
        }

        public IEnumerable<ObjectiveExternalDto> GetListOfObjectives()
        {
            List<ObjectiveExternalDto> objectives = new List<ObjectiveExternalDto>();
            try
            {
                return FindByDef(ObjectTypeID.WORK, FindByDef(ObjectTypeID.DEFECT, objectives));
            }
            catch
            {
                return null;
            }
        }

        private List<ObjectiveExternalDto> FindByDef(string objectTypeId, List<ObjectiveExternalDto> list)
        {
            var queryCom = TdmsConnection.TDMS.CreateQuery();
            queryCom.AddCondition(TDMSQueryConditionType.tdmQueryConditionObjectDef, objectTypeId);

            foreach (TDMSObject obj in queryCom.Objects)
            {
                var mapped = mapper.ToDto(obj);
                if (mapped != null)
                    list.Add(mapped);
            }

            return list;
        }
    }
}
