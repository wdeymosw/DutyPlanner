using DutyPlanner.Infrastructure.Settings;
using System;
using System.Collections.Generic;
using System.Text;

namespace DutyPlanner.Services.Statistics
{
    public sealed class YearPeriodService : IYearPeriodService
    {
        private readonly ISettingsService _settings;

        public YearPeriodService(ISettingsService settings)
        {
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
        }

        public (DateTime Start, DateTime End) GetPeriod(int reportingYear)
        {
            int startMonth = _settings.Current.YearStartMonth;

            var start = new DateTime(reportingYear, startMonth, 1)
                .AddYears(-1);

            var end = start.AddYears(1).AddDays(-1);

            return (start, end);
        }
    }
}
