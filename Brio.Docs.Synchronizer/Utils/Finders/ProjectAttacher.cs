using System.Threading.Tasks;
using Brio.Docs.Database;
using Brio.Docs.Database.Extensions;
using Brio.Docs.Database.Models;
using Brio.Docs.Synchronization.Extensions;
using Brio.Docs.Synchronization.Interfaces;
using Brio.Docs.Synchronization.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Brio.Docs.Synchronization.Utilities.Finders
{
    public class ProjectAttacher : IAttacher<Project>
    {
        private readonly DMContext context;
        private readonly ILogger<ProjectAttacher> logger;

        public ProjectAttacher(DMContext context, ILogger<ProjectAttacher> logger)
        {
            this.context = context;
            this.logger = logger;

            logger.LogTrace("ProjectAttacher created");
        }

        public async Task AttachExisting(SynchronizingTuple<Project> tuple)
        {
            var id = tuple.ExternalID;
            var needToAttach = !string.IsNullOrEmpty(id) && (tuple.Local == null || tuple.Synchronized == null);
            logger.LogStartAction(tuple, needToAttach ? LogLevel.Debug : LogLevel.Trace);

            if (!needToAttach)
                return;

            tuple.Local ??= await context.Projects
               .Unsynchronized()
               .FirstOrDefaultAsync(x => x.ExternalID == id)
               .ConfigureAwait(false);

            tuple.Synchronized ??= await context.Projects
               .Synchronized()
               .FirstOrDefaultAsync(x => x.ExternalID == id)
               .ConfigureAwait(false);

            logger.LogDebug(
                "AttachExisting project ends with tuple ({Local}, {Synchronized}, {Remote})",
                tuple.Local?.ID,
                tuple.Synchronized?.ID,
                tuple.ExternalID);
        }
    }
}
