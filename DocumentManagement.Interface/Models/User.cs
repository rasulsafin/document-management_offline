namespace MRS.DocumentManagement.Interface.Models
{
    public class User
    {
        public static readonly User Anonymous = new User(ID<User>.InvalidID, string.Empty, string.Empty);

        public ID<User> ID { get; }
        public string Login { get; }
        public string Name { get; }

        public User(ID<User> id, string login, string name)
        {
            ID = id;
            Login = login.Trim();
            Name = name.Trim();
        }
    }
}
