using System;
using System.Collections.Generic;
using Brio.Docs.Connections.Bim360.Synchronization.Models;

namespace Brio.Docs.Connections.Bim360.Synchronization.Utilities
{
    internal class LinkedInfoComparer : IEqualityComparer<LinkedInfo>
    {
        public bool Equals(LinkedInfo x, LinkedInfo y)
        {
            if (ReferenceEquals(x, y))
            {
                return true;
            }

            if (ReferenceEquals(x, null))
            {
                return false;
            }

            if (ReferenceEquals(y, null))
            {
                return false;
            }

            if (x.GetType() != y.GetType())
            {
                return false;
            }

            return x.Urn == y.Urn && x.Version == y.Version && x.Offset.Equals(y.Offset);
        }

        public int GetHashCode(LinkedInfo obj)
        {
            return HashCode.Combine(obj.Urn, obj.Version, obj.Offset);
        }
    }
}
