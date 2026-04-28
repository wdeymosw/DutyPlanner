using DutyPlanner.Application.Interfaces;
using DutyPlanner.Presentation.Commands;
using DutyPlanner.Infrastructure.Localization;
using DutyPlanner.Infrastructure.MessageService;
using DutyPlanner.Infrastructure.Settings;
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

        private async void ExportExcel()
        {
            var exportFolder = Path.Combine(AppContext.BaseDirectory, _settings.Current.DefaultExportFolder);
            var culture = new CultureInfo(_settings.Current.Language);
            var monthName = culture.DateTimeFormat.GetMonthName(Data.Month);
            var filePath = Path.Combine(exportFolder, $"{monthName}-{Data.Year}.xlsx");

            await ExportHelper.RunExportAsync(
                filePath,
                () => _excelExportService.ExportMonthStatisticsAsync(Data, filePath),
                _settings, _fileLauncher, _messages, _localization);
        }


    }
}
