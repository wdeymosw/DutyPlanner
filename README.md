# DutyPlanner

A Windows desktop application for managing shift schedules and duty assignments, built with WPF and .NET 10.

## Features

- **Schedule Management** — Create and edit monthly duty schedules with day-by-day user assignments
- **User Management** — Add and configure staff with assigned working hours
- **Statistics** — View monthly and yearly summaries of hours and duty distribution
- **Export** — Generate Excel spreadsheets and PDF reports from schedule data
- **Localization** — Supports English, Russian, and Ukrainian
- **File-based Storage** — All data stored as local JSON files, no database required

## Tech Stack

- .NET 10 / WPF (Windows only)
- MVVM architecture with `Microsoft.Extensions.DependencyInjection`
- [ClosedXML](https://github.com/ClosedXML/ClosedXML) — Excel export
- [QuestPDF](https://www.questpdf.com/) — PDF generation
- [FontAwesome5.WPF](https://github.com/MartinTopfsteiner/FontAwesome5) — UI icons

## Requirements

- Windows 10 or later
- [.NET 10.0 SDK](https://dotnet.microsoft.com/download)

## Getting Started

```bash
git clone <repository-url>
cd DutyPlanner
dotnet run
```

Or open `DutyPlanner.sln` in Visual Studio and press **F5**.

## Project Structure

```
DutyPlanner/
├── Infrastrustures/   # File storage, commands, settings, localization
├── Models/            # Data models and DTOs
├── Services/          # Business logic, report and dialog services
└── Presentation/      # Views, ViewModels, converters, behaviors
```

## Data Storage

Data is persisted as JSON files in the following layout:

```
Data/
├── Users/users.json
└── [Year]/[Month]/[Day].json
settings.json
```

---

# DutyPlanner

Десктопное приложение для Windows для управления сменными расписаниями и назначениями дежурств, построенное на WPF и .NET 10.

## Возможности

- **Управление расписанием** — Создание и редактирование ежемесячных расписаний с назначением сотрудников по дням
- **Управление пользователями** — Добавление и настройка сотрудников с указанием рабочих часов
- **Статистика** — Просмотр месячных и годовых сводок по часам и распределению дежурств
- **Экспорт** — Генерация Excel-таблиц и PDF-отчётов из данных расписания
- **Локализация** — Поддержка английского, русского и украинского языков
- **Файловое хранилище** — Все данные хранятся в локальных JSON-файлах, база данных не требуется

## Технологический стек

- .NET 10 / WPF (только Windows)
- Архитектура MVVM с `Microsoft.Extensions.DependencyInjection`
- [ClosedXML](https://github.com/ClosedXML/ClosedXML) — экспорт в Excel
- [QuestPDF](https://www.questpdf.com/) — генерация PDF
- [FontAwesome5.WPF](https://github.com/MartinTopfsteiner/FontAwesome5) — иконки интерфейса

## Требования

- Windows 10 или новее
- [.NET 10.0 SDK](https://dotnet.microsoft.com/download)

## Начало работы

```bash
git clone <repository-url>
cd DutyPlanner
dotnet run
```

Или откройте `DutyPlanner.sln` в Visual Studio и нажмите **F5**.

## Структура проекта

```
DutyPlanner/
├── Infrastrustures/   # Файловое хранилище, команды, настройки, локализация
├── Models/            # Модели данных и DTO
├── Services/          # Бизнес-логика, сервисы отчётов и диалогов
└── Presentation/      # Представления, ViewModel, конвертеры, поведения
```

## Хранение данных

Данные хранятся в JSON-файлах по следующей структуре:

```
Data/
├── Users/users.json
└── [Year]/[Month]/[Day].json
settings.json
```

---

# DutyPlanner

Десктопний застосунок для Windows для керування змінними розкладами та призначеннями чергувань, побудований на WPF і .NET 10.

## Можливості

- **Керування розкладом** — Створення та редагування щомісячних розкладів із призначенням співробітників по днях
- **Керування користувачами** — Додавання та налаштування співробітників із зазначенням робочих годин
- **Статистика** — Перегляд місячних і річних зведень по годинах і розподілу чергувань
- **Експорт** — Генерація Excel-таблиць і PDF-звітів із даних розкладу
- **Локалізація** — Підтримка англійської, російської та української мов
- **Файлове сховище** — Усі дані зберігаються в локальних JSON-файлах, база даних не потрібна

## Технологічний стек

- .NET 10 / WPF (тільки Windows)
- Архітектура MVVM з `Microsoft.Extensions.DependencyInjection`
- [ClosedXML](https://github.com/ClosedXML/ClosedXML) — експорт в Excel
- [QuestPDF](https://www.questpdf.com/) — генерація PDF
- [FontAwesome5.WPF](https://github.com/MartinTopfsteiner/FontAwesome5) — іконки інтерфейсу

## Вимоги

- Windows 10 або новіше
- [.NET 10.0 SDK](https://dotnet.microsoft.com/download)

## Початок роботи

```bash
git clone <repository-url>
cd DutyPlanner
dotnet run
```

Або відкрийте `DutyPlanner.sln` у Visual Studio і натисніть **F5**.

## Структура проєкту

```
DutyPlanner/
├── Infrastrustures/   # Файлове сховище, команди, налаштування, локалізація
├── Models/            # Моделі даних і DTO
├── Services/          # Бізнес-логіка, сервіси звітів і діалогів
└── Presentation/      # Представлення, ViewModel, конвертери, поведінки
```

## Зберігання даних

Дані зберігаються в JSON-файлах за такою структурою:

```
Data/
├── Users/users.json
└── [Year]/[Month]/[Day].json
settings.json
```
