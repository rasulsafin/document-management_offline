using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Brio.Docs.Connections.LementPro.Models
{
    [DataContract]
    public class ObjectBaseValue
    {
        [DataMember(Name = "type")]
        public LementProType Type { get; set; }

        [DataMember(Name = "lastModifiedDate")]
        public string LastModifiedDate { get; set; }

        [DataMember(Name = "closeDate")]
        public string CloseDate { get; set; }

        [DataMember(Name = "name")]
        public string Name { get; set; }

        [DataMember(Name = "description")]
        public string Description { get; set; }

        [DataMember(Name = "parentDocument")]
        public string ParentDocument { get; set; }

        [DataMember(Name = "parentTask")]
        public string ParentTask { get; set; }

        [DataMember(Name = "project")]
        public LementProProject Project { get; set; }

        [DataMember(Name = "id")]
        public int? ID { get; set; }

        [DataMember(Name = "MSProjectID")]
        public string MSProjectID { get; set; }

        [DataMember(Name = "MSProjectUniqueID")]
        public string MSProjectUniqueID { get; set; }

        [DataMember(Name = "BaselineStartDate")]
        public string BaselineStartDate { get; set; }

        [DataMember(Name = "BaselineEndDate")]
        public string BaselineEndDate { get; set; }

        [DataMember(Name = "Progress")]
        public string Progress { get; set; }

        [DataMember(Name = "bimRef")]
        public BimRef BimRef { get; set; }

        [DataMember(Name = "bimElementsState")]
        public string BimElementsState { get; set; }

        /// <summary>
        /// Custom creator field.
        /// </summary>
        [DataMember(Name = "i60099")]
        public dynamic I60099 { get; set; }

        /// <summary>
        /// Custom Bim elements text field.
        /// </summary>
        [DataMember(Name = "i66444")]
        public string I66444 { get; set; }

        /// <summary>
        /// Custom AuthorExternalId text field.
        /// </summary>
        [DataMember(Name = "i66474")]
        public string I66474 { get; set; }

        [DataMember(Name = "linkedDocuments")]
        public dynamic LinkedDocuments { get; set; }

        [DataMember(Name = "registeredDocuments")]
        public dynamic RegisteredDocuments { get; set; }

        [DataMember(Name = "subTasks")]
        public dynamic SubTasks { get; set; }

        [DataMember(Name = "completedSubTasks")]
        public dynamic CompletedSubTasks { get; set; }

        [DataMember(Name = "files")]
        public List<File> Files { get; set; }

        [DataMember(Name = "documentResolutionFiles")]
        public dynamic DocumentResolutionFiles { get; set; }

        [DataMember(Name = "creationDate")]
        public string CreationDate { get; set; }

        [DataMember(Name = "startDate")]
        public string StartDate { get; set; }

        [DataMember(Name = "endDate")]
        public string EndDate { get; set; }

        [DataMember(Name = "isRouteTask")]
        public bool? IsRouteTask { get; set; }

        [DataMember(Name = "isExpired")]
        public bool? IsExpired { get; set; }

        [DataMember(Name = "routeId")]
        public string RouteId { get; set; }

        [DataMember(Name = "isResolution")]
        public bool? IsResolution { get; set; }

        [DataMember(Name = "managers")]
        public UserShortInfo Managers { get; set; }

        [DataMember(Name = "controllers")]
        public List<UserShortInfo> Controllers { get; set; }

        [DataMember(Name = "executors")]
        public List<UserShortInfo> Executors { get; set; }

        [DataMember(Name = "bimVersionNum")]
        public int? BimVersionNum { get; set; }
    }
}
