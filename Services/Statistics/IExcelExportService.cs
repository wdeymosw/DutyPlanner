using DutyPlanner.Models;
using DutyPlanner.Models.Dto;

namespace DutyPlanner.Services
{
    public interface IExcelExportService
    {
        void ExportMonthStatistics(MonthStatisticsDto data, string filePath);

        void ExportYearStatistics(YearStatisticsDto data, string filePath);
    }
}
