using System;
using System.Collections.Generic;
using System.Text;

namespace DutyPlanner.Infrastructure.Settings
{
    public interface ISettingsService
    {
        public AppSettings Current { get; }

        event EventHandler? SettingsChanged;
        void Load();
        void Save();
    }
}
