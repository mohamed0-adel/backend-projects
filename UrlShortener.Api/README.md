# UrlShortener API

A small URL shortener built with ASP.NET Core 10 Minimal APIs, EF Core 10 and SQL Server.

## Endpoints

| Method | Route | Purpose |
|---|---|---|
| POST | `/api/links` | Create a short link with an optional custom code and expiry |
| GET | `/{code}` | Redirect to the original URL and increment the click count |
| GET | `/api/links/{code}` | Return link statistics |
| DELETE | `/api/links/{code}` | Delete a link |

## Run

Use SQL Server LocalDB on Windows, or update `ConnectionStrings:Default` in `appsettings.json`. From this folder:

```powershell
dotnet restore
dotnet run
```

Create a link through Swagger, then open the returned `shortPath` on the same host. The initial database migration runs automatically at startup.

## Design notes

- Only absolute `http` and `https` URLs are accepted, and URLs longer than 2048 characters are rejected.
- Generated codes use a cryptographically secure random-number generator.
- Code comparison and the unique index use a binary collation, so uppercase and lowercase codes remain distinct on a case-insensitive SQL Server database.
- The database unique index handles concurrent custom-code requests; generated codes retry after a collision.
- Clicks are incremented with a single SQL update, avoiding lost increments when requests arrive together.
- The redirect checks expiry both before and during the update. Expired links return `410 Gone`.
- API responses use a small stats DTO rather than exposing the EF entity directly.
