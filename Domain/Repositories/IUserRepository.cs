using DutyPlanner.Models;

namespace DutyPlanner.Domain.Repositories
{
    public interface IUserRepository
    {
        IReadOnlyList<User> GetAll();
        void Save(IEnumerable<User> users);
    }
}
