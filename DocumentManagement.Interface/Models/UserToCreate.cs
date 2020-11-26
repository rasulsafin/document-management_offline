namespace MRS.DocumentManagement.Interface.Models
{
    public struct UserToCreate
    {
        public UserToCreate(string login, string password, string name)
        {
            Login = login?.Trim();
            Password = password?.Trim();
            Name = name?.Trim();
        }

        public string Login { get; }
        public string Password { get; }
        public string Name { get; }
    }
}
