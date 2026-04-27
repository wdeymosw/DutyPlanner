using System;
using System.Collections.Generic;
using System.Text;

namespace DutyPlanner.Models
{
    public readonly record struct YearMonth(int Year, int Month)
    {
        public override string ToString()
            => $"{Month:D2}.{Year}";
    }
}
