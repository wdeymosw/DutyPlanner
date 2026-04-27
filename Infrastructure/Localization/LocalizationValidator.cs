using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;

namespace DutyPlanner.Infrastructure.Localization
{
    internal static class LocalizationValidator
    {
        private static readonly string[] Languages = ["ru", "en", "uk"];

        public static void Validate()
        {
            var dictionaries = Languages
                .ToDictionary(lang => lang, LoadDictionary);

            var referenceKeys = dictionaries["ru"].Keys
                .Cast<object>()
                .Select(k => k.ToString()!)
                .ToHashSet();

            foreach (var lang in Languages.Where(l => l != "ru"))
            {
                var keys = dictionaries[lang].Keys
                    .Cast<object>()
                    .Select(k => k.ToString()!)
                    .ToHashSet();

                var missing = referenceKeys.Except(keys).OrderBy(k => k).ToList();

                if (missing.Count > 0)
                {
                    Debug.WriteLine(
                        $"[LocalizationValidator] '{lang}' is missing {missing.Count} key(s):\n" +
                        string.Join("\n", missing.Select(k => $"  • {k}")));
                }
            }
        }

        private static ResourceDictionary LoadDictionary(string lang)
        {
            var uri = $"pack://application:,,,/Presentation/Resources/Localization/Strings.{lang}.xaml";
            return new ResourceDictionary { Source = new Uri(uri, UriKind.Absolute) };
        }
    }
}
