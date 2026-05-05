using DutyPlanner.Domain.Repositories;
using DutyPlanner.Presentation.Commands;
using DutyPlanner.Models;
using System.Collections.ObjectModel;
using System.Windows.Threading;

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

        #region Comment
        private string _comment = "";
        public string Comment
        {
            get => _comment;
            set
            {
                if (Set(ref _comment, value))
                    RestartSaveTimer();
            }
        }

        private DispatcherTimer? _saveTimer;

        private void RestartSaveTimer()
        {
            _saveTimer?.Stop();
            _saveTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(500) };
            _saveTimer.Tick += async (_, _) =>
            {
                _saveTimer!.Stop();
                try { await SaveDayAsync(); } catch { }
            };
            _saveTimer.Start();
        }
        #endregion

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

            RemoveUserCommand = new LambdaCommand<DayUserViewModel>(async user => await RemoveUserAsync(user));

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

        private async void AddFromSidebar(UserViewModel user, DayUserPlacement placement)
        {
            if (_users.Any(u => u.UserID == user.Id))
                return;

            var vm = new DayUserViewModel(new DayUserDto(user.Id, user.Name, user.Hours))
            {
                Placement = placement
            };

            SubscribeUser(vm);
            _users.Add(vm);

            if (placement == DayUserPlacement.Active)
                ActiveUsers.Add(vm);
            else
                ReserveUsers.Add(vm);

            try { await SaveDayAsync(); }
            catch { /* сохранение не удалось — исключение не должно упасть в поток UI */ }
        }


        public async void MoveUser(DayUserViewModel user, DayUserPlacement target)
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

            try { await SaveDayAsync(); }
            catch { /* сохранение не удалось — исключение не должно упасть в поток UI */ }
        }


        private async Task RemoveUserAsync(DayUserViewModel user)
        {
            if (user == null)
                return;

            UnsubscribeUser(user);
            _users.Remove(user);

            //  удалить из UI
            ActiveUsers.Remove(user);
            ReserveUsers.Remove(user);

            // сохранить
            await SaveDayAsync();
        }


        /// <summary>
        /// Load Users from storage
        /// </summary>
        private void Load()
        {
            foreach (var u in _users)
                UnsubscribeUser(u);

            ActiveUsers.Clear();
            ReserveUsers.Clear();
            _users.Clear();

            var dayFile = _dayRepository.Load(FilePath);
            _comment = dayFile.Comment;

            foreach (var dto in dayFile.Users)
            {
                var user = new DayUserViewModel(dto);
                SubscribeUser(user);
                _users.Add(user);

                if (dto.Placement == DayUserPlacement.Active)
                    ActiveUsers.Add(user);
                else
                    ReserveUsers.Add(user);
            }
        }

        private void SubscribeUser(DayUserViewModel vm)
            => vm.PropertyChanged += OnUserPropertyChanged;

        private void UnsubscribeUser(DayUserViewModel vm)
            => vm.PropertyChanged -= OnUserPropertyChanged;

        private void OnUserPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(DayUserViewModel.Hours))
                RestartSaveTimer();
        }

        /// <summary>
        /// Save day data (users + comment) to storage asynchronously
        /// </summary>
        private Task SaveDayAsync()
            => _dayRepository.SaveAsync(FilePath, new DayFileDto
            {
                Comment = _comment,
                Users = _users.Select(u => u.ToDto()).ToList()
            });
    }
}
