namespace Brio.Docs.Connections.MrsPro.Interfaces
{
    /// <summary>
    /// File of the element.
    /// </summary>
    public interface IElementAttachment : IElement
    {
        /// <summary>
        /// Date the element was created.
        /// </summary>
        public long CreatedDate { get; set; }

        /// <summary>
        /// Original name of the file.
        /// </summary>
        public string OriginalFileName { get; set; }

        /// <summary>
        /// Current file name.
        /// </summary>
        public string FileName { get; set; }
    }
}
