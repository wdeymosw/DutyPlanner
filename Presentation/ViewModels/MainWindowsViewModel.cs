using DutyPlanner.Application.DTOs;
using DutyPlanner.Domain.Repositories;
using DutyPlanner.Presentation.Commands;
using DutyPlanner.Infrastructure.Localization;
using DutyPlanner.Infrastructure.MessageService;
using DutyPlanner.Infrastructure.Settings;
using DutyPlanner.Services;
using DutyPlanner.Services.Statistics;
using System.Collections.ObjectModel;

namespace DutyPlanner.Presentation.ViewModels
{
    public class MainWindowsViewModel : ViewModel
    {
        #region Services
        private readonly IDialogService _dialogService;
        private readonly IMessageService _messageService;
        private readonly IMonthReportService _reportService;
        private readonly IMonthManagementService _monthService;
        private readonly ISettingsService _settingsService;
        private readonly ILocalizationService _localization;
        private readonly IYearPeriodService _yearPeriodService;
        private readonly IDayRepository _dayRepository;
        #endregion

        public ObservableCollection<MonthPageViewModel> Pages { get; } = new();


        public string? CurrentYearName => CurrentPage?.Year.ToString();

        public string? CurrentMonthName => CurrentPage?.MonthName;


        #region CurrentPage
        private MonthPageViewModel? _currentPage;
        public MonthPageViewModel? CurrentPage
        {
            get => _currentPage;
            set
            {
                if (Set(ref _currentPage, value))
                    OnCurrentPageChanged(value);
            }
        }
        #endregion

        #region IsSelected
        private bool _isSelected;
        public bool IsSelected
        {
            get => _isSelected;
            set => Set(ref _isSelected, value);
        }
        #endregion

        #region leftsidebar
        /// <summary>
        /// Left sidebar with the list of users
        /// </summary>
        public SidebarUsersViewModel SidebarUsers { get; }
        #endregion

        #region Title
        /// <summary>
        /// Title of window
        /// </summary>
        private string _Title = "My Application";
        public string Title
        {
            get => _Title;
            set => Set(ref _Title, value);
        }
        #endregion



        #region Commands
        // Add Month with AddMontDialog
        public LambdaCommand AddMonthCommand { get; }
        public LambdaCommand RemoveMonthCommand { get; }
        public LambdaCommand<MonthPageViewModel> SelectPageCommand { get; }

        public LambdaCommand PrevMonthCommand { get; }
        public LambdaCommand NextMonthCommand { get; }

        public LambdaCommand ExportPdfCommand { get; }

        public LambdaCommand OpenSettingsCommand { get; }

        public LambdaCommand OpenMonthStatisticsCommand { get; }
        public LambdaCommand OpenYearStatisticsCommand { get; }

        // Navigation Commands


        #endregion


        #region Constructor
        public MainWindowsViewModel(
            SidebarUsersViewModel sidebarUsers,
            IYearPeriodService yearPeriodService,
            IDialogService dialogService,
            IMessageService messageService,
            IMonthReportService reportService,
            IMonthManagementService monthService,
            ISettingsService settingsService,
            ILocalizationService localization,
            IDayRepository dayRepository)
        {
            // Services

            SidebarUsers = sidebarUsers;
            _dialogService = dialogService;
            _messageService = messageService;
            _reportService = reportService;
            _monthService = monthService;
            _settingsService = settingsService;
            _localization = localization;
            _yearPeriodService = yearPeriodService ?? throw new ArgumentNullException(nameof(yearPeriodService));
            _dayRepository = dayRepository;

            _settingsService.SettingsChanged += OnSettingsChanged;

            _localization.LanguageChanged += (_, _) =>
            {
                OnLanguageChanged();
            };


            #region Commands

            AddMonthCommand = new LambdaCommand(AddMonth);// Добавляем месяц
            RemoveMonthCommand = new LambdaCommand(RemoveMonth, () => CurrentPage != null); // Удаляем месяц
            SelectPageCommand = new LambdaCommand<MonthPageViewModel>(SelectPage); // выбираем месяц
            ExportPdfCommand = new LambdaCommand(ExportPdf); // конверт месяца(планирование) в PDF
            OpenSettingsCommand = new LambdaCommand(() => _dialogService.ShowSettingsDialog());
            OpenMonthStatisticsCommand = new LambdaCommand(OpenMonthStatistics, () => CurrentPage != null); // Статистика по Месяцу в отдельное окно
            OpenYearStatisticsCommand = new LambdaCommand(OpenYearStatistics, () => CurrentPage != null); // Статистика по Году в одтельное окно



            PrevMonthCommand = new LambdaCommand(() => NavigateMonth(-1), () => CanNavigate(-1));

            NextMonthCommand = new LambdaCommand(() => NavigateMonth(1), () => CanNavigate(1));



            #endregion

            LoadPages();

        }
        #endregion


