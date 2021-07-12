namespace MRS.DocumentManagement.Connection.MrsPro.Services
{
    public class Service
    {
        public Service(MrsProHttpConnection connection)
        {
            HttpConnection = connection;
        }

        protected MrsProHttpConnection HttpConnection { get; }
    }
}
