using DutyPlanner.Models;

namespace DutyPlanner.Domain.Repositories
{
    public interface IDayRepository
    {
        bool Exists(string filePath);
        List<DayUserDto> Load(string filePath);
        void Save(string filePath, IEnumerable<DayUserDto> users);
        void InitializeDay(string filePath);
        void Delete(string filePath);
        string[] GetDayFilePaths(string folderPath);
        void EnsureFolder(string folderPath);

        Task<List<DayUserDto>> LoadAsync(string filePath);
        Task SaveAsync(string filePath, IEnumerable<DayUserDto> users);
    }
}
