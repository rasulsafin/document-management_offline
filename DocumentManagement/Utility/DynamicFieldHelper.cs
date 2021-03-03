using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using MRS.DocumentManagement.Database;
using MRS.DocumentManagement.Database.Models;
using MRS.DocumentManagement.Interface.Dtos;

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

        internal async Task<IDynamicFieldDto> BuildObjectDynamicField(DynamicField dynamicField)
        {
            if (dynamicField.Type == DynamicFieldType.OBJECT.ToString())
            {
                DynamicFieldDto objDynamicField = (DynamicFieldDto)mapper.Map<IDynamicFieldDto>(dynamicField);

                var children = context.DynamicFields.Where(x => x.ParentFieldID == dynamicField.ID);

                foreach (var child in children)
                {
                    var result = await BuildObjectDynamicField(child);

                    if (objDynamicField.Values == null)
                        objDynamicField.Values = new List<IDynamicFieldDto>();

                    objDynamicField.Values.Add(result);
                }

                return objDynamicField;
            }
            else if (dynamicField.Type == DynamicFieldType.ENUM.ToString())
            {
                // TODO: Test enum. Is this gonna work???
                EnumerationFieldDto enumDynamicField = (EnumerationFieldDto)mapper.Map<IDynamicFieldDto>(dynamicField);

                var enumValue = await context.EnumerationValues
                    .Include(x => x.EnumerationType)
                    .FirstOrDefaultAsync(x => x.ID == int.Parse(dynamicField.Value));
                enumDynamicField.EnumerationType = mapper.Map<EnumerationTypeDto>(enumValue.EnumerationType);
                enumDynamicField.Value = mapper.Map<EnumerationValueDto>(enumValue);

                return enumDynamicField;
            }

            return mapper.Map<IDynamicFieldDto>(dynamicField);
        }

        internal async Task AddDynamicFields(IDynamicFieldDto field, int objectiveID, int parentID = -1)
        {
            var dynamicField = mapper.Map<DynamicField>(field);

            if (parentID != -1)
                dynamicField.ParentFieldID = parentID;
            else
                dynamicField.ObjectiveID = objectiveID;

            await context.DynamicFields.AddAsync(dynamicField);
            await context.SaveChangesAsync();

            if (field is DynamicFieldDto dynamicFieldDto)
            {
                foreach (var childField in dynamicFieldDto.Values)
                {
                    await AddDynamicFields(childField, objectiveID, dynamicField.ID);
                }
            }
        }

        internal async Task UpdateDynamicField(IDynamicFieldDto field, int objectiveID, int parentID = -1)
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

                if (field is DynamicFieldDto dynamicFieldDto)
                {
                    foreach (var child in dynamicFieldDto.Values)
                    {
                        await UpdateDynamicField(child, objectiveID, dbField.ID);
                    }
                }

                // TODO: Enum Update?
            }
        }

        private string GetValue(IDynamicFieldDto field)
        {
            return field.Type switch
            {
                DynamicFieldType.STRING => (field as StringFieldDto).Value,
                DynamicFieldType.BOOL => (field as BoolFieldDto).Value.ToString(),
                DynamicFieldType.INTEGER => (field as IntFieldDto).Value.ToString(),
                DynamicFieldType.FLOAT => (field as FloatFieldDto).Value.ToString(),
                DynamicFieldType.DATE => (field as DateFieldDto).Value.ToString(),
                DynamicFieldType.ENUM => (field as EnumerationFieldDto).Value.ToString(),
                _ => null,
            };
        }
    }
}
