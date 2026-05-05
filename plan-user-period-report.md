# План: Отчёт по пользователю за период

## Суть фичи

В контекстном меню каждого пользователя в сайдбаре добавляется пункт «Отчёт за период».
По нажатию открывается модальное окно с разбивкой часов по месяцам за текущий отчётный период,
итогом, сравнением с минимальной нормой (из настроек) и подсчётом, сколько часов ещё нужно до нормы.

Отчётный период берётся из `YearPeriodService.GetPeriod(DateTime.Today.Year)` —
тот же период, что используется в «Статистика за год».

---

## Шаг 1 — AppSettings: добавить MinimumHours

**Файл:** `Infrastructure/Settings/AppSettings.cs`

```csharp
public int MinimumHours { get; set; } = 72;
```

---

## Шаг 2 — SettingsViewModel: поле MinimumHours

**Файл:** `Presentation/ViewModels/SettingsViewModel.cs`

Добавить:
- приватное поле `_minimumHoursText` (string), инициализируется из `source.MinimumHours.ToString()`
- свойство `MinimumHoursText` (string) с вызовом `RaiseOkCanExecuteChanged()` в сеттере
- в `CanOk()` — проверка `int.TryParse(MinimumHoursText, out var v) && v >= 0`
- в `ApplyChanges()` — `target.MinimumHours = int.Parse(MinimumHoursText)`
- в `Clone()` — `MinimumHours = source.MinimumHours`
- в конструкторе — инициализация `_minimumHoursText`

---

## Шаг 3 — SettingsDialog.xaml: поле ввода нормы часов

**Файл:** `Presentation/Windows/SettingsDialog.xaml`

Между блоком ComboBox месяца и блоком Language добавить:

```xml
<Label Content="{DynamicResource Settings_MinimumHours}"/>
<TextBox Text="{Binding MinimumHoursText, UpdateSourceTrigger=PropertyChanged}"
         PreviewTextInput="NumbersOnly_PreviewTextInput"/>
```

Обработчик `NumbersOnly_PreviewTextInput` — в code-behind `SettingsDialog.xaml.cs`:
принимает только цифры (regex `^\d+$` на введённый символ).

Также увеличить `Height` окна с `450` до `500`.

---

## Шаг 4 — Локализация (3 файла)

**Файлы:**
- `Presentation/Resources/Localization/Strings.ru.xaml`
- `Presentation/Resources/Localization/Strings.en.xaml`
- `Presentation/Resources/Localization/Strings.uk.xaml`

Добавить ключи (пример для ru):

| Ключ | ru | en | uk |
|---|---|---|---|
| `Settings_MinimumHours` | Минимум часов за период | Minimum hours per period | Мінімум годин за період |
| `Sidebar_ShowReport` | Отчёт за период | Period report | Звіт за період |
| `UserReport_Period` | Период | Period | Період |
| `UserReport_Month` | Месяц | Month | Місяць |
| `UserReport_Hours` | Часы | Hours | Години |
| `UserReport_Total` | Итого | Total | Всього |
| `UserReport_MinNorm` | Норма | Norm | Норма |
| `UserReport_Remaining` | До нормы | To norm | До норми |
| `UserReport_NormDone` | Норма выполнена | Norm achieved | Норму виконано |

---

## Шаг 5 — UserViewModel: добавить ShowReportCommand

**Файл:** `Presentation/ViewModels/UserViewModel.cs`

- Добавить в конструктор параметр `Action<UserViewModel> showReport`
- Добавить `ShowReportCommand = new LambdaCommand(() => showReport(this))`

---

## Шаг 6 — SidebarUsersViewModel: callback и зависимость от IDialogService

**Файл:** `Presentation/ViewModels/SidebarUsersViewModel.cs`

- Добавить в конструктор параметр `IDialogService dialogService`
- В `CreateUserVm` передать callback:
  ```csharp
  showReport: user => _dialogService.ShowUserPeriodReport(user.Id, user.Name)
  ```

---

## Шаг 7 — SidebarUsersListView.xaml: пункт контекстного меню

**Файл:** `Presentation/Views/SidebarUsersListView.xaml`

В `ContextMenu` после `<Separator/>` и до `Remove` добавить:

```xml
<MenuItem Header="{DynamicResource Sidebar_ShowReport}"
          Command="{Binding ShowReportCommand}"/>
<Separator/>
```

---

## Шаг 8 — IDialogService: добавить метод

**Файл:** `Services/DialogServices/IDialogService.cs`

