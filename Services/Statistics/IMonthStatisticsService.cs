using DutyPlanner.Models.Dto;

namespace DutyPlanner.Services
{
    public interface IMonthStatisticsService
    {
        MonthStatisticsDto BuildMonth(int year, int month, string folderPath);
    }
}
