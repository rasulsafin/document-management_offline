using System;
using System.Collections.Generic;
using MRS.DocumentManagement.Connection.Tdms.Helpers;
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
                TDMSObject objective = TdmsConnection.tdms.GetObjectByGUID(id);
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
                else if (objectiveDto.ObjectiveType.Name == ObjectTypeID.WORK)
                {
                    parent = objectiveDto.ProjectExternalID;
                    type = ObjectTypeID.WORK;
                } else
                {
                    throw new Exception();
                }

                if (string.IsNullOrEmpty(parent))
                    throw new Exception();

                TDMSObject obj = TdmsConnection.tdms.GetObjectByGUID(parent);
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
                TDMSObject objective = TdmsConnection.tdms.GetObjectByGUID(objectiveDto.ExternalID);
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
                TDMSObject obj = TdmsConnection.tdms.GetObjectByGUID(id);
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

        public IEnumerable<ObjectiveExternalDto> GetListOfObjectives(string id)
        {
            throw new NotImplementedException();
        }
    }
}
