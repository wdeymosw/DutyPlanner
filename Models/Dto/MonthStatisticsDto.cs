namespace DutyPlanner.Models.Dto
{
    public sealed class MonthStatisticsDto
    {
        public int Year { get; init; }
        public int Month { get; init; }

        public string FolderPath { get; init; } = null!;

        public IReadOnlyList<DateTime> Days { get; init; } = Array.Empty<DateTime>();
        public IReadOnlyList<MonthStatisticsRowDto> Rows { get; init; } = Array.Empty<MonthStatisticsRowDto>();
    }
}
