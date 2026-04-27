using DutyPlanner.Domain.Repositories;
using DutyPlanner.Presentation.Commands;
using DutyPlanner.Infrastructure.Localization;
using DutyPlanner.Services;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Windows.Input;

namespace DutyPlanner.Presentation.ViewModels
{
    public class MonthPageViewModel : ViewModel
    {

        private readonly IDialogService _dialogService;
        private readonly IDayRepository _dayRepository;

        public int Year { get; }
        public int Month { get; }
        public string FolderPath { get; }


        private bool _isSelected;
        public bool IsSelected
        {
            get => _isSelected;
            set => Set(ref _isSelected, value);
        }


        public string MonthName => CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(Month);


        public ObservableCollection<DayViewModel> Days { get; } = new();


        #region Commands
        public ICommand AddDayCommand { get; }
        public ICommand RemoveDayCommand { get; }
        #endregion

        public MonthPageViewModel(int year, int month, string folderPath, IDialogService dialogService, ILocalizationService localization, IDayRepository dayRepository)
        {
            Year = year;
            Month = month;
            FolderPath = folderPath;
            _dialogService = dialogService;
            _dayRepository = dayRepository;

            localization.LanguageChanged += (_, _) =>
            {
                OnPropertyChanged(nameof(MonthName));
            };


            AddDayCommand = new LambdaCommand(AddDay);
            RemoveDayCommand = new LambdaCommand<DayViewModel>(RemoveDay);

            EnsureFolderExists();
            CreateDefaultDays();
            LoadDays();
        }

        public void LoadDays()
        {
            Days.Clear();

            foreach (var file in _dayRepository.GetDayFilePaths(FolderPath))
            {
                var fileName = Path.GetFileNameWithoutExtension(file);
                var parts = fileName.Split('-');

                if (!int.TryParse(parts[0], out int day))
                    continue;

                DateTime date;
                try
                {
                    date = new DateTime(Year, Month, day);
                }
                catch
                {
                    continue;
                }

                Days.Add(new DayViewModel(date, file, _dayRepository));
            }
        }


        public void CreateDefaultDays()
        {
            int daysInMonth = DateTime.DaysInMonth(Year, Month);

            for (int d = 1; d <= daysInMonth; d++)
            {
                var date = new DateTime(Year, Month, d);

                if (date.DayOfWeek is DayOfWeek.Friday or DayOfWeek.Saturday)
                {
                    string fileName = $"{d:00}-{date.DayOfWeek}.json";
                    string filePath = Path.Combine(FolderPath, fileName);

                    _dayRepository.InitializeDay(filePath);
                }
            }

            LoadDays();
        }

        private void AddDay()
        {
            Func<DateTime, bool> isDuplicate =
                d => Days.Any(day => day.Date.Date == d.Date);


            var dialogVm = _dialogService.ShowAddDayDialog(
                Year,
                Month,
                isDuplicate);

            if (dialogVm == null) return; // пользователь отменил

            var date = dialogVm.SelectedDate;

            if (date.Year != date.Year || date.Month != Month)
                return;

            string fileName = $"{date.Day:00}-{date.DayOfWeek}.json";
            string filePath = Path.Combine(FolderPath, fileName);

            _dayRepository.InitializeDay(filePath);

            Days.Add(new DayViewModel(date, filePath, _dayRepository));
        }

        private void RemoveDay(DayViewModel day)
        {
            if (day == null) return;
            _dayRepository.Delete(day.FilePath);
            Days.Remove(day);
        }

        private void EnsureFolderExists()
        {
            _dayRepository.EnsureFolder(FolderPath);
        }
    }
}
