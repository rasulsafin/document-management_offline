namespace MRS.DocumentManagement.Interface.Dtos
{
    public class UserDto
    {
        public static readonly UserDto Anonymous = new UserDto(ID<UserDto>.InvalidID, string.Empty, string.Empty);

        public ID<UserDto> ID { get; }
        public string Login { get; }
        public string Name { get; }

        public UserDto(ID<UserDto> id, string login, string name)
        {
            ID = id;
            Login = login.Trim();
            Name = name.Trim();
        }
    }
}
