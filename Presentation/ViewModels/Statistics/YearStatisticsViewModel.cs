using DutyPlanner.Application.Interfaces;
using DutyPlanner.Presentation.Commands;
using DutyPlanner.Infrastructure.Localization;
using DutyPlanner.Infrastructure.MessageService;
using DutyPlanner.Infrastructure.Settings;
using DutyPlanner.Models;
using DutyPlanner.Services;
using DutyPlanner.Services.Statistics;
using System.Globalization;
using System.IO;

namespace DutyPlanner.Presentation.ViewModels
{
    public sealed class YearStatisticsViewModel: ViewModel, IRequestCloseViewModel
    {
        public YearStatisticsDto Data { get; }
        private readonly IExcelExportService _excelExportService;
        private readonly ISettingsService _settings;
        private readonly IMessageService _messages;
        private readonly ILocalizationService _localization;
        private readonly IFileLauncherService _fileLauncher;

        private CultureInfo _culture;

        public IReadOnlyList<YearMonth> Months => Data.Months;

        public IReadOnlyList<YearStatisticsRowDto> Rows => Data.Rows;

        public string Title =>
            $"{_localization["Statistic_For"]} " +
            $"{Data.PeriodStart.ToString("MMMM yyyy", _culture)} – " +
            $"{Data.PeriodEnd.ToString("MMMM yyyy", _culture)}";

        public LambdaCommand CloseCommand { get; }
        public LambdaCommand ExportCommand { get; }

        public event Action<bool?>? RequestClose;

        public YearStatisticsViewModel(YearStatisticsDto data,
                                       IExcelExportService excelExportService,
                                       IMessageService messages,
                                       ISettingsService settings,
                                       ILocalizationService localization,
                                       IFileLauncherService fileLauncher)
        {
            Data = data ?? throw new ArgumentNullException(nameof(data));
            _excelExportService = excelExportService;
            _settings = settings;
            _messages = messages;
            _localization = localization;
            _fileLauncher = fileLauncher;

            _culture = new CultureInfo(settings.Current.Language);
            localization.LanguageChanged += (_, _) =>
            {
                _culture = new CultureInfo(_settings.Current.Language);
                OnPropertyChanged(nameof(Title));
            };

            ExportCommand = new LambdaCommand(ExportToExcel);
            CloseCommand = new LambdaCommand(() => RequestClose?.Invoke(true));
        }

        private async void ExportToExcel()
        {
            var exportFolder = Path.Combine(AppContext.BaseDirectory, _settings.Current.DefaultExportFolder);
            var filePath = Path.Combine(exportFolder, $"{Data.PeriodStart:yyyyMM}-{Data.PeriodEnd:yyyyMM}.xlsx");

            await ExportHelper.RunExportAsync(
                filePath,
                () => _excelExportService.ExportYearStatisticsAsync(Data, filePath),
                _settings, _fileLauncher, _messages, _localization);
        }
    }
}
