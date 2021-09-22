using Brio.Docs.Interface.Dtos;

namespace Brio.Docs.Interface
{
    /// <summary>
    /// Context of the working with external connection.
    /// </summary>
    public interface IConnectionContext
    {
        /// <summary>
        /// Gets synchronizer for projects.
        /// </summary>
        ISynchronizer<ProjectExternalDto> ProjectsSynchronizer { get; }

        /// <summary>
        /// Gets synchronizer for objectives.
        /// </summary>
        ISynchronizer<ObjectiveExternalDto> ObjectivesSynchronizer { get; }
    }
}
