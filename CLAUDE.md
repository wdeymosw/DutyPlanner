# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Build & Run

```bash
# Build
dotnet build DutyPlanner.sln

# Run
dotnet run --project DutyPlanner.csproj
```

There are no automated tests in this project.

## Architecture Overview

DutyPlanner is a WPF desktop application (.NET 10 Windows) for managing duty schedules, with Excel and PDF export. It uses MVVM + Dependency Injection (`Microsoft.Extensions.DependencyInjection` registered in `App.xaml.cs`).

### Layer Structure

```
Models/           — Domain entities (User, YearMonth, MonthDescriptor) and DTOs
Services/         — Service interfaces and implementations (user, month, statistics, export)
Infrastrustures/  — Infrastructure: commands, file storage, localization, settings, messaging
                    NOTE: folder name has a typo ("Infrastrustures" not "Infrastructure") — do not rename without a full grep/replace across ~25 files
Presentation/     — ViewModels, Views (XAML), Windows, Converters, Behaviors, Resources
```

### Data Persistence

All data is stored as JSON files on disk:
- `Data/Users/users.json` — user list
- `Data/<YearMonth>/` — monthly schedule data in JSON files

`IJsonFileStorage<T>` in `Infrastrustures/JsonFileStorage/` is the intended abstraction for JSON I/O. The `Data/` directory and `settings.json` are gitignored.

### Known Architectural Issues (see REFACTORING_PLAN.md)

A detailed refactoring roadmap exists at `REFACTORING_PLAN.md`. Key violations to be aware of when editing:

- **DayViewModel** and **MonthPageViewModel** contain direct `File`/`Directory` I/O — should go through services
- **DayUserDto** (in `Models/Dto/`) imports from `Presentation.ViewModels` — circular dependency
- **UserService** uses `File.ReadAllText` directly instead of `IJsonFileStorage`
- **IMonthReportService** / `PdfMonthReportService` accept a ViewModel as a parameter — violates layer separation
- **MessageService** has hardcoded Russian strings instead of using `ILocalizationService`
- **MonthDescriptor** (domain model) holds a `FolderPath` — file-system detail leaking into domain

When adding new features, route file I/O through `IJsonFileStorage`/`IFileStorage` and avoid referencing `Presentation` from `Models` or `Services`.

### Localization

Three languages are supported: Russian (`ru`), Ukrainian (`uk`), English (`en`). Resource dictionaries live in `Presentation/Resources/`. English localization is incomplete (~7 of 45+ keys). Use `ILocalizationService` to retrieve strings; do not hardcode Russian text in services or infrastructure.

### Key Dependencies

| Package | Purpose |
|---|---|
| `ClosedXML` | Excel export |
| `QuestPDF` | PDF report generation |
| `FontAwesome5.WPF` | UI icons |
| `Microsoft.Extensions.DependencyInjection` | DI container |
| `Microsoft-WindowsAPICodePack-Shell` | Windows shell integration |
