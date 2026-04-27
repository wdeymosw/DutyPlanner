using DutyPlanner.Application.Interfaces;
using System.Diagnostics;
using System.IO;

namespace DutyPlanner.Infrastructure.Shell
{
    internal class FileLauncherService : IFileLauncherService
    {
        public void OpenIfExists(string filePath)
        {
            if (!File.Exists(filePath))
                return;

            Process.Start(new ProcessStartInfo
            {
                FileName = filePath,
                UseShellExecute = true
            });
        }
    }
}
