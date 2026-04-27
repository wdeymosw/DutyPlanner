using DutyPlanner.Infrastrustures.Localization;
using DutyPlanner.Models;
using DutyPlanner.Models.Dto;

namespace DutyPlanner.Services
{
    public interface IExcelExportService
    {
        void ExportMonthStatistics(MonthStatisticsDto data, string filePath, ILocalizationService localization);

        void ExportYearStatistics( YearStatisticsDto data, string filePath, ILocalizationService localization);
    }
}
