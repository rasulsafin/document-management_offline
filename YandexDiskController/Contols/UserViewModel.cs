using System;
using System.Collections.ObjectModel;
using MRS.DocumentManagement.Connection;
using MRS.DocumentManagement.Connection.YandexDisk;
using MRS.DocumentManagement.Interface.Dtos;
using MRS.DocumentManagement.Models;
using WPFStorage.Base;
using WPFStorage.Dialogs;

namespace MRS.DocumentManagement.Contols
{
    public class UserViewModel : BaseViewModel
    {
        private DiskManager yandex;
        private UserModel selectedUser;

        public ObservableCollection<UserModel> Users { get; set; } = ObjectModel.Users;
        public UserModel SelectedUser
        {
            get => selectedUser; set
        {
            selectedUser = value;
            OnPropertyChanged();
        }
        }
        public int NextId
        {
            get => Properties.Settings.Default.UserNextId;
            set
            {
                Properties.Settings.Default.UserNextId = value;
                Properties.Settings.Default.Save();
                OnPropertyChanged();
            }
        }

        public HCommand AddUserCommand { get; }
        public HCommand DelUserCommand { get; }
        public HCommand ZeroIdCommand { get; }
        public HCommand EditUserCommand { get; }
        public HCommand UpdateCommand { get; }

        public UserViewModel()
        {
            AddUserCommand = new HCommand(AddUser);
            DelUserCommand = new HCommand(DelUser);
            ZeroIdCommand = new HCommand(ZeroId);
            EditUserCommand = new HCommand(EditUser);
            UpdateCommand = new HCommand(Update);
            SelectedUser = new UserModel();
            Update();
        }

        private void Update()
        {
            ObjectModel.UpdateUsers();
        }

        private void EditUser()
        {
            if (SelectedUser != null)
            {
                ObjectModel.Synchronizer.Update(SelectedUser.dto.ID);
                ObjectModel.SaveUsers();
            }
        }

        private void ZeroId()
        {
            NextId = 1;
        }

        private void DelUser()
        {
            if (SelectedUser != null)
            {
                if (WinBox.ShowQuestion($"Удалить пользователя {SelectedUser.Name}?", "Удаление"))
                {
                    ObjectModel.Synchronizer.Update(SelectedUser.dto.ID);
                    Users.Remove(SelectedUser);
                    ObjectModel.SaveUsers();
                }
            }
        }

        private void AddUser()
        {
            if (SelectedUser != null)
            {
                UserModel model = new UserModel(SelectedUser.dto);
                model.ID = NextId++;
                Users.Add(model);
                ObjectModel.SaveUsers();
                ObjectModel.Synchronizer.Update(model.dto.ID);
            }
            else
            {
                UserModel model = new UserModel();
                model.ID = NextId++;
                model.Login = "newUser";
                model.Name = "Новый пользователь";
                Users.Add(model);
                ObjectModel.SaveUsers();
                ObjectModel.Synchronizer.Update(model.dto.ID);
            }
        }
    }
}