        #region LoadPages
        private void LoadPages()
        {
            Pages.Clear();

            var months = _monthService.LoadExistingMonths()
                .OrderBy(m => m.Year)
                .ThenBy(m => m.Month);

            foreach (var m in months)
            {
                Pages.Add(new MonthPageViewModel(
                    m.Year,
                    m.Month,
                    m.FolderPath,
                    _dialogService,
                    _localization,
                    _dayRepository));
            }

            RestoreLastPage();


        }
        #endregion

        #region Add Month
        private void AddMonth()
        {
            var dialogVm = _dialogService.ShowAddMonthDialog(
                        (y, m) => Pages.Any(p => p.Year == y && p.Month == m));

            if (dialogVm == null)
                return;

            var m = _monthService.CreateMonth(
                dialogVm.SelectedYear,
                dialogVm.SelectedMonth!.Month);

            var vm = new MonthPageViewModel(
                m.Year,
                m.Month,
                m.FolderPath,
                _dialogService,
                _localization,
                _dayRepository);

            vm.CreateDefaultDays();

            Pages.Add(vm);
            CurrentPage = vm;
            LoadPages();
        }
        #endregion

        #region Remove Month
        private void RemoveMonth()
        {
            if (CurrentPage == null)
                return;

            if (!_messageService.Confirm(
                $"{_localization["MainWindow_ConfirmDeleteMonth"]} {CurrentPage.MonthName} {CurrentPage.Year}?"))
                return;

            _monthService.DeleteMonth(CurrentPage.Year, CurrentPage.Month);

            Pages.Remove(CurrentPage);
            CurrentPage = Pages.FirstOrDefault();
        }
        #endregion


        #region ExportPdf
        private void ExportPdf()
        {
            if (CurrentPage == null)
                return;

            var exportDto = new MonthExportDto
            {
                Year = CurrentPage.Year,
                Month = CurrentPage.Month,
                FolderPath = CurrentPage.FolderPath,
                Days = CurrentPage.Days.Select(d => new DayExportDto
                {
                    Date = d.Date,
                    ActiveUserNames = d.ActiveUsers.Cast<DayUserViewModel>().Select(u => u.Name).ToList(),
                    ReserveUserNames = d.ReserveUsers.Cast<DayUserViewModel>().Select(u => u.Name).ToList()
                }).ToList()
            };

            try
            {
                _reportService.ExportMonthPdf(exportDto);

                _messageService.ShowInfo(
                    $"{_localization["Pdf_Successful_Preservation"]}",
                    $"{_localization["Pdf_ExportCompleted"]}");
            }
            catch (Exception ex)
            {
                _messageService.ShowError(
                    $"{_localization["Pdf_Error_Export"]}\n{ex.Message}",
                    $"{_localization["Export_Error"]}");
            }
        }
        #endregion

        private void SelectPage(MonthPageViewModel page)
        {
            if (page == null)
                return;

            foreach (var p in Pages)
                p.IsSelected = false;

            page.IsSelected = true;
            CurrentPage = page;
        }

        private void OpenMonthStatistics()
        {
            if (CurrentPage == null)
                return;

            _dialogService.ShowMonthStatistics(
                CurrentPage.Year,
                CurrentPage.Month

            );
        }

        private void OpenYearStatistics()
        {
            if (CurrentPage == null)
                return;

            var (start, end) = _yearPeriodService
                .GetPeriod(CurrentPage.Year);

            _dialogService.ShowYearStatistics(start, end);
        }


        private void OnCurrentPageChanged(MonthPageViewModel? page)
        {
            ((LambdaCommand)RemoveMonthCommand).RaiseCanExecuteChanged();
            ((LambdaCommand)PrevMonthCommand).RaiseCanExecuteChanged();
            ((LambdaCommand)NextMonthCommand).RaiseCanExecuteChanged();

            OnPropertyChanged(nameof(CurrentYearName));
            OnPropertyChanged(nameof(CurrentMonthName));

            if (page != null)
            {
                _settingsService.Current.LastOpenedYear = page.Year;
                _settingsService.Current.LastOpenedMonth = page.Month;
                _settingsService.Save();
            }
        }

        private void RestoreLastPage()
        {
            var year = _settingsService.Current.LastOpenedYear;
            var month = _settingsService.Current.LastOpenedMonth;

            MonthPageViewModel? page = null;

            if (year.HasValue && month.HasValue)
            {
                page = Pages.FirstOrDefault(p =>
                    p.Year == year.Value &&
                    p.Month == month.Value);
            }

            SelectPage(page ?? Pages.FirstOrDefault());

        }

        private void NavigateMonth(int offset)
        {
            if (CurrentPage == null) return;

            var index = Pages.IndexOf(CurrentPage);
            if (index < 0) return; // защита

            var newIndex = index + offset;

            if (newIndex < 0 || newIndex >= Pages.Count)
                return;

            SelectPage(Pages[newIndex]);
        }

        private bool CanNavigate(int offset)
        {
            if (CurrentPage == null) return false;

            var index = Pages.IndexOf(CurrentPage);
            var newIndex = index + offset;

            return newIndex >= 0 && newIndex < Pages.Count;
        }


        private void OnSettingsChanged(object? sender, EventArgs e)
        {
            OnPropertyChanged(nameof(CurrentMonthName));
        }
    }
}
