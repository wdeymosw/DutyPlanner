using System.Windows;

namespace DutyPlanner.Infrastrustures.MessageService
{
    internal class MessageService : IMessageService
    {
        public bool Confirm(string message, string title = "Подтверждение")
        {
            return MessageBox.Show(
                message,
                title,
                MessageBoxButton.YesNo,
                MessageBoxImage.Question)
                == MessageBoxResult.Yes;
        }

        public void ShowInfo(string message, string title = "Информация")
        {
            MessageBox.Show(
                message,
                title,
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }

        public void ShowError(string message, string title = "Ошибка")
        {
            MessageBox.Show(
                message,
                title,
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }
}
