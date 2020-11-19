using System.Collections.Generic;
using DocumentManagement.Interface.Models;
using System.Diagnostics.CodeAnalysis;

namespace DocumentManagement.Tests
{
    internal class ProjectComparer : AbstractModelComparer<Project>
    {
        public ProjectComparer(bool ignoreIDs = false) : base(ignoreIDs)
        {
        }

        public override bool NotNullEquals([DisallowNull] Project x, [DisallowNull] Project y)
        {
            var dataEquals = x.Title == y.Title;
            if (!IgnoreIDs)
                dataEquals = dataEquals && x.ID == y.ID;
            return dataEquals;
        }
    }
}
