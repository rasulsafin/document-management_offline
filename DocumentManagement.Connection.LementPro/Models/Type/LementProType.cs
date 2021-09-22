using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Brio.Docs.Connection.LementPro.Models
{
    [DataContract]
    public class LementProType
    {
        [DataMember(Name = "id")]
        public int? ID { get; set; }

        [DataMember(Name = "text")]
        public string Text { get; set; }

        [DataMember(Name = "name")]
        public string Name { get; set; }

        [DataMember(Name = "fullName")]
        public string FullName { get; set; }

        [DataMember(Name = "code")]
        public string Code { get; set; }

        [DataMember(Name = "sortWeight")]
        public double? SortWeight { get; set; }

        [DataMember(Name = "isAbstract")]
        public bool? IsAbstract { get; set; }

        [DataMember(Name = "isCategory")]
        public bool? IsCategory { get; set; }

        [DataMember(Name = "needStartRouteAfterCreateObject")]
        public bool? NeedStartRouteAfterCreateObject { get; set; }

        [DataMember(Name = "canExistOnlyAsPartOfOtherObject")]
        public bool? CanExistOnlyAsPartOfOtherObject { get; set; }

        [DataMember(Name = "routeSchema")]
        public string RouteSchema { get; set; }

        [DataMember(Name = "routeGlobalVariable")]
        public string RouteGlobalVariable { get; set; }

        [DataMember(Name = "rootType")]
        public string RootType { get; set; }

        [DataMember(Name = "parentId")]
        public string ParentId { get; set; }

        [DataMember(Name = "level")]
        public int? Level { get; set; }

        [DataMember(Name = "defaultTab")]
        public int? DefaultTab { get; set; }

        [DataMember(Name = "defaultTabAttributeDefId")]
        public string DefaultTabAttributeDefId { get; set; }

        [DataMember(Name = "isCreatable")]
        public bool? IsCreatable { get; set; }

        [DataMember(Name = "isBimIntegrated")]
        public bool? IsBimIntegrated { get; set; }

        [DataMember(Name = "hasSubTypes")]
        public bool? HasSubTypes { get; set; }

        [DataMember(Name = "counterValue")]
        public int? CounterValue { get; set; }

        [DataMember(Name = "items")]
        public List<LementProType> Items { get; set; }

        [DataMember(Name = "categoryId")]
        public int? CategoryId { get; set; }

        [DataMember(Name = "showActionsPage")]
        public bool? ShowActionsPage { get; set; }
    }
}
