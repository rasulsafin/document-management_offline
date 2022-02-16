using System.Collections.Generic;

namespace Brio.Docs.Client.Filters
{
    public class ObjectiveFilterParameters : PageParameters
    {
        public List<int> TypeIds { get; set; }

        public string BimElementGuid { get; set; }

        public string TitlePart { get; set; }

        public int? ExceptChildrenOf { get; set; }

        public List<int> Statuses { get; set; }

        public int? DateSortStatus { get; set; }

        public DateTime DateFrom { get; set; }

        public DateTime DateTo { get; set; }
    }
}
