namespace Brio.Docs.Client.Sorts
{
    public class ObjectiveSortParameters
    {
        public bool IsReverse;

        public Sorts Sort;

        public enum Sorts
        {
            ByTitle,
            ByCreateDate,
            ByFixDate,
            ByEditDate,
        }
    }
}
