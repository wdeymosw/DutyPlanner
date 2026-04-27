using DutyPlanner.Models;

namespace DutyPlanner.Services
{
    public interface IYearStatisticsService
    {
        /// <summary>
        /// Builds yearly statistics aggregated by months.
        /// Rows = users, columns = months (1–12), values = total hours per month.
        /// </summary>
        /// <param name="year">Target year</param>
        /// <returns>Prepared yearly statistics DTO</returns>
        //YearStatisticsDto BuildYear(int year);

        YearStatisticsDto BuildPeriod(DateTime start, DateTime end);
    }
}
