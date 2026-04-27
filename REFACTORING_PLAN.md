# План рефакторинга DutyPlanner под чистую архитектуру

## Выявленные проблемы

| # | Проблема | Файл | Приоритет |
|---|----------|------|-----------|
| 1 | `File.WriteAllText` / `JsonSerializer` прямо в ViewModel | `Presentation/ViewModels/DayViewModel.cs` | Критический |
| 2 | `Directory.GetFiles`, `File.Delete` прямо в ViewModel | `Presentation/ViewModels/MonthPageViewModel.cs` | Критический |
| 3 | `DayUserDto` импортирует ViewModel — циклическая зависимость | `Models/Dto/DayUserDto.cs` | Критический |
| 4 | `UserService.Load()` обходит `IJsonFileStorage`, хардкодит путь | `Services/UserService.cs` | Критический |
| 5 | `IMonthReportService` принимает `MonthPageViewModel` как параметр | `Services/ReportServices/IMonthReportService.cs` | Высокий |
| 6 | Статический `FileLauncher` вызывается из ViewModels | `Services/Statistics/FileLauncher.cs` | Высокий |
| 7 | `IExcelExportService` принимает `ILocalizationService` в параметрах метода (уже инжектирован в конструктор) | `Services/Statistics/IExcelExportService.cs` | Средний |
| 8 | Хардкодные русские строки в `MessageService` (заголовки по умолчанию) | `Infrastrustures/MessageService/MessageService.cs` | Средний |
| 9 | Хардкодная русская строка `"Статистика за..."` | `Presentation/ViewModels/Statistics/YearStatisticsViewModel.cs` | Средний |
| 10 | Хардкодная русская строка в `RemoveMonth()` | `Presentation/ViewModels/MainWindowsViewModel.cs` | Средний |
| 11 | Хардкодный русский заголовок в `FileDialogService` | `Infrastrustures/MessageService/FileDialogService.cs` | Средний |
| 12 | Опечатка `Infrastrustures` вместо `Infrastructure` (~25 файлов) | весь проект | Средний |
| 13 | `MonthManagementService` захватывает путь в конструкторе — не реагирует на смену настроек | `Services/ReportServices/MonthManagementService.cs` | Средний |
| 14 | `MonthDescriptor.FolderPath` — файловый артефакт в доменной модели | `Models/Dto/MonthDescriptor.cs` | Низкий |
| 15 | Нет `async`/`await` — файловый I/O блокирует UI-поток | весь проект | Низкий |
| 16 | `Strings.en.xaml` содержит только 7 из 45+ ключей | `Presentation/Resources/Localization/` | Низкий |

---

## Целевая структура

