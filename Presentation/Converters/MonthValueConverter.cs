using DutyPlanner.Models;
using System.Globalization;
using System.Windows.Data;

namespace DutyPlanner.Presentation.Converters
{
    public sealed class MonthValueConverter : IMultiValueConverter
    {
        public object Convert(
            object[] values,
            Type targetType,
            object parameter,
            CultureInfo culture)
        {
            if (values.Length != 2)
                return string.Empty;

            if (values[0] is not YearMonth ym)
                return string.Empty;

            if (values[1] is not YearStatisticsRowDto row)
                return string.Empty;

            if (!row.HoursByMonth.TryGetValue(ym, out var hours) || hours == 0)
                return "—";

            return hours.ToString(culture);
        }

        public object[] ConvertBack(
            object value,
            Type[] targetTypes,
            object parameter,
            CultureInfo culture)
            => throw new NotSupportedException();
    }
}
