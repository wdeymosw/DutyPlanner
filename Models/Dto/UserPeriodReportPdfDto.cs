namespace DutyPlanner.Models
{
    public record UserPeriodReportPdfDto(
        string UserName,
        DateTime PeriodStart,
        DateTime PeriodEnd,
        IReadOnlyList<(string MonthName, int Hours)> Months,
        int TotalHours,
        int MinimumHours);
}
