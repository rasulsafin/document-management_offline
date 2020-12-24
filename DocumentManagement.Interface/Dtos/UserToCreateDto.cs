namespace MRS.DocumentManagement.Interface.Dtos
{
    public struct UserToCreateDto
    {
        public string Login { get; }
        public string Password { get; }
        public string Name { get; }

        public UserToCreateDto(string login, string password, string name)
        {
            Login = login?.Trim();
            Password = password?.Trim();
            Name = name?.Trim();
        }
    }
}
