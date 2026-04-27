using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;

namespace DutyPlanner.Infrastructure.Localization
{
    internal static class LocalizationValidator
    {
        public static void Validate()
        {
            foreach (var lang in new[] { "ru", "en", "uk" })
            {
                var uri =
                    $"pack://application:,,,/Presentation/Resources/Localization/Strings.{lang}.xaml";

                _ = new ResourceDictionary
                {
                    Source = new Uri(uri, UriKind.Absolute)
                };
            }
        }
    }
}
