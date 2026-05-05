# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Build & Run

```bash
# Build
dotnet build DutyPlanner.sln

# Run
dotnet run --project DutyPlanner.csproj
```

### Tests

```bash
# Run all tests
dotnet test DutyPlanner.Tests/DutyPlanner.Tests.csproj

# Run a specific test class
dotnet test DutyPlanner.Tests/DutyPlanner.Tests.csproj --filter "FullyQualifiedName~UserServiceTests"
```

Test project uses **xUnit** + **NSubstitute** (mocking) + **FluentAssertions**. Tests live in `DutyPlanner.Tests/Unit/Services/`. Use `Substitute.For<IInterface>()` to mock repository dependencies; the service under test is constructed directly with the mock injected.

## Architecture Overview

DutyPlanner is a WPF desktop application (.NET 10 Windows) for managing duty schedules, with Excel and PDF export. It uses MVVM + Dependency Injection (`Microsoft.Extensions.DependencyInjection` registered in `App.xaml.cs`).

### Layer Structure

The codebase has a clean architecture target with two legacy layers (`Models/`, `Services/`) that haven't been fully migrated yet:

```
Domain/               — Repository interfaces only (IUserRepository, IDayRepository)
Application/          — Cross-cutting DTOs (MonthExportDto) and IFileLauncherService
Infrastructure/       — Implementations: FileStorage, JsonFileStorage, Localization,
                        Settings, MessageService, Persistence/Json/, Shell/
Models/               — Legacy: domain entities (User, YearMonth, MonthDescriptor) and
                        DTOs (DayUserDto, MonthStatisticsDto, etc.)
Services/             — Legacy: UserService, DialogServices/, ReportServices/,
                        Statistics/ (Excel + PDF export, month/year aggregates)
Presentation/         — ViewModels, Views (XAML), Windows, Converters, Behaviors,
                        Commands, Resources
```

Migration target: move `Models/` into `Domain/`/`Application/` and `Services/` into `Application/Services/`.

### DI Registration

`App.xaml.cs` registers ~20 services. Infrastructure services, core services, and main ViewModels (`MainWindowsViewModel`, `SidebarUsersViewModel`) are `Singleton`. Dialog ViewModels (`AddMonthDialogViewModel`, `AddDayDialogViewModel`, `SettingsViewModel`) and Windows are `Transient`. To add a new feature, register it there and inject via constructor. Switching from JSON to SQLite only requires:
1. Implementing `SqliteUserRepository : IUserRepository` and `SqliteDayRepository : IDayRepository`
2. Changing two lines in `App.xaml.cs`

### Services

`UserService` (legacy `Services/`) wraps `IUserRepository` with an **in-memory cache** — it loads the user list once and keeps mutations in memory, flushing to the repository on each write. If you add or remove a user and the sidebar doesn't reflect it, ensure all callers go through `UserService` rather than `IUserRepository` directly.

### Data Persistence

All data is stored as JSON files on disk:
- `Data/Users/users.json` — user list (via `IUserRepository` / `JsonUserRepository`)
- `Data/<YearMonth>/` — per-day JSON files (via `IDayRepository` / `JsonDayRepository`)

`IJsonFileStorage` in `Infrastructure/JsonFileStorage/` is the low-level JSON I/O abstraction. Both repository implementations wrap it. `IFileStorage` handles raw file operations. The `Data/` directory and `settings.json` are gitignored.

`AppSettings` (`Infrastructure/Settings/`) persists language, data folder path, export folder, last-opened month, and whether to open exports automatically. `MonthManagementService` reads `settings.Current.DataFolderPath` on every call (not in constructor) to react to runtime path changes.

### ViewModels and State

- `MainWindowsViewModel` — owns the `Pages` collection of `MonthPageViewModel` and all top-level commands.
- `MonthPageViewModel` — one per loaded month; holds a collection of `DayViewModel`, dispatches through `IDayRepository`.
- `DayViewModel` — one per day file; holds `ActiveUsers` / `ReserveUsers` as separate `ObservableCollection<DayUserViewModel>`. Saves asynchronously via `IDayRepository.SaveAsync`. `AddFromSidebar`, `MoveUser`, and export methods are `async void` fire-and-forget — they intentionally swallow exceptions with try/catch to prevent UI thread crashes. If the save fails silently, use `IMessageService` to surface the error rather than letting the exception propagate.
- `SidebarUsersViewModel` — left-panel user list; drag source for adding users to days.

**ViewModel base class** (`Presentation/ViewModels/Base/ViewModel.cs`) provides `Set<T>(ref field, value)` for property change notification — always use this instead of calling `OnPropertyChanged` manually.

**LambdaCommand** has three overloads — parameterless, `object?`-parameter, and generic `LambdaCommand<T>` for strongly-typed parameters. Optional `canExecute` predicate passed in constructor; call `RaiseCanExecuteChanged()` when its conditions change.

**BaseDialogViewModel** (`Presentation/ViewModels/Base/`) provides `OkCommand`, `CancelCommand`, and `RequestClose` event. New dialogs should inherit from it and implement `CanOk()`; call `RaiseOkCanExecuteChanged()` whenever the conditions that affect `CanOk()` change.

**Drag-and-drop**: `DragSourceBehavior` (static attached) starts `DragDrop.DoDragDrop()` from ListBox items. `DragTargetBehavior` drops onto a day panel and calls `DayViewModel.OnDrop(data, placement)` — placement is read from the target element's `Tag` property (`"Reserve"` = reserve slot). `OnDrop` is polymorphic: `UserViewModel` from the sidebar adds a new user; `DayUserViewModel` from within a day moves between Active/Reserve.

**InstanceId vs. domain Id**: `DayUserViewModel`, `UserViewModel`, and `DayUserDto` all carry both a domain `Id`/`UserID` (stable across saves) and a `InstanceId` (unique per ViewModel instance). Use `InstanceId` to track whether a user slot was removed and re-added (even with the same user), not for identity comparisons.

**`DayViewModel.ActiveUsers` / `ReserveUsers`** are `ObservableCollection` projections of an internal `_users` list (the single source of truth). When moving a user between placements, update `_users` first, then rebuild both collections.

### Localization

Three languages: Russian (`ru`), Ukrainian (`uk`), English (`en`). Resource dictionaries live in `Presentation/Resources/Localization/`. `ILocalizationService` swaps the entire `ResourceDictionary` at runtime and broadcasts `LanguageChanged`. Access strings via `_localization["Key"]` — missing keys return `!Key!`. Do not hardcode Russian text in services or infrastructure; always use localization keys.

When applying a language, all four `CultureInfo` properties must be set (`CurrentCulture`, `CurrentUICulture`, `DefaultThreadCurrentCulture`, `DefaultThreadCurrentUICulture`) — missing any one of them breaks calendar controls and date formatting.

`LocalizationValidator.Validate()` runs at startup and checks that all three resource dictionaries contain the same set of keys. When adding a new localization key, add it to all three files or the validator will throw.

### Key Dependencies

| Package | Purpose |
|---|---|
| `ClosedXML` | Excel export (`Services/Statistics/ExcelExportService`) |
| `QuestPDF` | PDF report generation (`Services/ReportServices/PdfMonthReportService`) |
| `FontAwesome5.WPF` | UI icons |
| `Microsoft.Extensions.DependencyInjection` | DI container |
| `Microsoft-WindowsAPICodePack-Shell` | Windows shell integration |
