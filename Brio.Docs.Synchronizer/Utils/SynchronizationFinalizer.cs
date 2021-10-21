using System.Linq;
using System.Threading.Tasks;
using Brio.Docs.Database;
using Microsoft.EntityFrameworkCore;

namespace Brio.Docs.Synchronization.Utils
{
    internal static class SynchronizationFinalizer
    {
        public static async Task Finalize(DMContext context)
            => await RemoveUnusedBimElements(context);

        private static async Task RemoveUnusedBimElements(DMContext context)
        {
            var list = await context.BimElements
               .Include(x => x.Objectives)
               .Where(x => !x.Objectives.Any())
               .ToListAsync();
            context.BimElements.RemoveRange(list);
        }
    }
}
