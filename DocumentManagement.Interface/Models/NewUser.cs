namespace DocumentManagement.Interface.Models
{
    public struct NewUser
    {
        public NewUser(string login, string password, string name)
        {
            Login = login;
            Password = password;
            Name = name;
        }

        public string Login { get; }
        public string Password { get; }
        public string Name { get; }
    }
}
