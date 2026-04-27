using DutyPlanner.Application.DTOs;

namespace DutyPlanner.Services
{
    public interface IMonthReportService
    {
        Task ExportMonthPdfAsync(MonthExportDto monthExport);
    }
}
