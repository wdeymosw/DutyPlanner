using DutyPlanner.Infrastructure.Localization;
using Microsoft.WindowsAPICodePack.Dialogs;

namespace DutyPlanner.Services
{
    public sealed class FileDialogService : IFileDialogService
    {
        private readonly ILocalizationService _localization;

        public FileDialogService(ILocalizationService localization)
        {
            _localization = localization;
        }

        public string? SelectFolder(string initialPath)
        {
            var dialog = new CommonOpenFileDialog
            {
                IsFolderPicker = true,
                InitialDirectory = initialPath,
                EnsurePathExists = true,
                Title = _localization["Settings_SelectFolder"]
            };

            return dialog.ShowDialog() == CommonFileDialogResult.Ok
                ? dialog.FileName
                : null;
        }
    }
}
