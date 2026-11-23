# ASP.NET Core API Portfolio

Three small APIs built with ASP.NET Core 10, Entity Framework Core and SQL Server. Each project is independent and has its own database, migrations and README.

| Project | Main topics |
|---|---|
| `TaskTracker.Api` | Controller-based CRUD, DTO validation, filtering, paging and aggregate queries |
| `ExpenseTracker.Api` | JWT authentication, BCrypt password hashing, user-scoped data and SQL aggregation |
| `UrlShortener.Api` | Minimal APIs, URL validation, unique short codes, expiry and atomic click counts |

## Requirements

- .NET 10 SDK
- SQL Server Express LocalDB on Windows, or another SQL Server instance
- Visual Studio 2026 or a recent C# editor

The default connection strings use three separate LocalDB databases. Change `ConnectionStrings:Default` in a project's configuration, or set the `ConnectionStrings__Default` environment variable, to use a different SQL Server instance. The LocalDB strings trust the local development certificate; use a valid SQL Server certificate and `TrustServerCertificate=False` outside local development.

## Run a project

Open a terminal in the project folder and run:

```powershell
dotnet restore
dotnet run
```

The APIs apply their checked-in EF Core migrations on startup. Each project includes launch profiles that set the Development environment and open Swagger when you run it locally.

## ExpenseTracker secret

The JWT signing key is intentionally not stored in `appsettings.json`. From `ExpenseTracker.Api`, create a local development secret:

```powershell
$key = [Convert]::ToBase64String([Security.Cryptography.RandomNumberGenerator]::GetBytes(32))
dotnet user-secrets set "Jwt:Key" $key
```

The application refuses to start if the key is missing or shorter than 32 UTF-8 bytes. In a deployed environment, provide it through the `Jwt__Key` environment variable or a proper secret store.

## Migrations

Install the EF Core command-line tool if it is not already available:

```powershell
dotnet tool install --global dotnet-ef --version 10.0.15
```

From an individual project folder, create and apply future migrations with:

```powershell
dotnet ef migrations add DescribeYourChange
dotnet ef database update
```

Do not use `EnsureCreated()` alongside migrations. The projects use migrations so future schema changes can be applied incrementally.
