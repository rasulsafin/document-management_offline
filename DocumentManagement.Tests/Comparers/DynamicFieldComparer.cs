﻿using System;
using System.Diagnostics.CodeAnalysis;
using MRS.DocumentManagement.Interface.Dtos;

namespace MRS.DocumentManagement.Tests
{
    internal class DynamicFieldComparer : AbstractModelComparer<DynamicFieldDto>
    {
        public DynamicFieldComparer(bool ignoreIDs)
            : base(ignoreIDs)
        {
        }

        public override bool NotNullEquals([DisallowNull] DynamicFieldDto x, [DisallowNull] DynamicFieldDto y)
        {
            var idMatched = IgnoreIDs ? true : x.ID == y.ID;
            var valueMatched = x.Value == y.Value && x.Value.GetType() == y.Value.GetType();

            return idMatched
                && valueMatched
                && x.Name == y.Name
                && x.Type == y.Type;
        }
    }
}