using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MRS.DocumentManagement.Connection.Tdms.Mappers;
using MRS.DocumentManagement.Interface;
using MRS.DocumentManagement.Interface.Dtos;
using TDMS;

namespace MRS.DocumentManagement.Connection.Tdms
{
    public class TdmsObjectivesSynchronizer : ISynchronizer<ObjectiveExternalDto>
    {
        private readonly ObjectiveMapper mapper = new ObjectiveMapper();

        public Task<ObjectiveExternalDto> Add(ObjectiveExternalDto objectiveDto)
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

            TDMSObject obj = TdmsConnection.connection.GetObjectByGUID(parent);
            TDMSObject objective = obj.Objects.Create(type);
            obj.Update();
            mapper.ToModel(objectiveDto, objective);
            objective.Update();
            obj.Update();

            return Task.FromResult(mapper.ToDto(objective));
        }

        public Task<ObjectiveExternalDto> Update(ObjectiveExternalDto objectiveDto)
        {
            TDMSObject objective = TdmsConnection.connection.GetObjectByGUID(objectiveDto.ExternalID);
            if (objective == null)
                throw new NullReferenceException();

            mapper.ToModel(objectiveDto, objective);
            objective.Update();

            return Task.FromResult(mapper.ToDto(objective));
        }

        public Task<ObjectiveExternalDto> Remove(ObjectiveExternalDto objectiveDto)
        {
            TDMSObject obj = TdmsConnection.connection.GetObjectByGUID(objectiveDto.ExternalID);
            var parent = obj.Parent;
            obj.Erase();
            parent?.Update();

            return Task.FromResult(objectiveDto);
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

        public ObjectiveExternalDto Get(string id)
        {
            try
            {
                TDMSObject objective = TdmsConnection.connection.GetObjectByGUID(id);
                return mapper.ToDto(objective);
            }
            catch
            {
                return null;
            }
        }

        private List<ObjectiveExternalDto> FindByDef(string objectTypeId, List<ObjectiveExternalDto> list)
        {
            var queryCom = TdmsConnection.connection.CreateQuery();
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
