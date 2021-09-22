using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Brio.Docs.Client.Dtos;
using Brio.Docs.Common.Dtos;
using Brio.Docs.Database;
using Brio.Docs.Database.Models;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;

namespace Brio.Docs.Utility
{
    public class DynamicFieldHelper
    {
        private readonly DMContext context;
        private readonly IMapper mapper;
        private readonly ILogger<DynamicFieldHelper> logger;

        public DynamicFieldHelper(DMContext context, IMapper mapper, ILogger<DynamicFieldHelper> logger)
        {
            this.context = context;
            this.mapper = mapper;
            this.logger = logger;
            logger.LogTrace("DynamicFieldHelper created");
        }

        internal async Task<DynamicFieldDto> BuildObjectDynamicField(DynamicField dynamicField)
        {
            logger.LogTrace("BuildObjectDynamicField started with dynamicField: {@DynamicField}", dynamicField);
            if (dynamicField.Type == DynamicFieldType.OBJECT.ToString())
            {
                DynamicFieldDto objDynamicField = mapper.Map<DynamicFieldDto>(dynamicField);
                logger.LogDebug("Mapped dynamic field: {@DynamicField}", objDynamicField);

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
            logger.LogTrace(
                "AddDynamicFields started with field: {@DynamicField}, objectiveID: {ObjectiveID}, parentID: {ParentID}",
                field,
                objectiveID,
                parentID);
            var dynamicField = mapper.Map<DynamicField>(field);
            logger.LogDebug("Mapped dynamic field: {@DynamicField}", dynamicField);

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
            logger.LogTrace(
                "UpdateDynamicField started with field: {@DynamicField}, objectiveID: {ObjectiveID}, parentID: {ParentID}",
                field,
                objectiveID,
                parentID);
            var dbField = await context.DynamicFields.FindAsync((int)field.ID);
            logger.LogDebug("Found dynamic field: {@DynamicField}", dbField);
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
            logger.LogTrace("GetValue started with field: {@DynamicField}", field);
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
