# CODE_REVIEW_FIXES.md

Исправления по результатам code review. Сгруппированы по риску и сложности.
Каждая фаза даёт зелёный билд. Фазы 1–3 независимы и могут выполняться параллельно.

---

## Фаза 1 — Баги (1–3 строки, нулевой риск)

- [x] **1.1** `MonthPageViewModel.cs:126` — тавтология в валидации даты
  ```csharp
  // было:
  if (date.Year != date.Year || date.Month != Month)
  // стало:
  if (date.Year != Year || date.Month != Month)
  ```

- [x] **1.2** `MonthStatisticsService.cs:29` — защита от `null` при загрузке файла
  ```csharp
  // было:
  var users = _storage.Load<List<DayUserDto>>(file)
  // стало:
  var users = (_storage.Load<List<DayUserDto>>(file) ?? [])
  ```

- [x] **1.3** `YearStatisticsViewModel.cs:77` — удалить неиспользуемую переменную
  ```csharp
  // удалить строку:
  var culture = new CultureInfo(_settings.Current.Language);
  ```

---

## Фаза 2 — Регрессия локализации (один файл)

> `MonthStatisticsViewModel` уже использует ключи. `YearStatisticsViewModel` регрессировал к хардкоду.

- [x] **2.1** `YearStatisticsViewModel.cs:88–106` — заменить все строки на ключи локализации

  | Хардкод | Ключ |
  |---|---|
  | `"Файл успешно экспортирован."` | `_localization["Excel_Successful_Preservation"]` |
  | `"Экспорт в Excel"` (все три вызова) | `_localization["Excel_ExportCompleted"]` |
  | `"Не удалось сохранить файл.\n\n"` | `_localization["Exel_Error_Export"]` |
  | `"Возможные причины:\n"` | `_localization["Excel_Strign_1"]` |
  | `"• файл уже открыт в Excel\n"` | `_localization["Excel_Strign_2"]` |
  | `"• нет прав на запись\n"` | `_localization["Excel_Strign_3"]` |
  | `"• диск недоступен\n\n"` | `_localization["Excel_Strign_4"]` |
  | `"Закройте файл и попробуйте снова."` | `_localization["Excel_String_5"]` |
  | `"Произошла ошибка при экспорте.\n\n"` | `_localization["Exel_Error_Export_2"]` |

---

## Фаза 3 — Удаление мёртвого кода

- [x] **3.1** Удалить `Services/Statistics/FileLauncher.cs` целиком
  — Полный дубликат `Infrastructure/Shell/FileLauncherService.cs`, нигде не вызывается.

- [x] **3.2** `YearStatisticsService.cs:21–68` — удалить закомментированный метод `BuildYear`

- [x] **3.3** `ExcelExportService.cs:150–152` — удалить приватный метод `MonthName(int month)`
  — Нигде не вызывается.

- [x] **3.4** `MonthStatisticsService.cs:103–113` — удалить приватный метод `EnumerateMonths`
  — Нигде не вызывается (только в `YearStatisticsService`, там свой экземпляр).

---

## Фаза 4 — Корректность: `async void`

> `AddFromSidebar` и `MoveUser` в `DayViewModel` — не event-handler'ы, но объявлены `async void`.
> Исключение из `SaveUsersAsync()` падает необработанным.

- [ ] **4.1** `DayViewModel.cs:66` (`AddFromSidebar`) — обернуть `await SaveUsersAsync()` в try-catch
  ```csharp
  private async void AddFromSidebar(UserViewModel user, DayUserPlacement placement)
  {
      // ... добавление в коллекцию ...
      try { await SaveUsersAsync(); }
      catch { /* логировать или уведомить пользователя */ }
  }
  ```

- [ ] **4.2** `DayViewModel.cs:87` (`MoveUser`) — то же самое

---

## Фаза 5 — Производительность: N+1 в `YearStatisticsService`

- [ ] **5.1** `YearStatisticsService.cs:82–97` — вынести `LoadExistingMonths()` за пределы цикла

  ```csharp
  // было: LoadExistingMonths() вызывается N раз (по разу на каждый месяц)
  foreach (var (year, month) in months)
  {
      var monthInfo = _monthManagement.LoadExistingMonths()
          .FirstOrDefault(m => m.Year == year && m.Month == month);
  
  // стало: один вызов, быстрый поиск по HashSet
  var existingMonths = _monthManagement.LoadExistingMonths()
      .ToHashSet(m => (m.Year, m.Month));          // один вызов до цикла
  
  foreach (var (year, month) in months)
  {
      if (!existingMonths.Contains((year, month)))
          continue;
  ```

