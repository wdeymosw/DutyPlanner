using DutyPlanner.Infrastrustures;
using DutyPlanner.Infrastrustures.Localization;
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

        // public MonthPageViewModel(){ }

        public MonthPageViewModel(int year, int month, string folderPath, IDialogService dialogService, ILocalizationService localization)
        {
            Year = year;
            Month = month;
            FolderPath = folderPath;
            _dialogService = dialogService;

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

            foreach (var file in Directory.GetFiles(FolderPath, "*.json"))
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

                Days.Add(new DayViewModel(date, file));
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

                    if (!File.Exists(filePath))
                        File.WriteAllText(filePath, "[]");
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

            if (!File.Exists(filePath))
                File.WriteAllText(filePath, "[]");

            Days.Add(new DayViewModel(date, filePath));
        }

        private void RemoveDay(DayViewModel day)
        {
            if (day == null) return;
            if (File.Exists(day.FilePath)) File.Delete(day.FilePath);
            Days.Remove(day);
        }

        private void EnsureFolderExists()
        {
            if (!Directory.Exists(FolderPath))
                Directory.CreateDirectory(FolderPath);
        }
    }
}
