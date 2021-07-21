using System.Collections.Generic;

namespace MRS.DocumentManagement.Connection.MrsPro.Interfaces
{
    /// <summary>
    /// MrsPro.Element that can be a DM.ObjectiveExternalDto.
    /// </summary>
    public interface IElementObject : IElement
    {
        /// <summary>
        /// Created date in Unix time format.
        /// Maps to CreationDate (DateTime).
        /// </summary>
        long CreatedDate { get; set; }

        /// <summary>
        /// Person who created this element.
        /// Maps to AuthorExternalId.
        /// </summary>
        string Owner { get; set; }

        /// <summary>
        /// 
        /// </summary>
        bool HasAttachments { get; set; }

        /// <summary>
        /// List of files attached.
        /// </summary>
        IEnumerable<IElementAttachment> Attachments { get; set; }
    }
}
