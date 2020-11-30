namespace MRS.DocumentManagement.Interface.Dtos
{
    public struct UserToCreateDto
    {
        public UserToCreateDto(string login, string password, string name)
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
