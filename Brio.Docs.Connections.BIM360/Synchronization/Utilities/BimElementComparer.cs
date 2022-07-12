using System;
using System.Collections.Generic;
using Brio.Docs.Integration.Dtos;

namespace Brio.Docs.Connections.Bim360.Synchronization.Utilities
{
    internal class BimElementComparer : IEqualityComparer<BimElementExternalDto>
    {
        public bool Equals(BimElementExternalDto x, BimElementExternalDto y)
        {
            if (ReferenceEquals(x, y))
                return true;

            if (ReferenceEquals(x, null))
                return false;

            if (ReferenceEquals(y, null))
                return false;

            return string.Equals(x.GlobalID, y.GlobalID, StringComparison.InvariantCulture) &&
                string.Equals(x.ParentName, y.ParentName, StringComparison.InvariantCulture);
        }

        public int GetHashCode(BimElementExternalDto obj)
        {
            HashCode hashCode = new ();
            hashCode.Add(obj.GlobalID, StringComparer.InvariantCulture);
            hashCode.Add(obj.ParentName, StringComparer.InvariantCulture);
            return hashCode.ToHashCode();
        }
    }
}
