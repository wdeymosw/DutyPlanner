using Microsoft.WindowsAPICodePack.Dialogs;

namespace DutyPlanner.Services
{
    public sealed class FileDialogService : IFileDialogService
    {
        public string? SelectFolder(string initialPath)
        {
            var dialog = new CommonOpenFileDialog
            {
                IsFolderPicker = true,
                InitialDirectory = initialPath,
                EnsurePathExists = true,
                Title = "Выберите папку данных"
            };

            return dialog.ShowDialog() == CommonFileDialogResult.Ok
                ? dialog.FileName
                : null;
        }
    }
}
