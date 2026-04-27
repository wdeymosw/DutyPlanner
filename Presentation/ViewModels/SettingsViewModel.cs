using DutyPlanner.Presentation.Commands;
using DutyPlanner.Infrastructure.Settings;
using DutyPlanner.Models;
using DutyPlanner.Presentation.ViewModels.Base;
using DutyPlanner.Services;
using System.Globalization;
using System.IO;

namespace DutyPlanner.Presentation.ViewModels
{
    public sealed class SettingsViewModel : BaseDialogViewModel, IRequestCloseViewModel
    {
        private readonly ISettingsService _settings;
        private readonly IFileDialogService _fileDialog;

        public IReadOnlyList<MonthItem> Months { get; }


        public event Action<bool?>? RequestClose;


        // Editable copy of settings
        public AppSettings Editable { get; }

        private MonthItem _selectedMonth;
        public MonthItem SelectedMonth
        {
            get => _selectedMonth;
            set
            {
                _selectedMonth = value;
                Editable.YearStartMonth = value.Month;
                OnPropertyChanged();
            }
        }

        public string DataFolderPath
        {
            get => Editable.DataFolderPath;
            set
            {
                Editable.DataFolderPath = value;
                OnPropertyChanged();
            }
        }

        public bool OpenPdfAfterExport
        {
            get => Editable.OpenPdfAfterExport;
            set
            {
                Editable.OpenPdfAfterExport = value;
                OnPropertyChanged();
            }
        }

        public bool OpenExcelAfterExport
        {
            get => Editable.OpenExcelAfterExport;
            set
            {
                Editable.OpenExcelAfterExport = value;
                OnPropertyChanged();
            }
        }




        #region Commands

        public LambdaCommand BrowseDataFolderCommand { get; }

        #endregion

        public SettingsViewModel(ISettingsService settings, IFileDialogService fileDialog)
        {
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
            _fileDialog = fileDialog ?? throw new ArgumentNullException(nameof(fileDialog));

            Editable = Clone(settings.Current);

            BrowseDataFolderCommand = new LambdaCommand(BrowseDataFolder);

            var culture = new CultureInfo(Editable.Language);

            Months = Enumerable.Range(1, 12)
                .Select(m => new MonthItem
                {
                    Month = m,
                    Name = culture.DateTimeFormat.GetMonthName(m)
                })
                .ToList();

            _selectedMonth = Months.First(m => m.Month == Editable.YearStartMonth);
        }

        protected override bool CanOk()
        {
            if (string.IsNullOrWhiteSpace(Editable.DataFolderPath)) return false;

            if (string.IsNullOrWhiteSpace(Editable.Language)) return false;

            return true;
        }


        protected override void OnOk()
        {
            ApplyChanges();
            RequestClose?.Invoke(true);
        }


        protected override void OnCancel()
        {

            RequestClose?.Invoke(false);
        }

        private void ApplyChanges()
        {
            var target = _settings.Current;

            target.DataFolderPath = Editable.DataFolderPath;
            target.DefaultExportFolder = Editable.DefaultExportFolder;
            target.Language = Editable.Language;
            target.OpenPdfAfterExport = Editable.OpenPdfAfterExport;
            target.OpenExcelAfterExport = Editable.OpenExcelAfterExport;
            target.YearStartMonth = Editable.YearStartMonth;
            target.LastOpenedYear = Editable.LastOpenedYear;
            target.LastOpenedMonth = Editable.LastOpenedMonth;

            _settings.Save();
        }

        private static AppSettings Clone(AppSettings source)
        {
            return new AppSettings
            {
                DataFolderPath = source.DataFolderPath,
                DefaultExportFolder = source.DefaultExportFolder,
                Language = source.Language,
                OpenPdfAfterExport = source.OpenPdfAfterExport,
                OpenExcelAfterExport = source.OpenExcelAfterExport,
                YearStartMonth = source.YearStartMonth,
                LastOpenedYear = source.LastOpenedYear,
                LastOpenedMonth = source.LastOpenedMonth
            };
        }


        private void BrowseDataFolder()
        {
            var selected = _fileDialog.SelectFolder(
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, DataFolderPath));

            if (selected == null)
                return;

            DataFolderPath = Path.GetRelativePath(
                AppDomain.CurrentDomain.BaseDirectory,
                selected);
        }


    }
}