```csharp
void ShowUserPeriodReport(Guid userId, string userName);
```

---

## Шаг 9 — DialogService: реализация

**Файл:** `Services/DialogServices/DialogService.cs`

```csharp
public void ShowUserPeriodReport(Guid userId, string userName)
{
    var yearPeriodService = _services.GetRequiredService<IYearPeriodService>();
    var yearStatsService  = _services.GetRequiredService<IYearStatisticsService>();
    var settingsService   = _services.GetRequiredService<ISettingsService>();

    var (start, end)   = yearPeriodService.GetPeriod(DateTime.Today.Year);
    var data           = yearStatsService.BuildPeriod(start, end);
    var minimumHours   = settingsService.Current.MinimumHours;

    var vm     = ActivatorUtilities.CreateInstance<UserPeriodReportViewModel>(
                     _services, data, userId, minimumHours);
    var dialog = CreateDialog<UserPeriodReportWindow>(vm);
    ShowDialog(dialog, vm);
}
```

---

## Шаг 10 — UserPeriodReportViewModel (новый файл)

**Файл:** `Presentation/ViewModels/Statistics/UserPeriodReportViewModel.cs`

```
Реализует IRequestCloseViewModel
Конструктор: (YearStatisticsDto data, Guid userId, int minimumHours, ILocalizationService localization)
```

Внутренний record:
```csharp
public record MonthRow(string MonthName, int Hours);
```

Свойства:
| Свойство | Тип | Описание |
|---|---|---|
| `Title` | string | Имя пользователя + диапазон дат |
| `MonthRows` | `IReadOnlyList<MonthRow>` | Список месяц/часы |
| `TotalHours` | int | Итог из `YearStatisticsRowDto.TotalHours` |
| `MinimumHours` | int | Из конструктора |
| `HoursToNorm` | int | `Math.Max(0, MinimumHours - TotalHours)` |
| `IsDeficient` | bool | `TotalHours < MinimumHours` |
| `CloseCommand` | LambdaCommand | `RequestClose?.Invoke(null)` |

Формирование `MonthRows`: пройтись по `data.Months`, для каждого `YearMonth` взять
`row?.HoursByMonth.GetValueOrDefault(month, 0) ?? 0`. Имя месяца — через
`new CultureInfo(currentLanguage).DateTimeFormat.GetMonthName(month.Month)` + год.

---

## Шаг 11 — UserPeriodReportWindow.xaml (новый файл)

**Файл:** `Presentation/Windows/Statistics/UserPeriodReportWindow.xaml`

Структура окна:
```
[Title — имя пользователя]
[Период: 01.10.2025 — 30.09.2026]

──────────────────────────
 Месяц          │ Часы
──────────────────────────
 Октябрь 2025   │  12
 Ноябрь 2025    │  24
 ...            │  ...
──────────────────────────
 Итого          │  [TotalHours]   ← цвет: красный/зелёный
──────────────────────────
 Норма          │  72
 До нормы       │  [HoursToNorm]  ← видимость: Collapsed если !IsDeficient
 Норма выполнена│               ← видимость: Collapsed если IsDeficient
──────────────────────────
      [Экспорт в PDF]  [Закрыть]
```

Цвет текста итоговой строки: триггер по `IsDeficient` → `Red` / `Green`.

---

## Шаг 12 — UserPeriodReportPdfDto (новый файл)

**Файл:** `Models/Dto/UserPeriodReportPdfDto.cs`

```csharp
public record UserPeriodReportPdfDto(
    string UserName,
    DateTime PeriodStart,
    DateTime PeriodEnd,
    IReadOnlyList<(string MonthName, int Hours)> Months,
    int TotalHours,
    int MinimumHours);
```

---

## Шаг 13 — IUserPeriodReportPdfService (новый файл)

**Файл:** `Services/ReportServices/IUserPeriodReportPdfService.cs`

```csharp
public interface IUserPeriodReportPdfService
{
    Task ExportAsync(UserPeriodReportPdfDto dto);
}
```

---

## Шаг 14 — UserPeriodReportPdfService (новый файл)

**Файл:** `Services/ReportServices/UserPeriodReportPdfService.cs`

Использует QuestPDF. Зависимости: `ILocalizationService`, `ISettingsService`, `IFileLauncherService`.

Поведение:
- Сохраняет файл в `settings.Current.DefaultExportFolder` с именем
  `Report_{UserName}_{start:yyyyMM}-{end:yyyyMM}.pdf`
