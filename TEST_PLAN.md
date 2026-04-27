# TEST_PLAN.md

План написания тестов для DutyPlanner. Тестов пока нет совсем — документ описывает с чего начать и в каком порядке.

---

## Стек

| Пакет | Роль |
|---|---|
| `xUnit` | Test runner |
| `NSubstitute` | Мокирование интерфейсов |
| `FluentAssertions` | Читаемые assert'ы |

---

## Структура проекта

```
DutyPlanner.sln
├── DutyPlanner.csproj          ← основной проект
└── DutyPlanner.Tests/
    ├── DutyPlanner.Tests.csproj
    ├── Unit/
    │   ├── Services/
    │   │   ├── UserServiceTests.cs
    │   │   ├── MonthStatisticsServiceTests.cs
    │   │   └── YearStatisticsServiceTests.cs
    │   ├── Repositories/
    │   │   ├── JsonUserRepositoryTests.cs
    │   │   └── JsonDayRepositoryTests.cs
    │   └── ViewModels/
    │       └── MonthPageViewModelTests.cs
    └── Integration/
        └── Infrastructure/
            └── JsonFileStorageTests.cs
```

`DutyPlanner.Tests.csproj` — тип `net10.0` (не `net10.0-windows`), чтобы интеграционные тесты шли в CI без WPF.

---

## Фаза 1 — Сервисы (моки через NSubstitute)

> Самый быстрый ROI: чистая бизнес-логика, зависимости мокируются в одну строку.

### 1.1 `UserServiceTests.cs`

Зависимость: `IUserRepository` (мок).

| # | Имя теста | Что проверяет |
|---|---|---|
| 1 | `GetAll_ReturnsAllUsersFromRepository` | Делегирует в `repository.GetAll()` без изменений |
| 2 | `Add_CreatesUserWithCorrectNameAndHours` | Возвращённый `User` имеет переданные `name` / `hours` |
| 3 | `Add_AssignsNonEmptyGuid` | `User.Id != Guid.Empty` |
| 4 | `Add_CallsSaveOnRepository` | После `Add()` вызывается `repository.Save(...)` |
| 5 | `Remove_SavesListWithoutRemovedUser` | После `Remove(id)` в `repository.Save(...)` нет пользователя с этим `Id` |
| 6 | `Update_ReplacesExistingUserById` | После `Update(user)` `GetAll()` возвращает обновлённое имя/часы |

### 1.2 `MonthStatisticsServiceTests.cs`

Зависимость: `IJsonFileStorage` (мок).

| # | Имя теста | Что проверяет |
|---|---|---|
| 1 | `BuildMonth_EmptyFolder_ReturnsZeroTotals` | `TotalActive == 0`, `TotalReserve == 0` при отсутствии файлов |
| 2 | `BuildMonth_NullFromStorage_TreatedAsEmpty` | Мок возвращает `null` — нет `NullReferenceException` (фикс 1.2) |
| 3 | `BuildMonth_CountsActiveAndReserveCorrectly` | Два файла с известными пользователями — суммы совпадают |
| 4 | `BuildMonth_IgnoresFilesOutsideMonth` | Файл не за запрошенный месяц — не входит в статистику |

### 1.3 `YearStatisticsServiceTests.cs`

Зависимости: `IUserService`, `IMonthStatisticsService`, `IMonthManagementService` (моки).

| # | Имя теста | Что проверяет |
|---|---|---|
| 1 | `BuildPeriod_CallsBuildMonthOnlyForExistingMonths` | `IMonthStatisticsService.BuildMonth` вызывается только для месяцев, которые есть в `LoadExistingMonths()` |
| 2 | `BuildPeriod_SkipsMonthsOutsideDateRange` | Месяц до `start` или после `end` не попадает в результат |
| 3 | `BuildPeriod_LoadExistingMonthsCalledOnce` | `IMonthManagementService.LoadExistingMonths()` вызван ровно 1 раз (проверка фикса N+1, фаза 5) |
| 4 | `BuildPeriod_AggregatesUserRows` | Строки статистики содержат всех пользователей из `IUserService.GetAll()` |

---

## Фаза 2 — Репозитории (мок `IJsonFileStorage`)

> Проверяем маппинг: как репозиторий сериализует и десериализует данные.

### 2.1 `JsonUserRepositoryTests.cs`

| # | Имя теста | Что проверяет |
|---|---|---|
| 1 | `GetAll_DeserializesUsersFromStorage` | Мок возвращает `List<User>` — `GetAll()` отдаёт их без изменений |
| 2 | `GetAll_StorageReturnsNull_ReturnsEmptyList` | `IJsonFileStorage.Load` → `null` — не падает, возвращает `[]` |
| 3 | `Save_SerializesCorrectUsersToStorage` | Переданные пользователи попадают в аргумент `storage.Save(...)` |

### 2.2 `JsonDayRepositoryTests.cs`

