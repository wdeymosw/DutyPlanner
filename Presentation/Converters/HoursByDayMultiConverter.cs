using DutyPlanner.Models.Dto;
using System.Globalization;
using System.Windows.Data;


namespace DutyPlanner.Presentation.Converters
{
    public sealed class HoursByDayMultiConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length != 2)
                return string.Empty;

            if (values[0] is not DateTime day)
                return string.Empty;

            if (values[1] is not MonthStatisticsRowDto row)
                return string.Empty;

            return row.HoursByDay.TryGetValue(day, out var hours) && hours > 0
                  ? hours.ToString(culture)
                  : string.Empty;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }
}