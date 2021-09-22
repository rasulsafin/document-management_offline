namespace Brio.Docs.Interface
{
    public struct RequestID
    {
        public RequestID(string id)
        {
            ID = id;
        }

        public string ID { get; set; }
    }
}
