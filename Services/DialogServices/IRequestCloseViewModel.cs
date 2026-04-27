namespace DutyPlanner.Services
{
    public interface IRequestCloseViewModel
    {
        event Action<bool?> RequestClose;
    }
}
