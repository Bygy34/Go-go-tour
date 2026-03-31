# GoGo Tour (Test Project)

Тестовый монолитный проект для турагентства:
- **Frontend**: React SPA (2 основные страницы: список туров и детали + форма заявки).
- **Admin UI**: создание/редактирование туров, просмотр заявок.
- **Backend**: ASP.NET Core Web API на **.NET 6** с разделением на слои в стиле Clean Architecture.
- **База данных**: реляционная SQLite через **Entity Framework Core (Code First)**.

## Структура

- `frontend/` — React SPA на Vite.
- `backend/src/GoGoTour.Domain` — сущности домена.
- `backend/src/GoGoTour.Application` — DTO + сервисы use-case.
- `backend/src/GoGoTour.Infrastructure` — EF Core DbContext и инфраструктура.
- `backend/src/GoGoTour.Api` — API контроллеры и DI-конфигурация.

## API

### Public
- `GET /api/tours` — активные туры.
- `GET /api/tours/{id}` — детали тура.
- `POST /api/bookings/{tourId}` — отправка заявки.

### Admin (header `X-Admin-Token`)
- `GET /api/tours/admin` — все туры.
- `POST /api/tours/admin` — создать тур.
- `PUT /api/tours/admin/{id}` — редактировать тур.
- `GET /api/bookings/admin` — все заявки.

## Локальный запуск

### Backend
1. Установить .NET 6 SDK.
2. Из `backend/src/GoGoTour.Api` выполнить:
   - `dotnet restore`
   - `dotnet run`

По умолчанию админ-токен в `appsettings.json`:
`super-admin-token`

### Frontend
1. Установить Node.js 18+.
2. Из `frontend` выполнить:
   - `npm install`
   - `npm run dev`

Frontend ожидает API по адресу `http://localhost:5000/api`.
