using DutyPlanner.Infrastrustures.Localization;
using System.Windows;

namespace DutyPlanner.Infrastrustures.MessageService
{
    internal class MessageService : IMessageService
    {
        private readonly ILocalizationService _localization;

        public MessageService(ILocalizationService localization)
        {
            _localization = localization;
        }

        public bool Confirm(string message, string title = "")
        {
            var t = string.IsNullOrEmpty(title) ? _localization["Dialog_Confirm_Title"] : title;
            return MessageBox.Show(
                message,
                t,
                MessageBoxButton.YesNo,
                MessageBoxImage.Question)
                == MessageBoxResult.Yes;
        }

        public void ShowInfo(string message, string title = "")
        {
            var t = string.IsNullOrEmpty(title) ? _localization["Dialog_Info_Title"] : title;
            MessageBox.Show(
                message,
                t,
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }

        public void ShowError(string message, string title = "")
        {
            var t = string.IsNullOrEmpty(title) ? _localization["Dialog_Error_Title"] : title;
            MessageBox.Show(
                message,
                t,
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }
}
