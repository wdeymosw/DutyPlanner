namespace DutyPlanner.Infrastrustures.MessageService
{
    public interface IMessageService
    {
        bool Confirm(string message, string title = "");

        void ShowInfo(string message, string title = "");

        void ShowError(string message, string title = "");
    }
}
