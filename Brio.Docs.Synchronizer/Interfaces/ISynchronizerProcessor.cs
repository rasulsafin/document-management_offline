using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Brio.Docs.Database;
using Brio.Docs.Synchronization.Models;

namespace Brio.Docs.Synchronization.Interfaces
{
    /// <summary>
    /// Represents tool to synchronize data.
    /// </summary>
    public interface ISynchronizerProcessor
    {
        /// <summary>
        /// Synchronizes all filtered data with each other.
        /// </summary>
        /// <param name="strategy">Strategy for performing actions on each synchronization tuple.</param>
        /// <param name="data">Synchronization parameters.</param>
        /// <param name="remoteCollection">The collection of external entities to be synchronized.</param>
        /// <param name="filter">The filter for entities.</param>
        /// <param name="token">The token to cancel operation.</param>
        /// <param name="progress">The progress of the operation.</param>
        /// <returns>The task of the operation with collection of failed information.</returns>
        /// <typeparam name="TDB">The type of entities for synchronization.</typeparam>
        /// <typeparam name="TDto">The data transfer model type of entities.</typeparam>
        Task<List<SynchronizingResult>> Synchronize<TDB, TDto>(
            ISynchronizationStrategy<TDB> strategy,
            SynchronizingData data,
            IEnumerable<TDB> remoteCollection,
            Expression<Func<TDB, bool>> filter,
            CancellationToken token,
            IProgress<double> progress = null)
            where TDB : class, ISynchronizable<TDB>, new();
    }
}
