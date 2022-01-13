using System.Collections.Generic;

namespace Brio.Docs.Synchronization.Interfaces
{
    /// <summary>
    /// Represents tool for map remote collections.
    /// </summary>
    /// <typeparam name="TDB">The type of entities for synchronization.</typeparam>
    /// <typeparam name="TDto">The data transfer model type of entities.</typeparam>
    public interface IMapper<TDB, TDto>
    {
        /// <summary>
        /// Maps remote DTOs to working type.
        /// </summary>
        /// <param name="externalDtos">External entities.</param>
        /// <returns>Working entities.</returns>
        IReadOnlyCollection<TDB> Map(IReadOnlyCollection<TDto> externalDtos);
    }
}
