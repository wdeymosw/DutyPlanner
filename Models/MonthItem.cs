namespace DutyPlanner.Models
{
   
    /// <summary>
    /// Represents a month with its numeric value and display name.
    /// </summary>
    public class MonthItem
    {
        public int Month { get; init; }
        public string Name { get; init; } = string.Empty;
    }
}
