namespace DutyPlanner.Models
{
    public sealed class YearStatisticsRowDto
    {
        public Guid UserId { get; init; }
        public string UserName { get; init; } = string.Empty;

        // Month -> total hours
        public Dictionary<YearMonth, int> HoursByMonth { get; } = new();

        public int TotalHours => HoursByMonth.Values.Sum();
    }
}
