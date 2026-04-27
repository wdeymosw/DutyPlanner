using DutyPlanner.Infrastrustures.Settings;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace DutyPlanner.Infrastrustures.FileStorage
{
    public sealed class FileStorage: IFileStorage
    {
        private readonly ISettingsService _settings;

        public FileStorage(ISettingsService settings)
        {
            _settings = settings;
        }

        public void Save(string relativePath, byte[] data)
        {
            var fullPath = GetFullPath(relativePath);

            var dir = Path.GetDirectoryName(fullPath);
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir!);

            File.WriteAllBytes(fullPath, data);
        }

        public bool Exists(string relativePath)
            => File.Exists(GetFullPath(relativePath));

        public string GetFullPath(string relativePath)
            => Path.Combine(_settings.Current.DataFolderPath, relativePath);
    }
}
