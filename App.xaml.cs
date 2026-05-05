using System.Windows;
using Microsoft.Extensions.DependencyInjection;

using DutyPlanner.Application.Interfaces;
using DutyPlanner.Domain.Repositories;
using DutyPlanner.Infrastructure.Persistence.Json;
using DutyPlanner.Infrastructure.Shell;
using DutyPlanner.Presentation.ViewModels;
using DutyPlanner.Presentation.Windows;
using DutyPlanner.Services;
using DutyPlanner.Infrastructure.Localization;
using DutyPlanner.Infrastructure.Settings;
using DutyPlanner.Infrastructure.MessageService;
using DutyPlanner.Infrastructure.FileStorage;
using DutyPlanner.Infrastructure.JsonFileStorage;
using DutyPlanner.Services.Statistics;

namespace DutyPlanner
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : System.Windows.Application
    {
        public IServiceProvider Services { get; private set; } = null!;

        public App()
        {
            InitializeComponent();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var services = new ServiceCollection();

            //services
            services.AddSingleton<ISettingsService, SettingsService>(); // For managing application settings
            services.AddSingleton<IFileDialogService, FileDialogService>(); // For showing file dialogs
            services.AddSingleton<IJsonFileStorage, JsonFileStorage>();// For storing data in JSON files
            services.AddSingleton<IFileStorage, FileStorage>(); // For general file operations

            services.AddSingleton<IUserRepository, JsonUserRepository>(); // JSON-backed user repository
            services.AddSingleton<IDayRepository, JsonDayRepository>(); // JSON-backed day repository
            services.AddSingleton<IFileLauncherService, FileLauncherService>(); // For opening files in shell

            services.AddSingleton<IUserService, UserService>(); // For managing users
            services.AddSingleton<IDialogService, DialogService>(); // For showing dialogs
            services.AddSingleton<IMessageService, MessageService>(); // For showing messages to the user
            
            services.AddSingleton<IMonthManagementService, MonthManagementService>(); // For managing months
            services.AddSingleton<IMonthStatisticsService, MonthStatisticsService>(); // For calculating month statistics
            services.AddSingleton<IYearPeriodService, YearPeriodService>();                                                                          // 
            services.AddSingleton<IYearStatisticsService, YearStatisticsService>(); // For calculating year statistics

            services.AddSingleton<ILocalizationService, LocalizationService>(); // For localization


            services.AddSingleton<IExcelExportService, ExcelExportService>(); // For exporting data to Excel
            services.AddSingleton<IMonthReportService, PdfMonthReportService>();// For generating month reports in PDF format
            services.AddSingleton<IUserPeriodReportPdfService, UserPeriodReportPdfService>();


            //viewModels
            services.AddSingleton<MainWindowsViewModel>();
            services.AddSingleton<SidebarUsersViewModel>();
            services.AddTransient<AddMonthDialogViewModel>();
            services.AddTransient<AddDayDialogViewModel>();
            services.AddTransient<SettingsViewModel>();



            //Windows
            services.AddTransient<MainWindow>();
            services.AddTransient<AddMonthDialog>();
            services.AddTransient<AddDayDialog>();
            services.AddTransient<SettingsDialog>();
            services.AddTransient<UserPeriodReportWindow>();





            Services = services.BuildServiceProvider();


            LocalizationValidator.Validate();// Check localization files correctness

            // ====== Load settings and apply language ======
            var settings = Services.GetRequiredService<ISettingsService>();
            // Localization service
            var localization = Services.GetRequiredService<ILocalizationService>();
            // Load settings
            settings.Load();
            // Apply language
            localization.ApplyLanguage(settings.Current.Language);
            // ========================
            // Show main window
            var mainWindow = Services.GetRequiredService<MainWindow>();
            // Show
            mainWindow.Show();
        }
    }

}
