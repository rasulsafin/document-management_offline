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

                    if (objDynamicField.Value == null)
                        objDynamicField.Value = new List<IDynamicFieldDto>();

                    objDynamicField.Value.Add(result);
                }

                return objDynamicField;
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
                foreach (var childField in dynamicFieldDto.Value)
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
                    foreach (var child in dynamicFieldDto.Value)
                    {
                        await UpdateDynamicField(child, objectiveID, dbField.ID);
                    }
                }
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
                DynamicFieldType.ENUM => (field as EnumerationFieldDto).Value.ID.ToString(),
                _ => null,
            };
        }
    }
}
