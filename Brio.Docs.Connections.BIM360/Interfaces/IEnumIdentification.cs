namespace Brio.Docs.Connections.Bim360.Interfaces
{
    /// <summary>
    /// Represents a class for a getting info about the enumeration.
    /// </summary>
    public interface IEnumIdentification
    {
        /// <summary>
        /// Gets whether the field can contain null value.
        /// </summary>
        bool CanBeNull { get; }

        /// <summary>
        /// Represents the identifier for current enum.
        /// </summary>
        string EnumExternalID { get; }

        /// <summary>
        /// Represents the display name of this property.
        /// </summary>
        string EnumDisplayName { get; }

        /// <summary>
        /// Gets identifier for a null value for the current field.
        /// </summary>
        string NullID { get; }
    }
}
