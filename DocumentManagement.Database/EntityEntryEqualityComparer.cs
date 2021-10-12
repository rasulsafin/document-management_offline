using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace MRS.DocumentManagement.Database
{
    public class EntityEntryEqualityComparer : IEqualityComparer<EntityEntry>
    {
        public bool Equals(EntityEntry x, EntityEntry y)
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

            return Equals(x.Entity, y.Entity);
        }

        public int GetHashCode(EntityEntry obj)
        {
            return obj.Entity != null ? obj.Entity.GetHashCode() : 0;
        }
    }
}
