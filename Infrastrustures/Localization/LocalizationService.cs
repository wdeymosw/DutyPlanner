using DutyPlanner.Infrastrustures.Settings;
using System.Globalization;
using System.Windows;

namespace DutyPlanner.Infrastrustures.Localization
{
    public class LocalizationService : ILocalizationService
    {
        private ResourceDictionary? _currentDictionary;

        public string CurrentLanguage { get; private set; } = "ru";

        public event EventHandler? LanguageChanged;


        public LocalizationService(ISettingsService settings)
        {
            settings.SettingsChanged += (_, _) =>
            {
                ApplyLanguage(settings.Current.Language);
            };
        }

        public void ApplyLanguage(string languageCode)
        {
            if (string.IsNullOrWhiteSpace(languageCode))
                languageCode = "ru";

            if (languageCode == CurrentLanguage)
                return;

            // 1️⃣ Определяем культуру
            var culture = languageCode switch
            {
                "en" => new CultureInfo("en-US"),
                "uk" => new CultureInfo("uk-UA"),
                _ => new CultureInfo("ru-RU")
            };

            // 2️⃣ УСТАНАВЛИВАЕМ культуру (КРИТИЧНО)
            CultureInfo.CurrentCulture = culture;
            CultureInfo.CurrentUICulture = culture;
            CultureInfo.DefaultThreadCurrentCulture = culture;
            CultureInfo.DefaultThreadCurrentUICulture = culture;

            // 3️⃣ Загружаем словарь ресурсов
            var uri = $"pack://application:,,,/Presentation/Resources/Localization/Strings.{languageCode}.xaml";

            var newDictionary = new ResourceDictionary
            {
                Source = new Uri(uri, UriKind.Absolute)
            };

            // 4️⃣ Меняем словарь в Application.Resources
            if (_currentDictionary != null)
            {
                System.Windows.Application.Current.Resources.MergedDictionaries.Remove(_currentDictionary);
            }

            System.Windows.Application.Current.Resources.MergedDictionaries.Add(newDictionary);
            _currentDictionary = newDictionary;

            // 5️⃣ Фиксируем язык
            CurrentLanguage = languageCode;

            // 6️⃣ Уведомляем UI
            LanguageChanged?.Invoke(this, EventArgs.Empty);
        }

        public string this[string key]
            => System.Windows.Application.Current.Resources[key]?.ToString()
               ?? $"!{key}!";
    }
}
