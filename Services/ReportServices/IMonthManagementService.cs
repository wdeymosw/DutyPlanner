using DutyPlanner.Models;

namespace DutyPlanner.Services
{
    public interface IMonthManagementService
    {
        IReadOnlyList<MonthDescriptor> LoadExistingMonths();
        MonthDescriptor CreateMonth(int year, int month);
        void DeleteMonth(int year, int month);
    }
}
