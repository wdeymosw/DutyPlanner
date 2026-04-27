namespace DutyPlanner.Services
{
    public interface IFileDialogService
    {
        string? SelectFolder(string initialPath);
    }
}
