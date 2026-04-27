using DocumentFormat.OpenXml.Office2013.Excel;
using DutyPlanner.Presentation.Commands;
using DutyPlanner.Infrastructure.Localization;
using DutyPlanner.Models;
using DutyPlanner.Presentation.ViewModels.Base;
using DutyPlanner.Services;
using System.Collections.ObjectModel;
using System.Globalization;

namespace DutyPlanner.Presentation.ViewModels
{
    public class AddMonthDialogViewModel : BaseDialogViewModel, IRequestCloseViewModel
    {
        public ObservableCollection<int> Years { get; } = new();
        public ObservableCollection<MonthItem> Months { get; } = new();

        private readonly Func<int, int, bool> _isDuplicate;

        public event Action<bool?>? RequestClose;

        #region SelectedYear
        /// <summary>
        /// Выбраный Год
        /// </summary>
        private int _selectedYear;
        public int SelectedYear
        {
            get => _selectedYear;
            set
            {
                if (Set(ref _selectedYear, value))
                    RaiseOkCanExecuteChanged();
            }
        }
        #endregion

        #region SelectedMonth
        /// <summary>
        /// SelectedMonth
        /// </summary>
        private MonthItem? _selectedMonth;
        public MonthItem? SelectedMonth
        {
            get => _selectedMonth;
            set
            {
                if (Set(ref _selectedMonth, value))
                    OkCommand?.RaiseCanExecuteChanged();
            }
        }
        #endregion


        public LambdaCommand OkCommand { get; }
        public LambdaCommand CancelCommand { get; }


        public AddMonthDialogViewModel(Func<int, int, bool> isDuplicate)
        {
            _isDuplicate = isDuplicate;

            OkCommand = new LambdaCommand(OnOk, CanOk);
            CancelCommand = new LambdaCommand(OnCancel);

            int currentYear = DateTime.Now.Year;
            for (int y = currentYear - 3; y <= currentYear + 3; y++)
                Years.Add(y);
            SelectedYear = currentYear;

            for (int m = 1; m <= 12; m++)
                Months.Add(new MonthItem { Month = m });

            SelectedMonth = Months[DateTime.Now.Month - 1];
        }

        protected override bool CanOk()
        {
            return SelectedMonth != null &&
                   !_isDuplicate(SelectedYear, SelectedMonth.Month);
        }

        private void OnOk() => RequestClose?.Invoke(true);
        private void OnCancel() => RequestClose?.Invoke(false);


    }
}
