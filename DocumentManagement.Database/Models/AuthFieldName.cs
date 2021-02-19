namespace MRS.DocumentManagement.Database.Models
{
    public class AuthFieldName
    {
        public int ID { get; set; }

        public string Name { get; set; }

        public int ConnectionTypeID { get; set; }

        public ConnectionType ConnectionType { get; set; }
    }
}