```
DutyPlanner/
├── Domain/                              ← новый слой, нет внешних зависимостей
│   ├── Entities/
│   │   ├── User.cs                      (перенести из Models/)
│   │   └── DayEntry.cs                  (новый — доменная сущность дня)
│   ├── ValueObjects/
│   │   └── YearMonth.cs                 (перенести из Models/)
│   └── Repositories/                    ← только интерфейсы
│       ├── IUserRepository.cs           (новый)
│       └── IDayRepository.cs            (новый — ключевая абстракция для SQLite)
│
├── Application/                         ← новый слой, зависит только от Domain
│   ├── Interfaces/
│   │   ├── IUserService.cs              (перенести из Services/Interfaces/)
│   │   ├── IMonthManagementService.cs   (перенести из Services/ReportServices/)
│   │   ├── IMonthStatisticsService.cs   (перенести из Services/Statistics/)
│   │   ├── IYearStatisticsService.cs    (перенести из Services/Statistics/)
│   │   ├── IYearPeriodService.cs        (перенести из Services/Statistics/)
│   │   ├── IExcelExportService.cs       (перенести, убрать localization из сигнатуры)
│   │   ├── IMonthReportService.cs       (перенести, заменить ViewModel → DTO)
│   │   ├── IMessageService.cs           (перенести из Infrastrustures/)
│   │   └── IFileLauncherService.cs      (новый — замена статическому FileLauncher)
│   ├── DTOs/
│   │   ├── DayUserDto.cs                (перенести из Models/Dto/, убрать зависимость от VM)
│   │   ├── MonthExportDto.cs            (новый — заменяет MonthPageViewModel в сигнатуре)
│   │   ├── MonthStatisticsDto.cs        (перенести из Models/Dto/)
│   │   ├── MonthStatisticsRowDto.cs     (перенести из Models/Dto/)
│   │   ├── YearStatisticsDto.cs         (перенести из Models/Dto/)
│   │   └── YearStatisticsRowDto.cs      (перенести из Models/Dto/)
│   └── Services/
│       ├── UserService.cs               (перенести, переписать под IUserRepository)
│       ├── MonthManagementService.cs    (перенести, читать путь при каждом вызове)
│       ├── MonthStatisticsService.cs    (перенести, использует IDayRepository)
│       ├── YearStatisticsService.cs     (перенести)
│       ├── YearPeriodService.cs         (перенести)
│       ├── ExcelExportService.cs        (перенести, убрать localization из параметров)
│       └── PdfMonthReportService.cs     (перенести, сигнатура через DTO)
│
├── Infrastructure/                      ← переименовать из Infrastrustures/
│   ├── Persistence/
│   │   ├── Json/
│   │   │   ├── IJsonFileStorage.cs      (перенести)
│   │   │   ├── JsonFileStorage.cs       (перенести, добавить async)
│   │   │   ├── JsonUserRepository.cs    (новый — реализует IUserRepository)
│   │   │   └── JsonDayRepository.cs     (новый — реализует IDayRepository)
│   │   └── FileStorage/
│   │       ├── IFileStorage.cs          (перенести)
│   │       └── FileStorage.cs           (перенести)
│   ├── Settings/
│   │   ├── AppSettings.cs               (перенести)
│   │   ├── ISettingsService.cs          (перенести)
│   │   └── SettingsService.cs           (перенести)
│   ├── Localization/
│   │   ├── ILocalizationService.cs      (перенести)
│   │   ├── LocalizationService.cs       (перенести)
│   │   └── LocalizationValidator.cs     (перенести, добавить проверку паритета ключей)
│   ├── Dialogs/
│   │   ├── IFileDialogService.cs        (перенести из Services/DialogServices/)
│   │   └── FileDialogService.cs         (перенести, заголовок из локализации)
│   ├── Shell/
│   │   └── FileLauncherService.cs       (новый — заменяет статический FileLauncher)
│   └── Messaging/
│       └── MessageService.cs            (перенести, убрать хардкод RU)
│
└── Presentation/                        ← структура папок без изменений
    ├── Commands/
    │   ├── CommandBase.cs               (перенести из Infrastrustures/Commands/)
    │   └── LambdaCommand.cs             (перенести)
    └── ViewModels/
        ├── DayViewModel.cs              (переписать — убрать весь System.IO)
        ├── MonthPageViewModel.cs        (переписать — убрать весь System.IO)
        └── ...                          (остальные без изменений структуры)
```

---

## Фазы выполнения

### Фаза 1 — Только добавление (нулевой риск)

Создаём новые файлы, не трогаем существующие. Билд зелёный после каждого шага.

- [x] **1.1** Создать `Domain/Repositories/IUserRepository.cs`
- [x] **1.2** Создать `Domain/Repositories/IDayRepository.cs`
- [x] **1.3** Создать `Application/Interfaces/IFileLauncherService.cs`
- [x] **1.4** Создать `Infrastructure/Shell/FileLauncherService.cs` (логика из статического `FileLauncher`)
- [x] **1.5** Создать `Infrastructure/Persistence/Json/JsonUserRepository.cs` (реализует `IUserRepository`)
- [x] **1.6** Создать `Infrastructure/Persistence/Json/JsonDayRepository.cs` (реализует `IDayRepository`)
- [x] **1.7** Исправить `DayUserDto` — убрать `using DutyPlanner.Presentation.ViewModels`, убрать конструктор от `UserViewModel`

### Фаза 2 — Устранение критических нарушений

Каждый шаг изолирован, система остаётся рабочей.

- [x] **2.1** `UserService` → использовать `IUserRepository` (убрать сырой `File.ReadAllText`)
- [x] **2.2** `DayViewModel` → использовать `IDayRepository` (убрать весь `System.IO` и `JsonSerializer`)
- [x] **2.3** `MonthPageViewModel` → использовать `IDayRepository` (убрать весь `System.IO`)
- [x] **2.4** Создать `Application/DTOs/MonthExportDto.cs`, изменить сигнатуру `IMonthReportService`
- [x] **2.5** `IExcelExportService` — убрать `ILocalizationService` из параметров методов
- [x] **2.6** Заменить вызовы статического `FileLauncher` на `IFileLauncherService` в ViewModels
- [x] **2.7** `MessageService` — локализовать заголовки по умолчанию
- [x] **2.8** `YearStatisticsViewModel.Title` — заменить `"Статистика за..."` на ключ локализации
- [x] **2.9** `MainWindowsViewModel.RemoveMonth()` — заменить русскую строку на ключ локализации
- [x] **2.10** `FileDialogService` — заголовок из локализации вместо хардкода

