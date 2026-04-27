namespace DutyPlanner.Models
{
    public sealed class YearStatisticsDto
    {
        public DateTime PeriodStart { get; init; }
        public DateTime PeriodEnd { get; init; }


        public IReadOnlyList<YearMonth> Months { get; init; } = Array.Empty<YearMonth>();
        public IReadOnlyList<YearStatisticsRowDto> Rows { get; init; } = Array.Empty<YearStatisticsRowDto>();
    }
}
