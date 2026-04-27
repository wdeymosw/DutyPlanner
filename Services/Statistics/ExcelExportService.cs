using ClosedXML.Excel;
using DutyPlanner.Infrastructure.Localization;
using DutyPlanner.Models;
using DutyPlanner.Models.Dto;
using System.Globalization;

namespace DutyPlanner.Services
{
    public sealed class ExcelExportService : IExcelExportService
    {

        private readonly ILocalizationService _localization;

        public ExcelExportService(ILocalizationService localization)
        {
            _localization = localization;
        }

        public void ExportMonthStatistics(MonthStatisticsDto data, string filePath)
        {
            

            using var workbook = new XLWorkbook();
            var ws = workbook.Worksheets.Add($"{data.Year}-{data.Month:00}");

            int row = 1;
            int col = 1;

            // ===== HEADER =====

            ws.Cell(row, col++).Value = $"{_localization["Common_Users"]}";

            foreach (var day in data.Days)
                ws.Cell(row, col++).Value = day.Day;

            ws.Cell(row, col).Value = $"{_localization["Dialogs_Total"]}";

            var header = ws.Range(1, 1, 1, col);
                header.Style.Font.Bold = true;
                header.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                header.Style.Fill.BackgroundColor = XLColor.LightGray;

            // ===== ROWS =====

            row++;

            // фиксируем диапазон дней 
            int firstDayCol = 2;
            int lastDayCol = firstDayCol + data.Days.Count - 1;

            foreach (var user in data.Rows)
            {
                col = 1;

                ws.Cell(row, col++).Value = user.UserName;

                foreach (var day in data.Days)
                {
                    ws.Cell(row, col).Value = user.HoursByDay[day];
                    ws.Cell(row, col).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    col++;
                }

                // ИТОГО — ФОРМУЛА
                var totalCell = ws.Cell(row, lastDayCol + 1);
                totalCell.FormulaA1 = $"SUM({ws.Cell(row, firstDayCol).Address}:{ws.Cell(row, lastDayCol).Address})";
                totalCell.Style.Font.Bold = true;

                row++;
            }

            ws.Columns().AdjustToContents();
            ws.RangeUsed().SetAutoFilter(false);
            ws.SheetView.FreezeRows(1);

            workbook.SaveAs(filePath);
        }

        // --- годовой экспорт уже обсуждали ---


        public void ExportYearStatistics(YearStatisticsDto data, string filePath)
        {
            using var workbook = new XLWorkbook();
            var ws = workbook.Worksheets.Add(
                $"{data.PeriodStart:yyyyMM}-{data.PeriodEnd:yyyyMM}");

            int row = 1;
            int col = 1;

            ws.Cell(row, col++).Value = _localization["Common_Users"];

            var culture = new CultureInfo(_localization.CurrentLanguage);

            foreach (var ym in data.Months)
            {
                ws.Cell(row, col++).Value =
                    new DateTime(ym.Year, ym.Month, 1)
                        .ToString("MMM yyyy", culture);
            }

            ws.Cell(row, col).Value = _localization["Dialogs_Total"];

            var header = ws.Range(1, 1, 1, col);
            header.Style.Font.Bold = true;
            header.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            header.Style.Fill.BackgroundColor = XLColor.LightGray;

            row++;

            int firstMonthCol = 2;
            int lastMonthCol = firstMonthCol + data.Months.Count - 1;

            foreach (var user in data.Rows)
            {
                col = 1;
                ws.Cell(row, col++).Value = user.UserName;

                foreach (var ym in data.Months)
                {
                    ws.Cell(row, col).Value =
                        user.HoursByMonth.TryGetValue(ym, out var hours)
                            ? hours
                            : 0;

                    ws.Cell(row, col).Style.Alignment.Horizontal =
                        XLAlignmentHorizontalValues.Center;
                    col++;
                }

                var totalCell = ws.Cell(row, lastMonthCol + 1);
                totalCell.FormulaA1 =
                    $"SUM({ws.Cell(row, firstMonthCol).Address}:{ws.Cell(row, lastMonthCol).Address})";
                totalCell.Style.Font.Bold = true;

                row++;
            }

            ws.Columns().AdjustToContents();
            ws.SheetView.FreezeRows(1);

            workbook.SaveAs(filePath);
        }

        private static string MonthName(int month)
            => new DateTime(2000, month, 1).ToString("MMMM");
    }
}

