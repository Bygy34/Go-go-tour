# GoGo Tour (MVC test project)

Обновленный тестовый проект без SPA/Angular и без реальной БД.

## Что сделано
- Переведено на **ASP.NET Core MVC (.NET 6)**.
- Код переведен на совместимый синтаксис **C# 6** (без `record`, top-level statements и новых языковых конструкций).
- Есть 2 пользовательские страницы:
  - `/` — выбор тура.
  - `/tours/{id}` — детали тура + форма записи.
- Есть админ-страница:
  - `/admin` — создание/редактирование туров и просмотр заявок.
- Уровень хранения данных замокан через **generic repository** и in-memory store (фиктивные данные).
- Добавлен единый solution-файл: `GoGoTour.sln`.

## Архитектура
- `GoGoTour.Domain` — сущности (`Tour`, `BookingRequest`).
- `GoGoTour.Application` — абстракция `IGenericRepository<T>`, DTO, бизнес-сервисы.
- `GoGoTour.Infrastructure` — `InMemoryStore` и `InMemoryGenericRepository<T>` (имитация БД).
- `GoGoTour.Api` — MVC контроллеры и Razor Views.

## Запуск
1. Установить .NET 6 SDK.
2. Открыть `GoGoTour.sln` в Visual Studio / Rider.
3. Запустить проект `GoGoTour.Api`.
