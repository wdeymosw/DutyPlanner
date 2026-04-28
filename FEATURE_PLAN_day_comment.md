# План: Поле комментария для дня

## Требования
- Один комментарий на весь день (не на пользователя)
- Отображается как 4-й столбец рядом с Актив / Резерв
- Редактируемый TextArea с переносом строк
- Автосохранение при вводе (debounce ~500ms)
- Отображается только в PDF-экспорте (не в Excel)

---

## Затронутые файлы

| Файл | Изменение |
|---|---|
| `Models/Dto/DayFileDto.cs` | **Создать** — обёртка для JSON файла дня |
| `Domain/Repositories/IDayRepository.cs` | Обновить сигнатуры Load/Save |
| `Infrastructure/Persistence/Json/JsonDayRepository.cs` | Новый формат + миграция старого |
| `Presentation/ViewModels/DayViewModel.cs` | Свойство Comment + debounce-сохранение |
| `Presentation/Resources/Templates/DayTemplate.xaml` | 4-й столбец с TextArea |
| `Presentation/Resources/Localization/Strings.ru.xaml` | Ключ `Day_Comment` |
| `Presentation/Resources/Localization/Strings.uk.xaml` | Ключ `Day_Comment` |
| `Presentation/Resources/Localization/Strings.en.xaml` | Ключ `Day_Comment` |
| `Application/DTOs/MonthExportDto.cs` | Добавить `Comment` в `DayExportDto` |
| `Presentation/ViewModels/MainWindowsViewModel.cs` | Передать `Comment` при экспорте |
| `Services/ReportServices/PdfMonthReportService.cs` | Новая колонка в PDF-таблице |

---

## Шаги реализации

### Шаг 1 — `Models/Dto/DayFileDto.cs` (создать)

Новый класс-обёртка для хранения данных дня в JSON.

```csharp
public class DayFileDto
{
    public string Comment { get; set; } = "";
    public List<DayUserDto> Users { get; set; } = [];
}
```

**Почему:** Текущий JSON — плоский массив `[...]`. Чтобы добавить комментарий (поле уровня дня, не пользователя), нужен объект-обёртка `{ "Comment": "...", "Users": [...] }`.

---

### Шаг 2 — `IDayRepository` — обновить интерфейс

Заменить `List<DayUserDto>` на `DayFileDto` в сигнатурах:

```csharp
DayFileDto Load(string filePath);
void Save(string filePath, DayFileDto data);
Task<DayFileDto> LoadAsync(string filePath);
Task SaveAsync(string filePath, DayFileDto data);
```

`InitializeDay` тоже обновить — сохранять `new DayFileDto()`.

---

### Шаг 3 — `JsonDayRepository` — реализация с миграцией старого формата

При загрузке: определить формат по первому символу JSON:
- Начинается с `[` → **старый формат** → обернуть в `DayFileDto { Comment = "", Users = <загруженный список> }`
- Начинается с `{` → **новый формат** → десериализовать как `DayFileDto`

При сохранении — всегда новый формат (`DayFileDto`).

Это обеспечивает бесшовную совместимость с уже созданными файлами.

---

### Шаг 4 — `DayViewModel` — свойство `Comment` + debounce

1. Загрузка: `Load()` читает `DayFileDto.Comment` в поле `_comment`.
2. Свойство:
   ```csharp
   private string _comment = "";
   public string Comment
   {
       get => _comment;
       set
       {
           if (Set(ref _comment, value))
               RestartSaveTimer();
       }
   }
   ```
3. Debounce через `DispatcherTimer` (интервал 500ms, `IsOneShot`-паттерн):
   ```csharp
   private DispatcherTimer? _saveTimer;

   private void RestartSaveTimer()
   {
       _saveTimer?.Stop();
       _saveTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(500) };
       _saveTimer.Tick += async (_, _) =>
       {
           _saveTimer.Stop();
           try { await SaveDayAsync(); } catch { }
       };
       _saveTimer.Start();
   }
   ```
4. `SaveDayAsync()` собирает `DayFileDto { Comment = Comment, Users = _users.Select(u => u.ToDto()) }`.
5. Переименовать старый `SaveUsersAsync()` → `SaveDayAsync()` и обновить все вызовы внутри `DayViewModel`.

