using System;
using System.Diagnostics.CodeAnalysis;
using MRS.DocumentManagement.Interface.Dtos;

namespace MRS.DocumentManagement.Tests
{
    internal class DynamicFieldComparer : AbstractModelComparer<IDynamicFieldDto>
    {
        public DynamicFieldComparer(bool ignoreIDs)
            : base(ignoreIDs)
        {
        }

        public override bool NotNullEquals([DisallowNull] IDynamicFieldDto x, [DisallowNull] IDynamicFieldDto y)
        {
            var idMatched = IgnoreIDs ? true : x.ID == y.ID;
            var valueMatched = GetValue(x) == GetValue(y);

            return idMatched
                && valueMatched
                && x.Name == y.Name
                && x.Type == y.Type;
        }

        private dynamic GetValue(IDynamicFieldDto x)
        {
            switch (x.Type)
            {
                case DynamicFieldType.BOOL:
                    return ((IDynamicFieldDto<bool>)x).Value;
                case DynamicFieldType.STRING:
                    return ((IDynamicFieldDto<string>)x).Value;
                case DynamicFieldType.INTEGER:
                    return ((IDynamicFieldDto<int>)x).Value;
                case DynamicFieldType.FLOAT:
                    return ((IDynamicFieldDto<float>)x).Value;
                case DynamicFieldType.DATE:
                    return ((IDynamicFieldDto<DateTime>)x).Value;
                case DynamicFieldType.OBJECT:
                    return ((IDynamicFieldDto<IDynamicFieldDto>)x).Value;
                case DynamicFieldType.ENUM:
                    return ((IDynamicFieldDto<EnumerationValueDto>)x).Value;
                default:
                    return typeof(IDynamicFieldDto);
            }
        }
    }
}