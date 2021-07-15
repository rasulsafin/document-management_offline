namespace MRS.DocumentManagement.Connection.MrsPro.Interfaces
{
    /// <summary>
    /// 
    /// </summary>
    public interface IElementAttachment : IElement
    {
        public string OriginalFileName { get; set; }

        public string FileName { get; set; }

    }
}
