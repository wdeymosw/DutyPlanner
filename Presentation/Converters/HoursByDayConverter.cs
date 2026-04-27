using DutyPlanner.Models.Dto;
using System.Globalization;
using System.Windows.Data;

namespace DutyPlanner.Presentation.Converters
{
    public class HoursByDayConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length != 2)
                return 0;

            if (values[0] is not DateTime day)
                return 0;

            if (values[1] is not MonthStatisticsRowDto row)
                return 0;

            return row.HoursByDay.TryGetValue(day, out var hours)
                ? hours
                : 0;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }
}
