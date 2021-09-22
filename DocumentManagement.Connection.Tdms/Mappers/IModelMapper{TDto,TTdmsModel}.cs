namespace Brio.Docs.Connection.Tdms.Mappers
{
    /// <summary>
    /// Interface for object mapping.
    /// </summary>
    /// <typeparam name="TDto">DtoObject type.</typeparam>
    /// <typeparam name="TTdmsModel">Model type.</typeparam>
    public interface IModelMapper<TDto, TTdmsModel>
    {
        /// <summary>
        /// Maps TdmsObject to DtoObject.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="tdmsObject">Object from tdms.</param>
        /// <returns>DtoObject.</returns>
        TDto ToDto(TTdmsModel tdmsObject);

        /// <summary>
        /// Maps DtoObject back to TdmsObject.
        /// </summary>
        /// <param name="objectDto">DtoObject.</param>
        /// <param name="model">Destination model.</param>
        /// <returns>Object from tdms.</returns>
        TTdmsModel ToModel(TDto objectDto, TTdmsModel model);
    }
}
