using DutyPlanner.Application.Interfaces;
using DutyPlanner.Infrastrustures;
using DutyPlanner.Infrastrustures.Localization;
using DutyPlanner.Infrastrustures.MessageService;
using DutyPlanner.Infrastrustures.Settings;
using DutyPlanner.Models.Dto;
using DutyPlanner.Services;
using DutyPlanner.Services.Statistics;
using System.Globalization;
using System.IO;

namespace DutyPlanner.Presentation.ViewModels
{
    public sealed class MonthStatisticsViewModel : ViewModel, IRequestCloseViewModel
    {
        private readonly IExcelExportService _excelExportService;
        private readonly ISettingsService _settings;
        private readonly IMessageService _messages;
        private readonly ILocalizationService _localization;
        private readonly IFileLauncherService _fileLauncher;

        public MonthStatisticsDto Data { get; }

        public IReadOnlyList<DateTime> Days => Data.Days;
        public IReadOnlyList<MonthStatisticsRowDto> Rows => Data.Rows;

        public string Title => $"{_localization["Statistic_For"]} {new DateTime(Data.Year, Data.Month, 1):MMMM} {Data.Year}";

        #region Commands

        public LambdaCommand ExportExcelCommand { get; }
        public LambdaCommand CloseCommand { get; }

        #endregion

        public event Action<bool?>? RequestClose;

        public MonthStatisticsViewModel(
            MonthStatisticsDto data,
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

            ExportExcelCommand = new LambdaCommand(ExportExcel);
            CloseCommand = new LambdaCommand(() => RequestClose?.Invoke(true));
        }

        private void ExportExcel()
        {
            var exportFolder = Path.Combine(
             AppContext.BaseDirectory,
             _settings.Current.DefaultExportFolder);

            Directory.CreateDirectory(exportFolder);

            var culture = new CultureInfo(_settings.Current.Language);
            var monthName = culture.DateTimeFormat.GetMonthName(Data.Month);

            var fileName = $"{monthName}-{Data.Year}.xlsx";
            var filePath = Path.Combine(exportFolder, fileName);

            try
            {
                _excelExportService.ExportMonthStatistics(Data, filePath);

                if (_settings.Current.OpenExcelAfterExport)
                    _fileLauncher.OpenIfExists(filePath);

                _messages.ShowInfo(
                    $"{_localization["Excel_Successful_Preservation"]}",
                    $"{_localization["Excel_ExportCompleted"]}");
            }

            catch (IOException)
            {
                _messages.ShowError(
                    $"{_localization["Exel_Error_Export"]}\n\n" +
                    $"{_localization["Excel_Strign_1"]}\n" +
                    $"{_localization["Excel_Strign_2"]}\n" +
                    $"{_localization["Excel_Strign_3"]}\n" +
                    $"{_localization["Excel_Strign_4"]}\n\n" +
                    $"{_localization["Excel_String_5"]}",
                    $"{_localization["Excel_ExportCompleted"]}");
            }
            catch (Exception ex)
            {
                _messages.ShowError(
                   $"{_localization["Exel_Error_Export_2"]}\n\n" + ex.Message,
                    $"{_localization["Excel_ExportCompleted"]}");
            }

        }


    }
}
