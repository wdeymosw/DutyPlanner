using DutyPlanner.Presentation.ViewModels;

namespace DutyPlanner.Services
{
    public interface IDialogService
    {
        AddMonthDialogViewModel? ShowAddMonthDialog(Func<int, int, bool> isDuplicate);

        AddDayDialogViewModel? ShowAddDayDialog(int year, int month, Func<DateTime, bool> isDuplicate);

        /// <summary>
        /// Show Setting dialog
        /// </summary>
        public SettingsViewModel? ShowSettingsDialog();

        /// <summary>
        /// Show Month statistic dialog
        /// </summary>
        /// <param name="year"> Year</param>
        /// <param name="month"> Month</param>
        /// <param name="folderPath">FolderPath</param>
        void ShowMonthStatistics(int year, int month);

        void ShowYearStatistics(DateTime start, DateTime end);

        /// <summary>Shows the period report window for a single user.</summary>
        void ShowUserPeriodReport(Guid userId);
    }
}
