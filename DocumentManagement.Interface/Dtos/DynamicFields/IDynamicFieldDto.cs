namespace MRS.DocumentManagement.Interface.Dtos
{
    /// <summary>
    /// Interface for Dynamic Values in Objective.
    /// </summary>
    public interface IDynamicFieldDto
    {
        /// <summary>
        /// ID of the object.
        /// </summary>
        ID<IDynamicFieldDto> ID { get; set; }

        /// <summary>
        /// Name to be displayed.
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// Type for current implementation of DynamicField.
        /// </summary>
        DynamicFieldType Type { get; }
    }
}
