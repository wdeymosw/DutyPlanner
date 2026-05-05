using DutyPlanner.Application.Interfaces;
using DutyPlanner.Infrastructure.Localization;
using DutyPlanner.Infrastructure.Settings;
using DutyPlanner.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.Globalization;
using System.IO;

namespace DutyPlanner.Services
{
    public class UserPeriodReportPdfService : IUserPeriodReportPdfService
    {
        private readonly ILocalizationService _localization;
        private readonly ISettingsService _settings;
        private readonly IFileLauncherService _fileLauncher;

        public UserPeriodReportPdfService(
            ILocalizationService localization,
            ISettingsService settings,
            IFileLauncherService fileLauncher)
        {
            _localization = localization;
            _settings = settings;
            _fileLauncher = fileLauncher;
        }

        public Task ExportAsync(UserPeriodReportPdfDto dto)
            => Task.Run(() => Export(dto));

        private void Export(UserPeriodReportPdfDto dto)
        {
            QuestPDF.Settings.License = LicenseType.Community;

            var exportFolder = Path.Combine(
                AppContext.BaseDirectory,
                _settings.Current.DefaultExportFolder);

            if (!Directory.Exists(exportFolder))
                Directory.CreateDirectory(exportFolder);

            var safeName = string.Concat(dto.UserName.Split(Path.GetInvalidFileNameChars()));
            var filePath = Path.Combine(
                exportFolder,
                $"Report_{safeName}_{dto.PeriodStart:yyyyMM}-{dto.PeriodEnd:yyyyMM}.pdf");

            var culture = new CultureInfo(_localization.CurrentLanguage);
            var isDeficient = dto.TotalHours < dto.MinimumHours;
            var hoursToNorm = Math.Max(0, dto.MinimumHours - dto.TotalHours);

            var doc = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A5);
                    page.Margin(20);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(11).FontFamily("Times New Roman"));

                    page.Header()
                        .Column(col =>
                        {
                            col.Item().AlignCenter().Text(dto.UserName).FontSize(14).SemiBold();
                            col.Item().AlignCenter().Text(
                                $"{dto.PeriodStart:MM.yyyy} — {dto.PeriodEnd:MM.yyyy}");
                        });

                    page.Content()
                        .PaddingTop(10)
                        .Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn();
                                columns.ConstantColumn(60);
                            });

                            // Header
                            table.Header(header =>
                            {
                                header.Cell().Element(HeaderCell).Text(_localization["UserReport_Month"]);
                                header.Cell().Element(HeaderCell).AlignCenter().Text(_localization["UserReport_Hours"]);
                            });

                            // Month rows
                            foreach (var (monthName, hours) in dto.Months)
                            {
                                table.Cell().Element(BodyCell).Text(monthName);
                                table.Cell().Element(BodyCell).AlignCenter().Text(hours.ToString());
                            }

                            // Total row
                            var totalColor = isDeficient ? Colors.Red.Medium : Colors.Green.Medium;
                            table.Cell().Element(TotalCell).Text(_localization["UserReport_Total"]).FontColor(totalColor).Bold();
                            table.Cell().Element(TotalCell).AlignCenter().Text(dto.TotalHours.ToString()).FontColor(totalColor).Bold();

                            // Norm row
                            table.Cell().Element(BodyCell).Text(_localization["UserReport_MinNorm"]);
                            table.Cell().Element(BodyCell).AlignCenter().Text(dto.MinimumHours.ToString());

                            // Deficit or success row
                            if (isDeficient)
                            {
                                table.Cell().Element(BodyCell).Text(_localization["UserReport_Remaining"]);
                                table.Cell().Element(BodyCell).AlignCenter().Text(hoursToNorm.ToString()).FontColor(Colors.Red.Medium);
                            }
                            else
                            {
                                table.Cell().ColumnSpan(2).Element(BodyCell)
                                    .AlignCenter().Text(_localization["UserReport_NormDone"]).FontColor(Colors.Green.Medium);
                            }
                        });
                });
            });

            doc.GeneratePdf(filePath);

            if (_settings.Current.OpenPdfAfterExport)
                _fileLauncher.OpenIfExists(filePath);
        }

        private static IContainer HeaderCell(IContainer c) =>
            c.Border(1).BorderColor(Colors.Grey.Lighten2)
             .Background(Colors.Grey.Lighten4).Padding(5)
             .AlignMiddle().DefaultTextStyle(x => x.SemiBold());

        private static IContainer BodyCell(IContainer c) =>
            c.Border(1).BorderColor(Colors.Grey.Lighten2).Padding(5).AlignMiddle();

        private static IContainer TotalCell(IContainer c) =>
            c.Border(1).BorderColor(Colors.Grey.Lighten2)
             .Background(Colors.Grey.Lighten5).Padding(5).AlignMiddle();
    }
}
