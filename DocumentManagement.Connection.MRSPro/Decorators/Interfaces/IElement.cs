namespace Brio.Docs.Connection.MrsPro.Interfaces
{
    /// <summary>
    /// Element from MRSPro.
    /// </summary>
    public interface IElement
    {
        /// <summary>
        /// Element's id.
        /// </summary>
        string Id { get; set; }

        /// <summary>
        /// Element's path from first parent to last.
        /// </summary>
        string Ancestry { get; set; }

        /// <summary>
        /// Element's type.
        /// </summary>
        string Type { get; set; }

        /// <summary>
        /// Parent type of the current element.
        /// </summary>
        string ParentType { get; set; }

        /// <summary>
        /// Parent's id.
        /// </summary>
        string ParentId { get; set; }
    }
}
