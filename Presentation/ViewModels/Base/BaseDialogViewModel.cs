using DutyPlanner.Presentation.Commands;
using DutyPlanner.Infrastructure.Localization;
using System.Globalization;
using System.Windows.Markup;

namespace DutyPlanner.Presentation.ViewModels.Base
{
    public abstract class BaseDialogViewModel : ViewModel
    {

        private readonly ILocalizationService _localization;
        public event Action<bool?>? RequestClose;

        public LambdaCommand OkCommand { get; }
        public LambdaCommand CancelCommand { get; }




        protected BaseDialogViewModel()
        {
            OkCommand = new LambdaCommand(OnOk, CanOk);
            CancelCommand = new LambdaCommand(OnCancel);



        }


        protected virtual void OnLanguageChanged(object? sender, EventArgs e)
        {
            OnPropertyChanged(nameof(CalendarLanguage));
        }

        public XmlLanguage CalendarLanguage =>
        XmlLanguage.GetLanguage(
            CultureInfo.CurrentCulture.IetfLanguageTag);



        protected abstract bool CanOk();



        public void RaiseOkCanExecuteChanged()
        {
            OkCommand.RaiseCanExecuteChanged();
        }

        protected virtual void OnOk()
        {
            RequestClose?.Invoke(true);
        }

        protected virtual void OnCancel()
        {
            RequestClose?.Invoke(false);
        }

    }
}
