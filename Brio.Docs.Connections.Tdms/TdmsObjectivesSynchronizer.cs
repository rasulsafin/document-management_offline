using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Brio.Docs.Connections.Tdms.Mappers;
using Brio.Docs.Integration;
using Brio.Docs.Integration.Dtos;
using Brio.Docs.Integration.Interfaces;
using TDMS;

namespace Brio.Docs.Connections.Tdms
{
    public class TdmsObjectivesSynchronizer : TdmsSynchronizer, ISynchronizer<ObjectiveExternalDto>
    {
        private readonly ObjectiveMapper mapper;

        public TdmsObjectivesSynchronizer(TDMSApplication tdms)
            : base(tdms)
        {
            this.mapper = new ObjectiveMapper(tdms);
        }

        public Task<ObjectiveExternalDto> Add(ObjectiveExternalDto objectiveDto)
        {
            string parent = string.Empty;
            string type = string.Empty;

            if (objectiveDto.ObjectiveType.ExternalId == ObjectTypeID.DEFECT)
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

            TDMSObject obj = tdms.GetObjectByGUID(parent);
            TDMSObject objective = obj.Objects.Create(type);
            obj.Update();
            mapper.ToModel(objectiveDto, objective);
            objective.Update();
            obj.Update();

            return Task.FromResult(mapper.ToDto(objective));
        }

        public Task<ObjectiveExternalDto> Update(ObjectiveExternalDto objectiveDto)
        {
            TDMSObject objective = tdms.GetObjectByGUID(objectiveDto.ExternalID);
            if (objective == null)
                throw new NullReferenceException();

            mapper.ToModel(objectiveDto, objective);
            objective.Update();

            return Task.FromResult(mapper.ToDto(objective));
        }

        public Task<ObjectiveExternalDto> Remove(ObjectiveExternalDto objectiveDto)
        {
            TDMSObject obj = tdms.GetObjectByGUID(objectiveDto.ExternalID);
            var parent = obj.Parent;
            obj.Erase();
            parent?.Update();

            return Task.FromResult(objectiveDto);
        }

        public ObjectiveExternalDto GetById(string id)
        {
            try
            {
                TDMSObject objective = tdms.GetObjectByGUID(id);
                return mapper.ToDto(objective);
            }
            catch
            {
                return null;
            }
        }

        public Task<IReadOnlyCollection<string>> GetUpdatedIDs(DateTime date)
        {
            ////FOR TESTS
            var defaultDate = new DateTime(2021, 1, 1);
            if (date < defaultDate)
                date = defaultDate;

            var jobs = FindByDef(ObjectTypeID.WORK, date);
            var issues = FindByDef(ObjectTypeID.DEFECT, date);

            return Task.FromResult<IReadOnlyCollection<string>>(jobs.Concat(issues).ToList());
        }

        public Task<IReadOnlyCollection<ObjectiveExternalDto>> Get(IReadOnlyCollection<string> ids)
        {
            var objectives = new List<ObjectiveExternalDto>();
            foreach (var objectiveId in ids)
            {
                var objective = GetById(objectiveId);
                if (objective != null)
                    objectives.Add(objective);
            }

            return Task.FromResult<IReadOnlyCollection<ObjectiveExternalDto>>(objectives);
        }
    }
}
