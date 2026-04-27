using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace DutyPlanner.Presentation.Converters
{
    internal class WeekendBrushConverter : IValueConverter
    {
        public Brush WeekdayBrush { get; set; } = Brushes.Transparent;
        public Brush WeekendBrush { get; set; } = Brushes.LightGray;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not DateTime date)
                return WeekdayBrush;

            return date.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday
                ? WeekendBrush
                : WeekdayBrush;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }
}
