using DutyPlanner.Domain.Repositories;
using DutyPlanner.Presentation.Commands;
using DutyPlanner.Models;
using System.Collections.ObjectModel;

namespace DutyPlanner.Presentation.ViewModels
{
    public class DayViewModel : ViewModel
    {
        public string FilePath { get; }
        public DateTime Date { get; }

        private readonly IDayRepository _dayRepository;

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
        #endregion

        /// <summary>
        /// ViewModels for days
        /// </summary>
        public DayViewModel(DateTime date, string filePath, IDayRepository dayRepository)
        {
            Date = date;
            FilePath = filePath;
            _dayRepository = dayRepository;

            RemoveUserCommand = new LambdaCommand<DayUserViewModel>(RemoveUser);

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

            var dtos = _dayRepository.Load(FilePath);

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
            _dayRepository.Save(FilePath, _users.Select(u => u.ToDto()));
        }
    }
}