| # | Имя теста | Что проверяет |
|---|---|---|
| 1 | `Exists_DelegatesToStorage` | `storage.Exists(path)` == `true` → `repository.Exists(path)` == `true` |
| 2 | `Load_ReturnsUsersFromStorage` | Мок отдаёт список `DayUserDto` — репозиторий возвращает его |
| 3 | `Save_PassesUsersToStorage` | Пользователи попадают в `storage.Save(path, ...)` |
| 4 | `LoadAsync_ReturnsUsersFromStorageAsync` | Async-вариант аналогично п. 2 |
| 5 | `SaveAsync_PassesUsersToStorageAsync` | Async-вариант аналогично п. 3 |

---

## Фаза 3 — Infrastructure (интеграционные, реальная ФС)

> `JsonFileStorage` работает напрямую с файлами — мокировать нет смысла.
> Каждый тест создаёт папку через `Path.GetTempPath() + Guid.NewGuid()` и удаляет в `Dispose`.

### 3.1 `JsonFileStorageTests.cs`

| # | Имя теста | Что проверяет |
|---|---|---|
| 1 | `Exists_FileDoesNotExist_ReturnsFalse` | Несуществующий путь → `false` |
| 2 | `Exists_FileExists_ReturnsTrue` | После `Save` → `true` |
| 3 | `SaveAndLoad_RoundTrip_PreservesData` | Сохранить объект → загрузить → содержимое идентично |
| 4 | `SaveAndLoad_List_RoundTrip` | То же для `List<T>` |
| 5 | `SaveAsync_LoadAsync_RoundTrip` | Async-вариант round-trip |
| 6 | `Load_MissingFile_ReturnsDefault` | Несуществующий файл → `default(T)` (или задокументированное поведение) |

---

## Фаза 4 — ViewModels (регрессия на баги)

> ViewModels с WPF-зависимостями (`ObservableCollection`, `Dispatcher`) требуют `[STAThread]` или адаптера. В xUnit достаточно атрибута `[WpfFact]` из пакета `Xunit.StaFact`.

### 4.1 `MonthPageViewModelTests.cs`

Зависимости: `IDayRepository`, `IDialogService`, `ILocalizationService` (моки).

| # | Имя теста | Что проверяет |
|---|---|---|
| 1 | `AddDay_DateWithCorrectYear_DayIsAdded` | День с правильным годом успешно добавляется (базовый happy-path) |
| 2 | `AddDay_DateWithWrongYear_DayIsNotAdded` | Дата с чужим годом отклоняется — список дней не меняется |
| 3 | `AddDay_DateWithWrongMonth_DayIsNotAdded` | Дата с чужим месяцем отклоняется (проверяет фикс 1.1: тавтология `date.Year != date.Year`) |
| 4 | `AddDay_DuplicateDate_DayIsNotAdded` | Повторная дата не добавляется |

---

## Фаза 5 — Локализация (парность ключей)

> Уже есть `LocalizationValidator` в проекте. Оформить его проверки как xUnit-тесты.

### 5.1 `LocalizationKeyParityTests.cs`

| # | Имя теста | Что проверяет |
|---|---|---|
| 1 | `EnglishDictionary_ContainsAllRussianKeys` | Каждый ключ из `Strings.ru.xaml` есть в `Strings.en.xaml` |
| 2 | `UkrainianDictionary_ContainsAllRussianKeys` | Каждый ключ из `Strings.ru.xaml` есть в `Strings.uk.xaml` |
| 3 | `NoDictionary_HasExtraKeysNotInRussian` | `en` и `uk` не содержат ключей, которых нет в `ru` |

---

## Порядок реализации

```
Фаза 1 — Сервисы      ← начать здесь, максимальный ROI
    │
    ▼
Фаза 2 — Репозитории  ← зависят от тех же интерфейсов, что уже замокированы
    │
    ▼
Фаза 3 — Integration  ← реальная ФС, нужен temp-каталог в Dispose
    │
    ▼
Фаза 4 — ViewModels   ← требует Xunit.StaFact, трогает WPF-типы
    │
    ▼
Фаза 5 — Локализация  ← отдельный слой, можно в любой момент
```

---

## Сводная таблица

| Фаза | Тест-файл | Кол-во тестов | Сложность |
|------|-----------|---------------|-----------|
| 1.1 | `UserServiceTests.cs` | 6 | Низкая |
| 1.2 | `MonthStatisticsServiceTests.cs` | 4 | Низкая |
| 1.3 | `YearStatisticsServiceTests.cs` | 4 | Низкая |
| 2.1 | `JsonUserRepositoryTests.cs` | 3 | Низкая |
| 2.2 | `JsonDayRepositoryTests.cs` | 5 | Низкая |
| 3.1 | `JsonFileStorageTests.cs` | 6 | Средняя |
| 4.1 | `MonthPageViewModelTests.cs` | 4 | Средняя |
| 5.1 | `LocalizationKeyParityTests.cs` | 3 | Низкая |
| **Итого** | **8 файлов** | **35 тестов** | |
