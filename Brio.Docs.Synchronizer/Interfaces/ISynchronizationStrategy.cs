using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Brio.Docs.Integration.Client;
using Brio.Docs.Synchronization.Models;

namespace Brio.Docs.Synchronization.Interfaces
{
    /// <summary>
    /// The strategy to synchronize needed entity type.
    /// </summary>
    /// <typeparam name="TDB">The type of entities for synchronization.</typeparam>
    /// <typeparam name="TDto">The data transfer model type of entities.</typeparam>
    public interface ISynchronizationStrategy<TDB, in TDto>
    {
        /// <summary>
        /// Synchronizes all filtered data with each other.
        /// </summary>
        /// <param name="data">Synchronization parameters.</param>
        /// <param name="connectionContext">The context for a working with external services.</param>
        /// <param name="remoteCollection">The collection of external entities to be synchronized.</param>
        /// <param name="token">The token to cancel operation.</param>
        /// <param name="dbFilter">The filter for local and synchronized entities.</param>
        /// <param name="remoteFilter">The filter for external entities.</param>
        /// <param name="parent">The parent of synchronizing collection.</param>
        /// <param name="progress">The progress of the operation.</param>
        /// <returns>The task of the operation with collection of failed information.</returns>
        Task<List<SynchronizingResult>> Synchronize(
            SynchronizingData data,
            IConnectionContext connectionContext,
            IEnumerable<TDB> remoteCollection,
            CancellationToken token,
            Expression<Func<TDB, bool>> dbFilter = null,
            Func<TDB, bool> remoteFilter = null,
            object parent = null,
            IProgress<double> progress = null);

        /// <summary>
        /// Maps external entities to the local model.
        /// </summary>
        /// <param name="externalDtos">The collection of external entities.</param>
        /// <returns>The mapped collection.</returns>
        IReadOnlyCollection<TDB> Map(IReadOnlyCollection<TDto> externalDtos);
    }
}
