namespace DutyPlanner.Infrastrustures.MessageService
{
    public interface IMessageService
    {
        bool Confirm(string message, string title = "Подтверждение");

        void ShowInfo(string message, string title = "Информация");

        void ShowError(string message, string title = "Ошибка");
    }
}
