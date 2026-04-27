using System;
using System.Collections.Generic;
using System.Text;

namespace DutyPlanner.Services.Statistics
{
    public interface IYearPeriodService
    {
        (DateTime Start, DateTime End) GetPeriod(int selectedYear);
    }
}
