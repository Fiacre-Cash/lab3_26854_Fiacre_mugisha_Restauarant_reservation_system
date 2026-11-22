# Restaurant Reservation System

A Windows Forms restaurant reservation and management application built with C#, .NET 8, and SQLite. The system uses a simple MVC-style organization to support customer reservations and administrative restaurant operations.

## Features

### Customer features

- Create a customer account and sign in securely.
- Create and manage restaurant reservations.
- Search and filter reservations.
- Browse menu items by category.
- Log out and switch users.

### Administrator features

- View reservation activity and reservation statistics.
- Search reservations by name, email, or reference number.
- Filter reservations by status and date range.
- Manage menu items and availability.
- Manage restaurant tables and table status.
- View dashboard information, including tonight's reservations, covers, pending reservations, and weekend activity.

## Technology stack

- C#
- .NET 8 (`net8.0-windows`)
- Windows Forms
- SQLite through `Microsoft.Data.Sqlite` 8.0.10
- MVC-style separation using controllers, data repositories, models, and views

## Project structure

```text
RestaurantReservation/
├── Controllers/   Application and session logic
├── Data/          SQLite database, repositories, and password hashing
├── Models/        Application data models
├── Views/         Windows Forms user interface
└── Restaurant Reservation System.csproj
```

## Requirements

- Windows 10 or later
- Visual Studio 2022 or later with the .NET desktop development workload, or the .NET 8 SDK
- A Windows environment capable of running Windows Forms applications

## Getting started

1. Clone the repository:

   ```bash
   git clone https://github.com/Fiacre-Cash/lab3_26854_Fiacre_mugisha_Restauarant_reservation_system.git
   ```

2. Open `RestaurantReservation/Restaurant Reservation System.csproj` in Visual Studio.
3. Restore NuGet packages if Visual Studio does not restore them automatically.
4. Build and run the project.

The application creates and initializes its SQLite database automatically on first startup. The database is stored at:

```text
%LocalAppData%\FiacreEats\restaurant.db
```

## Demo accounts

The first database initialization seeds the following accounts:

| Role | Email | Password |
| --- | --- | --- |
| Administrator | `admin@fiacreeats.com` | `Admin@123` |
| Customer | `customer1@fiacre.rw` | `pass1234` |

These credentials are for demonstration purposes only. Change or remove seeded credentials before using the application in a production environment.

## Database

The application creates the following SQLite tables automatically:

- `Users`
- `Reservations`
- `MenuItems`
- `RestaurantTables`

The database directory is created automatically when the application starts.

## Build from the command line

From the repository root, run:

```powershell
dotnet restore "RestaurantReservation\Restaurant Reservation System.csproj"
dotnet build "RestaurantReservation\Restaurant Reservation System.csproj"
dotnet run --project "RestaurantReservation\Restaurant Reservation System.csproj"
```

## Repository

[GitHub repository](https://github.com/Fiacre-Cash/lab3_26854_Fiacre_mugisha_Restauarant_reservation_system)
