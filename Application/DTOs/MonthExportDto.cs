namespace DutyPlanner.Application.DTOs
{
    public class DayExportDto
    {
        public DateTime Date { get; init; }
        public IReadOnlyList<string> ActiveUserNames { get; init; } = [];
        public IReadOnlyList<string> ReserveUserNames { get; init; } = [];
        public string Comment { get; init; } = "";
    }

    public class MonthExportDto
    {
        public int Year { get; init; }
        public int Month { get; init; }
        public string FolderPath { get; init; } = "";
        public IReadOnlyList<DayExportDto> Days { get; init; } = [];
    }
}
