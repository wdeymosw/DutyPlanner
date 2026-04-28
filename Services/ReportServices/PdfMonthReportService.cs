using DutyPlanner.Application.DTOs;
using DutyPlanner.Infrastructure.Localization;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.Diagnostics;
using System.Globalization;
using System.IO;

namespace DutyPlanner.Services
{
    public class PdfMonthReportService : IMonthReportService
    {
        private readonly ILocalizationService _localization;

        public PdfMonthReportService(ILocalizationService localization)
        {
            _localization = localization;
        }

        public Task ExportMonthPdfAsync(MonthExportDto monthExport)
            => Task.Run(() => ExportMonthPdf(monthExport));

        private void ExportMonthPdf(MonthExportDto monthExport)
        {
            QuestPDF.Settings.License = LicenseType.Community;

            if (monthExport == null || monthExport.Days.Count == 0)
                return;

            string folder = monthExport.FolderPath;
            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);

            var culture = new CultureInfo(_localization.CurrentLanguage);
            var monthName = culture.DateTimeFormat.GetMonthName(monthExport.Month);

            string filePath = Path.Combine(
                folder,
                $"Report_{monthName}_{monthExport.Year}.pdf");

            var doc = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4.Landscape());
                    page.MarginTop(25);
                    page.MarginLeft(120);
                    page.MarginRight(120);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x =>
                        x.FontSize(12)
                         .FontFamily("Times New Roman"));

                    // ===== HEADER =====
                    page.Header()
                        .AlignCenter()
                        .Text(
                            $"{_localization["Pdf_String_1"]}\n" +
                            $"{_localization["Pdf_String_2"]}\n" +
                            $"{_localization["Pdf_String_3"]} {monthName} {monthExport.Year}")
                        .FontSize(15)
                        .SemiBold();

                    // ===== CONTENT =====
                    page.Content()
                        .AlignTop()
                        .AlignCenter()
                        .Container()
                        .Padding(1)
                        .Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.ConstantColumn(60);   // Дата
                                columns.ConstantColumn(30);   // №
                                columns.ConstantColumn(50);   // Район
                                columns.RelativeColumn();     // Актив
                                columns.RelativeColumn();     // Резерв
                                columns.RelativeColumn();     // Комментарий
                            });

                            // ===== TABLE HEADER =====
                            table.Header(header =>
                            {
                                header.Cell().Element(HeaderCell).Text($"{_localization["Pdf_Date"]}");
                                header.Cell().Element(HeaderCell).Text("№");
                                header.Cell().Element(HeaderCell).Text($"{_localization["Pdf_District"]}");
                                header.Cell().Element(HeaderCell).Text($"{_localization["Pdf_FullName"]}");
                                header.Cell().Element(HeaderCell).Text($"{_localization["Pdf_Reserve"]}");
                                header.Cell().Element(HeaderCell).Text($"{_localization["Day_Comment"]}");
                            });

                            // ===== TABLE BODY =====
                            foreach (var day in monthExport.Days)
                            {
                                var activeUsers = string.Join(", ",
                                    day.ActiveUserNames.Select(ToShortName));

                                var reserveUsers = string.Join(", ",
                                    day.ReserveUserNames.Select(ToShortName));

                                table.Cell().Element(BodyCell)
                                    .Text(day.Date.ToString("ddd dd.MM", culture));

                                table.Cell().Element(BodyCell).Text("1, 2");
                                table.Cell().Element(BodyCell).Text("1–5 мкр.");
                                table.Cell().Element(BodyCell).Text(activeUsers);
                                table.Cell().Element(BodyCell).Text(reserveUsers);
                                table.Cell().Element(BodyCell).Text(day.Comment ?? "");
                            }
                        });
                });
            });

            doc.GeneratePdf(filePath);

            Process.Start(new ProcessStartInfo
            {
                FileName = filePath,
                UseShellExecute = true
            });
        }

        // ===== HELPERS =====

        /// <summary>
        /// Returns short name format: "Last F.M."
        /// </summary>
        private static string ToShortName(string fullName)
        {
            if (string.IsNullOrWhiteSpace(fullName))
                return string.Empty;

            var parts = fullName
                .Split(' ', StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length == 1)
                return parts[0];

            if (parts.Length == 2)
                return $"{parts[0]} {parts[1][0]}.";

            // Иванов Иван Иванович → Иванов И.И.
            return $"{parts[0]} {parts[1][0]}.{parts[2][0]}.";
        }

        private static IContainer HeaderCell(IContainer container) =>
            container
                .Border(1)
                .BorderColor(Colors.Grey.Lighten2)
                .Background(Colors.Grey.Lighten4)
                .Padding(5)
                .AlignCenter()
                .AlignMiddle()
                .DefaultTextStyle(x => x.SemiBold());

        private static IContainer BodyCell(IContainer container) =>
            container
                .Border(1)
                .BorderColor(Colors.Grey.Lighten2)
                .Padding(5)
                .AlignMiddle();
    }
}