### Фаза 3 — Переименование namespace (один атомарный коммит)

> **Важно:** все ~25 файлов переименовываются в одном коммите, иначе билд сломан.

- [x] **3.1** Переименовать папку `Infrastrustures/` → `Infrastructure/`
- [x] **3.2** Заменить все `namespace DutyPlanner.Infrastrustures` → `DutyPlanner.Infrastructure`
- [x] **3.3** Заменить все `using DutyPlanner.Infrastrustures` → `using DutyPlanner.Infrastructure`
- [x] **3.4** Переместить `Commands/` из `Infrastructure/` в `Presentation/Commands/`

### Фаза 4 — Подготовка к SQLite

- [ ] **4.1** Убрать `FolderPath` из `MonthDescriptor` — это файловый артефакт, не доменная модель
- [ ] **4.2** `MonthManagementService` — читать путь из `settings.Current.DataFolderPath` при каждом вызове, не в конструкторе

### Фаза 5 — Async I/O

> Выполнять после стабилизации Фаз 1–4.

- [ ] **5.1** `IJsonFileStorage` — добавить `LoadAsync<T>` / `SaveAsync<T>`
- [ ] **5.2** `IDayRepository` — добавить `LoadDayAsync` / `SaveDayAsync`
- [ ] **5.3** `DayViewModel.SaveUsers()` → `async Task SaveUsersAsync()`
- [ ] **5.4** `ExcelExportService` → `Task ExportMonthStatisticsAsync(...)` / `Task ExportYearStatisticsAsync(...)`
- [ ] **5.5** `PdfMonthReportService` → `Task ExportMonthPdfAsync(...)`

### Фаза 6 — Полнота локализации (независимо от остального)

- [ ] **6.1** Расширить `LocalizationValidator` — проверять паритет ключей между языками
- [ ] **6.2** Дополнить `Strings.en.xaml` с 7 до 45+ ключей
- [ ] **6.3** Аудит `Strings.uk.xaml` на соответствие `Strings.ru.xaml`
- [ ] **6.4** Добавить новые ключи из Фазы 2: `MainWindow_ConfirmDeleteMonth`, `Settings_SelectFolder`

---

## Порядок зависимостей

```
Фаза 1 (новые интерфейсы и репозитории)
        │
        ▼
Фаза 2 (устранение нарушений)
        │
        ▼
Фаза 3 (переименование namespace) ← один атомарный коммит
        │
        ▼
Фаза 4 (MonthDescriptor, настройки пути)
        │
        ▼
Фаза 5 (async I/O)

Фаза 6 ──── в любое время, независимо от остальных
```

**Независимые шаги (можно делать параллельно):**
- Фаза 6 целиком
- Шаги 2.4, 2.5 — только интерфейс и один вызов в VM
- Шаги 2.7–2.10 — каждый затрагивает один файл
- Шаг 4.2 — один файл, без изменения интерфейса

**Обязательный порядок:**
- Шаг 1.7 (исправить `DayUserDto`) → перед Шагом 2.2 (`DayViewModel`)
- Шаг 1.5 (`JsonUserRepository`) → перед Шагом 2.1 (`UserService`)
- Шаг 1.6 (`JsonDayRepository`) → перед Шагами 2.2 и 2.3
- Фаза 2 полностью → перед Фазой 3 (чтобы не переименовывать файлы дважды)

---

## Готовность к SQLite после рефакторинга

После завершения Фаз 1–4 миграция на SQLite сведётся к:

1. Добавить NuGet: `Microsoft.Data.Sqlite` / `Microsoft.EntityFrameworkCore.Sqlite`
2. Написать `SqliteUserRepository` реализующий `IUserRepository`
3. Написать `SqliteDayRepository` реализующий `IDayRepository`
4. В `App.xaml.cs` изменить **2 строки** регистрации DI:
   ```csharp
   // было:
   services.AddSingleton<IUserRepository, JsonUserRepository>();
   services.AddSingleton<IDayRepository, JsonDayRepository>();

   // стало:
   services.AddSingleton<IUserRepository, SqliteUserRepository>();
   services.AddSingleton<IDayRepository, SqliteDayRepository>();
   ```

Ни один ViewModel, ни один Application-сервис, ни одна статистика не изменится.
