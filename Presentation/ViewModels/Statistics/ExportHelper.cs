using DutyPlanner.Application.Interfaces;
using DutyPlanner.Infrastructure.Localization;
using DutyPlanner.Infrastructure.MessageService;
using DutyPlanner.Infrastructure.Settings;
using System.IO;

namespace DutyPlanner.Presentation.ViewModels
{
    internal static class ExportHelper
    {
        internal static async Task RunExportAsync(
            string filePath,
            Func<Task> exportAction,
            ISettingsService settings,
            IFileLauncherService fileLauncher,
            IMessageService messages,
            ILocalizationService localization)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);
            try
            {
                await exportAction();

                if (settings.Current.OpenExcelAfterExport)
                    fileLauncher.OpenIfExists(filePath);

                messages.ShowInfo(
                    localization["Excel_Successful_Preservation"],
                    localization["Excel_ExportCompleted"]);
            }
            catch (IOException)
            {
                messages.ShowError(BuildIoErrorText(localization), localization["Excel_ExportCompleted"]);
            }
            catch (Exception ex)
            {
                messages.ShowError(
                    $"{localization["Exel_Error_Export_2"]}\n\n{ex.Message}",
                    localization["Excel_ExportCompleted"]);
            }
        }

        private static string BuildIoErrorText(ILocalizationService localization) =>
            $"{localization["Exel_Error_Export"]}\n\n" +
            $"{localization["Excel_Strign_1"]}\n" +
            $"{localization["Excel_Strign_2"]}\n" +
            $"{localization["Excel_Strign_3"]}\n" +
            $"{localization["Excel_Strign_4"]}\n\n" +
            localization["Excel_String_5"];
    }
}
