using DutyPlanner.Infrastrustures;
using DutyPlanner.Models;
using System.Collections.ObjectModel;
using System.IO;
using System.Text.Json;

namespace DutyPlanner.Presentation.ViewModels
{
    public class DayViewModel : ViewModel
    {
        public string FilePath { get; }
        public DateTime Date { get; }


        /// <summary>
        /// single source colection of user data storage
        /// </summary>
        private readonly List<DayUserViewModel> _users = new();

        /// <summary>
        /// Projection for storing active users
        /// </summary>
        public ObservableCollection<DayUserViewModel> ActiveUsers { get; } = new();

        /// <summary>
        /// Projection for storing reserve users
        /// </summary>
        public ObservableCollection<DayUserViewModel> ReserveUsers { get; } = new();

        #region Commands
        /// <summary>
        /// command for remove users
        /// </summary>
        public LambdaCommand<DayUserViewModel> RemoveUserCommand { get; }
        // public LambdaCommand<object> DropCommand { get; }



        #endregion

        /// <summary>
        /// ViewModels for days
        /// </summary>
        /// <param name="date"></param>
        /// <param name="filePath"></param>
        public DayViewModel(DateTime date, string filePath)
        {
            Date = date;
            FilePath = filePath;

            RemoveUserCommand = new LambdaCommand<DayUserViewModel>(RemoveUser);
            // DropCommand = new LambdaCommand<object>(OnDrop);

            Load();
        }


        public void OnDrop(object data, DayUserPlacement placement)
        {
            if (data is UserViewModel sidebarUser)
            {
                AddFromSidebar(sidebarUser, placement);
                return;
            }

            if (data is DayUserViewModel dayUser)
            {
                MoveUser(dayUser, placement);
            }
        }

        private void AddFromSidebar(UserViewModel user, DayUserPlacement placement)
        {
            if (_users.Any(u => u.UserID == user.Id))
                return;

            var vm = new DayUserViewModel(new DayUserDto(user.Id, user.Name, user.Hours))
            {
                Placement = placement
            };

            _users.Add(vm);

            if (placement == DayUserPlacement.Active)
                ActiveUsers.Add(vm);
            else
                ReserveUsers.Add(vm);

            SaveUsers();
        }



        public void MoveUser(DayUserViewModel user, DayUserPlacement target)
        {
            if (user == null || user.Placement == target)
                return;

            // убрать с экрана
            ActiveUsers.Remove(user);
            ReserveUsers.Remove(user);

            // поменять состояние
            user.Placement = target;

            // добавить на экран
            if (target == DayUserPlacement.Active)
                ActiveUsers.Add(user);
            else
                ReserveUsers.Add(user);

            SaveUsers();
        }


        private void RemoveUser(DayUserViewModel user)
        {
            if (user == null)
                return;

            // удалить из хранилища
            _users.Remove(user);

            //  удалить из UI
            ActiveUsers.Remove(user);
            ReserveUsers.Remove(user);

            // сохранить
            SaveUsers();
        }



        /// <summary>
        /// Load Users from storage
        /// </summary>
        private void Load()
        {
            ActiveUsers.Clear();
            ReserveUsers.Clear();
            _users.Clear();

            if (!File.Exists(FilePath))
                return;

            var dtos = JsonSerializer.Deserialize<List<DayUserDto>>(
                File.ReadAllText(FilePath));

            if (dtos == null)
                return;

            foreach (var dto in dtos)
            {
                var user = new DayUserViewModel(dto);
                _users.Add(user);

                if (dto.Placement == DayUserPlacement.Active)
                    ActiveUsers.Add(user);
                else
                    ReserveUsers.Add(user);
            }
        }

        /// <summary>
        /// Save user to storage
        /// </summary>
        public void SaveUsers()
        {
            var dtos = _users.Select(u => u.ToDto()).ToList();
            File.WriteAllText(FilePath, JsonSerializer.Serialize(dtos,
                new JsonSerializerOptions { WriteIndented = true }));
        }


    }
}
