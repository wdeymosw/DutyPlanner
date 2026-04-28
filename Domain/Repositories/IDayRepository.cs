using DutyPlanner.Models;

namespace DutyPlanner.Domain.Repositories
{
    public interface IDayRepository
    {
        bool Exists(string filePath);
        DayFileDto Load(string filePath);
        void Save(string filePath, DayFileDto data);
        void InitializeDay(string filePath);
        void Delete(string filePath);
        string[] GetDayFilePaths(string folderPath);
        void EnsureFolder(string folderPath);

        Task<DayFileDto> LoadAsync(string filePath);
        Task SaveAsync(string filePath, DayFileDto data);
    }
}
