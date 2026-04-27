using DutyPlanner.Application.DTOs;

namespace DutyPlanner.Services
{
    public interface IMonthReportService
    {
        void ExportMonthPdf(MonthExportDto monthExport);
    }
}
