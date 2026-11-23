# TaskTracker API

A task management REST API built with ASP.NET Core 10, EF Core 10 and SQL Server.

## Features

- Create, read, update and delete tasks
- Filter by status and search titles
- Paginated results with a capped page size and stable ordering
- Task counts grouped by status
- DTOs and request validation, including whitespace-only title checks
- `201 Created`, `404 Not Found` and ProblemDetails responses
- SQL Server indexes for status and creation-date queries

## Run

Use SQL Server LocalDB on Windows, or update `ConnectionStrings:Default` in `appsettings.json` to point to another SQL Server instance. From this folder:

```powershell
dotnet restore
dotnet run
```

Open the Swagger URL printed in the console. The initial database migration runs automatically at startup.

## Example requests

```http
GET /api/tasks?status=InProgress&search=report&page=1&pageSize=10
GET /api/tasks/stats
POST /api/tasks
Content-Type: application/json

{
  "title": "Finish API documentation",
  "description": "Review the endpoints and examples",
  "dueDate": "2026-12-01T18:00:00Z"
}
```

## Notes

- Request DTOs keep persistence entities out of the API contract.
- Page sizes are limited to 50, and results have a deterministic secondary sort by ID.
- Status is stored as a readable string in SQL Server and serialized as a string in JSON.
- Read-only queries use `AsNoTracking()`.
