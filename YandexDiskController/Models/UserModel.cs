using System;
using System.Xml.Serialization;
using MRS.DocumentManagement.Interface.Dtos;
using WPFStorage.Base;

namespace MRS.DocumentManagement.Models
{
    public class UserModel : BaseViewModel
    {
        [XmlIgnore]
        public UserDto dto;

        public UserModel(UserDto dto)
        {
            this.dto = dto;
        }
        public UserModel()
        {
            this.dto = new UserDto(ID<UserDto>.InvalidID, "", "");
        }

        public int ID
        {
            get => (int)dto.ID;
            set
            {
                dto= new UserDto(new ID<UserDto>(value), dto.Login, dto.Name);
                OnPropertyChanged();
            }
        }

        public string Login
        {
            get => dto.Login;
            set
            {
                dto = new UserDto(dto.ID, value, dto.Name);
                OnPropertyChanged();
            }
        }

        public string Name
        {
            get => dto.Name;
            set
            {
                dto = new UserDto(dto.ID, dto.Login, value);
                OnPropertyChanged();
            }
        }

        public string Role
        {
            get => dto.Role.Name;
        }

        public static explicit operator UserModel(UserDto ident) => new UserModel(ident);
    }
}
