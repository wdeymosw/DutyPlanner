namespace DutyPlanner.Models.Dto
{
    public sealed class MonthStatisticsRowDto
    {
        public Guid UserId { get; init; }
        public string UserName { get; init; } = string.Empty;

        // key = Date (yyyy-MM-dd)
        public Dictionary<DateTime, int> HoursByDay { get; } = new();

        public int TotalHours => HoursByDay.Values.Sum();
    }
}
