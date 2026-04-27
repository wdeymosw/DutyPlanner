using DutyPlanner.Presentation.Commands;
using DutyPlanner.Infrastructure.Localization;
using DutyPlanner.Presentation.ViewModels.Base;
using DutyPlanner.Services;

namespace DutyPlanner.Presentation.ViewModels
{
    public class AddDayDialogViewModel : BaseDialogViewModel, IRequestCloseViewModel
    {
        private readonly Func<DateTime, bool> _isDuplicate;

        public event Action<bool?>? RequestClose;

        private DateTime _minDate;
        public DateTime MinDate
        {
            get => _minDate;
            set
            {
                if (Set(ref _minDate, value))
                    RaiseOkCanExecuteChanged();
            }
        }

        private DateTime _maxDate;
        public DateTime MaxDate
        {
            get => _maxDate;
            set
            {
                if (Set(ref _maxDate, value))
                    RaiseOkCanExecuteChanged();
            }
        }


        #region SelectedDate
        private DateTime _selectedDate;
        public DateTime SelectedDate
        {
            get => _selectedDate;
            set
            {
                if (Set(ref _selectedDate, value))
                    OkCommand.RaiseCanExecuteChanged();
            }
        }
        #endregion


        #region Command
        public LambdaCommand OkCommand { get; }
        public LambdaCommand CancelCommand { get; }

        #endregion

        public AddDayDialogViewModel(
            int year,
            int month,
            Func<DateTime, bool> isDuplicate)
        {
            _isDuplicate = isDuplicate ?? throw new ArgumentNullException(nameof(isDuplicate));

            OkCommand = new LambdaCommand(OnOk, CanOk);
            CancelCommand = new LambdaCommand(OnCancel);


            int daysInMonth = DateTime.DaysInMonth(year, month);

            MinDate = new DateTime(year, month, 1);
            MaxDate = new DateTime(year, month, daysInMonth);

            SelectedDate = MinDate;

            
        }


        protected override bool CanOk()
        {
            if (SelectedDate < MinDate || SelectedDate > MaxDate)
                return false;

            return !_isDuplicate(SelectedDate);
        }

        private void OnOk() => RequestClose?.Invoke(true);
        private void OnCancel() => RequestClose?.Invoke(false);
    }
}
