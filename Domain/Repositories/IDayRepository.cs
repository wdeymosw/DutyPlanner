using DutyPlanner.Models;

namespace DutyPlanner.Domain.Repositories
{
    public interface IDayRepository
    {
        bool Exists(string filePath);
        List<DayUserDto> Load(string filePath);
        void Save(string filePath, IEnumerable<DayUserDto> users);
    }
}
