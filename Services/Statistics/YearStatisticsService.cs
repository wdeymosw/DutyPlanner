using DutyPlanner.Models;

namespace DutyPlanner.Services
{
    public sealed class YearStatisticsService : IYearStatisticsService
    {
       private readonly IUserService _userService;
       private readonly IMonthStatisticsService _monthStatistics;
       private readonly IMonthManagementService _monthManagement;

    public YearStatisticsService(
        IUserService userService,
        IMonthStatisticsService monthStatistics,
        IMonthManagementService monthManagement)
    {
        _userService = userService;
        _monthStatistics = monthStatistics;
        _monthManagement = monthManagement;
    }

        public YearStatisticsDto BuildPeriod(DateTime start, DateTime end)
        {
            var rows = _userService.GetAll()
                .ToDictionary(
                    u => u.Id,
                    u => new YearStatisticsRowDto
                    {
                        UserId = u.Id,
                        UserName = u.Name
                    });

            var months = EnumerateMonths(start, end).ToList();

            var existingMonths = _monthManagement.LoadExistingMonths()
                .Select(m => (m.Year, m.Month))
                .ToHashSet();

            foreach (var (year, month) in months)
            {
                if (!existingMonths.Contains((year, month)))
                    continue;

                var key = new YearMonth(year, month);

                var monthDto = _monthStatistics.BuildMonth(
                    year,
                    month,
                    _monthManagement.GetFolderPath(year, month));

                foreach (var monthRow in monthDto.Rows)
                {
                    if (!rows.TryGetValue(monthRow.UserId, out var row))
                    {
                        row = new YearStatisticsRowDto
                        {
                            UserId = monthRow.UserId,
                            UserName = monthRow.UserName
                        };

                        rows[monthRow.UserId] = row;
                    }

                    row.HoursByMonth[key] =
                        row.HoursByMonth.GetValueOrDefault(key)
                        + monthRow.TotalHours;
                }
            }

            foreach (var row in rows.Values)
                foreach (var (y, m) in months)
                    row.HoursByMonth.TryAdd(new YearMonth(y, m), 0);

            return new YearStatisticsDto
            {
                PeriodStart = start,
                PeriodEnd = end,
                Months = months.Select(m => new YearMonth(m.Year, m.Month)).ToList(),
                Rows = rows.Values.OrderBy(r => r.UserName).ToList()
            };
        }


        private static IEnumerable<(int Year, int Month)> EnumerateMonths(DateTime start, DateTime end)
        {
            var current = new DateTime(start.Year, start.Month, 1);
            var last = new DateTime(end.Year, end.Month, 1);

            while (current <= last)
            {
                yield return (current.Year, current.Month);
                current = current.AddMonths(1);
            }
        }
    }
}
