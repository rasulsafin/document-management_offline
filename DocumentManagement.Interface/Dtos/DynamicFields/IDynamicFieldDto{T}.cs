namespace MRS.DocumentManagement.Interface.Dtos
{
    /// <summary>
    /// Interface for Generic Dynamic Values in Objective.
    /// </summary>
    /// <typeparam name="T">Value.</typeparam>
    public interface IDynamicFieldDto<T> : IDynamicFieldDto
    {
        /// <summary>
        /// Value of the Dynamic Field.
        /// </summary>
        T Value { get; set; }
    }
}
