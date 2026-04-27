using DutyPlanner.Infrastructure.Settings;
using DutyPlanner.Models;
using System.Globalization;
using System.IO;

namespace DutyPlanner.Services
{
    internal class MonthManagementService : IMonthManagementService
    {
        private readonly ISettingsService _settings;

        public MonthManagementService(ISettingsService settings)
        {
            _settings = settings;
        }

        public IReadOnlyList<MonthDescriptor> LoadExistingMonths()
        {
            var dataRoot = _settings.Current.DataFolderPath;
            var result = new List<MonthDescriptor>();

            if (!Directory.Exists(dataRoot))
                return result;

            foreach (var yearDir in Directory.GetDirectories(dataRoot))
            {
                if (!int.TryParse(Path.GetFileName(yearDir), out int year))
                    continue;

                foreach (var monthDir in Directory.GetDirectories(yearDir))
                {
                    var name = Path.GetFileName(monthDir);

                    if (!DateTime.TryParseExact(
                            name,
                            "MMMM",
                            CultureInfo.InvariantCulture,
                            DateTimeStyles.None,
                            out var date))
                        continue;

                    result.Add(new MonthDescriptor
                    {
                        Year = year,
                        Month = date.Month
                    });
                }
            }

            return result;
        }

        public MonthDescriptor CreateMonth(int year, int month)
        {
            Directory.CreateDirectory(GetFolderPath(year, month));

            return new MonthDescriptor
            {
                Year = year,
                Month = month
            };
        }

        public void DeleteMonth(int year, int month)
        {
            string folder = GetFolderPath(year, month);

            if (Directory.Exists(folder))
                Directory.Delete(folder, true);
        }

        public string GetFolderPath(int year, int month)
        {
            string monthName = CultureInfo.InvariantCulture.DateTimeFormat.GetMonthName(month);
            return Path.Combine(_settings.Current.DataFolderPath, year.ToString(), monthName);
        }
    }
}
