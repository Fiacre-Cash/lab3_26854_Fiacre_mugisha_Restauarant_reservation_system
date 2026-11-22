# Fiacre Eats - Restaurant Reservation System (WinForms)

Desktop port of the original React/Node web app, built with **C# / .NET 8 WinForms**
and a **SQLite** database. The user works with the database entirely through the GUI.

## Architecture (MVC)

| Layer | Folder | Responsibility |
|-------|--------|----------------|
| Model | `Models/`, `Data/` | Entities (`User`, `Reservation`, `MenuEntry`, `RestaurantTable`) and the repositories that talk to SQLite |
| View | `Views/` | WinForms windows and dialogs. They contain no SQL and no business rules |
| Controller | `Controllers/` | Validation, permissions (admin vs customer), and calls to the repositories. Views call controllers only |

## Features

**Customer:** register / log in, create / edit / cancel own reservations, browse the menu.

**Admin:** everything above for all reservations, plus confirm / complete / no-show / delete,
search and filter by name, status and date range, CSV export, menu CRUD (with sold-out toggle),
table CRUD with status cycling, and a dashboard (today's bookings, weekly covers, pending, weekend occupancy).

## Run

Requirements: Windows, [.NET 8 SDK](https://dotnet.microsoft.com/download).

```
dotnet restore
dotnet run
```

The database is created automatically on first launch at
`%LOCALAPPDATA%\FiacreEats\restaurant.db` (see `schema.sql`).

### Demo accounts

| Role | Email | Password |
|------|-------|----------|
| Admin | admin@fiacreeats.com | Admin@123 |
| Customer | customer1@fiacre.rw | pass1234 |

## Notes
- Passwords are stored as PBKDF2 hashes.
- Business rule carried over from the original: max 5 active bookings per date/time slot.
- Menu, tables and users, which were `localStorage` mock data in the web app, are now real database tables.
