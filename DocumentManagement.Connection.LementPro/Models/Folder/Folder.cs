using System.Runtime.Serialization;

namespace Brio.Docs.Connections.LementPro.Models
{
    [DataContract]
    public class Folder
    {
        // FolderKey class is not used because data presented as string in the field
        [DataMember(Name = "folderKey")]
        public string FolderKey { get; set; }

        [DataMember(Name = "hasGroupings")]
        public bool? HasGroupings { get; set; }

        [DataMember(Name = "hasSubFolders")]
        public bool? HasSubFolders { get; set; }

        [DataMember(Name = "isEditable")]
        public bool? IsEditable { get; set; }

        [DataMember(Name = "isGrouping")]
        public bool? IsGrouping { get; set; }

        [DataMember(Name = "isSelectable")]
        public bool? IsSelectable { get; set; }

        [DataMember(Name = "isSmart")]
        public bool? IsSmart { get; set; }

        [DataMember(Name = "isSystem")]
        public bool? IsSystem { get; set; }

        [DataMember(Name = "showObjectsCount")]
        public bool? ShowObjectsCount { get; set; }

        [DataMember(Name = "sortWeight")]
        public double? SortWeight { get; set; }

        [DataMember(Name = "text")]
        public string Text { get; set; }

        [DataMember(Name = "viewType")]
        public int? ViewType { get; set; }

        [DataMember(Name = "parentId")]
        public string ParentId { get; set; }

        [DataMember(Name = "hasExportTemplate")]
        public bool? HasExportTemplate { get; set; }
    }
}
