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
                    _localization["Excel_Successful_Preservation"],
                    _localization["Excel_ExportCompleted"]);
            }
            catch (IOException)
            {
                _messages.ShowError(
                    $"{_localization["Exel_Error_Export"]}\n\n" +
                    $"{_localization["Excel_Strign_1"]}\n" +
                    $"{_localization["Excel_Strign_2"]}\n" +
                    $"{_localization["Excel_Strign_3"]}\n" +
                    $"{_localization["Excel_Strign_4"]}\n\n" +
                    _localization["Excel_String_5"],
                    _localization["Excel_ExportCompleted"]);
            }
            catch (Exception ex)
            {
                _messages.ShowError(
                    $"{_localization["Exel_Error_Export_2"]}\n\n" + ex.Message,
                    _localization["Excel_ExportCompleted"]);
            }
        }
    }
}
