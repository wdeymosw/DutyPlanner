using DutyPlanner.Infrastructure.Localization;
using DutyPlanner.Infrastructure.MessageService;
using DutyPlanner.Models;
using DutyPlanner.Presentation.Commands;
using DutyPlanner.Presentation.ViewModels.Base;
using DutyPlanner.Services;
using System.Globalization;

namespace DutyPlanner.Presentation.ViewModels
{
    public sealed class UserPeriodReportViewModel : ViewModel, IRequestCloseViewModel
    {
        public record MonthRow(string MonthName, int Hours);

        private readonly IUserPeriodReportPdfService _pdfService;
        private readonly IMessageService _messageService;
        private readonly ILocalizationService _localization;

        public string Title { get; }
        public IReadOnlyList<MonthRow> MonthRows { get; }
        public int TotalHours { get; }
        public int MinimumHours { get; }
        public int HoursToNorm => Math.Max(0, MinimumHours - TotalHours);
        public bool IsDeficient => TotalHours < MinimumHours;
        public DateTime PeriodStart { get; }
        public DateTime PeriodEnd { get; }

        public LambdaCommand CloseCommand { get; }
        public LambdaCommand ExportPdfCommand { get; }

        public event Action<bool?>? RequestClose;

        public UserPeriodReportViewModel(
            YearStatisticsDto data,
            Guid userId,
            int minimumHours,
            ILocalizationService localization,
            IUserPeriodReportPdfService pdfService,
            IMessageService messageService)
        {
            _localization = localization;
            _pdfService = pdfService;
            _messageService = messageService;

            MinimumHours = minimumHours;
            PeriodStart = data.PeriodStart;
            PeriodEnd = data.PeriodEnd;

            var row = data.Rows.FirstOrDefault(r => r.UserId == userId);
            var userName = row?.UserName ?? string.Empty;

            var culture = new CultureInfo(localization.CurrentLanguage);
            Title = $"{userName}  {data.PeriodStart:dd.MM.yyyy} — {data.PeriodEnd:dd.MM.yyyy}";

            MonthRows = data.Months
                .Select(ym => new MonthRow(
                    culture.DateTimeFormat.GetMonthName(ym.Month) + " " + ym.Year,
                    row?.HoursByMonth.GetValueOrDefault(ym, 0) ?? 0))
                .ToList();

            TotalHours = row?.TotalHours ?? 0;

            CloseCommand = new LambdaCommand(() => RequestClose?.Invoke(null));

            ExportPdfCommand = new LambdaCommand(async () =>
            {
                var dto = new UserPeriodReportPdfDto(
                    userName, PeriodStart, PeriodEnd,
                    MonthRows.Select(r => (r.MonthName, r.Hours)).ToList(),
                    TotalHours, MinimumHours);
                try
                {
                    await _pdfService.ExportAsync(dto);
                }
                catch (Exception ex)
                {
                    _messageService.ShowError(ex.Message, _localization["Export_Error"]);
                }
            });
        }
    }
}
