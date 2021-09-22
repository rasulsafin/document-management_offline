namespace Brio.Docs.Client.Filters
{
    public class ObjectiveFilterParameters : PageParameters
    {
        public int? TypeId { get; set; }

        public string BimElementGuid { get; set; }

        public string BimElementParentName { get; set; }

        public string TitlePart { get; set; }

        public int? ExceptChildrenOf { get; set; }
    }
}
