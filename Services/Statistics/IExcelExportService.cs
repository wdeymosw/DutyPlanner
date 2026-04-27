using DutyPlanner.Models;
using DutyPlanner.Models.Dto;

namespace DutyPlanner.Services
{
    public interface IExcelExportService
    {
        Task ExportMonthStatisticsAsync(MonthStatisticsDto data, string filePath);

        Task ExportYearStatisticsAsync(YearStatisticsDto data, string filePath);
    }
}
