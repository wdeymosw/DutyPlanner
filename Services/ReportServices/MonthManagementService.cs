using DutyPlanner.Infrastructure.Settings;
using DutyPlanner.Models;
using System.Globalization;
using System.IO;

namespace DutyPlanner.Services
{
    internal class MonthManagementService : IMonthManagementService
    {
        private readonly string _dataRoot;

        public MonthManagementService(ISettingsService settings)
        {
            _dataRoot = settings.Current.DataFolderPath;
        }

        public IReadOnlyList<MonthDescriptor> LoadExistingMonths()
        {
            var result = new List<MonthDescriptor>();

            if (!Directory.Exists(_dataRoot))
                return result;

            foreach (var yearDir in Directory.GetDirectories(_dataRoot))
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
                        Month = date.Month,
                        FolderPath = monthDir
                    });
                }
            }

            return result;
        }

        public MonthDescriptor CreateMonth(int year, int month)
        {
            string monthName =
                CultureInfo.InvariantCulture.DateTimeFormat.GetMonthName(month);

            string folder = Path.Combine(_dataRoot, year.ToString(), monthName);
            Directory.CreateDirectory(folder);

            return new MonthDescriptor
            {
                Year = year,
                Month = month,
                FolderPath = folder
            };
        }

        public void DeleteMonth(int year, int month)
        {
            string monthName =
                CultureInfo.InvariantCulture.DateTimeFormat.GetMonthName(month);

            string folder = Path.Combine(_dataRoot, year.ToString(), monthName);

            if (Directory.Exists(folder))
                Directory.Delete(folder, true);
        }

    }
}
