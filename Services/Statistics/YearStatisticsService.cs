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

        /* public YearStatisticsDto BuildYear(int year)
         {
             // userName → row
             var users = _userService.GetAll();

             var rows = users.ToDictionary(
                 u => u.Id,
                 u => new YearStatisticsRowDto
                 {
                     UserId = u.Id,
                     UserName = u.Name
                 });

             // 2️⃣ Существующие месяцы
             var months = _monthManagement.LoadExistingMonths()
                 .Where(m => m.Year == year)
                 .OrderBy(m => m.Month)
                 .ToList();

             foreach (var month in months)
             {
                 var monthDto = _monthStatistics.BuildMonth(
                     month.Year,
                     month.Month,
                     month.FolderPath);

                 foreach (var monthRow in monthDto.Rows)
                 {
                     // здесь row ГАРАНТИРОВАННО существует
                     rows[monthRow.UserId]
                         .HoursByMonth[month.Month] = monthRow.TotalHours;
                 }
             }

             // 3️⃣ Гарантируем 12 месяцев
             foreach (var row in rows.Values)
                 for (int m = 1; m <= 12; m++)
                     row.HoursByMonth.TryAdd(m, 0);

             return new YearStatisticsDto
             {
                 Year = year,
                 Months = Enumerable.Range(1, 12).ToList(),
                 Rows = rows.Values
                     .OrderBy(r => r.UserName)
                     .ToList()
             };
         } */


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

            foreach (var (year, month) in months)
            {
                var monthInfo = _monthManagement
                    .LoadExistingMonths()
                    .FirstOrDefault(m => m.Year == year && m.Month == month);

                if (monthInfo == null)
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
