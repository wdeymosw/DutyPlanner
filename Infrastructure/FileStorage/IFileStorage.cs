namespace DutyPlanner.Infrastructure.FileStorage
{
    public interface IFileStorage
    {
        void Save(string relativePath, byte[] data);
        string GetFullPath(string relativePath);
        bool Exists(string relativePath);
    }
}