- Если папка не существует — создаёт её
- Если `settings.Current.OpenPdfAfterExport == true` — открывает файл через `IFileLauncherService`

Структура PDF (A5 Portrait):
```
Заголовок: [UserName]
Подзаголовок: [PeriodStart:MM.yyyy] — [PeriodEnd:MM.yyyy]

Таблица (2 колонки: Месяц | Часы):
  Октябрь 2025  │  12
  Ноябрь 2025   │  24
  ...
  ──────────────────
  Итого         │  [TotalHours]   ← красный/зелёный цвет текста
  Норма         │  [MinimumHours]
  До нормы      │  [HoursToNorm]  ← строка только если дефицит
  Норма выполнена  ← строка только если НЕ дефицит
```

Цвет текста строки «Итого»: `Colors.Red.Medium` если дефицит, `Colors.Green.Medium` иначе.

---

## Шаг 15 — UserPeriodReportViewModel: ExportPdfCommand

**Файл:** `Presentation/ViewModels/Statistics/UserPeriodReportViewModel.cs`

Добавить в конструктор: `IUserPeriodReportPdfService pdfService`, `IMessageService messageService`

```csharp
ExportPdfCommand = new LambdaCommand(async () =>
{
    var dto = new UserPeriodReportPdfDto(
        userName, PeriodStart, PeriodEnd,
        MonthRows.Select(r => (r.MonthName, r.Hours)).ToList(),
        TotalHours, MinimumHours);
    try
    {
        await _pdfService.ExportAsync(dto);
    }
    catch (Exception ex)
    {
        _messageService.ShowError(ex.Message, _localization["Export_Error"]);
    }
});
```

Свойства `PeriodStart` и `PeriodEnd` (DateTime) — добавить публичными.

---

## Шаг 16 — Локализация: добавить ключ экспорта

В таблицу ключей (шаг 4) добавить строку:

| Ключ | ru | en | uk |
|---|---|---|---|
| `UserReport_ExportPdf` | Экспорт в PDF | Export to PDF | Експорт у PDF |

Итого — **10 ключей** вместо 9.

---

## Шаг 17 — App.xaml.cs: регистрация

**Файл:** `App.xaml.cs`

```csharp
services.AddTransient<UserPeriodReportWindow>();
services.AddSingleton<IUserPeriodReportPdfService, UserPeriodReportPdfService>();
```

---

## Итого: затронутые файлы

| # | Файл | Действие |
|---|---|---|
| 1 | `Infrastructure/Settings/AppSettings.cs` | + поле `MinimumHours` |
| 2 | `Presentation/ViewModels/SettingsViewModel.cs` | + свойство `MinimumHoursText`, валидация, Clone, ApplyChanges |
| 3 | `Presentation/Windows/SettingsDialog.xaml` | + TextBox нормы |
| 4 | `Presentation/Windows/SettingsDialog.xaml.cs` | + обработчик ввода только цифр |
| 5 | `Presentation/Resources/Localization/Strings.ru.xaml` | + 10 ключей |
| 6 | `Presentation/Resources/Localization/Strings.en.xaml` | + 10 ключей |
| 7 | `Presentation/Resources/Localization/Strings.uk.xaml` | + 10 ключей |
| 8 | `Presentation/ViewModels/UserViewModel.cs` | + `showReport` callback + `ShowReportCommand` |
| 9 | `Presentation/ViewModels/SidebarUsersViewModel.cs` | + `IDialogService`, callback в `CreateUserVm` |
| 10 | `Presentation/Views/SidebarUsersListView.xaml` | + пункт меню `Sidebar_ShowReport` |
| 11 | `Services/DialogServices/IDialogService.cs` | + `ShowUserPeriodReport` |
| 12 | `Services/DialogServices/DialogService.cs` | + реализация `ShowUserPeriodReport` |
| 13 | `Presentation/ViewModels/Statistics/UserPeriodReportViewModel.cs` | **новый** |
| 14 | `Presentation/Windows/Statistics/UserPeriodReportWindow.xaml` | **новый** |
| 15 | `Presentation/Windows/Statistics/UserPeriodReportWindow.xaml.cs` | **новый** |
| 16 | `Models/Dto/UserPeriodReportPdfDto.cs` | **новый** |
| 17 | `Services/ReportServices/IUserPeriodReportPdfService.cs` | **новый** |
| 18 | `Services/ReportServices/UserPeriodReportPdfService.cs` | **новый** |
| 19 | `App.xaml.cs` | + регистрация окна и PDF-сервиса |
