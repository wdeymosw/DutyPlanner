using DutyPlanner.Models;

namespace DutyPlanner.Services
{
    public interface IUserPeriodReportPdfService
    {
        Task ExportAsync(UserPeriodReportPdfDto dto);
    }
}
