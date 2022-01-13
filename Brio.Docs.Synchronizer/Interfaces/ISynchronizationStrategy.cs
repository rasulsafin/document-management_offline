using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Brio.Docs.Integration.Interfaces;
using Brio.Docs.Synchronization.Models;

namespace Brio.Docs.Synchronization.Interfaces
{
    /// <summary>
    /// The strategy to synchronize needed entity type.
    /// </summary>
    /// <typeparam name="TDB">The type of entities for synchronization.</typeparam>
    /// <typeparam name="TDto">The data transfer model type of entities.</typeparam>
    public interface ISynchronizationStrategy<TDB, in TDto>
        where TDB : class
    {
        /// <summary>
        /// Adds entity to local database. Saves synchronized state.
        /// </summary>
        /// <param name="tuple">The synchronization tuple.</param>
        /// <param name="data">Synchronization parameters.</param>
        /// <param name="token">The token to cancel operation.</param>
        /// <returns>The task of the operation with information about fails.</returns>
        Task<SynchronizingResult> AddToLocal(
            SynchronizingTuple<TDB> tuple,
            SynchronizingData data,
            CancellationToken token);

        /// <summary>
        /// Adds entity to remote connection. Saves synchronized state.
        /// </summary>
        /// <param name="tuple">The synchronization tuple.</param>
        /// <param name="data">Synchronization parameters.</param>
        /// <param name="connectionContext">The context of the remote connection.</param>
        /// <param name="token">The token to cancel operation.</param>
        /// <returns>The task of the operation with information about fails.</returns>
        Task<SynchronizingResult> AddToRemote(
            SynchronizingTuple<TDB> tuple,
            SynchronizingData data,
            IConnectionContext connectionContext,
            CancellationToken token);

        /// <summary>
        /// Maps remote DTOs to working type.
        /// </summary>
        /// <param name="externalDtos">External entities.</param>
        /// <returns>Working entities.</returns>
        IReadOnlyCollection<TDB> Map(IReadOnlyCollection<TDto> externalDtos);

        /// <summary>
        /// Merges remote and local entities. Saves synchronized state.
        /// </summary>
        /// <param name="tuple">The synchronization tuple.</param>
        /// <param name="data">Synchronization parameters.</param>
        /// <param name="connectionContext">The context of the remote connection.</param>
        /// <param name="token">The token to cancel operation.</param>
        /// <returns>The task of the operation with information about fails.</returns>
        Task<SynchronizingResult> Merge(
            SynchronizingTuple<TDB> tuple,
            SynchronizingData data,
            IConnectionContext connectionContext,
            CancellationToken token);


        /// <summary>
        /// Removes entity to local database. Removes synchronized state.
        /// </summary>
        /// <param name="enumeration">All unsorted entities.</param>
        /// <returns>The sorted entities.</returns>
        public IEnumerable<TDB> Order(IEnumerable<TDB> enumeration);

        /// <summary>
        /// Removes entity to local database. Removes synchronized state.
        /// </summary>
        /// <param name="tuple">The synchronization tuple.</param>
        /// <param name="token">The token to cancel operation.</param>
        /// <returns>The task of the operation with information about fails.</returns>
        Task<SynchronizingResult> RemoveFromLocal(
            SynchronizingTuple<TDB> tuple,
            CancellationToken token);

        /// <summary>
        /// Removes entity from remote connection. Removes synchronized state.
        /// </summary>
        /// <param name="tuple">The synchronization tuple.</param>
        /// <param name="connectionContext">The context of the remote connection.</param>
        /// <param name="token">The token to cancel operation.</param>
        /// <returns>The task of the operation with information about fails.</returns>
        Task<SynchronizingResult> RemoveFromRemote(
            SynchronizingTuple<TDB> tuple,
            IConnectionContext connectionContext,
            CancellationToken token);
    }
}
