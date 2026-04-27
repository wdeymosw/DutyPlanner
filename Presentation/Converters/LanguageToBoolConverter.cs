using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace DutyPlanner.Presentation.Converters
{
    public sealed class LanguageToBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not string current || parameter is not string target)
                return false;

            return current == target;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isChecked && isChecked && parameter is string lang)
                return lang;

            return DependencyProperty.UnsetValue;
        }
    }
}