---

### Шаг 5 — `DayTemplate.xaml` — 4-й столбец

1. Добавить `<ColumnDefinition Width="*"/>` (Comment) после столбца Reserve.
2. Добавить `Border` аналогично Reserve (скруглённые углы справа).
3. Внутри — `TextBox` с:
   - `AcceptsReturn="True"` — перенос по Enter
   - `TextWrapping="Wrap"` — автоперенос
   - `Text="{Binding Comment, Mode=TwoWay, UpdateSourceTrigger=PropertyChanged}"`
   - `MinHeight="120"`, `VerticalScrollBarVisibility="Auto"`
   - Заголовок через `{DynamicResource Day_Comment}`

---

### Шаг 6 — Локализация (3 файла)

Добавить ключ в каждый файл:

| Файл | Значение |
|---|---|
| `Strings.ru.xaml` | `Комментарий` |
| `Strings.uk.xaml` | `Коментар` |
| `Strings.en.xaml` | `Comment` |

---

### Шаг 7 — `DayExportDto` + `MainWindowsViewModel`

В `DayExportDto` добавить:
```csharp
public string Comment { get; init; } = "";
```

В `MainWindowsViewModel.ExportPdf()` добавить в маппинг:
```csharp
Comment = d.Comment,
```

---

### Шаг 8 — `PdfMonthReportService` — колонка в PDF

1. Добавить `RelativeColumn()` в `ColumnsDefinition`.
2. Добавить заголовок колонки (`Day_Comment` или хардкод если PDF-специфично).
3. В теле таблицы добавить:
   ```csharp
   table.Cell().Element(BodyCell).Text(day.Comment ?? "");
   ```

> **Примечание:** Колонка добавляется последней, после Резерва.

---

### Шаг 9 — Миграция существующих JSON-файлов

Одноразовая ручная миграция 36 файлов в папке `Data/` из старого формата (плоский массив) в новый (объект с `Comment` и `Users`).

Выполняется PowerShell-скриптом **после** того, как шаги 1–8 реализованы и приложение собирается.

**Файлы для миграции** (36 штук в 4 папках):
- `Data/2026/January/` — 8 файлов
- `Data/2026/February/` — 8 файлов
- `Data/2026/March/` — 8 файлов
- `Data/2026/April/` — 9 файлов + 3 пустых

**Скрипт миграции** (`migrate_days.ps1`):
```powershell
$dataRoot = "D:\myProject\repo\wdeymosw\DutyPlanner\bin\Debug\net10.0-windows\Data"

Get-ChildItem -Path $dataRoot -Recurse -Filter "*.json" |
    Where-Object { $_.Name -ne "users.json" } |
    ForEach-Object {
        $content = Get-Content $_.FullName -Raw
        $trimmed = $content.TrimStart()

        if ($trimmed.StartsWith("[")) {
            $newContent = "{`n  `"Comment`": `"`",`n  `"Users`": $content`n}"
            Set-Content -Path $_.FullName -Value $newContent -Encoding UTF8
            Write-Host "Migrated: $($_.FullName)"
        } else {
            Write-Host "Skipped (already new format): $($_.FullName)"
        }
    }

Write-Host "Done."
```

> **Важно:** скрипт безопасен — файлы уже в новом формате (начинаются с `{`) пропускаются. Запускать один раз после деплоя шагов 1–8.

---

## Порядок выполнения

```
1 → 2 → 3   (data layer, не ломает UI)
     ↓
     4       (ViewModel, зависит от нового IDayRepository)
     ↓
     5 + 6   (UI + локализация, параллельно)
     ↓
     7 → 8   (экспорт)
     ↓
     9       (миграция существующих файлов — после сборки)
```

---

## Риски

| Риск | Решение |
|---|---|
| Существующие JSON-файлы (плоский массив) | Runtime-миграция в Шаге 3 + одноразовый скрипт в Шаге 9 |
| `InitializeDay` создаёт старый формат | Обновить в Шагах 2–3 |
| Частое сохранение при быстром вводе | Debounce 500ms в Шаге 4 |
| Скрипт испортит файлы повторным запуском | Проверка первого символа — уже новые файлы пропускаются |
