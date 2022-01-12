using System.Linq;
using System.Threading.Tasks;
using Brio.Docs.Database;
using Microsoft.EntityFrameworkCore;

namespace Brio.Docs.Synchronization.Utils
{
    internal static class SynchronizationFinalizer
    {
        public static async Task Finalize(DMContext context)
            => await RemoveUnusedBimElements(context).ConfigureAwait(false);

        private static async Task RemoveUnusedBimElements(DMContext context)
        {
            var list = await context.BimElements
               .Include(x => x.Objectives)
               .Where(x => !x.Objectives.Any())
               .ToListAsync()
               .ConfigureAwait(false);
            context.BimElements.RemoveRange(list);
        }
    }
}
