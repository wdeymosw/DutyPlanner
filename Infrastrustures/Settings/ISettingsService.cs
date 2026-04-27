using System;
using System.Collections.Generic;
using System.Text;

namespace DutyPlanner.Infrastrustures.Settings
{
    public interface ISettingsService
    {
        public AppSettings Current { get; }

        event EventHandler? SettingsChanged;
        void Load();
        void Save();
    }
}
