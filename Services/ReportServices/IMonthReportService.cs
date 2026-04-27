using DutyPlanner.Presentation.ViewModels;

namespace DutyPlanner.Services
{
    public interface IMonthReportService
    {
        void ExportMonthPdf(MonthPageViewModel monthPage);
    }
}
