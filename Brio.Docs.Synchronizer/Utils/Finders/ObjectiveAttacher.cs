using System.Threading.Tasks;
using Brio.Docs.Database;
using Brio.Docs.Database.Extensions;
using Brio.Docs.Database.Models;
using Brio.Docs.Synchronization.Extensions;
using Brio.Docs.Synchronization.Interfaces;
using Brio.Docs.Synchronization.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Linq;

namespace Brio.Docs.Synchronization.Utilities.Finders
{
    public class ObjectiveAttacher : IAttacher<Objective>
    {
        private readonly DMContext context;
        private readonly ILogger<ObjectiveAttacher> logger;

        public ObjectiveAttacher(DMContext context, ILogger<ObjectiveAttacher> logger)
        {
            this.context = context;
            this.logger = logger;

            logger.LogTrace("ObjectiveAttacher created");
        }

        public async Task AttachExisting(SynchronizingTuple<Objective> tuple)
        {
            var id = tuple.ExternalID;
            var needToAttach = !string.IsNullOrEmpty(id) && (tuple.Local == null || tuple.Synchronized == null);
            logger.LogStartAction(tuple, needToAttach ? LogLevel.Debug : LogLevel.Trace);

            if (!needToAttach)
                return;

            var (localProject, syncProject) = await SearchingUtilities
               .GetProjectsByRemote(context, tuple.Remote.ProjectID)
               .ConfigureAwait(false);

            tuple.Local ??= await context.Objectives
               .Unsynchronized()
               .Where(x => x.Project == localProject)
               .FirstOrDefaultAsync(x => x.ExternalID == id)
               .ConfigureAwait(false);

            tuple.Synchronized ??= await context.Objectives
               .Synchronized()
               .Where(x => x.Project == syncProject)
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
