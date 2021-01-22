using System;
using System.Collections.ObjectModel;
using System.IO;
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

        public HCommand SampleCommand { get; }

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
            SampleCommand = new HCommand(CreateSampleUser);
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
                    ObjectModel.Synchronizer.Delete(SelectedUser.dto.ID);
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
                UserModel model = CreateUser("newUser", "Новый пользователь");
                Users.Add(model);

                ObjectModel.SaveUsers();
                ObjectModel.Synchronizer.Update(model.dto.ID);
            }
        }

        private UserModel CreateUser(string login, string name)
        {
            UserModel model = new UserModel();
            model.ID = NextId++;
            model.Login = login;
            model.Name = name;
            return model;
        }

        private void CreateSampleUser()
        {
            string[] names;
            string[] logins;

            string loginsFile = "logins.txt";
            string namesFile = "namesUser.txt";
            if (!File.Exists(loginsFile))
            {
                if (WinBox.ShowQuestion($"Файл {loginsFile} не существует создать его?"))
                {
                    OpenHelper.Geany(loginsFile);
                }

                return;
            }
            else if (!File.Exists(namesFile))
            {
                if (WinBox.ShowQuestion($"Файл {namesFile} не существует создать его?"))
                {
                    OpenHelper.Geany(namesFile);
                }

                return;
            }
            else
            {
                names = File.ReadAllLines(namesFile);
                logins = File.ReadAllLines(loginsFile);

                Random random = new Random();
                int index1 = random.Next(0, logins.Length);
                int index2 = random.Next(0, names.Length);
                UserModel user = CreateUser(logins[index1], names[index2]);

                Users.Add(user);
                ObjectModel.SaveUsers();
                ObjectModel.Synchronizer.Update(SelectedUser.dto.ID);
            }
        }
    }
}
