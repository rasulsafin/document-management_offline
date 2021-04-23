using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MRS.DocumentManagement.Synchronization.Models;

namespace MRS.DocumentManagement.Synchronization.Utils
{
    internal static class SynchronizationFinalizer
    {
        public static async Task Finalize(SynchronizingData data)
            => await RemoveUnusedBimElements(data);

        private static async Task RemoveUnusedBimElements(SynchronizingData data)
        {
            var list = await data.Context.BimElements
               .Include(x => x.Objectives)
               .Where(x => !x.Objectives.Any())
               .ToListAsync();
            data.Context.BimElements.RemoveRange(list);
        }
    }
}
