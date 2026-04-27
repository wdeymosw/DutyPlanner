using DutyPlanner.Infrastrustures.JsonFileStorage;
using DutyPlanner.Models;
using DutyPlanner.Models.Dto;
using System.IO;

namespace DutyPlanner.Services
{
    public sealed class MonthStatisticsService : IMonthStatisticsService
    {
        private readonly IJsonFileStorage _storage;

        public MonthStatisticsService(IJsonFileStorage storage)
        {
            _storage = storage;
        }

        public MonthStatisticsDto BuildMonth(int year, int month, string folderPath)
        {
            // UserId -> row
            var rows = new Dictionary<Guid, MonthStatisticsRowDto>();
            var days = GetAllDays(year, month);

            foreach (var file in Directory.GetFiles(folderPath, "*.json"))
            {
                if (!TryParseDay(file, year, month, out var date))
                    continue;

                var users = _storage
                    .Load<List<DayUserDto>>(file)
                    .Where(u => u.Placement == DayUserPlacement.Active);

                foreach (var u in users)
                {
                    if (!rows.TryGetValue(u.UserID, out var row))
                    {
                        row = new MonthStatisticsRowDto
                        {
                            UserId = u.UserID,          // agregation on UserID
                            UserName = u.Name ?? "—"
                        };
                        rows[u.UserID] = row;
                    }

                    row.HoursByDay[date] = u.Hours;
                }
            }

            // all of days should be presented in each row
            foreach (var row in rows.Values)
                foreach (var d in days)
                    row.HoursByDay.TryAdd(d, 0);

            return new MonthStatisticsDto
            {
                Year = year,
                Month = month,
                FolderPath = folderPath,
                Days = days,
                Rows = rows.Values
                    .OrderBy(r => r.UserName)
                    .ToList()
            };
        }

        private static List<DateTime> GetAllDays(int year, int month)
        {
            var list = new List<DateTime>();
            int count = DateTime.DaysInMonth(year, month);

            for (int d = 1; d <= count; d++)
                list.Add(new DateTime(year, month, d));

            return list;
        }

        private static bool TryParseDay(string filePath, int year, int month, out DateTime date)
        {
            date = default;

            var name = Path.GetFileNameWithoutExtension(filePath);
            var parts = name.Split('-');

            return int.TryParse(parts[0], out int day)
                   && TryCreateDate(year, month, day, out date);
        }

        private static bool TryCreateDate( int y, int m, int d, out DateTime date)
        {
            try
            {
                date = new DateTime(y, m, d);
                return true;
            }
            catch
            {
                date = default;
                return false;
            }
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
