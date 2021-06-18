namespace MRS.DocumentManagement.Connection.MrsPro.Models
{
    /// <summary>
    /// MrsPro.Element that can be a DM.ObjectiveExternalDto.
    /// </summary>
    public interface IElement
    {
        /// <summary>
        /// Element id.
        /// Contains trueId and elementType ("trueId:type").
        /// Maps to ExternalID.
        /// </summary>
        string Id { get; set; }

        /// <summary>
        /// Path from parent entity to current element.
        /// </summary>
        string Ancestry { get; set; }

        /// <summary>
        /// Created date in Unix time format.
        /// Maps to CreationDate (DateTime).
        /// </summary>
        long CreatedDate { get; set; }

        /// <summary>
        /// Element type.
        /// Maps to DM.ObjectiveType.ExternalId.
        /// </summary>
        string Type { get; set; }

        /// <summary>
        /// Person who created this element.
        /// Maps to AuthorExternalId.
        /// </summary>
        string Owner { get; set; }

        /// <summary>
        /// Type of the parent entity.
        /// </summary>
        string ParentType { get; set; }

        /// <summary>
        /// Parent id.
        /// Maps to ParentObjectiveExternalID.
        /// </summary>
        string ParentId { get; set; }

        /// <summary>
        /// Name of the element.
        /// </summary>
        string Title { get; set; }

        /// <summary>
        /// Description.
        /// </summary>
        string Description { get; set; }

        /// <summary>
        /// Last modified date in Unix time format.
        /// </summary>
        long LastModifiedDate { get; set; }

        /// <summary>
        /// Due date in Unix time format.
        /// Maps to DueDate (DateTime).
        /// </summary>
        long DueDate { get; set; }

        /// <summary>
        /// State of the element.
        /// Maps to Status.
        /// </summary>
        string State { get; set; }
    }
}
