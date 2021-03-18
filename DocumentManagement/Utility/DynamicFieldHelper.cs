using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using MRS.DocumentManagement.Database;
using MRS.DocumentManagement.Database.Models;
using MRS.DocumentManagement.Interface.Dtos;
using Newtonsoft.Json.Linq;

namespace MRS.DocumentManagement.Utility
{
    public class DynamicFieldHelper
    {
        private readonly DMContext context;
        private readonly IMapper mapper;

        public DynamicFieldHelper(DMContext context, IMapper mapper)
        {
            this.context = context;
            this.mapper = mapper;
        }

        internal async Task<DynamicFieldDto> BuildObjectDynamicField(DynamicField dynamicField)
        {
            if (dynamicField.Type == DynamicFieldType.OBJECT.ToString())
            {
                DynamicFieldDto objDynamicField = mapper.Map<DynamicFieldDto>(dynamicField);

                var children = context.DynamicFields.Where(x => x.ParentFieldID == dynamicField.ID);

                var value = new List<DynamicFieldDto>();
                foreach (var child in children)
                {
                    var result = await BuildObjectDynamicField(child);
                    value.Add(result);
                }

                objDynamicField.Value = value;

                return objDynamicField;
            }

            return mapper.Map<DynamicFieldDto>(dynamicField);
        }

        internal async Task AddDynamicFields(DynamicFieldDto field, int objectiveID, int parentID = -1)
        {
            var dynamicField = mapper.Map<DynamicField>(field);

            if (parentID != -1)
                dynamicField.ParentFieldID = parentID;
            else
                dynamicField.ObjectiveID = objectiveID;

            await context.DynamicFields.AddAsync(dynamicField);
            await context.SaveChangesAsync();

            if (field.Type == DynamicFieldType.OBJECT)
            {
                var children = field.Value as ICollection<DynamicFieldDto>;

                foreach (var childField in children)
                {
                    await AddDynamicFields(childField, objectiveID, dynamicField.ID);
                }
            }
        }

        internal async Task UpdateDynamicField(DynamicFieldDto field, int objectiveID, int parentID = -1)
        {
            var dbField = await context.DynamicFields.FindAsync((int)field.ID);
            if (dbField == null)
            {
                await AddDynamicFields(field, objectiveID, parentID);
            }
            else
            {
                dbField.Name = field.Name;
                dbField.Value = GetValue(field);

                if (field.Type == DynamicFieldType.OBJECT)
                {
                    var children = (field.Value as JArray).ToObject<ICollection<DynamicFieldDto>>();

                    foreach (var child in children)
                    {
                        await UpdateDynamicField(child, objectiveID, dbField.ID);
                    }
                }
            }
        }

        private string GetValue(DynamicFieldDto field)
        {
            return field.Type switch
            {
                DynamicFieldType.STRING
                    or DynamicFieldType.BOOL
                    or DynamicFieldType.INTEGER
                    or DynamicFieldType.FLOAT
                    or DynamicFieldType.DATE => field.Value.ToString(),
                DynamicFieldType.ENUM => (field.Value as JObject).ToObject<Enumeration>().Value.ID.ToString(),
                _ => null,
            };
        }
    }
}
