using System.Diagnostics;
using System.IO;

namespace DutyPlanner.Services.Statistics
{
    internal static class FileLauncher
    {
        public static void OpenIfExists(string filePath)
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