---

## Фаза 6 — DIP: `MonthStatisticsService` обходит репозиторий

> Прямой вызов `Directory.GetFiles` в сервисе нарушает DIP — `IDayRepository.GetDayFilePaths` уже существует.

- [ ] **6.1** Добавить `IDayRepository` в конструктор `MonthStatisticsService`
  ```csharp
  // было:
  public MonthStatisticsService(IJsonFileStorage storage)

  // стало:
  public MonthStatisticsService(IJsonFileStorage storage, IDayRepository dayRepository)
  ```

- [ ] **6.2** Заменить `Directory.GetFiles(folderPath, "*.json")` на `_dayRepository.GetDayFilePaths(folderPath)`

- [ ] **6.3** Зарегистрировать обновлённый конструктор в `App.xaml.cs` (DI подхватит автоматически при наличии регистрации `IDayRepository`)

---

## Фаза 7 — Дедупликация: экспорт в Excel (средняя сложность)

> `MonthStatisticsViewModel.ExportExcel` и `YearStatisticsViewModel.ExportToExcel` идентичны по структуре:
> построить путь → `Directory.CreateDirectory` → `await export` → открыть файл → показать сообщения.

- [ ] **7.1** Создать `ExportHelper` (приватный метод или `static` хелпер в `Presentation/ViewModels/`)

  ```csharp
  // Сигнатура:
  private async Task RunExportAsync(
      string filePath,
      Func<Task> exportAction)
  {
      Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);
      try
      {
          await exportAction();
          if (_settings.Current.OpenExcelAfterExport)
              _fileLauncher.OpenIfExists(filePath);
          _messages.ShowInfo(_localization["Excel_Successful_Preservation"],
                             _localization["Excel_ExportCompleted"]);
      }
      catch (IOException)
      {
          _messages.ShowError(BuildIoErrorText(), _localization["Excel_ExportCompleted"]);
      }
      catch (Exception ex)
      {
          _messages.ShowError($"{_localization["Exel_Error_Export_2"]}\n\n{ex.Message}",
                              _localization["Excel_ExportCompleted"]);
      }
  }
  ```

- [ ] **7.2** Упростить `ExportExcel` и `ExportToExcel` до вызова `RunExportAsync`

---

## Фаза 8 — Минорные улучшения

- [ ] **8.1** `YearStatisticsViewModel.Title` — кэшировать `CultureInfo`
  ```csharp
  // Title вызывается из биндинга при каждом обновлении.
  // CultureInfo — тяжёлый объект, пересоздавать не нужно.
  // Подписаться на LanguageChanged и обновлять поле + NotifyPropertyChanged(nameof(Title))
  ```

---

## Порядок выполнения

```
Фазы 1, 2, 3  ←  независимы, можно параллельно (каждая — один файл или несколько строк)
      │
      ▼
Фаза 4  ←  async void (DayViewModel)
      │
      ▼
Фаза 5  ←  N+1 (YearStatisticsService)
      │
      ▼
Фаза 6  ←  DIP (MonthStatisticsService + App.xaml.cs)
      │
      ▼
Фаза 7  ←  Дедупликация (затрагивает два VM сразу)
      │
      ▼
Фаза 8  ←  по желанию, независимо
```

---

## Сводная таблица

| Фаза | Файлы | Строк изменений | Риск |
|------|-------|-----------------|------|
| 1 — Баги | 3 файла | ~6 | Нулевой |
| 2 — Локализация | 1 файл | ~20 | Нулевой |
| 3 — Мёртвый код | 4 файла | −80 | Нулевой |
| 4 — async void | 1 файл | ~10 | Низкий |
| 5 — N+1 | 1 файл | ~8 | Низкий |
| 6 — DIP | 2 файла | ~10 | Низкий |
| 7 — Дедупликация | 2 файла | ~40 | Средний |
| 8 — Кэш Title | 1 файл | ~10 | Низкий |
