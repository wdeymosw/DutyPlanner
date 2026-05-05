using DutyPlanner.Presentation.Commands;
using DutyPlanner.Models;
using DutyPlanner.Services;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace DutyPlanner.Presentation.ViewModels
{
    public class SidebarUsersViewModel : ViewModel
    {

        private readonly IUserService _userService;
        private readonly IDialogService _dialogService;

        public ObservableCollection<UserViewModel> Users { get; }

        #region NewUserName
        private string _newUserName = string.Empty;
        public string NewUserName
        {
            get => _newUserName;
            set
            {
                Set(ref _newUserName, value);
                AddUserCommand.RaiseCanExecuteChanged();
            }
        }
        #endregion

        #region SelectedUser
        private UserViewModel? _selectedUser;
        public UserViewModel? SelectedUser
        {
            get => _selectedUser;
            set
            {
                if (_selectedUser != null && _selectedUser.IsEditing)
                    EndEdit(_selectedUser);

                Set(ref _selectedUser, value);
            }
        }
        #endregion


        #region Commands
        public LambdaCommand AddUserCommand { get; }

        #endregion


        #region Constructor

        public SidebarUsersViewModel(IUserService userService, IDialogService dialogService)
        {
            _userService = userService;
            _dialogService = dialogService;

            Users = new ObservableCollection<UserViewModel>(
                _userService.GetAll()
                .Select(CreateUserVm)
            );

            // Команды
            AddUserCommand = new LambdaCommand(
                AddUser, () => !string.IsNullOrWhiteSpace(NewUserName));

        }
        #endregion


        #region Methods
        private void AddUser()
        {
            if (string.IsNullOrWhiteSpace(NewUserName)) return;

            var user = _userService.Add(NewUserName, 5);
            Users.Add(CreateUserVm(user));
            NewUserName = string.Empty;
        }


        private UserViewModel CreateUserVm(User user)
        => new UserViewModel(
            user.Id,
            user.Name,
            user.Hours,
            BeginEdit,
            EndEdit,
            RemoveUser,
            showReport: u => _dialogService.ShowUserPeriodReport(u.Id, u.Name)
        );

        private void RemoveUser(UserViewModel user)
        {
           
            
            _userService.Remove(user.Id);
            Users.Remove(user);


        }

        private void BeginEdit(UserViewModel user)
        {
            foreach (var u in Users)
                u.IsEditing = false;

            user.IsEditing = true;
        }


        public void EndEdit(UserViewModel user)
        {
            user.IsEditing = false;
            _userService.Update(new User(user.Id, user.Name, user.Hours));
        }
        #endregion
    }
}
