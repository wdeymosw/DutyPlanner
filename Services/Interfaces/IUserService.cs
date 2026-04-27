using DutyPlanner.Models;

namespace DutyPlanner.Services
{
    public interface IUserService
    {
        IReadOnlyList<User> GetAll();
        //User? GetById(Guid id);

        User Add(string name, int hours);
        void Update(User user);
        void Remove(Guid userId);
    }
}
