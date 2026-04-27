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

        public IReadOnlyList<YearMonth> Months => Data.Months;

        public IReadOnlyList<YearStatisticsRowDto> Rows => Data.Rows;

        public string Title
        {
            get
            {
                var culture = new CultureInfo(_settings.Current.Language);

                var start = Data.PeriodStart
                    .ToString("MMMM yyyy", culture);

                var end = Data.PeriodEnd
                    .ToString("MMMM yyyy", culture);

                return $"{_localization["Statistic_For"]} {start} – {end}";
            }
        }

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

            ExportCommand = new LambdaCommand(ExportToExcel);
            CloseCommand = new LambdaCommand(() => RequestClose?.Invoke(true));
        }

        private async void ExportToExcel()
        {
            var exportFolder = Path.Combine(
             AppContext.BaseDirectory,
             _settings.Current.DefaultExportFolder);

            Directory.CreateDirectory(exportFolder);

            var fileName = $"{Data.PeriodStart:yyyyMM}-{Data.PeriodEnd:yyyyMM}.xlsx";
            var filePath = Path.Combine(exportFolder, fileName);

            try
            {
                await _excelExportService.ExportYearStatisticsAsync(Data, filePath);

                if (_settings.Current.OpenExcelAfterExport)
                    _fileLauncher.OpenIfExists(filePath);

                _messages.ShowInfo(
                    "Файл успешно экспортирован.",
                    "Экспорт в Excel");
            }
            catch (IOException)
            {
                _messages.ShowError(
                    "Не удалось сохранить файл.\n\n" +
                    "Возможные причины:\n" +
                    "• файл уже открыт в Excel\n" +
                    "• нет прав на запись\n" +
                    "• диск недоступен\n\n" +
                    "Закройте файл и попробуйте снова.",
                    "Экспорт в Excel");
            }
            catch (Exception ex)
            {
                _messages.ShowError(
                    "Произошла ошибка при экспорте.\n\n" + ex.Message,
                    "Экспорт в Excel");
            }
        }
    }
}
