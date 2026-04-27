using DutyPlanner.Infrastructure.JsonFileStorage;

namespace DutyPlanner.Infrastructure.Settings
{
    internal sealed class SettingsService : ISettingsService
    {
        private readonly IJsonFileStorage _storage;
        private readonly string _settingsPath;


        public AppSettings Current { get; private set; } = new();

        public event EventHandler? SettingsChanged;

        public SettingsService(IJsonFileStorage storage)
        {
            _storage = storage ?? throw new ArgumentNullException(nameof(storage));
            _settingsPath = "settings.json";
        }

        public void Load()
        {
            if (_storage.Exists(_settingsPath))
                Current = _storage.Load<AppSettings>(_settingsPath);
            else
                Save();

            // ВАЖНО: сообщаем, что настройки готовы
            SettingsChanged?.Invoke(this, EventArgs.Empty);
        }


        public void Save()
        {
            _storage.Save(_settingsPath, Current);

            SettingsChanged?.Invoke(this, EventArgs.Empty);

            //TODO: Реализация перезагрузки после выбора и сохранения настроек
        }
    }
}
