namespace DutyPlanner.Infrastructure.Localization
{
    public interface ILocalizationService
    {
        string CurrentLanguage { get; }

        event EventHandler? LanguageChanged;

        void ApplyLanguage(string languageCode);

        string this[string key] { get; }
    }
}
