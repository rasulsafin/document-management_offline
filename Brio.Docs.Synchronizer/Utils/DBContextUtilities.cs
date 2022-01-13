using Microsoft.EntityFrameworkCore;

namespace Brio.Docs.Synchronization.Utils
{
    public static class DBContextUtilities
    {
        public static void ReloadContext(DbContext context)
        {
            context.ChangeTracker.Clear();
        }
    }
}
