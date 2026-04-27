using System.Globalization;
using System.Windows.Data;

namespace DutyPlanner.Presentation.Converters
{
    public sealed class CapitalizeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not string s || string.IsNullOrEmpty(s))
                return value;

            return culture.TextInfo.ToTitleCase(s);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }
}
