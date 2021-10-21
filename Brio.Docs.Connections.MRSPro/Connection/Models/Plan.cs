using System.Runtime.Serialization;
using Brio.Docs.Connections.MrsPro.Interfaces;

namespace Brio.Docs.Connections.MrsPro.Models
{
    [DataContract]
    public class Plan : IElementAttachment
    {
        [DataMember(Name = "ancestry")]
        public string Ancestry { get; set; }

        [DataMember(Name = "archiveInitiator")]
        public string ArchiveInitiator { get; set; }

        [DataMember(Name = "archivedDate")]
        public long ArchivedDate { get; set; }

        [DataMember(Name = "createdDate")]
        public long CreatedDate { get; set; }

        [DataMember(Name = "genplan")]
        public bool Genplan { get; set; }

        [DataMember(Name = "id")]
        public string Id { get; set; }

        [DataMember(Name = "name")]
        public string Name { get; set; }

        [DataMember(Name = "originalFileName")]
        public string OriginalFileName { get; set; }

        [DataMember(Name = "owner")]
        public string Owner { get; set; }

        [DataMember(Name = "parentId")]
        public string ParentId { get; set; }

        [DataMember(Name = "parentType")]
        public string ParentType { get; set; }

        [DataMember(Name = "restoredDate")]
        public long RestoredDate { get; set; }

        [DataMember(Name = "sheets")]
        public Sheet[] Sheets { get; set; }

        [DataMember(Name = "status")]
        public string Status { get; set; }

        [DataMember(Name = "type")]
        public string Type { get; set; }

        [DataMember(Name = "version")]
        public string Version { get; set; }

        public string FileName { get => Name; set => Name = value; }
    }

    [DataContract]
    public class Sheet
    {
        [DataMember(Name = "ancestry")]
        public string Ancestry { get; set; }

        [DataMember(Name = "archiveInitiator")]
        public string ArchiveInitiator { get; set; }

        [DataMember(Name = "archivedDate")]
        public long ArchivedDate { get; set; }

        [DataMember(Name = "createdDate")]
        public long CreatedDate { get; set; }

        [DataMember(Name = "error")]
        public string Error { get; set; }

        [DataMember(Name = "id")]
        public string Id { get; set; }

        [DataMember(Name = "name")]
        public string Name { get; set; }

        [DataMember(Name = "options")]
        public Options Options { get; set; }

        [DataMember(Name = "owner")]
        public string Owner { get; set; }

        [DataMember(Name = "parentId")]
        public string ParentId { get; set; }

        [DataMember(Name = "parentType")]
        public string ParentType { get; set; }

        [DataMember(Name = "position")]
        public int Position { get; set; }

        [DataMember(Name = "restoredDate")]
        public long RestoredDate { get; set; }

        [DataMember(Name = "sheetIdForVersion")]
        public string SheetIdForVersion { get; set; }

        [DataMember(Name = "size")]
        public Size Size { get; set; }

        [DataMember(Name = "status")]
        public string Status { get; set; }

        [DataMember(Name = "type")]
        public string Type { get; set; }

        [DataMember(Name = "urlConverted")]
        public string UrlConverted { get; set; }

        [DataMember(Name = "version")]
        public string Version { get; set; }
    }

    [DataContract]
    public class Options
    {
        [DataMember(Name = "displayUnits")]
        public string DisplayUnits { get; set; }

        [DataMember(Name = "planUnits")]
        public string PlanUnits { get; set; }

        [DataMember(Name = "scale")]
        public int Scale { get; set; }
    }

    [DataContract]
    public class Size
    {
        [DataMember(Name = "height")]
        public int Height { get; set; }

        [DataMember(Name = "width")]
        public int Width { get; set; }
    }
}
