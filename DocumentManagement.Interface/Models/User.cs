namespace DocumentManagement.Interface.Models
{
    public class User
    {
        public ID<User> ID { get; set; }
        public string Login { get; set; }
        public string Name { get; set; }
    }
}
