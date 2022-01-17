using Brio.Docs.Synchronization.Models;
using Microsoft.EntityFrameworkCore;

namespace Brio.Docs.Synchronization.Utils
{
    public static class DBContextUtilities
    {
        public static void ReloadContext(DbContext context, SynchronizingData data)
        {
            context.ChangeTracker.Clear();
            context.Attach(data.User);
        }
    }
}
