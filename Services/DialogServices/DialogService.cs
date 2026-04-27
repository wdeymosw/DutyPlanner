using Microsoft.Extensions.DependencyInjection;
using DutyPlanner.Presentation.ViewModels;
using DutyPlanner.Presentation.Windows;
using System.Windows;

namespace DutyPlanner.Services
{
    public class DialogService : IDialogService
    {

        private readonly IServiceProvider _services;

        public DialogService(IServiceProvider services)
        {
            _services = services;
        }

        /// <summary>
        /// Add Month dialog
        /// </summary>
        /// <param name="isDuplicate"></param>
        /// <returns></returns>
        public AddMonthDialogViewModel? ShowAddMonthDialog(Func<int, int, bool> isDuplicate)
        {
            var vm = ActivatorUtilities.CreateInstance<AddMonthDialogViewModel>(
                 _services, isDuplicate);

            var dialog = CreateDialog<AddMonthDialog>(vm);
            return ShowDialog(dialog, vm);
        }

        /// <summary>
        /// Add Day dialog
        /// </summary>
        /// <param name="year"></param>
        /// <param name="month"></param>
        /// <param name="isDuplicate"></param>
        /// <returns></returns>
        public AddDayDialogViewModel? ShowAddDayDialog(int year, int month, Func<DateTime, bool> isDuplicate)
        {
            var vm = ActivatorUtilities.CreateInstance<AddDayDialogViewModel>(
                 _services, year, month, isDuplicate);

            var dialog = CreateDialog<AddDayDialog>(vm);
            return ShowDialog(dialog, vm);
        }

        /// <summary>
        /// Setting dialog
        /// </summary>
        public SettingsViewModel? ShowSettingsDialog()
        {
            var vm = ActivatorUtilities.CreateInstance<SettingsViewModel>(_services);
            var dialog = CreateDialog<SettingsDialog>(vm);
            return ShowDialog(dialog, vm);
        }


        /// <summary>
        /// Show Month statistic dialog
        /// </summary>
        /// <param name="year"></param>
        /// <param name="month"></param>
        public void ShowMonthStatistics(int year, int month)
        {

            var statsService = _services.GetRequiredService<IMonthStatisticsService>();
            var monthService = _services.GetRequiredService<IMonthManagementService>();

            var monthInfo = monthService.LoadExistingMonths()
                .FirstOrDefault(m => m.Year == year && m.Month == month);

            if (monthInfo == null)
                return;

            var data = statsService.BuildMonth(
                year,
                month,
                monthInfo.FolderPath);

            var vm = ActivatorUtilities.CreateInstance<MonthStatisticsViewModel>(
                _services,
                data);

            var dialog = CreateDialog<MonthStatisticsWindow>(vm);
            ShowDialog(dialog, vm);
        }


        public void ShowYearStatistics(DateTime start, DateTime end)
        {
            var statsService = _services.GetRequiredService<IYearStatisticsService>();

            var data = statsService.BuildPeriod(start,end);

            var vm = ActivatorUtilities.CreateInstance<YearStatisticsViewModel>(
                _services, data);

            var dialog = CreateDialog<YearStatisticsWindow>(vm);
            ShowDialog(dialog, vm);
        }



        // ===== HELPERS =====
        private TDialog CreateDialog<TDialog>(object viewModel)
            where TDialog : Window
        {
            var dialog = ActivatorUtilities.CreateInstance<TDialog>(_services);
            dialog.DataContext = viewModel;
            dialog.Owner = Application.Current.MainWindow;
            dialog.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            return dialog;
        }

        private static TViewModel? ShowDialog<TViewModel>(
            Window dialog,
            TViewModel vm)
            where TViewModel : class, IRequestCloseViewModel
        {
            bool? result = null;

            vm.RequestClose += r =>
            {
                result = r;
                dialog.Close();
            };

            dialog.ShowDialog();
            return result == true ? vm : null;
        }
    }
}
