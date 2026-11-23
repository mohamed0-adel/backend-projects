# ExpenseTracker API

A personal expense API built with ASP.NET Core 10, EF Core 10 and SQL Server.

## Features

- Registration and login with BCrypt password hashing and JWT bearer authentication
- Expense CRUD with per-user data isolation
- Date-range and category filtering
- Grand total, count and per-category totals calculated in SQL Server
- Unique normalized email addresses and database-level amount validation
- Swagger bearer-token support in Development

## Run

Use SQL Server LocalDB on Windows, or update `ConnectionStrings:Default` in `appsettings.json`. Create a local JWT signing key from this folder:

```powershell
$key = [Convert]::ToBase64String([Security.Cryptography.RandomNumberGenerator]::GetBytes(32))
dotnet user-secrets set "Jwt:Key" $key
dotnet restore
dotnet run
```

Open Swagger, call `POST /api/auth/register`, copy the returned token, then click **Authorize** and paste the token. The initial database migration runs automatically at startup.

## Design notes

- Passwords are hashed with BCrypt and are never returned by the API.
- Login uses the same unauthorized response for unknown email addresses and incorrect passwords.
- The JWT key is not committed to source control. Use user-secrets locally and a secret store or environment variable (`Jwt__Key`) when deployed.
- All expense reads and writes are scoped to the authenticated user's ID.
- The summary endpoint uses SQL `GROUP BY` and `SUM`; it does not load every expense into application memory.
- SQL Server enforces a unique email index, positive expense amounts and the user/expense relationship.
