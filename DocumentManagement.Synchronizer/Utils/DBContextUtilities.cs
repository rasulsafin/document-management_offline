using Microsoft.EntityFrameworkCore;

namespace MRS.DocumentManagement.Synchronization.Utils
{
    public static class DBContextUtilities
    {
        public static void ReloadContext(DbContext context)
        {
            foreach (var entry in context.ChangeTracker.Entries())
            {
                if (entry.State == EntityState.Added)
                    entry.State = EntityState.Detached;

                entry.Reload();
            }
        }
    }
}
